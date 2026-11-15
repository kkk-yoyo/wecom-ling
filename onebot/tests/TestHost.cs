// TestHost.cs -- black-box test harness for the 微灵 (WeComSever) OneBot v11 shim.
//
// Build:  csc /nologo /platform:x86 /target:exe TestHost.cs
//         /r:WeComSever.exe /r:OneBotShim.dll /r:Newtonsoft.Json.dll
//         /r:System.dll /r:System.Core.dll /r:System.Xml.dll
//
// Runs against mock_server.py on http://127.0.0.1:6399.  Writes:
//   logs\testhost.log        human-readable trace + evidence
//   logs\commands.jsonl      every JSON command the shim handed to the fake 微灵 handler
//   logs\results_raw.json    machine-readable case results (rendered by render_report.py)
//
// C# 5 only (compiled by the .NET Framework 4.0 csc).
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using OneBotShim;

internal static class Program
{
    // ------------------------------------------------------------------ setup
    const string MockBase = "http://127.0.0.1:6399";
    const string Token = "testtoken";
    const string RobotId = "77880001";
    const long RobotIdLong = 77880001L;

    static string Root;          // tests\bin
    static string TestsDir;      // tests
    static string LogDir;        // tests\logs
    static string ShimLogPath;   // tests\bin\onebot_v11.log
    static StreamWriter HostLog;
    static StreamWriter CmdLog;
    static readonly object CmdLock = new object();
    static readonly List<JObject> Commands = new List<JObject>();
    static int _sendSeq = 0;

    static WxSocketClient Shim1;
    static Func<string> ShimToken;
    static List<string> Shim1Status = new List<string>();

    // frames polled from the mock, ordered by seq (both directions)
    static readonly List<JObject> Frames = new List<JObject>();
    static int FrameCursor = 0;

    static readonly List<CaseResult> Results = new List<CaseResult>();
    static CaseResult Current;
    static List<string> CurrentEvidence;
    static string _lastEcho = "";
    static int _echoSeq = 0;
    static string _startedAt;

    // ---------------------------------------------------------------- results
    sealed class CaseResult
    {
        public string Id;
        public string Name;
        public string Group;
        public string Status;
        public string Error = "";
        public long DurationMs;
        public List<string> Evidence = new List<string>();
    }

    sealed class CheckFailed : Exception
    {
        public CheckFailed(string msg) : base(msg) { }
    }

    static void Note(string fmt, params object[] args)
    {
        string s = args.Length == 0 ? fmt : string.Format(CultureInfo.InvariantCulture, fmt, args);
        if (CurrentEvidence != null) CurrentEvidence.Add(s);
        if (HostLog != null) HostLog.WriteLine("    " + s);
    }

    static void Check(bool cond, string what)
    {
        if (!cond) throw new CheckFailed("assertion failed: " + what);
        Note("OK  " + what);
    }

    static string Scalar(JToken t)
    {
        if (t == null) return "<missing>";
        switch (t.Type)
        {
            case JTokenType.Null: return "<null>";
            case JTokenType.String: return (string)t;
            case JTokenType.Integer: return ((long)t).ToString(CultureInfo.InvariantCulture);
            case JTokenType.Boolean: return ((bool)t) ? "true" : "false";
            case JTokenType.Float: return ((double)t).ToString(CultureInfo.InvariantCulture);
            default: return t.ToString(Formatting.None);
        }
    }

    static void Eq(string expected, JToken actual, string what)
    {
        string got = Scalar(actual);
        if (got != expected)
            throw new CheckFailed("assertion failed: " + what + " (expected \"" + expected + "\", got \"" + got + "\")");
        Note("OK  " + what + " == " + expected);
    }

    static void EqLong(long expected, JToken actual, string what)
    {
        if (actual == null || actual.Type != JTokenType.Integer)
            throw new CheckFailed("assertion failed: " + what + " (expected integer " + expected + ", got "
                                  + (actual == null ? "<missing>" : actual.Type.ToString() + " " + actual.ToString(Formatting.None)) + ")");
        long got = (long)actual;
        if (got != expected)
            throw new CheckFailed("assertion failed: " + what + " (expected " + expected + ", got " + got + ")");
        Note("OK  " + what + " == " + expected + " (整型)");
    }

    static void CheckContains(string haystack, string needle, string what)
    {
        if (haystack == null || haystack.IndexOf(needle, StringComparison.Ordinal) < 0)
            throw new CheckFailed("assertion failed: " + what + " (substring \"" + needle + "\" not found)");
        Note("OK  " + what + " (找到 \"" + needle + "\")");
    }

    static void HasKey(JObject o, string key, string what)
    {
        if (o == null || o[key] == null)
            throw new CheckFailed("assertion failed: " + what + " (missing key \"" + key + "\")");
        Note("OK  " + what + " (存在 " + key + ")");
    }

    static void NoKey(JObject o, string key, string what)
    {
        if (o != null && o[key] != null)
            throw new CheckFailed("assertion failed: " + what + " (unexpected key \"" + key + "\" = "
                                  + o[key].ToString(Formatting.None) + ")");
        Note("OK  " + what + " (无 " + key + ")");
    }

    static JToken P(JToken root, params string[] names)
    {
        JToken cur = root;
        foreach (string n in names)
        {
            if (cur == null) return null;
            JObject o = cur as JObject;
            if (o == null) return null;
            cur = o[n];
        }
        return cur;
    }

    static JObject JO(params object[] kv)
    {
        JObject o = new JObject();
        for (int i = 0; i + 1 < kv.Length; i += 2)
        {
            string key = (string)kv[i];
            object v = kv[i + 1];
            JToken tok;
            if (v == null) tok = JValue.CreateNull();
            else if (v is JToken) tok = (JToken)v;
            else if (v is string) tok = new JValue((string)v);
            else if (v is int) tok = new JValue((int)v);
            else if (v is long) tok = new JValue((long)v);
            else if (v is bool) tok = new JValue((bool)v);
            else if (v is double) tok = new JValue((double)v);
            else if (v is JArray) tok = (JArray)v;
            else tok = JToken.FromObject(v);
            o[key] = tok;
        }
        return o;
    }

    // ------------------------------------------------------------------- http
    static string HttpRaw(string method, string path, string body, int timeoutMs)
    {
        HttpWebRequest req = (HttpWebRequest)WebRequest.Create(MockBase + path);
        req.Method = method;
        req.Timeout = timeoutMs;
        req.ReadWriteTimeout = timeoutMs;
        req.Proxy = null;
        req.KeepAlive = false;
        if (body != null)
        {
            byte[] bytes = Encoding.UTF8.GetBytes(body);
            req.ContentType = "application/json; charset=utf-8";
            req.ContentLength = bytes.Length;
            using (Stream s = req.GetRequestStream()) s.Write(bytes, 0, bytes.Length);
        }
        try
        {
            using (HttpWebResponse resp = (HttpWebResponse)req.GetResponse())
            using (Stream rs = resp.GetResponseStream())
            using (StreamReader reader = new StreamReader(rs, Encoding.UTF8))
                return reader.ReadToEnd();
        }
        catch (WebException ex)
        {
            HttpWebResponse hr = ex.Response as HttpWebResponse;
            if (hr == null) throw;
            string text = "";
            try
            {
                using (Stream rs = hr.GetResponseStream())
                using (StreamReader reader = new StreamReader(rs, Encoding.UTF8))
                    text = reader.ReadToEnd();
            }
            catch { }
            return "{\"_http_status\":" + (int)hr.StatusCode + ",\"_body\":" + JsonConvert.ToString(text) + "}";
        }
    }

    static JObject HttpJson(string method, string path, JToken body, int timeoutMs)
    {
        string raw = HttpRaw(method, path, body == null ? null : body.ToString(Formatting.None), timeoutMs);
        try
        {
            return JObject.Parse(raw);
        }
        catch (Exception ex)
        {
            throw new CheckFailed("bad JSON from mock " + path + ": " + raw + " (" + ex.Message + ")");
        }
    }

    static void WaitMockReady(int timeoutMs)
    {
        Stopwatch sw = Stopwatch.StartNew();
        Exception last = null;
        while (sw.ElapsedMilliseconds < timeoutMs)
        {
            try
            {
                JObject p = HttpJson("GET", "/ctl/ping", null, 2000);
                if (Scalar(p["ok"]) == "true")
                {
                    HostLog.WriteLine("mock ready: " + p.ToString(Formatting.None));
                    return;
                }
            }
            catch (Exception ex) { last = ex; }
            Thread.Sleep(250);
        }
        throw new CheckFailed("mock server not reachable on " + MockBase + " within " + timeoutMs + " ms"
                              + (last == null ? "" : ": " + last.Message));
    }

    // ------------------------------------------------------------ mock media
    static string MockMediaUrl(string name)
    {
        return MockBase + "/media/" + name;
    }

    static JObject MediaInfo(string name)
    {
        JObject r = HttpJson("GET", "/ctl/media_info?name=" + Uri.EscapeDataString(name), null, 8000);
        if (Scalar(r["ok"]) != "true")
            throw new CheckFailed("mock /ctl/media_info failed for " + name + ": " + r.ToString(Formatting.None));
        return r;
    }

    static int MediaHits(string name)
    {
        JArray arr = MockMeta()["media_hits"] as JArray;
        int n = 0;
        if (arr != null)
            foreach (JToken t in arr)
            {
                JObject o = t as JObject;
                if (o != null && Scalar(o["name"]) == name) n++;
            }
        return n;
    }

    static byte[] DecodeBase64Payload(string s, string label)
    {
        if (s == null) throw new CheckFailed(label + ": payload 为 null");
        const string prefix = "base64://";
        if (s.StartsWith(prefix, StringComparison.Ordinal)) s = s.Substring(prefix.Length);
        try { return Convert.FromBase64String(s); }
        catch (Exception ex) { throw new CheckFailed(label + ": base64 解码失败: " + ex.Message); }
    }

    static void CheckBytes(byte[] actual, byte[] expected, string label)
    {
        if (actual == null || expected == null) throw new CheckFailed(label + ": 字节为 null");
        Check(actual.Length == expected.Length,
              label + " 长度 (" + actual.Length + " == 期望 " + expected.Length + ")");
        for (int i = 0; i < expected.Length; i++)
            if (actual[i] != expected[i])
                throw new CheckFailed(label + ": 第 " + i + " 字节不一致 (" + actual[i] + " != " + expected[i] + ")");
        Note("OK  " + label + " 字节完全一致（" + actual.Length + " bytes）");
    }

    static byte[] MediaFixtureBytes(string name)
    {
        return DecodeBase64Payload(Scalar(MediaInfo(name)["base64"]), "media fixture " + name);
    }

    // 最小合成 MP4 头（ftyp 盒子），用于出站 video 段 → 11067 验扩展名 .mp4
    static byte[] SampleMp4()
    {
        byte[] b = new byte[]
        {
            0x00, 0x00, 0x00, 0x18, 0x66, 0x74, 0x79, 0x70,
            0x6D, 0x70, 0x34, 0x32, 0x0D, 0x0E, 0x0A, 0x0B
        };
        return b;
    }

    // GIF89a + 16 字节填充，用于出站动图段 → 11070 验扩展名 .gif
    static byte[] SampleGif()
    {
        byte[] b = new byte[22];
        byte[] head = Encoding.ASCII.GetBytes("GIF89a");
        Array.Copy(head, b, head.Length);
        return b;
    }

    static string WriteTempFile(string name, byte[] bytes)
    {
        string path = Path.Combine(Root, name);
        File.WriteAllBytes(path, bytes);
        Note("临时文件: " + path + " (" + bytes.Length + " bytes)");
        return path;
    }

    // ----------------------------------------------------------------- frames
    static void PollFrames()
    {
        JObject st = HttpJson("GET", "/ctl/state?since=" + FrameCursor, null, 8000);
        JArray arr = st["frames"] as JArray;
        if (arr == null) return;
        foreach (JToken t in arr)
        {
            JObject f = t as JObject;
            if (f == null) continue;
            int seq = f["seq"] == null ? 0 : f["seq"].Value<int>();
            if (seq > FrameCursor) FrameCursor = seq;
            Frames.Add((JObject)f.DeepClone());
        }
    }

    static int MaxSeq()
    {
        PollFrames();
        int m = 0;
        foreach (JObject f in Frames)
        {
            int s = f["seq"] == null ? 0 : f["seq"].Value<int>();
            if (s > m) m = s;
        }
        return m;
    }

    static JObject WaitInboundFrom(int startSeq, Func<JObject, bool> pred, int timeoutMs, string what)
    {
        Stopwatch sw = Stopwatch.StartNew();
        while (sw.ElapsedMilliseconds < timeoutMs)
        {
            PollFrames();
            foreach (JObject f in Frames)
            {
                int seq = f["seq"] == null ? 0 : f["seq"].Value<int>();
                if (seq <= startSeq) continue;
                if (Scalar(f["dir"]) != "in") continue;
                JObject data = f["data"] as JObject;
                if (data == null) continue;
                if (pred(data)) return (JObject)data.DeepClone();
            }
            Thread.Sleep(70);
        }
        throw new CheckFailed("timeout after " + timeoutMs + " ms waiting for inbound frame: " + what);
    }

    static void CheckNoInbound(int startSeq, Func<JObject, bool> pred, int durationMs, string what)
    {
        Stopwatch sw = Stopwatch.StartNew();
        while (sw.ElapsedMilliseconds < durationMs)
        {
            PollFrames();
            foreach (JObject f in Frames)
            {
                int seq = f["seq"] == null ? 0 : f["seq"].Value<int>();
                if (seq <= startSeq) continue;
                if (Scalar(f["dir"]) != "in") continue;
                JObject data = f["data"] as JObject;
                if (data == null) continue;
                if (pred(data))
                    throw new CheckFailed("unexpected inbound frame (" + what + "): " + data.ToString(Formatting.None));
            }
            Thread.Sleep(70);
        }
        Note("OK  " + durationMs + "ms 内未收到 " + what);
    }

    static void WaitFor(Func<bool> cond, int timeoutMs, string what)
    {
        Stopwatch sw = Stopwatch.StartNew();
        while (sw.ElapsedMilliseconds < timeoutMs)
        {
            if (cond()) return;
            Thread.Sleep(50);
        }
        throw new CheckFailed("timeout after " + timeoutMs + " ms waiting for: " + what);
    }

    static JObject MockMeta()
    {
        return HttpJson("GET", "/ctl/state?since=999999999", null, 8000);
    }

    static int MockOpenCount()
    {
        JArray a = MockMeta()["open_conns"] as JArray;
        return a == null ? 0 : a.Count;
    }

    static int MockConnCount()
    {
        JArray a = MockMeta()["connections"] as JArray;
        return a == null ? 0 : a.Count;
    }

    static int MockRejectCount()
    {
        JArray a = MockMeta()["rejections"] as JArray;
        return a == null ? 0 : a.Count;
    }

    // --------------------------------------------------------------- commands
    static Task<Robot.Common.CommandExecResult> OnCommand(JObject body)
    {
        JObject copy = (JObject)body.DeepClone();
        lock (CmdLock)
        {
            Commands.Add(copy);
            CmdLog.WriteLine(copy.ToString(Formatting.None));
            CmdLog.Flush();
        }
        int type = -1;
        if (body["type"] != null && body["type"].Type == JTokenType.Integer) type = body["type"].Value<int>();

        JToken data = null;
        switch (type)
        {
            case 11029:
            case 11069:
            case 11030:
            case 11031:
            case 11067:
            case 11070:
                _sendSeq++;
                data = JO("server_id", "srv-" + type + "-" + _sendSeq, "local_id", "loc-" + _sendSeq);
                break;
            case 11038:
                data = JO("room_list", new JArray(
                    JO("conversation_id", "R:12345", "room_name", "测试群A", "total", 3),
                    JO("conversation_id", "R:77777", "group_name", "测试群B", "total", 5)));
                break;
            case 11036:
                data = JO("user_list", new JArray(
                    JO("user_id", "67890", "username", "好友甲", "remark", "备注甲"),
                    JO("conversation_id", "S:12345_" + RobotId, "remark", "好友乙")));
                break;
            case 11040:
                data = JO("member_list", new JArray(
                    JO("user_id", "22222", "name", "成员甲", "is_admin", false),
                    JO("user_id", "33333", "nickname", "成员乙", "group_nickname", "群昵称乙")));
                break;
            case 11039:
                data = JO("user_id", "44444", "name", "陌生人甲", "username", "陌生人甲");
                break;
            case 11035:
                data = JO("user_id", RobotId);
                break;
        }
        Robot.Common.CommandExecResult result = new Robot.Common.CommandExecResult();
        result.Ok = true;
        if (data != null) result.Data = data;
        TaskCompletionSource<Robot.Common.CommandExecResult> tcs =
            new TaskCompletionSource<Robot.Common.CommandExecResult>();
        tcs.SetResult(result);
        return tcs.Task;
    }

    static int CommandCount()
    {
        lock (CmdLock) return Commands.Count;
    }

    static JObject WaitCommand(int startIdx, int type, int timeoutMs, string what)
    {
        Stopwatch sw = Stopwatch.StartNew();
        while (sw.ElapsedMilliseconds < timeoutMs)
        {
            lock (CmdLock)
            {
                for (int i = startIdx; i < Commands.Count; i++)
                {
                    JObject c = Commands[i];
                    if (c["type"] != null && c["type"].Type == JTokenType.Integer
                        && c["type"].Value<int>() == type)
                        return (JObject)c.DeepClone();
                }
            }
            Thread.Sleep(60);
        }
        throw new CheckFailed("timeout waiting for 微灵 command type " + type + " (" + what + ")");
    }

    static int FindCommandIndex(int startIdx, int type)
    {
        lock (CmdLock)
        {
            for (int i = startIdx; i < Commands.Count; i++)
            {
                JObject c = Commands[i];
                if (c["type"] != null && c["type"].Type == JTokenType.Integer
                    && c["type"].Value<int>() == type)
                    return i;
            }
        }
        return -1;
    }

    // ------------------------------------------------------------------ shims
    static WxSocketClient MakeShim(string channel, Func<string> getUrl,
                                   Func<string> getToken, List<string> statusLog)
    {
        WxSocketClient shim = new WxSocketClient(
            channel, "",
            getUrl, getToken,
            delegate { return "test-client"; },
            new WeComConsoleServer.Core.WxWebSocketClient.CommandHandler(OnCommand));
        shim.StatusChanged += delegate
        {
            string t;
            try { t = shim.StatusText + "|" + shim.StatusBackground + "|" + (shim.IsConnected ? "True" : "False"); }
            catch { t = "?"; }
            lock (statusLog) statusLog.Add(t);
            HostLog.WriteLine("    [" + channel + "] status changed: " + t);
        };
        return shim;
    }

    static int InjectTo(WxSocketClient shim, int type, JObject data)
    {
        return InjectTo(shim, type, data, RobotId, 1);
    }

    static int InjectTo(WxSocketClient shim, int type, JObject data, string robotId, int client)
    {
        int before = MaxSeq();
        JObject evt = JO("type", type, "client", client, "data", data);
        if (robotId != null) evt["robot_id"] = robotId;
        bool sent = shim.TrySendEvent(evt.ToString(Formatting.None));
        Note("注入微灵事件 type={0} TrySendEvent={1}", type, sent ? "true" : "false");
        return before;
    }

    static int Inject(int type, JObject data)
    {
        return InjectTo(Shim1, type, data);
    }

    // ---------------------------------------------------------------- actions
    static JObject DoAction(string action, JObject p, string tag, int timeoutMs)
    {
        _echoSeq++;
        string echo = "echo-" + tag + "-" + _echoSeq;
        int before = MaxSeq();
        JObject req = JO("action", action, "params", p, "echo", echo);
        JObject r = HttpJson("POST", "/ctl/action", req, 8000);
        if (Scalar(r["ok"]) != "true")
            throw new CheckFailed("mock /ctl/action failed: " + r.ToString(Formatting.None));
        JObject resp = WaitInboundFrom(before,
            delegate (JObject x) { return x["echo"] != null && Scalar(x["echo"]) == echo; },
            timeoutMs, action + " 的响应 (echo=" + echo + ")");
        _lastEcho = echo;
        Note("ACTION {0} params={1} -> {2}", action,
             p == null ? "{}" : p.ToString(Formatting.None), resp.ToString(Formatting.None));
        return resp;
    }

    static JObject DoAction(string action, JObject p, string tag)
    {
        return DoAction(action, p, tag, 15000);
    }

    // ------------------------------------------------------------ registration
    static void Run(string id, string name, string group, Action body)
    {
        Current = new CaseResult();
        Current.Id = id;
        Current.Name = name;
        Current.Group = group;
        Current.Status = "PASS";
        CurrentEvidence = Current.Evidence;
        HostLog.WriteLine("==== " + id + " [" + group + "] " + name + " ====");
        Console.WriteLine("-- " + id + " " + name);
        Stopwatch sw = Stopwatch.StartNew();
        try
        {
            body();
        }
        catch (CheckFailed ex)
        {
            Current.Status = "FAIL";
            Current.Error = ex.Message;
            HostLog.WriteLine("    FAIL: " + ex.Message);
            Console.WriteLine("   FAIL " + id + ": " + ex.Message);
        }
        catch (Exception ex)
        {
            Current.Status = "FAIL";
            Current.Error = ex.GetType().Name + ": " + ex.Message;
            HostLog.WriteLine("    FAIL(exception): " + ex.ToString());
            Console.WriteLine("   FAIL " + id + ": " + ex.Message);
        }
        sw.Stop();
        Current.DurationMs = sw.ElapsedMilliseconds;
        HostLog.WriteLine("---- " + id + " => " + Current.Status + " (" + Current.DurationMs + " ms)");
        lock (Results) Results.Add(Current);
    }

    // =================================================================== tests
    static void RunAll()
    {
        // ---------------- handshake / bootstrap
        Run("T01", "启动前无 self_id 不连接；注入 11026 登录后建立连接并带上 self_id", "handshake", delegate
        {
            // (a) 无 self_id 时不连接。使用 personal 通道构造临时 shim：
            //     shim 的 ResolveSelfId/QuerySelfIdViaHttp 对 personal 通道直接返回 null，
            //     不会去探测 127.0.0.1:8060（本机可能正运行真实微灵实例），保证测试自包含且确定。
            List<string> waitStatus = new List<string>();
            WxSocketClient waitShim = MakeShim("personal-wait",
                delegate { return "ws://127.0.0.1:6399/ws"; },
                delegate { return Token; }, waitStatus);
            waitShim.Start();
            Eq("连接中", waitShim.StatusText, "无 self_id 时 StatusText=连接中");
            Eq("#E6A23C", waitShim.StatusBackground, "无 self_id 时 StatusBackground=#E6A23C");
            Thread.Sleep(1200);
            Check(MockOpenCount() == 0, "self_id 为空时 1.2s 内不建立 WS 连接（NetLoop 等待登录）");
            Check(!waitShim.IsConnected, "无 self_id 时 IsConnected 仍为 false");
            waitShim.Stop();
            waitShim.Dispose();

            // (b) 主 shim：先注入 11026 写入 self_id，再 Start()。此时 ResolveSelfId 直接返回该值，
            //     不会触发 8060 HTTP 自探测，连接的 X-Self-ID 可确定地断言为 RobotId。
            ShimToken = delegate { return Token; };
            Shim1 = MakeShim("wecom-test",
                delegate { return "ws://127.0.0.1:6399/ws"; }, ShimToken, Shim1Status);
            Eq("未连接", Shim1.StatusText, "构造后 StatusText");
            Eq("#909399", Shim1.StatusBackground, "构造后 StatusBackground");
            Check(!Shim1.IsConnected, "构造后 IsConnected==false");

            int before = Inject(11026, JO("user_id", RobotId, "username", "机器人昵称",
                                          "account", RobotId, "acctid", RobotId));
            Note("启动前注入 11026 设置 self_id（未连接时 TrySendEvent 返回 false，但 CaptureState 已生效）seq={0}", before);

            Shim1.Start();
            WaitFor(delegate { return Shim1.IsConnected; }, 12000, "注入 11026 并 Start 后 IsConnected==true");
            Eq("已连接", Shim1.StatusText, "连接后 StatusText=已连接");
            Eq("#22AA22", Shim1.StatusBackground, "连接后 StatusBackground=#22AA22");

            JObject meta = MockMeta();
            JArray conns = meta["connections"] as JArray;
            Check(conns != null && conns.Count >= 1, "Mock 记录到连接");
            JObject h = ((JObject)conns[0])["headers"] as JObject;
            Eq(RobotId, h["x-self-id"], "11026 后连接的 X-Self-ID 使用 robot_id/data.user_id");
        });

        Run("T02", "握手头：X-Client-Role / Authorization / X-Self-ID / 路径", "handshake", delegate
        {
            JObject meta = MockMeta();
            JArray conns = meta["connections"] as JArray;
            Check(conns != null && conns.Count >= 1, "存在连接记录");
            JObject conn = (JObject)conns[0];
            JObject h = conn["headers"] as JObject;
            Eq("/ws", conn["path"], "WS 路径");
            Eq("Universal", h["x-client-role"], "X-Client-Role 头");
            Eq("Bearer testtoken", h["authorization"], "Authorization 头");
            Eq(RobotId, h["x-self-id"], "X-Self-ID 头");
        });

        Run("T03", "连接建立后发送 lifecycle.connect 元事件", "handshake", delegate
        {
            JObject life = WaitInboundFrom(0, delegate (JObject p)
            {
                return Scalar(p["post_type"]) == "meta_event"
                    && Scalar(p["meta_event_type"]) == "lifecycle";
            }, 8000, "lifecycle.connect");
            Eq("connect", life["sub_type"], "lifecycle.sub_type");
            EqLong(RobotIdLong, life["self_id"], "lifecycle.self_id");
            HasKey(life, "time", "lifecycle 含 time");
        });

        // ---------------- event translation
        Run("T05", "11041 群文本 → group 消息（group_id/text 段/raw_message/ling）", "events", delegate
        {
            JObject data = JO("conversation_id", "R:12345", "sender", "11111", "sender_name", "张三",
                              "receiver", "12345", "content", "你好，世界", "send_time", 1710000000,
                              "server_id", "55550001", "local_id", "l1", "is_self", false,
                              "payload_base64", "QUJD");
            int before = Inject(11041, data);
            JObject m = WaitInboundFrom(before, delegate (JObject p)
            {
                return Scalar(p["post_type"]) == "message" && Scalar(p["message_type"]) == "group";
            }, 8000, "group 消息事件");
            Eq("message", m["post_type"], "post_type");
            Eq("group", m["message_type"], "message_type");
            Eq("normal", m["sub_type"], "sub_type");
            EqLong(12345, m["group_id"], "group_id");
            NoKey(m, "user_id", "群消息无顶层 user_id");
            EqLong(55550001, m["message_id"], "message_id（server_id 转数字）");
            EqLong(RobotIdLong, m["self_id"], "self_id");
            EqLong(1710000000, m["time"], "time（秒值透传）");
            Eq("张三", P(m, "sender", "nickname"), "sender.nickname");
            EqLong(11111, P(m, "sender", "user_id"), "sender.user_id");
            Eq("member", P(m, "sender", "role"), "sender.role");
            EqLong(0, m["font"], "font");
            JArray segs = m["message"] as JArray;
            Check(segs != null && segs.Count == 1, "message 仅 1 个段");
            Eq("text", segs[0]["type"], "段类型 text");
            Eq("你好，世界", P(segs[0], "data", "text"), "段文本");
            Eq("你好，世界", m["raw_message"], "raw_message");
            JObject ling = m["ling"] as JObject;
            Check(ling != null, "顶层 ling 存在");
            EqLong(11041, ling["type"], "ling.type 保留原始事件类型");
            Eq("R:12345", P(ling, "data", "conversation_id"), "ling.data.conversation_id");
            NoKey(ling["data"] as JObject, "payload_base64", "ling 中剥离 data.payload_base64");
        });

        Run("T06", "11041 私聊文本 S:67890_<self> → private 消息 user_id=67890", "events", delegate
        {
            JObject data = JO("conversation_id", "S:67890_" + RobotId, "sender", "67890", "sender_name", "李四",
                              "content", "私聊内容", "send_time", 1710000123000, "server_id", "55550002");
            int before = Inject(11041, data);
            JObject m = WaitInboundFrom(before, delegate (JObject p)
            {
                return Scalar(p["post_type"]) == "message" && Scalar(p["message_type"]) == "private";
            }, 8000, "private 消息事件");
            Eq("private", m["message_type"], "message_type");
            Eq("friend", m["sub_type"], "sub_type");
            EqLong(67890, m["user_id"], "user_id（取最后一个 _ 之前部分）");
            NoKey(m, "group_id", "私聊消息无 group_id");
            EqLong(1710000123, m["time"], "毫秒 send_time 被 /1000");
            JArray segs = m["message"] as JArray;
            Check(segs != null && segs.Count == 1, "message 仅 1 个段");
            Eq("私聊内容", P(segs[0], "data", "text"), "段文本");
            Eq("李四", P(m, "sender", "nickname"), "sender.nickname");
        });

        Run("T07", "is_self 消息不推送（is_self=true / sender==self_id）", "events", delegate
        {
            int before = Inject(11041, JO("conversation_id", "R:12345", "sender", "11111",
                                          "content", "自己的群消息", "is_self", true,
                                          "send_time", 1710000200, "server_id", "55550011"));
            CheckNoInbound(before, delegate (JObject p) { return Scalar(p["post_type"]) == "message"; },
                           1500, "is_self=true 的消息事件");

            before = Inject(11041, JO("conversation_id", "S:67890_" + RobotId, "sender", RobotId,
                                      "content", "自己的私聊消息", "is_self", false,
                                      "send_time", 1710000201, "server_id", "55550012"));
            CheckNoInbound(before, delegate (JObject p) { return Scalar(p["post_type"]) == "message"; },
                           1500, "sender==self_id 的消息事件");
        });

        Run("T08", "11041 at_list → at 段（mixed / 纯 at / 保留 self at）", "events", delegate
        {
            JObject data = JO("conversation_id", "R:12345", "sender", "11111", "sender_name", "张三",
                              "content", "大家好", "send_time", 1710000300, "server_id", "55550021",
                              "at_list", new JArray("22222", JO("user_id", "33333"),
                                                    JO("user_id", RobotId)));
            int before = Inject(11041, data);
            JObject m = WaitInboundFrom(before, delegate (JObject p)
            {
                return Scalar(p["post_type"]) == "message" && Scalar(p["message_type"]) == "group";
            }, 8000, "group 消息（含 at）");
            JArray segs = m["message"] as JArray;
            // 2026-11-12 修正：@机器人（at self）必须保留 at 段，否则 AstrBot 唤醒判定不触发；
            // 故 mixed 含 3 个 at 段（22222 / 33333 / RobotId）+ 1 个 text 段，共 4 段。
            Check(segs != null && segs.Count == 4, "mixed: 3 个 at + 1 个 text 段，共 4 段");
            Eq("at", segs[0]["type"], "段0 类型 at");
            // 源码 AddAtSegment: data["qq"] = new JValue(userId)（userId 为 string）→ qq 是字符串；
            // OneBot v11 规范的 at 段 qq 亦为字符串，故断言字符串形式。
            Eq("22222", P(segs[0], "data", "qq"), "段0 qq (字符串条目)");
            Eq("String", P(segs[0], "data", "qq").Type.ToString(), "段0 qq 的 JSON 类型为 string");
            Eq("at", segs[1]["type"], "段1 类型 at");
            Eq("33333", P(segs[1], "data", "qq"), "段1 qq (JObject user_id)");
            Eq("at", segs[2]["type"], "段2 类型 at (self)");
            Eq(RobotId, P(segs[2], "data", "qq"), "段2 qq (self 也保留)");
            Eq("text", segs[3]["type"], "段3 类型 text");
            Eq("大家好", P(segs[3], "data", "text"), "段3 文本");
            // SegmentsToPlainText 逐段拼接：at → "@"+qq、text → 原文，无分隔符
            Eq("@22222@33333@" + RobotId + "大家好", m["raw_message"], "raw_message 拼接 @ 与文本");

            JObject pure = JO("conversation_id", "R:12345", "sender", "11111", "sender_name", "张三",
                              "content", "", "send_time", 1710000301, "server_id", "55550022",
                              "at_list", new JArray("44444"));
            before = Inject(11041, pure);
            JObject m2 = WaitInboundFrom(before, delegate (JObject p)
            {
                return Scalar(p["post_type"]) == "message" && Scalar(p["message_id"]) == "55550022";
            }, 8000, "纯 at 消息");
            JArray segs2 = m2["message"] as JArray;
            Check(segs2 != null && segs2.Count == 1, "纯 at: 仅 1 个 at 段（空 content 不生成 text 段）");
            Eq("at", segs2[0]["type"], "纯 at 段类型");
            Eq("44444", P(segs2[0], "data", "qq"), "纯 at qq");
            Eq("@44444", m2["raw_message"], "纯 at raw_message");
        });

        Run("T09", "11042 图片 → image 段（cdn.url / md5 作为 file）", "events", delegate
        {
            JObject data = JO("conversation_id", "R:12345", "sender", "11111", "sender_name", "张三",
                              "send_time", 1710000400, "server_id", "55550031",
                              "cdn", JO("url", "https://cdn.example.com/img/a.png", "md5", "abc123",
                                        "file_name", "a.png"));
            int before = Inject(11042, data);
            JObject m = WaitInboundFrom(before, delegate (JObject p)
            {
                return Scalar(p["post_type"]) == "message" && Scalar(p["message_id"]) == "55550031";
            }, 8000, "图片消息");
            JArray segs = m["message"] as JArray;
            Check(segs != null && segs.Count == 1, "图片消息 1 个段");
            Eq("image", segs[0]["type"], "段类型 image");
            Eq("abc123", P(segs[0], "data", "file"), "image.data.file 取 cdn.md5");
            Eq("https://cdn.example.com/img/a.png", P(segs[0], "data", "url"), "image.data.url 取 cdn.url");
        });

        Run("T10", "11045 文件 → file 段", "events", delegate
        {
            JObject data = JO("conversation_id", "R:12345", "sender", "11111", "sender_name", "张三",
                              "send_time", 1710000500, "server_id", "55550041", "local_id", "l41",
                              "cdn", JO("url", "https://cdn.example.com/f/报告.pdf", "md5", "ff00",
                                        "file_name", "报告.pdf"));
            int before = Inject(11045, data);
            JObject m = WaitInboundFrom(before, delegate (JObject p)
            {
                return Scalar(p["post_type"]) == "message" && Scalar(p["message_id"]) == "55550041";
            }, 8000, "文件消息");
            JArray segs = m["message"] as JArray;
            Check(segs != null && segs.Count == 1, "文件消息 1 个段");
            Eq("file", segs[0]["type"], "段类型 file");
            Eq("报告.pdf", P(segs[0], "data", "name"), "file.data.name 取 cdn.file_name（AstrBot File 需要）");
            Eq("https://cdn.example.com/f/报告.pdf", P(segs[0], "data", "file"), "file.data.file 取 cdn.url");
            Eq("https://cdn.example.com/f/报告.pdf", P(segs[0], "data", "url"), "file.data.url");
            NoKey((JObject)segs[0]["data"], "file_id", "file 段不再输出 file_id（新结构 {name,file,url}）");
        });

        Run("T11", "11123 撤回 → notice（group_recall / friend_recall）", "events", delegate
        {
            int before = Inject(11123, JO("room_id", "12345", "message_server_id", "55550001",
                                          "op_user_id", "11111"));
            JObject g = WaitInboundFrom(before, delegate (JObject p)
            {
                return Scalar(p["post_type"]) == "notice" && Scalar(p["notice_type"]) == "group_recall";
            }, 8000, "群撤回 notice");
            EqLong(12345, g["group_id"], "群撤回 group_id");
            EqLong(55550001, g["message_id"], "群撤回 message_id");
            EqLong(11111, g["operator_id"], "群撤回 operator_id");
            Check(g["ling"] != null, "群撤回含 ling");

            before = Inject(11123, JO("room_id", "0", "message_server_id", "55550002",
                                      "op_user_id", "67890"));
            JObject f = WaitInboundFrom(before, delegate (JObject p)
            {
                return Scalar(p["post_type"]) == "notice" && Scalar(p["notice_type"]) == "friend_recall";
            }, 8000, "私聊撤回 notice");
            EqLong(67890, f["user_id"], "私聊撤回 user_id");
            EqLong(55550002, f["message_id"], "私聊撤回 message_id");
            Check(f["ling"] != null, "私聊撤回含 ling");
        });

        Run("T12", "11072 群成员增加 → notice group_increase", "events", delegate
        {
            JObject data = JO("room_conversation_id", "R:12345", "op_user_id", "11111",
                              "member_list", new JArray(JO("user_id", "99999", "name", "新人")));
            int before = Inject(11072, data);
            JObject n = WaitInboundFrom(before, delegate (JObject p)
            {
                return Scalar(p["post_type"]) == "notice" && Scalar(p["notice_type"]) == "group_increase";
            }, 8000, "group_increase notice");
            Eq("invite", n["sub_type"], "sub_type invite");
            EqLong(12345, n["group_id"], "group_id");
            EqLong(99999, n["user_id"], "user_id=member_list[0].user_id");
            EqLong(11111, n["operator_id"], "operator_id");
            Check(n["ling"] != null, "notice 含 ling");
        });

        Run("T45", "11173 群成员邀请 → notice group_increase/invite（新分派）", "events", delegate
        {
            JObject data = JO("room_conversation_id", "R:12345", "op_user_id", "11111",
                              "member_list", new JArray(JO("user_id", "55555", "name", "被邀请人")));
            int before = Inject(11173, data);
            JObject n = WaitInboundFrom(before, delegate (JObject p)
            {
                return Scalar(p["post_type"]) == "notice" && Scalar(p["notice_type"]) == "group_increase";
            }, 8000, "11173 group_increase notice");
            Eq("invite", n["sub_type"], "sub_type invite");
            EqLong(12345, n["group_id"], "group_id");
            EqLong(55555, n["user_id"], "user_id");
            EqLong(11111, n["operator_id"], "operator_id");
            Check(n["ling"] != null, "11173 notice 含 ling");
            Check(Scalar(P(n, "ling", "type")) == "11173", "ling.type == 11173");
        });

        Run("T13", "未知事件类型 99999 被忽略（不推送 ws 帧）", "events", delegate
        {
            int before = Inject(99999, JO("foo", "bar"));
            CheckNoInbound(before, delegate (JObject p)
            {
                return p["post_type"] == null && p["status"] == null
                    && p["meta_event_type"] == null && p["action"] == null;
            }, 1500, "未知事件产生的任何非心跳帧");
        });

        // ---------------- actions
        Run("T14", "send_private_msg 文本 → 微灵命令 {client:1,type:11029,conversation_id:'S:67890_<self>'} + ok/echo/message_id", "actions", delegate
        {
            int cmdStart = CommandCount();
            JObject resp = DoAction("send_private_msg",
                JO("user_id", "67890", "message", "私聊回复"), "T14");
            Eq("ok", resp["status"], "status");
            EqLong(0, resp["retcode"], "retcode");
            Check(resp["echo"] != null && Scalar(resp["echo"]) == _lastEcho, "echo 原样返回");
            HasKey(resp, "data", "含 data");
            string mid = Scalar(P(resp, "data", "message_id"));
            Check(mid != null && mid.StartsWith("srv-11029-", StringComparison.Ordinal),
                  "message_id 来自 handler 的 server_id: " + mid);

            JObject cmd = WaitCommand(cmdStart, 11029, 8000, "send_private_msg 命令");
            EqLong(1, cmd["client"], "命令 client=1（数字）");
            EqLong(11029, cmd["type"], "命令 type");
            Eq("S:67890_" + RobotId, P(cmd, "data", "conversation_id"), "命令 conversation_id");
            Eq("私聊回复", P(cmd, "data", "content"), "命令 content");
        });

        Run("T15", "send_group_msg 无 at → 11029 R:12345", "actions", delegate
        {
            int cmdStart = CommandCount();
            JObject resp = DoAction("send_group_msg",
                JO("group_id", "12345", "message", "群回复"), "T15");
            Eq("ok", resp["status"], "status");
            JObject cmd = WaitCommand(cmdStart, 11029, 8000, "send_group_msg 命令");
            Eq("R:12345", P(cmd, "data", "conversation_id"), "命令 conversation_id");
            Eq("群回复", P(cmd, "data", "content"), "命令 content");
            NoKey(cmd["data"] as JObject, "at_list", "无 at 时不带 at_list");
        });

        Run("T16", "send_group_msg 含 at → 11069 + data.at_list", "actions", delegate
        {
            int cmdStart = CommandCount();
            JArray msg = new JArray(
                JO("type", "text", "data", JO("text", "Hi ")),
                JO("type", "at", "data", JO("qq", "22222")),
                JO("type", "at", "data", JO("qq", "33333")));
            JObject resp = DoAction("send_group_msg",
                JO("group_id", "12345", "message", msg), "T16");
            Eq("ok", resp["status"], "status");
            JObject cmd = WaitCommand(cmdStart, 11069, 8000, "send_group_msg@ 命令");
            Eq("R:12345", P(cmd, "data", "conversation_id"), "命令 conversation_id");
            Eq("Hi ", P(cmd, "data", "content"), "命令 content（at 段不产生文本）");
            JArray atList = P(cmd, "data", "at_list") as JArray;
            Check(atList != null && atList.Count == 2, "at_list 含 2 个用户");
            EqLong(22222, atList[0], "at_list[0]");
            EqLong(33333, atList[1], "at_list[1]");
        });

        Run("T17", "send_msg message_type=private → 11029", "actions", delegate
        {
            int cmdStart = CommandCount();
            JObject resp = DoAction("send_msg",
                JO("message_type", "private", "user_id", "67890", "message", "send_msg私聊"), "T17");
            Eq("ok", resp["status"], "status");
            JObject cmd = WaitCommand(cmdStart, 11029, 8000, "send_msg private 命令");
            Eq("S:67890_" + RobotId, P(cmd, "data", "conversation_id"), "命令 conversation_id");
            Eq("send_msg私聊", P(cmd, "data", "content"), "命令 content");
        });

        Run("T18", "send_msg message_type=group → 11029", "actions", delegate
        {
            int cmdStart = CommandCount();
            JObject resp = DoAction("send_msg",
                JO("message_type", "group", "group_id", "12345", "message", "send_msg群"), "T18");
            Eq("ok", resp["status"], "status");
            JObject cmd = WaitCommand(cmdStart, 11029, 8000, "send_msg group 命令");
            Eq("R:12345", P(cmd, "data", "conversation_id"), "命令 conversation_id");
        });

        Run("T19", "get_login_info → user_id/nickname", "actions", delegate
        {
            JObject resp = DoAction("get_login_info", JO(), "T19");
            Eq("ok", resp["status"], "status");
            EqLong(RobotIdLong, P(resp, "data", "user_id"), "user_id");
            Eq("机器人昵称", P(resp, "data", "nickname"), "nickname 来自 11026 data.username");
        });

        Run("T20", "get_status → online/good", "actions", delegate
        {
            JObject resp = DoAction("get_status", JO(), "T20");
            Eq("ok", resp["status"], "status");
            Eq("true", P(resp, "data", "online"), "online");
            Eq("true", P(resp, "data", "good"), "good");
        });

        Run("T21", "get_version_info → 实现信息", "actions", delegate
        {
            JObject resp = DoAction("get_version_info", JO(), "T21");
            Eq("ok", resp["status"], "status");
            Eq("wecom-ling-onebot", P(resp, "data", "app_name"), "app_name");
            Eq("1.11.0", P(resp, "data", "app_version"), "app_version");
            Eq("v11", P(resp, "data", "protocol_version"), "protocol_version");
            Eq("微灵 OneBot v11 改造版", P(resp, "data", "app_full_name"), "app_full_name");
        });

        Run("T22", "get_group_list → 11038 数据转 [{group_id,group_name}]", "actions", delegate
        {
            int cmdStart = CommandCount();
            JObject resp = DoAction("get_group_list", JO(), "T22");
            Eq("ok", resp["status"], "status");
            WaitCommand(cmdStart, 11038, 8000, "get_group_list 的 11038 命令");
            JArray groups = resp["data"] as JArray;
            Check(groups != null && groups.Count == 2, "群列表 2 项");
            EqLong(12345, groups[0]["group_id"], "群1 group_id（R: 前缀已剥离）");
            Eq("测试群A", groups[0]["group_name"], "群1 group_name");
            EqLong(77777, groups[1]["group_id"], "群2 group_id");
            Eq("测试群B", groups[1]["group_name"], "群2 group_name");
        });

        Run("T23", "get_friend_list → 11036 数据转 [{user_id,nickname}]", "actions", delegate
        {
            int cmdStart = CommandCount();
            JObject resp = DoAction("get_friend_list", JO(), "T23");
            Eq("ok", resp["status"], "status");
            WaitCommand(cmdStart, 11036, 8000, "get_friend_list 的 11036 命令");
            JArray users = resp["data"] as JArray;
            Check(users != null && users.Count == 2, "好友列表 2 项");
            EqLong(67890, users[0]["user_id"], "好友1 user_id");
            Eq("好友甲", users[0]["nickname"], "好友1 nickname");
            EqLong(12345, users[1]["user_id"], "好友2 user_id 由 conversation_id 解析");
            Eq("好友乙", users[1]["nickname"], "好友2 nickname（remark 回退）");
        });

        Run("T24", "get_group_info → 复用群列表", "actions", delegate
        {
            JObject resp = DoAction("get_group_info", JO("group_id", "12345"), "T24");
            Eq("ok", resp["status"], "status");
            EqLong(12345, P(resp, "data", "group_id"), "group_id");
            Eq("测试群A", P(resp, "data", "group_name"), "group_name");
        });

        Run("T25", "get_group_member_info → 11040 数据", "actions", delegate
        {
            int cmdStart = CommandCount();
            JObject resp = DoAction("get_group_member_info",
                JO("group_id", "12345", "user_id", "22222"), "T25");
            Eq("ok", resp["status"], "status");
            JObject cmd = WaitCommand(cmdStart, 11040, 8000, "get_group_member_info 的 11040 命令");
            Eq("R:12345", P(cmd, "data", "conversation_id"), "11040 查询 conversation_id");
            EqLong(22222, P(resp, "data", "user_id"), "成员 user_id");
            Eq("成员甲", P(resp, "data", "nickname"), "成员 nickname");
        });

        Run("T26", "get_stranger_info → 11039 数据", "actions", delegate
        {
            int cmdStart = CommandCount();
            JObject resp = DoAction("get_stranger_info", JO("user_id", "44444"), "T26");
            Eq("ok", resp["status"], "status");
            WaitCommand(cmdStart, 11039, 8000, "get_stranger_info 的 11039 命令");
            EqLong(44444, P(resp, "data", "user_id"), "user_id");
            Eq("陌生人甲", P(resp, "data", "nickname"), "nickname");
        });

        Run("T27", "get_msg → 命中消息缓存（由 T05 注入的消息）", "actions", delegate
        {
            JObject resp = DoAction("get_msg", JO("message_id", "55550001"), "T27");
            Eq("ok", resp["status"], "status");
            EqLong(55550001, P(resp, "data", "message_id"), "缓存消息 message_id");
            Eq("message", P(resp, "data", "post_type"), "缓存消息 post_type");
            Check(P(resp, "data", "ling") != null, "缓存消息含 ling");
        });

        Run("T28", "get_msg 未知 message_id → failed/500 + message not found", "actions", delegate
        {
            JObject resp = DoAction("get_msg", JO("message_id", "999999999"), "T28");
            Eq("failed", resp["status"], "status");
            EqLong(500, resp["retcode"], "retcode");
            CheckContains(Scalar(resp["msg"]), "message not found: 999999999", "错误信息");
        });

        Run("T29", "can_send_image → yes:true", "actions", delegate
        {
            JObject resp = DoAction("can_send_image", JO(), "T29");
            Eq("ok", resp["status"], "status");
            Eq("true", P(resp, "data", "yes"), "yes");
        });

        Run("T30", "已知但不支持的动作 → failed/500；未知动作 → failed/1404", "actions", delegate
        {
            JObject resp = DoAction("set_group_name",
                JO("group_id", "12345", "group_name", "x"), "T30a");
            Eq("failed", resp["status"], "set_group_name status");
            EqLong(500, resp["retcode"], "set_group_name retcode");
            CheckContains(Scalar(resp["msg"]), "action not supported by 微灵 OneBot shim: set_group_name",
                          "set_group_name 错误信息");

            JObject resp2 = DoAction("nonexistent_action_xyz", JO(), "T30b");
            Eq("failed", resp2["status"], "未知动作 status");
            EqLong(1404, resp2["retcode"], "未知动作 retcode");
            Eq("unsupported action: nonexistent_action_xyz", Scalar(resp2["msg"]), "未知动作错误信息");
        });

        Run("T31", "出站 image 段（base64 PNG）→ 先 11030 落盘（绝对路径/.png）再 11029 文本，返回 ok", "actions", delegate
        {
            int cmdStart = CommandCount();
            byte[] png = MediaFixtureBytes("pic.png");
            JArray msg = new JArray(
                JO("type", "image", "data", JO("file", "base64://" + Convert.ToBase64String(png))),
                JO("type", "text", "data", JO("text", "图片说明")));
            JObject resp = DoAction("send_private_msg", JO("user_id", "67890", "message", msg), "T31");
            Eq("ok", resp["status"], "status");
            EqLong(0, resp["retcode"], "retcode");
            Eq(_lastEcho, resp["echo"], "echo 原样回传");

            JObject mediaCmd = WaitCommand(cmdStart, 11030, 8000, "出站图片的 11030 媒体命令");
            Eq("S:67890_" + RobotId, P(mediaCmd, "data", "conversation_id"), "11030 conversation_id");
            string path = Scalar(P(mediaCmd, "data", "file"));
            Check(!string.IsNullOrEmpty(path) && Path.IsPathRooted(path), "11030 data.file 为绝对路径");
            Check(File.Exists(path), "11030 data.file 指向已存在文件");
            Check(path.EndsWith(".png", StringComparison.OrdinalIgnoreCase), "11030 data.file 扩展名 .png");
            CheckBytes(File.ReadAllBytes(path), png, "11030 落盘内容与 base64 一致");

            JObject textCmd = WaitCommand(cmdStart, 11029, 8000, "图片后的 11029 文本命令");
            Eq("S:67890_" + RobotId, P(textCmd, "data", "conversation_id"), "11029 conversation_id");
            Eq("图片说明", P(textCmd, "data", "content"), "11029 content");

            int mediaIdx = FindCommandIndex(cmdStart, 11030);
            int textIdx = FindCommandIndex(cmdStart, 11029);
            Check(mediaIdx >= 0 && textIdx >= 0 && mediaIdx < textIdx, "命令顺序：先 11030 媒体后 11029 文本");
            Check(CommandCount() == cmdStart + 2, "共 2 条命令（图片 + 文本）");
            Check(Scalar(P(resp, "data", "message_id")).StartsWith("srv-11029-", StringComparison.Ordinal),
                  "message_id 来自最后一条文本命令结果");
        });

        Run("T32", "空消息 → failed/500 empty message", "actions", delegate
        {
            JObject resp = DoAction("send_private_msg", JO("user_id", "67890", "message", ""), "T32");
            Eq("failed", resp["status"], "status");
            EqLong(500, resp["retcode"], "retcode");
            CheckContains(Scalar(resp["msg"]), "empty message", "错误信息");
        });

        Run("T33", "get_image 命中已登记媒体 → 按需 HTTP 下载并返回 base64://", "actions", delegate
        {
            string url = MockMediaUrl("pic.png");
            JObject data = JO("conversation_id", "R:12345", "sender", "11111", "sender_name", "张三",
                              "send_time", 1710000600, "server_id", "55550101",
                              "cdn", JO("url", url, "file_name", "pic.png"));
            int before = Inject(11042, data);
            JObject m = WaitInboundFrom(before, delegate (JObject p)
            {
                return Scalar(p["post_type"]) == "message" && Scalar(p["message_id"]) == "55550101";
            }, 8000, "图片消息（媒体登记用）");
            JArray segs = m["message"] as JArray;
            Check(segs != null && segs.Count == 1, "图片消息 1 个 image 段");
            Eq("pic.png", P(segs[0], "data", "file"), "image 段 file 取 cdn.file_name");
            Eq(url, P(segs[0], "data", "url"), "image 段 url");

            int hitsBefore = MediaHits("pic.png");
            JObject resp = DoAction("get_image", JO("file", "pic.png"), "T33");
            Eq("ok", resp["status"], "get_image status");
            EqLong(0, resp["retcode"], "retcode");
            string payload = Scalar(P(resp, "data", "file"));
            Check(payload != null && payload.StartsWith("base64://", StringComparison.Ordinal),
                  "data.file 形如 base64://...");
            CheckBytes(DecodeBase64Payload(payload, "get_image"), MediaFixtureBytes("pic.png"), "get_image 下载内容");
            Eq("pic.png", P(resp, "data", "file_name"), "data.file_name 保留 cdn.file_name");
            Check(MediaHits("pic.png") > hitsBefore, "Mock 记录到 shim 的按需 HTTP 下载请求");
        });

        Run("T42", "get_file → base64；get_group_file_url/get_private_file_url → 原始 CDN URL", "actions", delegate
        {
            string url = MockMediaUrl("doc.txt");
            JObject data = JO("conversation_id", "R:12345", "sender", "11111", "sender_name", "张三",
                              "send_time", 1710000700, "server_id", "55550201", "local_id", "l201",
                              "cdn", JO("url", url, "file_name", "报告.txt"));
            int before = Inject(11045, data);
            JObject m = WaitInboundFrom(before, delegate (JObject p)
            {
                return Scalar(p["post_type"]) == "message" && Scalar(p["message_id"]) == "55550201";
            }, 8000, "文件消息（媒体登记用）");
            JArray segs = m["message"] as JArray;
            Check(segs != null && segs.Count == 1, "文件消息 1 个 file 段");
            Eq("报告.txt", P(segs[0], "data", "name"), "file 段 name 取 cdn.file_name");
            Eq(url, P(segs[0], "data", "file"), "file 段 file 取 cdn.url");
            Eq(url, P(segs[0], "data", "url"), "file 段 url");

            JObject resp = DoAction("get_file", JO("file_id", "55550201"), "T42get");
            Eq("ok", resp["status"], "get_file status");
            CheckBytes(DecodeBase64Payload(Scalar(P(resp, "data", "file")), "get_file"),
                       MediaFixtureBytes("doc.txt"), "get_file 下载内容");
            Eq("报告.txt", P(resp, "data", "file_name"), "get_file file_name");

            JObject gu = DoAction("get_group_file_url", JO("file_id", "55550201"), "T42groupurl");
            Eq("ok", gu["status"], "get_group_file_url status");
            Eq(url, P(gu, "data", "url"), "get_group_file_url 返回原始 CDN url（未缓存字节）");
            Eq("报告.txt", P(gu, "data", "file_name"), "get_group_file_url file_name");

            JObject pu = DoAction("get_private_file_url", JO("file_id", "55550201"), "T42privurl");
            Eq("ok", pu["status"], "get_private_file_url status");
            Eq(url, P(pu, "data", "url"), "get_private_file_url 返回原始 CDN url");
        });

        Run("T43", "get_record 直传 HTTP file → 按需下载 base64（未登记媒体分支）", "actions", delegate
        {
            string url = MockMediaUrl("rec.silk");
            int hitsBefore = MediaHits("rec.silk");
            JObject resp = DoAction("get_record", JO("file", url), "T43");
            Eq("ok", resp["status"], "get_record status");
            CheckBytes(DecodeBase64Payload(Scalar(P(resp, "data", "file")), "get_record"),
                       MediaFixtureBytes("rec.silk"), "get_record 下载内容");
            Eq("file", P(resp, "data", "file_name"), "未登记媒体的 file_name 默认 file");
            Check(MediaHits("rec.silk") > hitsBefore, "Mock 记录到 get_record 的下载请求");
        });

        Run("T44", "媒体未命中 → failed/500：media not available / file url not available", "actions", delegate
        {
            JObject r1 = DoAction("get_image", JO("file", "no-such-media"), "T44a");
            Eq("failed", r1["status"], "get_image 未命中 status");
            EqLong(500, r1["retcode"], "retcode");
            CheckContains(Scalar(r1["msg"]), "media not available: no-such-media", "错误信息");

            JObject r2 = DoAction("get_group_file_url", JO("file_id", "no-such-file"), "T44b");
            Eq("failed", r2["status"], "get_group_file_url 未命中 status");
            EqLong(500, r2["retcode"], "retcode");
            CheckContains(Scalar(r2["msg"]), "file url not available (需要消息内自带 cdn.url)", "错误信息");
        });

        Run("T34", "出站 face 段被忽略，文本正常发送", "actions", delegate
        {
            int cmdStart = CommandCount();
            JArray msg = new JArray(
                JO("type", "text", "data", JO("text", "表情")),
                JO("type", "face", "data", JO("id", "14")));
            JObject resp = DoAction("send_private_msg", JO("user_id", "67890", "message", msg), "T34");
            Eq("ok", resp["status"], "status");
            JObject cmd = WaitCommand(cmdStart, 11029, 8000, "face 段消息命令");
            Eq("表情", P(cmd, "data", "content"), "content 仅文本部分");
        });

        Run("T40", "发送缺字段的负向分支：missing group_id / missing user_id", "actions", delegate
        {
            JObject resp = DoAction("send_group_msg", JO("message", "x"), "T40a");
            Eq("failed", resp["status"], "send_group_msg 缺 group_id status");
            EqLong(500, resp["retcode"], "retcode");
            CheckContains(Scalar(resp["msg"]), "missing group_id", "错误信息");

            JObject resp2 = DoAction("send_private_msg", JO("message", "x"), "T40b");
            Eq("failed", resp2["status"], "send_private_msg 缺 user_id status");
            EqLong(500, resp2["retcode"], "retcode");
            CheckContains(Scalar(resp2["msg"]), "missing user_id", "错误信息");
        });

        Run("T41", "send_group_msg 仅 at 段（无文本）→ 11069 且 content 为空", "actions", delegate
        {
            int cmdStart = CommandCount();
            JArray msg = new JArray(JO("type", "at", "data", JO("qq", "22222")));
            JObject resp = DoAction("send_group_msg",
                JO("group_id", "12345", "message", msg), "T41");
            Eq("ok", resp["status"], "status");
            JObject cmd = WaitCommand(cmdStart, 11069, 8000, "纯 at 群消息命令");
            Eq("", P(cmd, "data", "content"), "content 为空字符串");
            JArray atList = P(cmd, "data", "at_list") as JArray;
            Check(atList != null && atList.Count == 1, "at_list 含 1 个用户");
            EqLong(22222, atList[0], "at_list[0]");
        });

        // ---------------- 扩展能力（本轮新增 shim 能力验收）
        Run("T46", "出站 send_group_msg reply+at+text → 11069（reply 忽略、文本与 at 正常）", "actions", delegate
        {
            int cmdStart = CommandCount();
            JArray msg = new JArray(
                JO("type", "reply", "data", JO("id", "srv-11029-1")),
                JO("type", "at", "data", JO("qq", "22222")),
                JO("type", "text", "data", JO("text", "回复你")));
            JObject resp = DoAction("send_group_msg", JO("group_id", "12345", "message", msg), "T46");
            Eq("ok", resp["status"], "status");
            EqLong(0, resp["retcode"], "retcode");
            Eq(_lastEcho, resp["echo"], "echo 原样回传");
            JObject cmd = WaitCommand(cmdStart, 11069, 8000, "reply+at+text 群消息命令");
            Eq("R:12345", P(cmd, "data", "conversation_id"), "conversation_id");
            Eq("回复你", P(cmd, "data", "content"), "content（reply 段被忽略，正文保留）");
            JArray atList = P(cmd, "data", "at_list") as JArray;
            Check(atList != null && atList.Count == 1, "at_list 含 1 个用户");
            EqLong(22222, atList[0], "at_list[0]");
            Check(CommandCount() == cmdStart + 1, "reply 段不产生额外命令（仅 1 条 11069）");
            Check(Scalar(P(resp, "data", "message_id")).StartsWith("srv-11069-", StringComparison.Ordinal),
                  "message_id 来自 11069 结果");
        });

        Run("T47", "出站 at qq=all 与 face 段被忽略 → 11029 无 at_list", "actions", delegate
        {
            int cmdStart = CommandCount();
            JArray msg = new JArray(
                JO("type", "text", "data", JO("text", "看@全体")),
                JO("type", "at", "data", JO("qq", "all")),
                JO("type", "face", "data", JO("id", "14")));
            JObject resp = DoAction("send_group_msg", JO("group_id", "12345", "message", msg), "T47");
            Eq("ok", resp["status"], "status");
            EqLong(0, resp["retcode"], "retcode");
            JObject cmd = WaitCommand(cmdStart, 11029, 8000, "@全体 群消息命令");
            Eq("R:12345", P(cmd, "data", "conversation_id"), "conversation_id");
            Eq("看@全体", P(cmd, "data", "content"), "content 仅文本（@全体 不作为 at_list）");
            NoKey((JObject)cmd["data"], "at_list", "@all 忽略后降级为 11029，无 at_list");
            Check(CommandCount() == cmdStart + 1, "仅 1 条命令（face/@all 不产生命令）");
        });

        Run("T48", "出站 video 段（base64 mp4）→ 11067（落盘绝对路径、扩展名 .mp4）", "actions", delegate
        {
            int cmdStart = CommandCount();
            byte[] mp4 = SampleMp4();
            JArray msg = new JArray(JO("type", "video",
                "data", JO("file", "base64://" + Convert.ToBase64String(mp4))));
            JObject resp = DoAction("send_private_msg", JO("user_id", "67890", "message", msg), "T48");
            Eq("ok", resp["status"], "status");
            EqLong(0, resp["retcode"], "retcode");
            JObject cmd = WaitCommand(cmdStart, 11067, 8000, "video 媒体命令");
            Eq("S:67890_" + RobotId, P(cmd, "data", "conversation_id"), "11067 conversation_id");
            string path = Scalar(P(cmd, "data", "file"));
            Check(!string.IsNullOrEmpty(path) && Path.IsPathRooted(path), "11067 data.file 为绝对路径");
            Check(File.Exists(path), "11067 data.file 指向已存在文件");
            Check(path.EndsWith(".mp4", StringComparison.OrdinalIgnoreCase), "落盘扩展名为 .mp4");
            CheckBytes(File.ReadAllBytes(path), mp4, "11067 媒体文件内容与 base64 一致");
            Check(CommandCount() == cmdStart + 1, "仅 1 条媒体命令（无文本段）");
            Check(Scalar(P(resp, "data", "message_id")).StartsWith("srv-11067-", StringComparison.Ordinal),
                  "message_id 来自 11067 结果");
        });

        Run("T49", "出站 file 段（绝对路径）→ 11031（保留 data.file_name）", "actions", delegate
        {
            int cmdStart = CommandCount();
            byte[] content = Encoding.UTF8.GetBytes("微灵文件发送测试内容");
            string src = WriteTempFile("tmp_upload_source.txt", content);
            JArray msg = new JArray(JO("type", "file",
                "data", JO("name", "报告.txt", "file", src)));
            JObject resp = DoAction("send_group_msg", JO("group_id", "12345", "message", msg), "T49");
            Eq("ok", resp["status"], "status");
            EqLong(0, resp["retcode"], "retcode");
            JObject cmd = WaitCommand(cmdStart, 11031, 8000, "file 媒体命令");
            Eq("R:12345", P(cmd, "data", "conversation_id"), "11031 conversation_id");
            string path = Scalar(P(cmd, "data", "file"));
            Check(!string.IsNullOrEmpty(path) && Path.IsPathRooted(path), "11031 data.file 为绝对路径");
            Check(File.Exists(path), "11031 data.file 指向已存在文件");
            Check(path.EndsWith(".txt", StringComparison.OrdinalIgnoreCase), "落盘扩展名为 .txt");
            CheckBytes(File.ReadAllBytes(path), content, "11031 媒体文件内容与源文件一致");
            Eq("报告.txt", P(cmd, "data", "file_name"), "11031 data.file_name 保留段中的 name");
        });

        Run("T50", "出站 image 段 gif 变体（data.type=gif / file 后缀 .gif）→ 11070", "actions", delegate
        {
            int cmdStart = CommandCount();
            byte[] gif = SampleGif();
            JArray msg = new JArray(JO("type", "image",
                "data", JO("file", "base64://" + Convert.ToBase64String(gif), "type", "gif")));
            JObject resp = DoAction("send_private_msg", JO("user_id", "67890", "message", msg), "T50a");
            Eq("ok", resp["status"], "data.type=gif status");
            JObject cmd = WaitCommand(cmdStart, 11070, 8000, "gif 图片命令（type=gif）");
            string path = Scalar(P(cmd, "data", "file"));
            Check(!string.IsNullOrEmpty(path) && File.Exists(path), "11070 data.file 存在");
            Check(path.EndsWith(".gif", StringComparison.OrdinalIgnoreCase), "11070 落盘扩展名 .gif");
            CheckBytes(File.ReadAllBytes(path), gif, "11070 媒体文件内容与 base64 一致");

            int cmdStart2 = CommandCount();
            string gifSrc = WriteTempFile("tmp_anim.gif", SampleGif());
            JArray msg2 = new JArray(JO("type", "image", "data", JO("file", gifSrc)));
            JObject resp2 = DoAction("send_private_msg", JO("user_id", "67890", "message", msg2), "T50b");
            Eq("ok", resp2["status"], "file 后缀 .gif status");
            JObject cmd2 = WaitCommand(cmdStart2, 11070, 8000, "gif 图片命令（.gif 文件路径）");
            string path2 = Scalar(P(cmd2, "data", "file"));
            Check(path2.EndsWith(".gif", StringComparison.OrdinalIgnoreCase), "11070(.gif 路径) 落盘扩展名 .gif");
        });

        Run("T51", "出站 record 段 → failed/500（暂不支持发送语音），且不下发微灵命令", "actions", delegate
        {
            int cmdStart = CommandCount();
            JArray msg = new JArray(JO("type", "record",
                "data", JO("file", "https://cdn.example.com/a.silk")));
            JObject resp = DoAction("send_private_msg", JO("user_id", "67890", "message", msg), "T51");
            Eq("failed", resp["status"], "status");
            EqLong(500, resp["retcode"], "retcode");
            CheckContains(Scalar(resp["msg"]), "暂不支持发送语音", "错误信息");
            Check(CommandCount() == cmdStart, "record 段在解析阶段被拒绝，未下发微灵命令");
        });

        Run("T52", "入站 11046 位置 → location 段（lat/lon/title/content）", "events", delegate
        {
            int before = Inject(11046, JO("conversation_id", "R:12345", "sender", "11111",
                                          "sender_name", "张三", "send_time", 1710000800,
                                          "server_id", "55550301",
                                          "latitude", 31.2304, "longitude", 121.4737,
                                          "title", "公司", "address", "上海市黄浦区某路 1 号"));
            JObject m = WaitInboundFrom(before, delegate (JObject p)
            {
                return Scalar(p["post_type"]) == "message" && Scalar(p["message_id"]) == "55550301";
            }, 8000, "位置消息");
            JArray segs = m["message"] as JArray;
            Check(segs != null && segs.Count == 1, "位置消息 1 个段");
            Eq("location", segs[0]["type"], "段类型 location");
            Eq("31.2304", P(segs[0], "data", "lat"), "location.data.lat 取 latitude");
            Eq("121.4737", P(segs[0], "data", "lon"), "location.data.lon 取 longitude");
            Eq("公司", P(segs[0], "data", "title"), "location.data.title");
            Eq("上海市黄浦区某路 1 号", P(segs[0], "data", "content"), "location.data.content 取 address");
        });

        Run("T53", "入站 11047 链接 → share 段（url/title/content/image）", "events", delegate
        {
            int before = Inject(11047, JO("conversation_id", "R:12345", "sender", "11111",
                                          "sender_name", "张三", "send_time", 1710000900,
                                          "server_id", "55550302",
                                          "url", "https://news.example.com/a/1",
                                          "title", "新闻标题", "desc", "新闻摘要",
                                          "image_url", "https://cdn.example.com/img/thumb.png"));
            JObject m = WaitInboundFrom(before, delegate (JObject p)
            {
                return Scalar(p["post_type"]) == "message" && Scalar(p["message_id"]) == "55550302";
            }, 8000, "链接消息");
            JArray segs = m["message"] as JArray;
            Check(segs != null && segs.Count == 1, "链接消息 1 个段");
            Eq("share", segs[0]["type"], "段类型 share");
            Eq("https://news.example.com/a/1", P(segs[0], "data", "url"), "share.data.url");
            Eq("新闻标题", P(segs[0], "data", "title"), "share.data.title");
            Eq("新闻摘要", P(segs[0], "data", "content"), "share.data.content 取 desc");
            Eq("https://cdn.example.com/img/thumb.png", P(segs[0], "data", "image"),
               "share.data.image 取 image_url");

            // 空 url/title 时降级为文本段（避免 AstrBot 收到空的 share 段）
            before = Inject(11047, JO("conversation_id", "R:12345", "sender", "11111",
                                      "sender_name", "张三", "send_time", 1710000901,
                                      "server_id", "55550305", "content", "链接描述文本"));
            JObject m2 = WaitInboundFrom(before, delegate (JObject p)
            {
                return Scalar(p["post_type"]) == "message" && Scalar(p["message_id"]) == "55550305";
            }, 8000, "空链接降级消息");
            JArray segs2 = m2["message"] as JArray;
            Check(segs2 != null && segs2.Count == 1, "空 url/title 降级为 1 个段");
            Eq("text", segs2[0]["type"], "降级段类型 text");
            Eq("[链接] 链接描述文本", P(segs2[0], "data", "text"), "降级文本内容取 content");
        });

        Run("T54", "入站 11050 名片 → contact 段（type=qq，id 为 JSON 数字）", "events", delegate
        {
            int before = Inject(11050, JO("conversation_id", "R:12345", "sender", "11111",
                                          "sender_name", "张三", "send_time", 1710001000,
                                          "server_id", "55550303",
                                          "nickname", "名片昵称", "user_id", "12345678"));
            JObject m = WaitInboundFrom(before, delegate (JObject p)
            {
                return Scalar(p["post_type"]) == "message" && Scalar(p["message_id"]) == "55550303";
            }, 8000, "名片消息");
            JArray segs = m["message"] as JArray;
            Check(segs != null && segs.Count == 1, "名片消息 1 个段");
            Eq("contact", segs[0]["type"], "段类型 contact");
            Eq("qq", P(segs[0], "data", "type"), "contact.data.type=qq");
            EqLong(12345678, P(segs[0], "data", "id"), "contact.data.id 为 JSON 数字");
        });

        Run("T55", "入站 11048 动图 → image 段（file=name，url=cdn url）", "events", delegate
        {
            string url = MockMediaUrl("anim.gif");
            int before = Inject(11048, JO("conversation_id", "R:12345", "sender", "11111",
                                          "sender_name", "张三", "send_time", 1710001100,
                                          "server_id", "55550304",
                                          "name", "anim.gif", "url", url));
            JObject m = WaitInboundFrom(before, delegate (JObject p)
            {
                return Scalar(p["post_type"]) == "message" && Scalar(p["message_id"]) == "55550304";
            }, 8000, "动图消息");
            JArray segs = m["message"] as JArray;
            Check(segs != null && segs.Count == 1, "动图消息 1 个段");
            Eq("image", segs[0]["type"], "段类型 image");
            Eq("anim.gif", P(segs[0], "data", "file"), "image.data.file 取 name");
            Eq(url, P(segs[0], "data", "url"), "image.data.url");
        });

        Run("T56", "get_group_member_list → 11040 查询带顶层 getData:true，返回成员数组", "actions", delegate
        {
            int cmdStart = CommandCount();
            JObject resp = DoAction("get_group_member_list", JO("group_id", "12345"), "T56");
            Eq("ok", resp["status"], "status");
            EqLong(0, resp["retcode"], "retcode");
            JObject cmd = WaitCommand(cmdStart, 11040, 8000, "get_group_member_list 的 11040 查询命令");
            Eq("true", Scalar(cmd["getData"]), "命令顶层 getData=true");
            EqLong(11040, cmd["type"], "命令 type");
            Eq("R:12345", P(cmd, "data", "conversation_id"), "查询 conversation_id");
            JArray arr = resp["data"] as JArray;
            Check(arr != null && arr.Count == 2, "返回 2 个成员");
            EqLong(22222, P(arr[0], "user_id"), "成员0 user_id");
            Eq("成员甲", P(arr[0], "nickname"), "成员0 nickname 取 name");
            EqLong(33333, P(arr[1], "user_id"), "成员1 user_id");
            Eq("群昵称乙", P(arr[1], "card"), "成员1 card 取 group_nickname");
        });

        Run("T57", "get_group_msg_history → ok 且返回空数组（不回调微灵 handler）", "actions", delegate
        {
            int cmdStart = CommandCount();
            JObject resp = DoAction("get_group_msg_history", JO("group_id", "12345", "count", 20), "T57");
            Eq("ok", resp["status"], "status");
            EqLong(0, resp["retcode"], "retcode");
            JArray arr = resp["data"] as JArray;
            Check(arr != null && arr.Count == 0, "data 为空数组");
            Check(CommandCount() == cmdStart, "未下发微灵命令");
        });

        Run("T58", "入站 11200 未映射消息 → message 事件（content/at_list 透传）", "events", delegate
        {
            int before = Inject(11200, JO("conversation_id", "R:12345", "sender", "11111",
                                          "sender_name", "张三", "send_time", 1710001200,
                                          "server_id", "55550401", "content", "未映射消息内容",
                                          "at_list", new JArray("22222")));
            JObject m = WaitInboundFrom(before, delegate (JObject p)
            {
                return Scalar(p["post_type"]) == "message" && Scalar(p["message_id"]) == "55550401";
            }, 8000, "11200 未映射消息事件");
            Eq("group", m["message_type"], "message_type");
            EqLong(12345, m["group_id"], "group_id");
            JArray segs = m["message"] as JArray;
            Check(segs != null && segs.Count == 2, "11200 消息含 at + text 共 2 段");
            Eq("at", segs[0]["type"], "段0 类型 at");
            Eq("22222", P(segs[0], "data", "qq"), "at 段 qq");
            Eq("text", segs[1]["type"], "段1 类型 text");
            Eq("未映射消息内容", P(segs[1], "data", "text"), "text 段内容取 content");
            Eq("@22222未映射消息内容", m["raw_message"], "raw_message 拼接");
            EqLong(11200, P(m, "ling", "type"), "ling.type 保留原始 11200");
        });

        Run("T59", "入站 11200 已读上报 → 不推送（message_type / event_desc 两种判定）", "events", delegate
        {
            int before = Inject(11200, JO("conversation_id", "R:12345", "sender", "11111",
                                          "message_type", "read_receipt", "server_id", "55550402"));
            CheckNoInbound(before, delegate (JObject p) { return Scalar(p["post_type"]) == "message"; },
                           1500, "message_type=read_receipt 的 11200");
            before = Inject(11200, JO("conversation_id", "R:12345", "sender", "11111",
                                      "event_desc", "已读上报", "server_id", "55550403"));
            CheckNoInbound(before, delegate (JObject p) { return Scalar(p["post_type"]) == "message"; },
                           1500, "event_desc=已读上报 的 11200");
        });

        Run("T60", "出站 image 段（http(s) URL）→ 经 Mock 下载落盘后发 11030", "actions", delegate
        {
            int cmdStart = CommandCount();
            string url = MockMediaUrl("pic.png");
            int hitsBefore = MediaHits("pic.png");
            JArray msg = new JArray(JO("type", "image", "data", JO("file", url)));
            JObject resp = DoAction("send_private_msg", JO("user_id", "67890", "message", msg), "T60");
            Eq("ok", resp["status"], "status");
            EqLong(0, resp["retcode"], "retcode");
            JObject cmd = WaitCommand(cmdStart, 11030, 8000, "http URL 图片的 11030 命令");
            Eq("S:67890_" + RobotId, P(cmd, "data", "conversation_id"), "11030 conversation_id");
            string path = Scalar(P(cmd, "data", "file"));
            Check(!string.IsNullOrEmpty(path) && Path.IsPathRooted(path), "11030 data.file 为绝对路径");
            Check(File.Exists(path), "11030 data.file 指向已存在文件");
            Check(path.EndsWith(".png", StringComparison.OrdinalIgnoreCase), "落盘扩展名 .png");
            CheckBytes(File.ReadAllBytes(path), MediaFixtureBytes("pic.png"), "落盘内容为 Mock 下载的 PNG");
            Check(MediaHits("pic.png") > hitsBefore, "Mock 记录到 shim 的 HTTP 下载请求");
            Check(CommandCount() == cmdStart + 1, "仅 1 条 11030 命令（无文本段）");
        });

        Run("T61", "出站 image 段（file:// 前缀 + 绝对路径）→ 11030", "actions", delegate
        {
            int cmdStart = CommandCount();
            byte[] png = MediaFixtureBytes("pic.png");
            string src = WriteTempFile("tmp_src_for_fileprefix.png", png);
            string fileUri = "file:///" + src.Replace('\\', '/');
            Note("file:// 源: {0}", fileUri);
            JArray msg = new JArray(JO("type", "image", "data", JO("file", fileUri)));
            JObject resp = DoAction("send_private_msg", JO("user_id", "67890", "message", msg), "T61");
            Eq("ok", resp["status"], "status");
            EqLong(0, resp["retcode"], "retcode");
            JObject cmd = WaitCommand(cmdStart, 11030, 8000, "file:// 图片的 11030 命令");
            string path = Scalar(P(cmd, "data", "file"));
            Check(!string.IsNullOrEmpty(path) && Path.IsPathRooted(path), "11030 data.file 为绝对路径");
            Check(File.Exists(path), "11030 data.file 指向已存在文件");
            Check(path.EndsWith(".png", StringComparison.OrdinalIgnoreCase), "落盘扩展名 .png");
            CheckBytes(File.ReadAllBytes(path), png, "file:// 源文件内容一致");
        });

        Run("T62", "出站未知消息段（json）→ failed/500 提示不支持，不下发微灵命令", "actions", delegate
        {
            int cmdStart = CommandCount();
            JArray msg = new JArray(JO("type", "json", "data", JO("data", "{}")));
            JObject resp = DoAction("send_group_msg", JO("group_id", "12345", "message", msg), "T62");
            Eq("failed", resp["status"], "status");
            EqLong(500, resp["retcode"], "retcode");
            CheckContains(Scalar(resp["msg"]), "不支持", "错误信息提示不支持");
            Check(CommandCount() == cmdStart, "未知消息段不产生微灵命令");
        });

        // ---------------- heartbeat (waits for the 30 s timer)
        Run("T04", "心跳 meta_event（interval=30s）+ 心跳周期内无周期性重连（看门狗已删除）", "handshake", delegate
        {
            JObject hb = WaitInboundFrom(0, delegate (JObject p)
            {
                return Scalar(p["post_type"]) == "meta_event"
                    && Scalar(p["meta_event_type"]) == "heartbeat";
            }, 45000, "heartbeat");
            EqLong(30000, hb["interval"], "heartbeat.interval");
            Eq("true", P(hb, "status", "online"), "heartbeat.status.online");
            Eq("true", P(hb, "status", "good"), "heartbeat.status.good");
            EqLong(RobotIdLong, hb["self_id"], "heartbeat.self_id");
            Check(MockConnCount() == 1, "心跳等待期间没有周期性重连（总连接数仍为 1）");
            Check(Shim1.IsConnected, "心跳周期后连接仍存活");
        });

        // ---------------- resilience / lifecycle
        Run("T36", "Mock 主动断开 → shim 约 1s 后自动重连", "resilience", delegate
        {
            int connsBefore = MockConnCount();
            Check(MockOpenCount() >= 1, "断开前存在活跃连接");
            long msStart = DateTime.UtcNow.Ticks / TimeSpan.TicksPerMillisecond;
            JObject r = HttpJson("POST", "/ctl/close_all", null, 8000);
            Check(Scalar(r["ok"]) == "true", "mock close_all 成功");
            WaitFor(delegate { return MockOpenCount() >= 1; }, 12000, "shim 重新建立连接");
            WaitFor(delegate { return Shim1.IsConnected; }, 12000, "shim IsConnected 恢复");
            long latency = DateTime.UtcNow.Ticks / TimeSpan.TicksPerMillisecond - msStart;
            Note("断开→重连完成耗时约 {0} ms", latency);
            Check(latency < 6000, "重连耗时 < 6s（ReconnectMinMs=1000 + 握手）");
            Check(MockConnCount() > connsBefore, "Mock 记录到新的连接（重连发生）");
            Eq("已连接", Shim1.StatusText, "重连后 StatusText=已连接");
        });

        Run("T37", "Stop 后不再重连、状态回到未连接", "lifecycle", delegate
        {
            Shim1.Stop();
            Check(!Shim1.IsConnected, "Stop 后 IsConnected==false");
            Eq("未连接", Shim1.StatusText, "Stop 后 StatusText=未连接");
            Eq("#909399", Shim1.StatusBackground, "Stop 后 StatusBackground=#909399");
            WaitFor(delegate { return MockOpenCount() == 0; }, 5000, "Mock 侧连接关闭");
            int conns = MockConnCount();
            Thread.Sleep(1800);
            Check(MockConnCount() == conns, "Stop 后 1.8s 内没有新的重连连接");
        });

        Run("T35", "token 不符 → Mock 401 拒绝；shim 重试不崩溃；改正后连上", "resilience", delegate
        {
            List<string> status2 = new List<string>();
            string tokenValue = "badtoken";
            Func<string> getToken = delegate { return tokenValue; };
            WxSocketClient shim2 = MakeShim("wecom-bad-token",
                delegate { return "ws://127.0.0.1:6399/ws"; }, getToken, status2);
            InjectTo(shim2, 11026, JO("user_id", RobotId, "username", "机器人昵称"));
            int rejBefore = MockRejectCount();
            shim2.Start();
            WaitFor(delegate { return MockRejectCount() > rejBefore; }, 15000, "Mock 记录到 401 拒绝");
            JObject meta = MockMeta();
            JArray rejs = meta["rejections"] as JArray;
            JObject last = (JObject)rejs[rejs.Count - 1];
            Eq("Bearer badtoken", P(last, "headers", "authorization"), "被拒绝握手的 Authorization");
            Check(!shim2.IsConnected, "token 错误时 IsConnected==false");
            Eq("连接中", shim2.StatusText, "token 错误时 StatusText=连接中（重试中）");
            Check(shim2.StatusBackground == "#E6A23C", "token 错误时 StatusBackground=#E6A23C");
            Thread.Sleep(1200);
            tokenValue = Token;
            WaitFor(delegate { return shim2.IsConnected; }, 15000, "改回正确 token 后连接成功");
            Eq("已连接", shim2.StatusText, "改正 token 后 StatusText=已连接");
            Check(MockOpenCount() >= 1, "Mock 侧存在活跃连接");
            shim2.Stop();
            shim2.Dispose();
        });

        // ---------------- log / status evidence
        Run("T38", "onebot_v11.log 证据：连接/生命周期/重连/忽略事件/动作日志", "log", delegate
        {
            Thread.Sleep(300);
            Check(File.Exists(ShimLogPath), "日志文件存在: " + ShimLogPath);
            string log = File.ReadAllText(ShimLogPath, Encoding.UTF8);
            Note("日志大小 {0} 字节", log.Length);
            CheckContains(log, "[OneBot:wecom-test]", "channel 日志前缀");
            CheckContains(log, "connected: ws://127.0.0.1:6399/ws", "连接成功日志");
            CheckContains(log, "lifecycle connect queued", "lifecycle 入队日志");
            CheckContains(log, "server closed connection", "服务端断开日志");
            CheckContains(log, "reconnect in 1000 ms", "重连延迟日志");
            CheckContains(log, "<- action send_private_msg", "入站动作日志");
            CheckContains(log, "不映射为 OneBot 事件", "未映射事件日志（99999）");
            CheckContains(log, "[OneBot:wecom-bad-token]", "坏 token shim 的日志前缀");
            CheckContains(log, "connection failed:", "连接失败日志（token 401）");
            Check(log.IndexOf("watchdog", StringComparison.OrdinalIgnoreCase) < 0,
                  "日志中没有 watchdog 静默断连记录（新 shim 已彻底删除看门狗）");
            Check(log.IndexOf("120000 ms; closing socket", StringComparison.OrdinalIgnoreCase) < 0,
                  "日志中没有 120s 静默强制关链记录");
        });

        Run("T39", "StatusChanged 事件序列：未连接/连接中/已连接/未连接 与背景色", "lifecycle", delegate
        {
            string trail = string.Join("\n", Shim1Status.ToArray());
            Note("状态序列:\n{0}", trail);
            CheckContains(trail, "连接中|#E6A23C|False", "启动时 连接中/#E6A23C");
            CheckContains(trail, "已连接|#22AA22|True", "连接后 已连接/#22AA22");
            CheckContains(trail, "未连接|#909399|False", "Stop 后 未连接/#909399");
        });
    }

    // =================================================================== main
    static void Init()
    {
        Root = AppDomain.CurrentDomain.BaseDirectory;
        TestsDir = Path.GetFullPath(Path.Combine(Root, ".."));
        LogDir = Path.Combine(TestsDir, "logs");
        Directory.CreateDirectory(LogDir);
        ShimLogPath = Path.Combine(Root, "onebot_v11.log");
        HostLog = new StreamWriter(Path.Combine(LogDir, "testhost.log"), false, new UTF8Encoding(false));
        HostLog.AutoFlush = true;
        CmdLog = new StreamWriter(Path.Combine(LogDir, "commands.jsonl"), false, new UTF8Encoding(false));
        CmdLog.AutoFlush = true;
        _startedAt = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
        HostLog.WriteLine("TestHost start " + _startedAt);
        HostLog.WriteLine("Root=" + Root);
        HostLog.WriteLine("TestsDir=" + TestsDir);
        HostLog.WriteLine("ShimLogPath=" + ShimLogPath);
        HostLog.WriteLine("MockBase=" + MockBase);
    }

    static int WriteResults()
    {
        int pass = 0;
        int fail = 0;
        JArray arr = new JArray();
        foreach (CaseResult r in Results)
        {
            if (r.Status == "PASS") pass++; else fail++;
            JArray ev = new JArray();
            foreach (string s in r.Evidence) ev.Add(s);
            arr.Add(JO("id", r.Id, "name", r.Name, "group", r.Group, "status", r.Status,
                       "duration_ms", r.DurationMs, "error", r.Error, "evidence", ev));
        }
        JObject summary = JO("total", Results.Count, "passed", pass, "failed", fail,
                             "mock_base", MockBase, "robot_id", RobotId, "token", Token);
        JObject root = JO("started_at", _startedAt,
                          "finished_at", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
                          "summary", summary,
                          "cases", arr);
        string path = Path.Combine(LogDir, "results_raw.json");
        File.WriteAllText(path, root.ToString(Formatting.Indented), new UTF8Encoding(false));
        HostLog.WriteLine("RESULTS " + path);
        HostLog.WriteLine("TOTAL=" + Results.Count + " PASS=" + pass + " FAIL=" + fail);
        Console.WriteLine("TOTAL=" + Results.Count + " PASS=" + pass + " FAIL=" + fail);
        foreach (CaseResult r in Results)
            if (r.Status != "PASS")
                Console.WriteLine("FAIL " + r.Id + " " + r.Name + " :: " + r.Error);
        return fail;
    }

    static void Cleanup()
    {
        try { if (Shim1 != null) { Shim1.Stop(); Shim1.Dispose(); } }
        catch { }
        try { if (CmdLog != null) { CmdLog.Flush(); CmdLog.Dispose(); } }
        catch { }
        try { if (HostLog != null) HostLog.Flush(); }
        catch { }
    }

    static int Main(string[] args)
    {
        Init();
        int fatal = 0;
        try
        {
            WaitMockReady(30000);
            HttpJson("POST", "/ctl/reset", null, 8000);
            Note("mock 已就绪，开始 " + _startedAt);
            RunAll();
        }
        catch (Exception ex)
        {
            fatal = 1;
            HostLog.WriteLine("FATAL: " + ex.ToString());
            Console.WriteLine("FATAL: " + ex.Message);
        }
        Cleanup();
        int fails = WriteResults();
        try { HostLog.Flush(); HostLog.Dispose(); }
        catch { }
        if (fatal != 0) return 2;
        return fails == 0 ? 0 : 1;
    }
}
