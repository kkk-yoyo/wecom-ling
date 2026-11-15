// AstrBotProbe: 用真实 AstrBot（aiocqhttp 反向 WS，6200/token 114514）验证 OneBotShim.WxSocketClient。
// 编译（C#5 / x86）：
//   csc /nologo /target:exe /platform:x86 /out:AstrBotProbe.exe ^
//       /r:System.dll /r:System.Core.dll /r:System.Xml.dll ^
//       /r:WeComSever.exe /r:OneBotShim.dll /r:Newtonsoft.Json.dll AstrBotProbe.cs
// 运行前：把 微灵1.11.0_免卡密.exe 复制为本目录 WeComSever.exe（Unblock-File），
//        把 OneBotShim.dll、lib\Newtonsoft.Json.dll 复制到本目录；
//        先运行 http_stub_8060.py 提供 self_id 探测。
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Threading;
using System.Threading.Tasks;

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

using Robot.Common;

namespace AstrBotProbe
{
    internal static class Program
    {
        private static readonly List<string> Commands = new List<string>();
        private static long _seq;

        private static Task<CommandExecResult> OnCommand(JObject payload)
        {
            lock (Commands)
            {
                Commands.Add(payload.ToString(Formatting.None));
            }
            Console.WriteLine("[probe] 微灵命令 <- " + payload.ToString(Formatting.None));
            CommandExecResult result = new CommandExecResult();
            result.Ok = true;
            result.Message = "ok";
            JObject data = new JObject();
            data["server_id"] = new JValue("probe-" + (1000 + Interlocked.Increment(ref _seq)).ToString(CultureInfo.InvariantCulture));
            result.Data = data;
            return Task.FromResult(result);
        }

        private static void SendEvent(OneBotShim.WxSocketClient client, int type, JObject data)
        {
            JObject evt = new JObject();
            evt["client"] = new JValue(1);
            evt["type"] = new JValue(type);
            evt["robot_id"] = new JValue("1688855837869858");
            evt["data"] = data;
            bool sent = client.TrySendEvent(evt.ToString(Formatting.None));
            Console.WriteLine("[probe] 推送微灵事件 type=" + type.ToString(CultureInfo.InvariantCulture) + " sent=" + sent);
        }

        private static bool WaitUntil(Func<bool> condition, int timeoutMs)
        {
            DateTime deadline = DateTime.UtcNow.AddMilliseconds(timeoutMs);
            while (DateTime.UtcNow < deadline)
            {
                if (condition())
                {
                    return true;
                }
                Thread.Sleep(250);
            }
            return condition();
        }

        private static int Main(string[] args)
        {
            string url = args.Length > 0 ? args[0] : "ws://127.0.0.1:6200";
            string token = args.Length > 1 ? args[1] : "114514";
            Console.WriteLine("[probe] url=" + url + " token=" + (string.IsNullOrEmpty(token) ? "(none)" : "(set)"));

            OneBotShim.WxSocketClient client = new OneBotShim.WxSocketClient(
                "wecom",
                "/wx/ws",
                delegate { return url; },
                delegate { return token; },
                delegate { return "AstrBotProbe"; },
                new WeComConsoleServer.Core.WxWebSocketClient.CommandHandler(OnCommand));
            client.StatusChanged += delegate { Console.WriteLine("[probe] 状态变化: " + client.StatusText + " connected=" + client.IsConnected); };

            client.Start();
            bool connected = WaitUntil(delegate { return client.IsConnected; }, 60000);
            Console.WriteLine("[probe] connected=" + connected + " status=" + client.StatusText);
            if (!connected)
            {
                client.Dispose();
                return 2;
            }

            // 1) 登录事件（仅用于 shim 记录 self_id/nickname，不会转发为 OneBot 事件）
            JObject login = new JObject();
            login["user_id"] = new JValue("1688855837869858");
            login["username"] = new JValue("ProbeBot");
            login["account"] = new JValue("1688855837869858");
            SendEvent(client, 11026, login);
            Thread.Sleep(1000);

            // 2) 私聊文本消息 -> AstrBot 应收到 message/private
            JObject priv = new JObject();
            priv["conversation_id"] = new JValue("S:10001_1688855837869858");
            priv["conversation_type"] = new JValue("private");
            priv["sender"] = new JValue("10001");
            priv["sender_name"] = new JValue("Tester");
            priv["content"] = new JValue("你好，这是一条 OneBot v11 私聊测试消息");
            priv["content_type"] = new JValue(2);
            priv["server_id"] = new JValue("srv-priv-1");
            priv["local_id"] = new JValue("loc-priv-1");
            priv["send_time"] = new JValue(NowUnix());
            priv["is_self"] = new JValue(false);
            priv["is_pc"] = new JValue(false);
            SendEvent(client, 11041, priv);
            Thread.Sleep(1500);

            // 3) 群聊文本消息 -> AstrBot 应收到 message/group
            JObject grp = new JObject();
            grp["conversation_id"] = new JValue("R:20002");
            grp["conversation_type"] = new JValue("group");
            grp["sender"] = new JValue("10002");
            grp["sender_name"] = new JValue("GroupTester");
            grp["content"] = new JValue("群聊测试消息 onebot v11");
            grp["content_type"] = new JValue(2);
            grp["server_id"] = new JValue("srv-grp-1");
            grp["local_id"] = new JValue("loc-grp-1");
            grp["send_time"] = new JValue(NowUnix());
            grp["is_self"] = new JValue(false);
            grp["is_pc"] = new JValue(false);
            SendEvent(client, 11041, grp);
            Thread.Sleep(1500);

            // 4) 群通知（成员增加）-> notice/group_increase
            JObject notice = new JObject();
            notice["room_conversation_id"] = new JValue("R:20002");
            notice["room_name"] = new JValue("测试群");
            notice["op_user_id"] = new JValue("10003");
            JArray members = new JArray();
            JObject member = new JObject();
            member["user_id"] = new JValue("10004");
            member["name"] = new JValue("新成员");
            members.Add(member);
            notice["member_list"] = members;
            SendEvent(client, 11072, notice);

            // 5) 等待 AstrBot 处理事件/调用动作（LLM 回复会触发 send_private_msg/send_group_msg）
            Console.WriteLine("[probe] 等待 AstrBot 下发动作（最多 90 秒）...");
            WaitUntil(delegate { lock (Commands) { return Commands.Count > 0; } }, 90000);
            lock (Commands)
            {
                Console.WriteLine("[probe] 收到微灵命令数=" + Commands.Count.ToString(CultureInfo.InvariantCulture));
                for (int i = 0; i < Commands.Count; i++)
                {
                    Console.WriteLine("[probe]   #" + i.ToString(CultureInfo.InvariantCulture) + " " + Commands[i]);
                }
            }

            client.Stop();
            Thread.Sleep(500);
            client.Dispose();
            Console.WriteLine("[probe] done");
            return 0;
        }

        private static long NowUnix()
        {
            return (long)(DateTime.UtcNow - new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc)).TotalSeconds;
        }
    }
}
