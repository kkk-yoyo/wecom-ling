// ============================================================================
// 微灵 (WeComSever) OneBot v11 改造 shim
// ----------------------------------------------------------------------------
// 该文件实现一个与原始 WeComConsoleServer.Core.WxWebSocketClient 公开接口完全
// 一致的客户端类型，但内部协议由「微灵自研 WS 协议」替换为「OneBot v11 反向
// WebSocket 协议（客户端角色）」。
//
// 公开接口（会被注入回原程序集）：
//   ctor(string channel, string defaultPath, Func<string> getUrl,
//        Func<string> getToken, Func<string> getClientName, CommandHandler onCommand)
//   void Start(); void Stop(); bool TrySendEvent(string json); void Dispose();
//   bool IsConnected { get; } string StatusText { get; } string StatusBackground { get; }
//   event Action StatusChanged;
//
// C# 5 语法约束：不使用 lambda / async / await / yield / ?. / $"" / 名称运算符，
// 以便编译产物不产生额外的编译器生成嵌套类型，便于 IL 移植。
// ============================================================================

using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

using Robot.Common;
using CmdHandler = WeComConsoleServer.Core.WxWebSocketClient.CommandHandler;

namespace OneBotShim
{
    public sealed class WxSocketClient
    {
        // ---------------------------------------------------------------- 常量
        private const int ReconnectMinMs = 1000;
        private const int ReconnectMaxMs = 30000;
        private const int ConnectTimeoutMs = 15000;
        private const int ActionTimeoutMs = 60000;
        private const int HeartbeatIntervalMs = 30000;
        private const int OutQueueLimit = 2000;
        private const int MessageCacheLimit = 500;
        private const int MediaCacheLimitBytes = 32 * 1024 * 1024;
        private const int FileServerPortMin = 18890;
        private const int FileServerPortMax = 18920;

        // ---------------------------------------------------------------- 字段
        private readonly string _channel;
        private readonly string _defaultPath;
        private readonly Func<string> _getUrl;
        private readonly Func<string> _getToken;
        private readonly Func<string> _getClientName;
        private readonly CmdHandler _onCommand;

        private readonly object _syncRoot = new object();
        private readonly object _queueLock = new object();
        private readonly object _sendLock = new object();
        private readonly object _cacheLock = new object();
        private readonly object _mediaLock = new object();
        private readonly object _logLock = new object();
        private readonly object _fileServerLock = new object();

        private string _logPrefix;
        private volatile bool _userWanted;
        private volatile bool _connected;
        private volatile bool _disposed;

        private ClientWebSocket _socket;
        private Thread _netThread;
        private Thread _sendThread;
        private AutoResetEvent _sendSignal;
        private Queue<string> _outQueue;

        private DateTime _lastRecvUtc;
        private DateTime _lastHeartbeatUtc;
        private DateTime _lastSelfIdProbeUtc;
        private int _reconnectDelayMs;

        private string _selfId;
        private string _nickname;
        private string _clientName;
        private string _lastClientToken;

        private readonly Dictionary<string, string> _convClients;
        private readonly Dictionary<string, string> _msgCache;
        private readonly Queue<string> _msgCacheOrder;
        private readonly Dictionary<int, bool> _ignoredTypes;

        private readonly Dictionary<string, object[]> _media;
        private readonly Queue<string> _mediaOrder;
        private long _mediaBytesTotal;

        private TcpListener _fileListener;
        private int _filePort;
        private Thread _fileThread;

        private long _nextMessageId;

        // 状态变化通知（主窗口绑定；名称与目标字段一致，便于 IL 移植复用）
        public event Action StatusChanged;

        // ---------------------------------------------------------------- 属性
        public bool IsConnected
        {
            get { return this._connected; }
        }

        public string StatusText
        {
            get
            {
                if (!this._userWanted)
                {
                    return "未连接";
                }
                if (this._connected)
                {
                    return "已连接";
                }
                return "连接中";
            }
        }

        public string StatusBackground
        {
            get
            {
                if (!this._userWanted)
                {
                    return "#909399";
                }
                if (this._connected)
                {
                    return "#22AA22";
                }
                return "#E6A23C";
            }
        }

        // ---------------------------------------------------------------- 构造
        public WxSocketClient(string channel, string defaultPath, Func<string> getUrl,
            Func<string> getToken, Func<string> getClientName, CmdHandler onCommand)
        {
            this._channel = channel == null ? "" : channel;
            this._defaultPath = defaultPath == null ? "" : defaultPath;
            this._getUrl = getUrl;
            this._getToken = getToken;
            this._getClientName = getClientName;
            this._onCommand = onCommand;

            this._sendSignal = new AutoResetEvent(false);
            this._outQueue = new Queue<string>();
            this._convClients = new Dictionary<string, string>();
            this._msgCache = new Dictionary<string, string>();
            this._msgCacheOrder = new Queue<string>();
            this._ignoredTypes = new Dictionary<int, bool>();
            this._media = new Dictionary<string, object[]>();
            this._mediaOrder = new Queue<string>();
            this._reconnectDelayMs = ReconnectMinMs;
            this._lastClientToken = "1";
            this._nextMessageId = 100000;
            this._logPrefix = "[OneBot:" + this._channel + "] ";
            this.Log("client created; channel=" + this._channel + "; defaultPath=" + this._defaultPath);
        }

        // ---------------------------------------------------------------- 启停
        public void Start()
        {
            try
            {
                this._disposed = false;
                lock (this._syncRoot)
                {
                    if (this._userWanted)
                    {
                        this.Log("Start ignored (already started)");
                        return;
                    }
                    this._userWanted = true;
                    this._reconnectDelayMs = ReconnectMinMs;
                    if (this._netThread == null)
                    {
                        this._netThread = new Thread(new ThreadStart(this.NetLoop));
                        this._netThread.IsBackground = true;
                        this._netThread.Name = "OneBotNetLoop";
                        this._netThread.Start();
                    }
                    if (this._sendThread == null)
                    {
                        this._sendThread = new Thread(new ThreadStart(this.SendLoop));
                        this._sendThread.IsBackground = true;
                        this._sendThread.Name = "OneBotSendLoop";
                        this._sendThread.Start();
                    }
                }
                this.Log("Start requested");
                this.RaiseStatus();
            }
            catch (Exception ex)
            {
                this.Log("Start error: " + Flatten(ex));
            }
        }

        public void Stop()
        {
            Thread netThread = null;
            Thread sendThread = null;
            bool wasWanted;
            try
            {
                lock (this._syncRoot)
                {
                    wasWanted = this._userWanted;
                    this._userWanted = false;
                    netThread = this._netThread;
                    sendThread = this._sendThread;
                }
                if (!wasWanted)
                {
                    this.Log("Stop ignored (not started)");
                    return;
                }
                this.Log("Stop requested");
                this.CloseSocketQuietly();
                this.SignalSend();
                try
                {
                    if (netThread != null && netThread != Thread.CurrentThread)
                    {
                        netThread.Join(3000);
                    }
                }
                catch (Exception)
                {
                }
                try
                {
                    if (sendThread != null && sendThread != Thread.CurrentThread)
                    {
                        sendThread.Join(2000);
                    }
                }
                catch (Exception)
                {
                }
                lock (this._syncRoot)
                {
                    if (this._netThread == netThread)
                    {
                        this._netThread = null;
                    }
                    if (this._sendThread == sendThread)
                    {
                        this._sendThread = null;
                    }
                }
                this._connected = false;
                this.RaiseStatus();
            }
            catch (Exception ex)
            {
                this.Log("Stop error: " + Flatten(ex));
            }
        }

        public void Dispose()
        {
            try
            {
                this.Stop();
                this._disposed = true;
                lock (this._fileServerLock)
                {
                    if (this._fileListener != null)
                    {
                        try
                        {
                            this._fileListener.Stop();
                        }
                        catch (Exception)
                        {
                        }
                        this._fileListener = null;
                    }
                }
                lock (this._queueLock)
                {
                    this._outQueue.Clear();
                }
                this.Log("Disposed");
            }
            catch (Exception ex)
            {
                this.Log("Dispose error: " + Flatten(ex));
            }
        }

        // -------------------------------------------------------------- 事件入口
        // 微灵在收到微信事件时调用；此处负责转译成 OneBot v11 事件并发往 OneBot 实现。
        // 返回 true 表示已成功投递给 OneBot v11 连接（调用方据此决定是否回落 HTTP 回调）。
        public bool TrySendEvent(string json)
        {
            if (string.IsNullOrEmpty(json))
            {
                return false;
            }
            bool sent = false;
            try
            {
                JObject evt = JObject.Parse(json);
                this.CaptureState(evt);
                string onebotJson = this.TranslateEvent(evt);
                if (onebotJson != null && this._connected)
                {
                    this.EnqueueOut(onebotJson);
                    sent = true;
                }
            }
            catch (Exception ex)
            {
                this.Log("TrySendEvent error: " + Flatten(ex));
            }
            return sent;
        }

        // -------------------------------------------------------------- 网络循环
        private void NetLoop()
        {
            this.Log("net loop started");
            while (this._userWanted)
            {
                string selfId = this.ResolveSelfId();
                if (string.IsNullOrEmpty(selfId))
                {
                    this.SleepInterruptible(1000);
                    continue;
                }
                ClientWebSocket ws = null;
                try
                {
                    string url = this.BuildUrl();
                    if (url == null)
                    {
                        this.SleepInterruptible(2000);
                        continue;
                    }
                    ws = new ClientWebSocket();
                    ws.Options.KeepAliveInterval = TimeSpan.FromSeconds(20);
                    string token = this.GetTokenSafe();
                    if (!string.IsNullOrEmpty(token))
                    {
                        try
                        {
                            ws.Options.SetRequestHeader("Authorization", "Bearer " + token);
                        }
                        catch (Exception ex)
                        {
                            this.Log("set Authorization header failed: " + ex.Message);
                        }
                    }
                    try
                    {
                        ws.Options.SetRequestHeader("X-Self-ID", selfId);
                        ws.Options.SetRequestHeader("X-Client-Role", "Universal");
                    }
                    catch (Exception ex)
                    {
                        this.Log("set X-* headers failed: " + ex.Message);
                    }
                    this.Log("connecting " + RedactUrl(url) + " (self_id=" + selfId + ", client=" + this._channel + ")");
                    Task connectTask = ws.ConnectAsync(new Uri(url), CancellationToken.None);
                    if (!connectTask.Wait(ConnectTimeoutMs))
                    {
                        throw new TimeoutException("connect timeout");
                    }
                    if (ws.State != WebSocketState.Open)
                    {
                        throw new Exception("websocket state " + ws.State);
                    }
                    this._socket = ws;
                    this._connected = true;
                    this._lastRecvUtc = DateTime.UtcNow;
                    this._lastHeartbeatUtc = DateTime.UtcNow;
                    this._reconnectDelayMs = ReconnectMinMs;
                    this.Log("connected: " + RedactUrl(url));
                    this.RaiseStatus();
                    this.SendLifecycle("connect");
                    this.ReceiveLoop(ws);
                }
                catch (Exception ex)
                {
                    this.Log("connection failed: " + Flatten(ex));
                }
                finally
                {
                    this._connected = false;
                    this._socket = null;
                    try
                    {
                        if (ws != null && (ws.State == WebSocketState.Open || ws.State == WebSocketState.CloseReceived))
                        {
                            Task closeTask = ws.CloseAsync(WebSocketCloseStatus.NormalClosure, "bye", CancellationToken.None);
                            closeTask.Wait(2000);
                        }
                    }
                    catch (Exception)
                    {
                    }
                    try
                    {
                        if (ws != null)
                        {
                            ws.Dispose();
                        }
                    }
                    catch (Exception)
                    {
                    }
                    this.RaiseStatus();
                }
                if (!this._userWanted)
                {
                    break;
                }
                int delay = this._reconnectDelayMs;
                this.Log("reconnect in " + delay + " ms");
                this.SleepInterruptible(delay);
                if (this._reconnectDelayMs < ReconnectMaxMs)
                {
                    this._reconnectDelayMs = this._reconnectDelayMs * 2;
                    if (this._reconnectDelayMs > ReconnectMaxMs)
                    {
                        this._reconnectDelayMs = ReconnectMaxMs;
                    }
                }
            }
            this.Log("net loop exited");
        }

        private void ReceiveLoop(ClientWebSocket ws)
        {
            byte[] buffer = new byte[65536];
            MemoryStream accumulator = new MemoryStream();
            while (this._userWanted && ws.State == WebSocketState.Open)
            {
                WebSocketReceiveResult result = null;
                try
                {
                    Task<WebSocketReceiveResult> receiveTask = ws.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);
                    // 不用固定超时中断：OneBot 实现通常在事件之外不主动发帧，
                    // 固定读取超时会造成无意义的周期性重连。改为轮询 socket 状态，
                    // 由 TCP/WebSocket 层（含 KeepAlive 20s 探测）负责发现链路断开。
                    while (!receiveTask.Wait(1000))
                    {
                        if (!this._userWanted || ws.State != WebSocketState.Open)
                        {
                            this.Log("receive aborted (socket closed)");
                            break;
                        }
                    }
                    if (!receiveTask.IsCompleted || receiveTask.IsFaulted || receiveTask.IsCanceled)
                    {
                        break;
                    }
                    result = receiveTask.Result;
                }
                catch (Exception ex)
                {
                    this.Log("receive error: " + ex.Message);
                    break;
                }
                this._lastRecvUtc = DateTime.UtcNow;
                if (result.MessageType == WebSocketMessageType.Close)
                {
                    this.Log("server closed connection: " + result.CloseStatus + " " + result.CloseStatusDescription);
                    break;
                }
                if (result.Count > 0)
                {
                    accumulator.Write(buffer, 0, result.Count);
                }
                if (!result.EndOfMessage)
                {
                    continue;
                }
                string text = Encoding.UTF8.GetString(accumulator.ToArray());
                accumulator.SetLength(0);
                this.HandleInbound(text);
            }
            accumulator.Dispose();
        }

        private void HandleInbound(string text)
        {
            if (string.IsNullOrEmpty(text))
            {
                return;
            }
            JObject request = null;
            try
            {
                request = JObject.Parse(text);
            }
            catch (Exception ex)
            {
                this.Log("inbound parse error: " + ex.Message + " raw=" + Short(text));
                return;
            }
            string action = PickString(request, "action");
            if (!string.IsNullOrEmpty(action))
            {
                this.HandleAction(request, action);
                return;
            }
            this.Log("inbound frame without action ignored: " + Short(text));
        }

        // ------------------------------------------------------------- 动作分发
        private void HandleAction(JObject request, string action)
        {
            JToken echo = request["echo"];
            JToken paramsToken = request["params"];
            JObject parameters = paramsToken as JObject;
            if (parameters == null && paramsToken == null)
            {
                parameters = new JObject();
            }
            JToken data = null;
            string status = "ok";
            int retcode = 0;
            string message = "";
            try
            {
                this.Log("<- action " + action);
                data = this.ExecuteAction(action, parameters);
                if (data == null)
                {
                    status = "failed";
                    retcode = 1404;
                    message = "unsupported action: " + action;
                }
            }
            catch (Exception ex)
            {
                status = "failed";
                retcode = 500;
                message = ex.Message;
                this.Log("action " + action + " failed: " + Flatten(ex));
            }
            JObject response = new JObject();
            response["status"] = new JValue(status);
            response["retcode"] = new JValue(retcode);
            response["msg"] = new JValue(message);
            response["data"] = data == null ? JValue.CreateNull() : data;
            if (echo != null)
            {
                response["echo"] = echo;
            }
            this.EnqueueOut(response.ToString(Formatting.None));
        }

        private JToken ExecuteAction(string action, JObject p)
        {
            if (p == null)
            {
                p = new JObject();
            }
            switch (action)
            {
                case "send_private_msg":
                    return this.SendMessage("private", p);
                case "send_group_msg":
                    return this.SendMessage("group", p);
                case "send_msg":
                    {
                        string messageType = PickString(p, "message_type");
                        if (messageType == "group")
                        {
                            return this.SendMessage("group", p);
                        }
                        return this.SendMessage("private", p);
                    }
                case "get_login_info":
                    return this.GetLoginInfo();
                case "get_status":
                    return this.GetStatus();
                case "get_version_info":
                    return this.GetVersionInfo();
                case "get_group_list":
                    return this.GetGroupListArray();
                case "get_friend_list":
                    return this.GetFriendListArray();
                case "get_group_info":
                    return this.GetGroupInfo(p);
                case "get_group_member_info":
                    return this.GetGroupMemberInfo(p);
                case "get_group_member_list":
                    return this.GetGroupMemberListArray(p);
                case "get_group_msg_history":
                    return new JArray();
                case "get_stranger_info":
                    return this.GetStrangerInfo(p);
                case "get_msg":
                    return this.GetMsg(p);
                case "can_send_image":
                    {
                        JObject d = new JObject();
                        d["yes"] = new JValue(true);
                        return d;
                    }
                case "can_send_record":
                    {
                        JObject d = new JObject();
                        d["yes"] = new JValue(true);
                        return d;
                    }
                case "get_image":
                case "get_record":
                case "get_file":
                    return this.GetMediaAction(action, p);
                case "get_group_file_url":
                case "get_private_file_url":
                    return this.GetFileUrl(p);
                case "set_group_name":
                case "set_group_card":
                case "set_group_ban":
                case "set_friend_add_request":
                case "set_group_add_request":
                    throw new Exception("action not supported by 微灵 OneBot shim: " + action);
                default:
                    return null;
            }
        }

        // ------------------------------------------------------------- 微灵适配
        private JObject GetLoginInfo()
        {
            JObject d = new JObject();
            d["user_id"] = IdToken(this._selfId);
            d["nickname"] = new JValue(this._nickname == null ? this.GetClientNameSafe() : this._nickname);
            return d;
        }

        private JObject GetStatus()
        {
            JObject d = new JObject();
            d["online"] = new JValue(this._connected && !string.IsNullOrEmpty(this._selfId));
            d["good"] = new JValue(true);
            return d;
        }

        private JObject GetVersionInfo()
        {
            JObject d = new JObject();
            d["app_name"] = new JValue("wecom-ling-onebot");
            d["app_version"] = new JValue("1.11.0");
            d["protocol_version"] = new JValue("v11");
            d["app_full_name"] = new JValue("微灵 OneBot v11 改造版");
            return d;
        }

        private JArray GetGroupListArray()
        {
            JArray list = new JArray();
            if (this.IsPersonalChannel())
            {
                return list;
            }
            try
            {
                JObject body = this.BuildQueryCommand(11038, new JObject());
                CommandExecResult result = this.InvokeCommand(body);
                JObject data = ResultData(result);
                JArray rooms = FindFirstArray(data);
                if (rooms != null)
                {
                    for (int i = 0; i < rooms.Count; i++)
                    {
                        JObject room = rooms[i] as JObject;
                        if (room == null)
                        {
                            continue;
                        }
                        string conversation = PickString(room, "room_conversation_id", "conversation_id");
                        string name = PickString(room, "room_name", "group_name", "nickname");
                        string id = StripConversationPrefix(conversation, "R:");
                        if (string.IsNullOrEmpty(id))
                        {
                            continue;
                        }
                        JObject item = new JObject();
                        item["group_id"] = IdToken(id);
                        item["group_name"] = new JValue(name == null ? "" : name);
                        list.Add(item);
                    }
                }
            }
            catch (Exception ex)
            {
                this.Log("get_group_list failed: " + Flatten(ex));
            }
            return list;
        }

        private JArray GetFriendListArray()
        {
            JArray list = new JArray();
            if (this.IsPersonalChannel())
            {
                return list;
            }
            try
            {
                JObject body = this.BuildQueryCommand(11036, new JObject());
                CommandExecResult result = this.InvokeCommand(body);
                JObject data = ResultData(result);
                JArray contacts = FindFirstArray(data);
                if (contacts != null)
                {
                    for (int i = 0; i < contacts.Count; i++)
                    {
                        JObject contact = contacts[i] as JObject;
                        if (contact == null)
                        {
                            continue;
                        }
                        string conversation = PickString(contact, "conversation_id");
                        string name = PickString(contact, "name", "nickname", "username", "remark");
                        string id = conversation != null ? this.ParsePeerFromPrivate(conversation) : PickString(contact, "user_id", "acctid");
                        if (string.IsNullOrEmpty(id))
                        {
                            continue;
                        }
                        JObject item = new JObject();
                        item["user_id"] = IdToken(id);
                        item["nickname"] = new JValue(name == null ? "" : name);
                        item["remark"] = new JValue("");
                        list.Add(item);
                    }
                }
            }
            catch (Exception ex)
            {
                this.Log("get_friend_list failed: " + Flatten(ex));
            }
            return list;
        }

        private JObject GetGroupInfo(JObject p)
        {
            string groupId = PickString(p, "group_id");
            JArray groups = this.GetGroupListArray();
            for (int i = 0; i < groups.Count; i++)
            {
                JObject item = groups[i] as JObject;
                if (item != null && ScalarEquals(item["group_id"], groupId))
                {
                    return (JObject)item.DeepClone();
                }
            }
            JObject d = new JObject();
            d["group_id"] = IdToken(groupId);
            d["group_name"] = new JValue("");
            return d;
        }

        private JObject GetGroupMemberInfo(JObject p)
        {
            string groupId = PickString(p, "group_id");
            string userId = PickString(p, "user_id");
            JObject d = new JObject();
            d["user_id"] = IdToken(userId);
            d["nickname"] = new JValue("");
            d["card"] = new JValue("");
            d["sex"] = new JValue("unknown");
            d["age"] = new JValue(0);
            d["role"] = new JValue("member");
            if (this.IsPersonalChannel() || string.IsNullOrEmpty(groupId))
            {
                return d;
            }
            try
            {
                JObject query = new JObject();
                query["conversation_id"] = new JValue("R:" + groupId);
                JObject body = this.BuildQueryCommand(11040, query);
                CommandExecResult result = this.InvokeCommand(body);
                JObject data = ResultData(result);
                JArray members = FindFirstArray(data);
                if (members != null)
                {
                    for (int i = 0; i < members.Count; i++)
                    {
                        JObject member = members[i] as JObject;
                        if (member == null)
                        {
                            continue;
                        }
                        string memberId = PickString(member, "user_id");
                        if (!ScalarEquals(memberId, userId))
                        {
                            continue;
                        }
                        d["nickname"] = new JValue(PickString(member, "name", "nickname", "group_nickname"));
                        d["card"] = new JValue(PickString(member, "group_nickname", "name"));
                        return d;
                    }
                }
            }
            catch (Exception ex)
            {
                this.Log("get_group_member_info failed: " + Flatten(ex));
            }
            return d;
        }

        private JArray GetGroupMemberListArray(JObject p)
        {
            JArray list = new JArray();
            string groupId = PickString(p, "group_id");
            if (this.IsPersonalChannel() || string.IsNullOrEmpty(groupId))
            {
                return list;
            }
            try
            {
                JObject query = new JObject();
                query["conversation_id"] = new JValue("R:" + groupId);
                JObject body = this.BuildQueryCommand(11040, query);
                CommandExecResult result = this.InvokeCommand(body);
                JObject data = ResultData(result);
                JArray members = FindFirstArray(data);
                if (members != null)
                {
                    for (int i = 0; i < members.Count; i++)
                    {
                        JObject member = members[i] as JObject;
                        if (member == null)
                        {
                            continue;
                        }
                        string memberId = PickString(member, "user_id");
                        if (string.IsNullOrEmpty(memberId))
                        {
                            continue;
                        }
                        JObject item = new JObject();
                        item["user_id"] = IdToken(memberId);
                        item["nickname"] = new JValue(PickString(member, "name", "nickname", "group_nickname"));
                        item["card"] = new JValue(PickString(member, "group_nickname", "name"));
                        item["sex"] = new JValue("unknown");
                        item["age"] = new JValue(0);
                        item["role"] = new JValue("member");
                        list.Add(item);
                    }
                }
            }
            catch (Exception ex)
            {
                this.Log("get_group_member_list failed: " + Flatten(ex));
            }
            return list;
        }

        private JObject GetStrangerInfo(JObject p)
        {
            string userId = PickString(p, "user_id");
            JObject d = new JObject();
            d["user_id"] = IdToken(userId);
            d["nickname"] = new JValue("");
            d["sex"] = new JValue("unknown");
            d["age"] = new JValue(0);
            if (this.IsPersonalChannel() || string.IsNullOrEmpty(userId))
            {
                return d;
            }
            try
            {
                JObject query = new JObject();
                query["user_id"] = IdToken(userId);
                JObject body = this.BuildQueryCommand(11039, query);
                CommandExecResult result = this.InvokeCommand(body);
                JObject data = ResultData(result);
                if (data != null)
                {
                    d["nickname"] = new JValue(PickString(data, "name", "nickname", "username"));
                }
            }
            catch (Exception ex)
            {
                this.Log("get_stranger_info failed: " + Flatten(ex));
            }
            return d;
        }

        private JObject GetMsg(JObject p)
        {
            string messageId = PickString(p, "message_id");
            if (!string.IsNullOrEmpty(messageId))
            {
                lock (this._cacheLock)
                {
                    string cached;
                    if (this._msgCache.TryGetValue(messageId, out cached))
                    {
                        try
                        {
                            return JObject.Parse(cached);
                        }
                        catch (Exception ex)
                        {
                            this.Log("get_msg cache parse failed: " + ex.Message);
                        }
                    }
                }
            }
            throw new Exception("message not found: " + messageId);
        }

        private JObject GetMediaAction(string action, JObject p)
        {
            string file = PickString(p, "file", "file_id");
            object[] entry = this.GetMediaEntry(file);
            byte[] bytes = entry == null ? null : (byte[])entry[0];
            string url = entry != null && entry.Length > 3 ? (string)entry[3] : null;
            if (bytes == null && string.IsNullOrEmpty(url) && !string.IsNullOrEmpty(file) &&
                (file.StartsWith("http://", StringComparison.OrdinalIgnoreCase) || file.StartsWith("https://", StringComparison.OrdinalIgnoreCase)))
            {
                url = file;
            }
            if (bytes == null && !string.IsNullOrEmpty(url))
            {
                bytes = this.DownloadMedia(url);
            }
            if (bytes == null)
            {
                throw new Exception("media not available: " + file);
            }
            JObject d = new JObject();
            d["file"] = new JValue("base64://" + Convert.ToBase64String(bytes));
            d["file_name"] = new JValue(entry != null && entry.Length > 1 && entry[1] != null ? (string)entry[1] : "file");
            return d;
        }

        private JObject GetFileUrl(JObject p)
        {
            string fileId = PickString(p, "file_id", "file");
            object[] entry = this.GetMediaEntry(fileId);
            if (entry == null)
            {
                throw new Exception("file url not available (需要消息内自带 cdn.url): " + fileId);
            }
            byte[] bytes = (byte[])entry[0];
            string url = entry.Length > 3 ? (string)entry[3] : null;
            JObject d = new JObject();
            if (bytes != null)
            {
                // 已缓存的字节由本地媒体服务提供（仅本机回环可用）。
                this.EnsureFileServer();
                d["url"] = new JValue("http://127.0.0.1:" + this._filePort.ToString(CultureInfo.InvariantCulture) + "/media/" + Uri.EscapeDataString(fileId == null ? "" : fileId));
            }
            else if (!string.IsNullOrEmpty(url))
            {
                d["url"] = new JValue(url);
            }
            else
            {
                throw new Exception("file url not available: " + fileId);
            }
            d["file_name"] = new JValue(entry.Length > 1 && entry[1] != null ? (string)entry[1] : "file");
            return d;
        }

        // ------------------------------------------------------------- 消息发送
        private JObject SendMessage(string kind, JObject p)
        {
            if (this.IsPersonalChannel())
            {
                return this.SendPersonalMessage(kind, p);
            }
            string text;
            List<string> atList = new List<string>();
            List<JObject> mediaList = new List<JObject>();
            this.ParseOutgoingSegments(p["message"], out text, atList, mediaList);
            if (string.IsNullOrEmpty(text) && atList.Count == 0 && mediaList.Count == 0)
            {
                throw new Exception("empty message");
            }
            string peer;
            if (kind == "group")
            {
                peer = PickString(p, "group_id");
                if (string.IsNullOrEmpty(peer))
                {
                    throw new Exception("missing group_id");
                }
            }
            else
            {
                peer = PickString(p, "user_id");
                if (string.IsNullOrEmpty(peer))
                {
                    throw new Exception("missing user_id");
                }
            }
            string conversationId = kind == "group" ? "R:" + peer : "S:" + peer + "_" + (this._selfId == null ? "" : this._selfId);
            string clientToken = this.LookupClient(conversationId);
            CommandExecResult lastResult = null;
            for (int i = 0; i < mediaList.Count; i++)
            {
                JObject media = mediaList[i];
                string mediaType = PickString(media, "type");
                JObject mediaData = media["data"] as JObject;
                string mediaPath = this.MaterializeOutgoingMedia(mediaType, mediaData);
                int mediaCommand;
                if (mediaType == "video")
                {
                    mediaCommand = 11067;
                }
                else if (mediaType == "file")
                {
                    mediaCommand = 11031;
                }
                else if (this.IsGifSegment(mediaType, mediaData, mediaPath))
                {
                    mediaCommand = 11070;
                }
                else
                {
                    mediaCommand = 11030;
                }
                JObject mediaPayload = new JObject();
                mediaPayload["conversation_id"] = new JValue(conversationId);
                mediaPayload["file"] = new JValue(mediaPath);
                string displayName = mediaData == null ? null : PickString(mediaData, "name", "file_name");
                if (mediaType == "file" && !string.IsNullOrEmpty(displayName))
                {
                    mediaPayload["file_name"] = new JValue(displayName);
                }
                JObject mediaBody = this.BuildCommand(mediaCommand, mediaPayload, clientToken);
                CommandExecResult mediaResult = this.InvokeCommand(mediaBody);
                if (mediaResult == null || !mediaResult.Ok)
                {
                    throw new Exception("发送媒体失败(" + mediaType + "): " + (mediaResult == null ? "微灵未返回结果（超时或未登录）" : (mediaResult.Message == null ? (mediaResult.Error == null ? "发送失败" : mediaResult.Error) : mediaResult.Message)));
                }
                lastResult = mediaResult;
            }
            if (!string.IsNullOrEmpty(text) || atList.Count > 0)
            {
                JObject data = new JObject();
                data["conversation_id"] = new JValue(conversationId);
                data["content"] = new JValue(text == null ? "" : text);
                int commandType;
                if (kind == "group" && atList.Count > 0)
                {
                    commandType = 11069;
                    JArray atArray = new JArray();
                    for (int i = 0; i < atList.Count; i++)
                    {
                        atArray.Add(IdToken(atList[i]));
                    }
                    data["at_list"] = atArray;
                }
                else
                {
                    commandType = 11029;
                }
                JObject body = this.BuildCommand(commandType, data, clientToken);
                CommandExecResult result = this.InvokeCommand(body);
                if (result == null || !result.Ok)
                {
                    throw new Exception(result == null ? "微灵未返回结果（超时或未登录）" : (result.Message == null ? (result.Error == null ? "发送失败" : result.Error) : result.Message));
                }
                lastResult = result;
            }
            JObject d = new JObject();
            d["message_id"] = this.ResultMessageId(lastResult);
            return d;
        }

        private JObject SendPersonalMessage(string kind, JObject p)
        {
            string text;
            List<string> atList = new List<string>();
            List<JObject> mediaList = new List<JObject>();
            this.ParseOutgoingSegments(p["message"], out text, atList, mediaList);
            if (mediaList.Count > 0)
            {
                throw new Exception("个人微信通道当前仅支持文本消息");
            }
            if (string.IsNullOrEmpty(text))
            {
                throw new Exception("个人微信通道当前仅支持文本消息");
            }
            string peer = kind == "group" ? PickString(p, "group_id") : PickString(p, "user_id");
            JObject data = new JObject();
            data["wxid"] = new JValue(peer == null ? "" : peer);
            data["msg"] = new JValue(text);
            JObject body = new JObject();
            body["action"] = new JValue("send_text_msg");
            body["client"] = IdToken(this.LookupClient(""));
            body["data"] = data;
            CommandExecResult result = this.InvokeCommand(body);
            if (result != null && result.Ok)
            {
                JObject d = new JObject();
                d["message_id"] = this.ResultMessageId(result);
                return d;
            }
            throw new Exception(result == null ? "微灵个人微信通道未响应" : (result.Message == null ? "发送失败" : result.Message));
        }

        private void ParseOutgoingSegments(JToken message, out string text, List<string> atList, List<JObject> mediaList)
        {
            text = "";
            if (message == null)
            {
                return;
            }
            if (message.Type == JTokenType.String)
            {
                text = (string)message;
                return;
            }
            JArray array = message as JArray;
            if (array != null)
            {
                for (int i = 0; i < array.Count; i++)
                {
                    this.AppendOutgoingSegment(array[i], ref text, atList, mediaList);
                }
                return;
            }
            this.AppendOutgoingSegment(message, ref text, atList, mediaList);
        }

        private void AppendOutgoingSegment(JToken segment, ref string text, List<string> atList, List<JObject> mediaList)
        {
            JObject obj = segment as JObject;
            if (obj == null)
            {
                if (segment.Type == JTokenType.String)
                {
                    text = text + (string)segment;
                }
                return;
            }
            string type = PickString(obj, "type");
            JObject data = obj["data"] as JObject;
            if (type == "text")
            {
                text = text + (data == null ? "" : PickString(data, "text"));
                return;
            }
            if (type == "at")
            {
                string qq = data == null ? null : PickString(data, "qq");
                // 微灵 11069 的 at_list 只接受真实 user_id；@全体 降级为纯文本发送。
                if (!string.IsNullOrEmpty(qq) && qq != "all")
                {
                    atList.Add(qq);
                }
                return;
            }
            if (type == "face")
            {
                return;
            }
            // AstrBot 回复默认带 reply 段；微灵没有引用回复能力，静默忽略以保留正文。
            if (type == "reply")
            {
                return;
            }
            if (type == "image" || type == "video" || type == "file")
            {
                if (mediaList != null)
                {
                    mediaList.Add(obj);
                }
                return;
            }
            if (type == "record")
            {
                throw new Exception("暂不支持发送语音：微灵未提供语音发送指令（可改用文本或文件）");
            }
            throw new Exception("当前版本不支持发送 " + type + " 消息段（支持：文本、@、图片、视频、文件、动图）");
        }

        // ----------------------------------------------------- 出站媒体落地（本机文件）
        private string MaterializeOutgoingMedia(string segmentType, JObject data)
        {
            string source = data == null ? null : PickString(data, "file", "path", "url");
            string name = data == null ? null : PickString(data, "name", "file_name");
            if (string.IsNullOrEmpty(source) && data != null)
            {
                source = PickString(data, "url");
            }
            if (string.IsNullOrEmpty(source))
            {
                throw new Exception("媒体消息段缺少 file/url");
            }
            byte[] bytes = null;
            if (source.StartsWith("base64://", StringComparison.OrdinalIgnoreCase))
            {
                try
                {
                    bytes = Convert.FromBase64String(source.Substring(9));
                }
                catch (Exception)
                {
                    throw new Exception("媒体 base64 内容解码失败");
                }
            }
            else if (source.StartsWith("http://", StringComparison.OrdinalIgnoreCase) || source.StartsWith("https://", StringComparison.OrdinalIgnoreCase))
            {
                bytes = this.DownloadMedia(source);
                if (bytes == null)
                {
                    throw new Exception("媒体下载失败: " + RedactUrl(source));
                }
                if (string.IsNullOrEmpty(name))
                {
                    name = GuessNameFromUrl(source);
                }
            }
            else if (source.StartsWith("file://", StringComparison.OrdinalIgnoreCase))
            {
                string local = source.Substring(7);
                if (local.StartsWith("/", StringComparison.Ordinal))
                {
                    local = local.Substring(1);
                }
                source = Uri.UnescapeDataString(local);
            }
            if (bytes == null)
            {
                string path = source;
                if (!Path.IsPathRooted(path))
                {
                    throw new Exception("媒体文件路径必须是绝对路径: " + path);
                }
                if (!File.Exists(path))
                {
                    throw new Exception("媒体文件不存在: " + path);
                }
                bytes = File.ReadAllBytes(path);
                if (string.IsNullOrEmpty(name))
                {
                    name = Path.GetFileName(path);
                }
            }
            if (bytes == null || bytes.Length == 0)
            {
                throw new Exception("媒体内容为空");
            }
            if (bytes.Length > MediaCacheLimitBytes)
            {
                throw new Exception("媒体内容超过 " + (MediaCacheLimitBytes / (1024 * 1024)).ToString(CultureInfo.InvariantCulture) + "MB 限制");
            }
            string extension = ChooseMediaExtension(segmentType, name, bytes);
            string directory = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "onebot_media_tmp");
            Directory.CreateDirectory(directory);
            string target = Path.Combine(directory, "media_" + DateTime.Now.ToString("yyyyMMddHHmmssfff", CultureInfo.InvariantCulture) + "_" + this.NextMessageIdLong().ToString(CultureInfo.InvariantCulture) + extension);
            File.WriteAllBytes(target, bytes);
            CleanupMediaTempDir(directory);
            return target;
        }

        private static string GuessNameFromUrl(string url)
        {
            try
            {
                int query = url.IndexOf('?');
                string clean = query >= 0 ? url.Substring(0, query) : url;
                return Path.GetFileName(clean);
            }
            catch (Exception)
            {
                return null;
            }
        }

        private static string ChooseMediaExtension(string segmentType, string name, byte[] bytes)
        {
            if (bytes != null && bytes.Length >= 12)
            {
                if (bytes[0] == 0x89 && bytes[1] == 0x50 && bytes[2] == 0x4E && bytes[3] == 0x47)
                {
                    return ".png";
                }
                if (bytes[0] == 0xFF && bytes[1] == 0xD8)
                {
                    return ".jpg";
                }
                if (bytes[0] == 0x47 && bytes[1] == 0x49 && bytes[2] == 0x46)
                {
                    return ".gif";
                }
                if (bytes[0] == 0x52 && bytes[1] == 0x49 && bytes[2] == 0x46 && bytes[3] == 0x46)
                {
                    return ".webp";
                }
                if (bytes[4] == 0x66 && bytes[5] == 0x74 && bytes[6] == 0x79 && bytes[7] == 0x70)
                {
                    return ".mp4";
                }
            }
            string extension = string.IsNullOrEmpty(name) ? null : Path.GetExtension(name);
            if (!string.IsNullOrEmpty(extension) && extension.Length <= 6)
            {
                return extension.ToLowerInvariant();
            }
            if (segmentType == "video")
            {
                return ".mp4";
            }
            if (segmentType == "file")
            {
                return ".bin";
            }
            return ".png";
        }

        private static void CleanupMediaTempDir(string directory)
        {
            try
            {
                string[] files = Directory.GetFiles(directory);
                DateTime cutoff = DateTime.Now.AddHours(-1);
                for (int i = 0; i < files.Length; i++)
                {
                    try
                    {
                        if (File.GetLastWriteTime(files[i]) < cutoff)
                        {
                            File.Delete(files[i]);
                        }
                    }
                    catch (Exception)
                    {
                    }
                }
            }
            catch (Exception)
            {
            }
        }

        private bool IsGifSegment(string segmentType, JObject data, string path)
        {
            if (segmentType != "image")
            {
                return false;
            }
            string declared = data == null ? null : PickString(data, "type", "sub_type");
            if (!string.IsNullOrEmpty(declared) && declared.IndexOf("gif", StringComparison.OrdinalIgnoreCase) >= 0)
            {
                return true;
            }
            if (!string.IsNullOrEmpty(path) && path.EndsWith(".gif", StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }
            return false;
        }

        // ------------------------------------------------------------- 事件翻译
        private string TranslateEvent(JObject evt)
        {
            if (this.IsPersonalChannel() && !IsIntegerToken(evt["type"]))
            {
                return this.BuildPersonalEvent(evt);
            }
            int type = PickInt(evt, "type", -1);
            JObject data = evt["data"] as JObject;
            if (data == null)
            {
                data = new JObject();
            }
            switch (type)
            {
                case 11041:
                case 11042:
                case 11043:
                case 11044:
                case 11045:
                case 11046:
                case 11047:
                case 11048:
                case 11049:
                case 11050:
                case 11066:
                case 11068:
                case 11124:
                case 11195:
                    return this.BuildMessageEvent(evt, type, data);
                case 11200:
                {
                    // 11200 里混有「已读上报」等非聊天消息，直接丢弃，避免污染 OneBot 消息流。
                    string unmappedType = PickString(data, "message_type", "message_type_name", "event_desc");
                    if (unmappedType == "read_receipt" || unmappedType == "已读上报")
                    {
                        this.Log("event type 11200 read_receipt ignored");
                        return null;
                    }
                    return this.BuildMessageEvent(evt, type, data);
                }
                case 11123:
                    return this.BuildRecallEvent(evt, data);
                case 11063:
                    return this.BuildFriendRequestEvent(evt, data);
                case 11072:
                case 11073:
                case 11074:
                case 11075:
                case 11076:
                case 11077:
                case 11078:
                case 11173:
                    return this.BuildNoticeEvent(evt, type, data);
                default:
                    if (!this._ignoredTypes.ContainsKey(type))
                    {
                        this._ignoredTypes[type] = true;
                        this.Log("event type " + type.ToString(CultureInfo.InvariantCulture) + " 不映射为 OneBot 事件（忽略）: " + Short(evt.ToString(Formatting.None)));
                    }
                    return null;
            }
        }

        private string BuildMessageEvent(JObject evt, int type, JObject data)
        {
            string selfId = this._selfId;
            bool isSelf = PickBool(data, "is_self", false);
            string senderId = PickString(data, "sender", "sender_id");
            if (isSelf || (!string.IsNullOrEmpty(selfId) && senderId == selfId))
            {
                return null;
            }
            string conversationId = PickString(data, "conversation_id");
            string kind;
            string peer;
            bool parsed = this.ParseConversation(conversationId, out kind, out peer);
            if (!parsed)
            {
                string conversationType = PickString(data, "conversation_type");
                if (conversationType == "group" || conversationType == "customer_service")
                {
                    kind = "group";
                }
                else
                {
                    kind = "private";
                }
                peer = senderId;
            }
            if (string.IsNullOrEmpty(peer))
            {
                peer = senderId;
            }
            string senderName = PickString(data, "sender_name", "name", "nickname");
            JArray segments = this.BuildSegments(type, data, kind);
            string messageId = PickString(data, "server_id", "message_id", "local_id");
            if (string.IsNullOrEmpty(messageId))
            {
                messageId = this.NextMessageIdString();
            }
            long sendTime = PickLong(data, "send_time", 0);
            if (sendTime > 100000000000L)
            {
                sendTime = sendTime / 1000L;
            }
            if (sendTime <= 0)
            {
                sendTime = NowUnix();
            }
            JObject result = new JObject();
            result["time"] = new JValue(sendTime);
            result["self_id"] = IdToken(selfId);
            result["post_type"] = new JValue("message");
            result["message_type"] = new JValue(kind);
            result["sub_type"] = new JValue(kind == "group" ? "normal" : "friend");
            result["message_id"] = IdToken(messageId);
            result["message"] = segments;
            result["raw_message"] = new JValue(SegmentsToPlainText(segments));
            result["font"] = new JValue(0);
            JObject sender = new JObject();
            sender["user_id"] = IdToken(senderId);
            sender["nickname"] = new JValue(senderName == null ? "" : senderName);
            sender["card"] = new JValue(senderName == null ? "" : senderName);
            sender["role"] = new JValue("member");
            result["sender"] = sender;
            if (kind == "group")
            {
                result["group_id"] = IdToken(peer);
            }
            else
            {
                result["user_id"] = IdToken(peer);
            }
            result["ling"] = BuildLingPayload(evt);
            string json = result.ToString(Formatting.None);
            this.CacheMessage(messageId, json);
            if (!string.IsNullOrEmpty(conversationId))
            {
                lock (this._cacheLock)
                {
                    string clientToken = this._lastClientToken;
                    this._convClients[conversationId] = clientToken;
                }
            }
            return json;
        }

        private JArray BuildSegments(int type, JObject data, string kind)
        {
            JArray segments = new JArray();
            if (type == 11041)
            {
                JArray atList = data["at_list"] as JArray;
                if (kind == "group" && atList != null)
                {
                    for (int i = 0; i < atList.Count; i++)
                    {
                        JToken item = atList[i];
                        string uid = null;
                        JObject itemObj = item as JObject;
                        if (itemObj != null)
                        {
                            uid = PickString(itemObj, "user_id", "id");
                        }
                        else if (item.Type == JTokenType.String)
                        {
                            uid = (string)item;
                        }
                        if (!string.IsNullOrEmpty(uid))
                        {
                            this.AddAtSegment(segments, uid);
                        }
                    }
                }
                string text = PickString(data, "content", "text");
                if (!string.IsNullOrEmpty(text))
                {
                    this.AddTextSegment(segments, text);
                }
                return segments;
            }
            if (type == 11200)
            {
                // 11200 = 微灵未映射类型的消息（5.0.13.6005 新增）；尽量按文本消息透传给 OneBot，
                // 避免未知消息被静默丢弃导致机器人看不到用户内容。
                JArray unmappedAtList = data["at_list"] as JArray;
                if (kind == "group" && unmappedAtList != null)
                {
                    for (int i = 0; i < unmappedAtList.Count; i++)
                    {
                        JToken item = unmappedAtList[i];
                        string uid = null;
                        JObject itemObj = item as JObject;
                        if (itemObj != null)
                        {
                            uid = PickString(itemObj, "user_id", "id");
                        }
                        else if (item.Type == JTokenType.String)
                        {
                            uid = (string)item;
                        }
                        if (!string.IsNullOrEmpty(uid))
                        {
                            this.AddAtSegment(segments, uid);
                        }
                    }
                }
                string unmappedText = PickString(data, "content", "text", "content_summary");
                if (string.IsNullOrEmpty(unmappedText))
                {
                    unmappedText = "[未映射消息] " + (PickString(data, "message_type_name", "message_type") == null ? "" : PickString(data, "message_type_name", "message_type"));
                }
                this.AddTextSegment(segments, unmappedText);
                return segments;
            }
            if (type == 11042)
            {
                JObject cdn = data["cdn"] as JObject;
                string url = cdn == null ? null : PickString(cdn, "url", "md_url", "ld_url");
                string name = cdn == null ? null : PickString(cdn, "md5", "file_name", "url");
                if (!string.IsNullOrEmpty(url))
                {
                    this.RegisterMedia(name, url, name);
                    this.AddMediaSegment(segments, "image", name, url);
                }
                else
                {
                    this.AddTextSegment(segments, "[图片]");
                }
                return segments;
            }
            if (type == 11043)
            {
                JObject cdn = data["cdn"] as JObject;
                string url = cdn == null ? null : PickString(cdn, "url");
                if (!string.IsNullOrEmpty(url))
                {
                    string videoName = PickString(cdn, "file_name", "md5");
                    this.RegisterMedia(videoName, url, videoName);
                    this.AddMediaSegment(segments, "video", videoName, url);
                }
                else
                {
                    this.AddTextSegment(segments, "[视频]");
                }
                return segments;
            }
            if (type == 11044)
            {
                JObject c2c = data["c2c_cdn"] as JObject;
                string fileIdValue = c2c == null ? null : PickString(c2c, "file_id");
                JObject seg = new JObject();
                seg["type"] = new JValue("record");
                JObject segData = new JObject();
                segData["file"] = new JValue(fileIdValue == null ? "" : fileIdValue);
                seg["data"] = segData;
                segments.Add(seg);
                return segments;
            }
            if (type == 11045)
            {
                JObject cdn = data["cdn"] as JObject;
                string url = cdn == null ? null : PickString(cdn, "url");
                string fileName = cdn == null ? null : PickString(cdn, "file_name");
                if (string.IsNullOrEmpty(fileName))
                {
                    fileName = "file";
                }
                JObject seg = new JObject();
                seg["type"] = new JValue("file");
                JObject segData = new JObject();
                // AstrBot 的 File 组件构造签名为 (name, file, url)，缺少 name 会抛错丢段。
                segData["name"] = new JValue(fileName);
                segData["file"] = new JValue(string.IsNullOrEmpty(url) ? fileName : url);
                if (!string.IsNullOrEmpty(url))
                {
                    segData["url"] = new JValue(url);
                    this.RegisterMedia(PickString(data, "server_id", "local_id"), url, fileName);
                }
                seg["data"] = segData;
                segments.Add(seg);
                return segments;
            }
            if (type == 11046)
            {
                JObject seg = new JObject();
                seg["type"] = new JValue("location");
                JObject segData = new JObject();
                segData["lat"] = new JValue(PickDouble(data, "latitude", "lat"));
                segData["lon"] = new JValue(PickDouble(data, "longitude", "lon", "lng"));
                string locationTitle = PickString(data, "title");
                segData["title"] = new JValue(locationTitle == null ? "" : locationTitle);
                string address = PickString(data, "address");
                segData["content"] = new JValue(address == null ? "" : address);
                seg["data"] = segData;
                segments.Add(seg);
                return segments;
            }
            if (type == 11047)
            {
                JObject seg = new JObject();
                seg["type"] = new JValue("share");
                JObject segData = new JObject();
                string linkUrl = PickString(data, "url");
                string linkTitle = PickString(data, "title");
                string linkDesc = PickString(data, "desc");
                segData["url"] = new JValue(linkUrl == null ? "" : linkUrl);
                segData["title"] = new JValue(linkTitle == null ? "" : linkTitle);
                segData["content"] = new JValue(linkDesc == null ? "" : linkDesc);
                string imageUrl = PickString(data, "image_url");
                if (!string.IsNullOrEmpty(imageUrl))
                {
                    segData["image"] = new JValue(imageUrl);
                }
                if (string.IsNullOrEmpty(linkUrl) && string.IsNullOrEmpty(linkTitle))
                {
                    this.AddTextSegment(segments, "[链接] " + (PickString(data, "content") == null ? "" : PickString(data, "content")));
                    return segments;
                }
                seg["data"] = segData;
                segments.Add(seg);
                return segments;
            }
            if (type == 11048)
            {
                string url = PickString(data, "url");
                if (!string.IsNullOrEmpty(url))
                {
                    string gifName = PickString(data, "name");
                    this.RegisterMedia(gifName, url, gifName);
                    this.AddMediaSegment(segments, "image", gifName, url);
                }
                else
                {
                    this.AddTextSegment(segments, "[动图]");
                }
                return segments;
            }
            if (type == 11049)
            {
                this.AddTextSegment(segments, "[红包] " + (PickString(data, "desc") == null ? "" : PickString(data, "desc")));
                return segments;
            }
            if (type == 11050)
            {
                string cardNickname = PickString(data, "nickname");
                string cardUserId = PickString(data, "user_id");
                long contactId;
                if (!string.IsNullOrEmpty(cardUserId) && long.TryParse(cardUserId, NumberStyles.Integer, CultureInfo.InvariantCulture, out contactId))
                {
                    JObject seg = new JObject();
                    seg["type"] = new JValue("contact");
                    JObject segData = new JObject();
                    segData["type"] = new JValue("qq");
                    segData["id"] = new JValue(contactId);
                    seg["data"] = segData;
                    segments.Add(seg);
                }
                else
                {
                    this.AddTextSegment(segments, "[名片] " + (cardNickname == null ? "" : cardNickname));
                }
                return segments;
            }
            if (type == 11066)
            {
                this.AddTextSegment(segments, "[小程序] " + (PickString(data, "title", "appname") == null ? "" : PickString(data, "title", "appname")));
                return segments;
            }
            if (type == 11068)
            {
                this.AddTextSegment(segments, "[图文] " + (PickString(data, "text_content") == null ? "" : PickString(data, "text_content")));
                return segments;
            }
            if (type == 11124)
            {
                this.AddTextSegment(segments, "[视频号] " + (PickString(data, "desc") == null ? "" : PickString(data, "desc")) + " " + (PickString(data, "url") == null ? "" : PickString(data, "url")));
                return segments;
            }
            if (type == 11195)
            {
                this.AddTextSegment(segments, "[视频号直播] " + (PickString(data, "desc") == null ? "" : PickString(data, "desc")));
                return segments;
            }
            this.AddTextSegment(segments, "[消息类型 " + type.ToString(CultureInfo.InvariantCulture) + "]");
            return segments;
        }

        private string BuildRecallEvent(JObject evt, JObject data)
        {
            string roomId = PickString(data, "room_id");
            JObject result = new JObject();
            result["time"] = new JValue(NowUnix());
            result["self_id"] = IdToken(this._selfId);
            result["post_type"] = new JValue("notice");
            bool group = !string.IsNullOrEmpty(roomId) && roomId != "0";
            result["notice_type"] = new JValue(group ? "group_recall" : "friend_recall");
            result["message_id"] = IdToken(PickString(data, "message_server_id"));
            result["operator_id"] = IdToken(PickString(data, "op_user_id"));
            if (group)
            {
                result["group_id"] = IdToken(roomId);
            }
            else
            {
                result["user_id"] = IdToken(PickString(data, "op_user_id"));
            }
            result["ling"] = BuildLingPayload(evt);
            return result.ToString(Formatting.None);
        }

        private string BuildFriendRequestEvent(JObject evt, JObject data)
        {
            JObject result = new JObject();
            result["time"] = new JValue(NowUnix());
            result["self_id"] = IdToken(this._selfId);
            result["post_type"] = new JValue("request");
            result["request_type"] = new JValue("friend");
            result["user_id"] = IdToken(PickString(data, "user_id"));
            result["comment"] = new JValue(PickString(data, "verify", "remark") == null ? "" : PickString(data, "verify", "remark"));
            string userId = PickString(data, "user_id");
            result["flag"] = new JValue("ling-friend-" + (userId == null ? "" : userId));
            result["ling"] = BuildLingPayload(evt);
            return result.ToString(Formatting.None);
        }

        private string BuildNoticeEvent(JObject evt, int type, JObject data)
        {
            JObject result = new JObject();
            result["time"] = new JValue(NowUnix());
            result["self_id"] = IdToken(this._selfId);
            result["post_type"] = new JValue("notice");
            string conversation = PickString(data, "room_conversation_id", "conversation_id");
            string groupId = StripConversationPrefix(conversation, "R:");
            JArray memberList = data["member_list"] as JArray;
            string memberId = null;
            if (memberList != null && memberList.Count > 0)
            {
                JObject first = memberList[0] as JObject;
                if (first != null)
                {
                    memberId = PickString(first, "user_id");
                }
            }
            string operatorId = PickString(data, "op_user_id");
            if (type == 11072 || type == 11074 || type == 11173)
            {
                result["notice_type"] = new JValue("group_increase");
                result["sub_type"] = new JValue("invite");
                result["group_id"] = IdToken(groupId);
                result["operator_id"] = IdToken(operatorId);
                result["user_id"] = IdToken(memberId == null ? this._selfId : memberId);
            }
            else if (type == 11073)
            {
                result["notice_type"] = new JValue("group_decrease");
                result["sub_type"] = new JValue(operatorId == memberId ? "leave" : "kick");
                result["group_id"] = IdToken(groupId);
                result["operator_id"] = IdToken(operatorId);
                result["user_id"] = IdToken(memberId);
            }
            else if (type == 11075)
            {
                result["notice_type"] = new JValue("group_decrease");
                result["sub_type"] = new JValue("leave");
                result["group_id"] = IdToken(groupId);
                result["operator_id"] = IdToken(this._selfId);
                result["user_id"] = IdToken(this._selfId);
            }
            else if (type == 11076)
            {
                result["notice_type"] = new JValue("friend_add");
                result["user_id"] = IdToken(PickString(data, "user_id"));
            }
            else if (type == 11077)
            {
                result["notice_type"] = new JValue("notify");
                result["sub_type"] = new JValue("friend_delete");
                result["user_id"] = IdToken(PickString(data, "user_id"));
            }
            else if (type == 11078)
            {
                result["notice_type"] = new JValue("notify");
                result["sub_type"] = new JValue("group_name_change");
                result["group_id"] = IdToken(groupId);
                result["group_name"] = new JValue(PickString(data, "room_name") == null ? "" : PickString(data, "room_name"));
            }
            else
            {
                return null;
            }
            result["ling"] = BuildLingPayload(evt);
            return result.ToString(Formatting.None);
        }

        private string BuildPersonalEvent(JObject evt)
        {
            JObject data = evt["data"] as JObject;
            if (data == null)
            {
                data = evt;
            }
            string eventType = PickString(data, "event_type", "even_type");
            string fromUser = PickString(data, "from_user", "from_wxid", "wxid", "sender");
            string roomId = PickString(data, "room_id", "chatroom", "room_wxid");
            string content = PickString(data, "content", "msg", "text", "message");
            if (!string.IsNullOrEmpty(eventType) || string.IsNullOrEmpty(content))
            {
                this.Log("personal event not mapped: " + Short(evt.ToString(Formatting.None)));
                return null;
            }
            bool group = !string.IsNullOrEmpty(roomId);
            JObject result = new JObject();
            result["time"] = new JValue(NowUnix());
            result["self_id"] = IdToken(this._selfId);
            result["post_type"] = new JValue("message");
            result["message_type"] = new JValue(group ? "group" : "private");
            result["sub_type"] = new JValue("friend");
            result["message_id"] = IdToken(PickString(data, "msg_id", "message_id", "MsgId"));
            JArray segments = new JArray();
            this.AddTextSegment(segments, content);
            result["message"] = segments;
            result["raw_message"] = new JValue(content);
            result["font"] = new JValue(0);
            JObject sender = new JObject();
            sender["user_id"] = IdToken(fromUser);
            sender["nickname"] = new JValue(PickString(data, "from_user_name", "nickname") == null ? "" : PickString(data, "from_user_name", "nickname"));
            sender["card"] = new JValue("");
            sender["role"] = new JValue("member");
            result["sender"] = sender;
            if (group)
            {
                result["group_id"] = IdToken(roomId);
            }
            else
            {
                result["user_id"] = IdToken(fromUser);
            }
            result["ling"] = BuildLingPayload(evt);
            return result.ToString(Formatting.None);
        }

        private static JToken BuildLingPayload(JObject evt)
        {
            try
            {
                JObject clone = (JObject)evt.DeepClone();
                JObject data = clone["data"] as JObject;
                if (data != null)
                {
                    data.Remove("payload_base64");
                }
                return clone;
            }
            catch (Exception)
            {
                return evt;
            }
        }

        // ------------------------------------------------------------- 媒体缓存
        private void RegisterMedia(string key, string url, string fileName)
        {
            // 只记录消息自带的 CDN URL，供 get_image/get_record/get_file/get_*_file_url 查询。
            // 不下载内容（保持低内存），需要内容时再按需下载并转 base64。
            if (string.IsNullOrEmpty(key) || string.IsNullOrEmpty(url))
            {
                return;
            }
            lock (this._mediaLock)
            {
                if (this._media.ContainsKey(key))
                {
                    return;
                }
                this._media[key] = new object[] { null, fileName == null ? "file" : fileName, "application/octet-stream", url };
                this._mediaOrder.Enqueue(key);
            }
        }

        private byte[] DownloadMedia(string url)
        {
            try
            {
                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
                request.Method = "GET";
                request.Timeout = 15000;
                request.ReadWriteTimeout = 15000;
                request.Proxy = null;
                using (HttpWebResponse response = (HttpWebResponse)request.GetResponse())
                using (Stream stream = response.GetResponseStream())
                {
                    MemoryStream buffer = new MemoryStream();
                    byte[] chunk = new byte[65536];
                    int read;
                    while ((read = stream.Read(chunk, 0, chunk.Length)) > 0)
                    {
                        buffer.Write(chunk, 0, read);
                        if (buffer.Length > MediaCacheLimitBytes)
                        {
                            this.Log("media too large, truncated: " + RedactUrl(url));
                            break;
                        }
                    }
                    return buffer.ToArray();
                }
            }
            catch (Exception ex)
            {
                this.Log("media download failed: " + RedactUrl(url) + " " + ex.Message);
                return null;
            }
        }

        private object[] GetMediaEntry(string key)
        {
            if (string.IsNullOrEmpty(key))
            {
                return null;
            }
            lock (this._mediaLock)
            {
                object[] entry;
                if (this._media.TryGetValue(key, out entry))
                {
                    return entry;
                }
            }
            return null;
        }

        private void CacheMedia(string key, byte[] bytes, string fileName, string contentType)
        {
            if (string.IsNullOrEmpty(key) || bytes == null)
            {
                return;
            }
            if (bytes.Length > MediaCacheLimitBytes)
            {
                return;
            }
            lock (this._mediaLock)
            {
                if (this._media.ContainsKey(key))
                {
                    return;
                }
                this._media[key] = new object[] { bytes, fileName == null ? "file" : fileName, contentType == null ? "application/octet-stream" : contentType, null };
                this._mediaOrder.Enqueue(key);
                this._mediaBytesTotal = this._mediaBytesTotal + bytes.Length;
                while (this._mediaBytesTotal > MediaCacheLimitBytes && this._mediaOrder.Count > 0)
                {
                    string oldest = this._mediaOrder.Dequeue();
                    object[] old;
                    if (this._media.TryGetValue(oldest, out old))
                    {
                        this._media.Remove(oldest);
                        this._mediaBytesTotal = this._mediaBytesTotal - ((byte[])old[0]).Length;
                    }
                }
            }
        }

        // ------------------------------------------------------------- 本地文件服务
        private void EnsureFileServer()
        {
            lock (this._fileServerLock)
            {
                if (this._fileListener != null)
                {
                    return;
                }
                Exception lastError = null;
                for (int port = FileServerPortMin; port <= FileServerPortMax; port++)
                {
                    try
                    {
                        TcpListener listener = new TcpListener(IPAddress.Loopback, port);
                        listener.Start();
                        this._fileListener = listener;
                        this._filePort = port;
                        this._fileThread = new Thread(new ThreadStart(this.FileServerLoop));
                        this._fileThread.IsBackground = true;
                        this._fileThread.Name = "OneBotFileServer";
                        this._fileThread.Start();
                        this.Log("local media server: http://127.0.0.1:" + port.ToString(CultureInfo.InvariantCulture) + "/media/");
                        return;
                    }
                    catch (Exception ex)
                    {
                        lastError = ex;
                    }
                }
                throw new Exception("无法启动本地媒体服务: " + (lastError == null ? "no port" : lastError.Message));
            }
        }

        private void FileServerLoop()
        {
            while (true)
            {
                TcpListener listener = this._fileListener;
                if (listener == null)
                {
                    break;
                }
                TcpClient client;
                try
                {
                    client = listener.AcceptTcpClient();
                }
                catch (Exception)
                {
                    break;
                }
                WaitCallback callback = new WaitCallback(this.HandleFileClient);
                ThreadPool.QueueUserWorkItem(callback, client);
            }
        }

        private void HandleFileClient(object state)
        {
            TcpClient client = state as TcpClient;
            if (client == null)
            {
                return;
            }
            try
            {
                using (client)
                {
                    NetworkStream stream = client.GetStream();
                    stream.ReadTimeout = 5000;
                    stream.WriteTimeout = 5000;
                    byte[] buffer = new byte[1024];
                    MemoryStream headerBuffer = new MemoryStream();
                    string headerText = null;
                    while (headerBuffer.Length < 16384)
                    {
                        int read = stream.Read(buffer, 0, buffer.Length);
                        if (read <= 0)
                        {
                            break;
                        }
                        headerBuffer.Write(buffer, 0, read);
                        string ascii = Encoding.ASCII.GetString(headerBuffer.ToArray());
                        if (ascii.IndexOf("\r\n\r\n", StringComparison.Ordinal) >= 0)
                        {
                            headerText = ascii;
                            break;
                        }
                    }
                    if (headerText == null)
                    {
                        return;
                    }
                    int lineEnd = headerText.IndexOf('\n');
                    string requestLine = (lineEnd > 0 ? headerText.Substring(0, lineEnd) : headerText).Trim();
                    string[] parts = requestLine.Split(' ');
                    if (parts.Length < 2)
                    {
                        return;
                    }
                    string path = parts[1];
                    int queryIndex = path.IndexOf('?');
                    if (queryIndex >= 0)
                    {
                        path = path.Substring(0, queryIndex);
                    }
                    string key = null;
                    if (path.StartsWith("/media/", StringComparison.Ordinal))
                    {
                        key = Uri.UnescapeDataString(path.Substring(7));
                    }
                    object[] entry = key == null ? null : this.GetMediaEntry(key);
                    if (entry == null)
                    {
                        WriteHttpResponse(stream, "404 Not Found", "text/plain", Encoding.UTF8.GetBytes("not found"));
                        return;
                    }
                    WriteHttpResponse(stream, "200 OK", (string)entry[2], (byte[])entry[0]);
                }
            }
            catch (Exception ex)
            {
                this.Log("file server request error: " + ex.Message);
            }
        }

        private static void WriteHttpResponse(NetworkStream stream, string status, string contentType, byte[] body)
        {
            string header = "HTTP/1.1 " + status + "\r\nContent-Type: " + contentType + "\r\nContent-Length: " + body.Length.ToString(CultureInfo.InvariantCulture) + "\r\nConnection: close\r\n\r\n";
            byte[] headerBytes = Encoding.ASCII.GetBytes(header);
            stream.Write(headerBytes, 0, headerBytes.Length);
            stream.Write(body, 0, body.Length);
            stream.Flush();
        }

        // ------------------------------------------------------------- 发送侧
        private void SendLoop()
        {
            this.Log("send loop started");
            while (this._userWanted)
            {
                this._sendSignal.WaitOne(500);
                this.FlushOutQueue();
                if (!this._connected || this._socket == null)
                {
                    continue;
                }
                DateTime now = DateTime.UtcNow;
                if ((now - this._lastHeartbeatUtc).TotalMilliseconds >= HeartbeatIntervalMs)
                {
                    this._lastHeartbeatUtc = now;
                    this.SendHeartbeat();
                }
                // 说明：这里不再使用「入站静默看门狗」。AstrBot 等 OneBot 实现在空闲时通常
                // 不会主动发送帧，120 秒静默不等于连接失效；若据此断连会导致周期性重连并丢事件。
                // 连接有效性交给 TCP/WebSocket 自身 KeepAlive（ClientWebSocket.KeepAliveInterval，
                // 见 NetLoop）与发送失败处理来发现。
            }
            this.Log("send loop exited");
        }

        private void FlushOutQueue()
        {
            if (!this._connected || this._socket == null)
            {
                return;
            }
            string[] batch = null;
            lock (this._queueLock)
            {
                if (this._outQueue.Count > 0)
                {
                    batch = this._outQueue.ToArray();
                    this._outQueue.Clear();
                }
            }
            if (batch == null)
            {
                return;
            }
            for (int i = 0; i < batch.Length; i++)
            {
                this.SendTextFrame(batch[i]);
            }
        }

        private void SendTextFrame(string text)
        {
            ClientWebSocket ws = this._socket;
            if (ws == null || !this._connected)
            {
                this.Log("send dropped (not connected): " + Short(text));
                return;
            }
            try
            {
                byte[] bytes = Encoding.UTF8.GetBytes(text);
                lock (this._sendLock)
                {
                    Task sendTask = ws.SendAsync(new ArraySegment<byte>(bytes), WebSocketMessageType.Text, true, CancellationToken.None);
                    if (!sendTask.Wait(10000))
                    {
                        this.Log("send timeout");
                        this.CloseSocketQuietly();
                        return;
                    }
                }
                this.Log("-> " + Short(text));
            }
            catch (Exception ex)
            {
                this.Log("send error: " + ex.Message);
                this.CloseSocketQuietly();
            }
        }

        private void EnqueueOut(string json)
        {
            if (string.IsNullOrEmpty(json))
            {
                return;
            }
            lock (this._queueLock)
            {
                if (this._outQueue.Count >= OutQueueLimit)
                {
                    this._outQueue.Dequeue();
                    this.Log("outbound queue overflow; dropped oldest frame");
                }
                this._outQueue.Enqueue(json);
            }
            this.SignalSend();
        }

        private void SignalSend()
        {
            try
            {
                this._sendSignal.Set();
            }
            catch (Exception)
            {
            }
        }

        private void SendHeartbeat()
        {
            JObject evt = new JObject();
            evt["time"] = new JValue(NowUnix());
            evt["self_id"] = IdToken(this._selfId);
            evt["post_type"] = new JValue("meta_event");
            evt["meta_event_type"] = new JValue("heartbeat");
            JObject status = new JObject();
            status["online"] = new JValue(true);
            status["good"] = new JValue(true);
            evt["status"] = status;
            evt["interval"] = new JValue(HeartbeatIntervalMs);
            this.EnqueueOut(evt.ToString(Formatting.None));
        }

        private void SendLifecycle(string subType)
        {
            JObject evt = new JObject();
            evt["time"] = new JValue(NowUnix());
            evt["self_id"] = IdToken(this._selfId);
            evt["post_type"] = new JValue("meta_event");
            evt["meta_event_type"] = new JValue("lifecycle");
            evt["sub_type"] = new JValue(subType);
            this.EnqueueOut(evt.ToString(Formatting.None));
            this.Log("lifecycle " + subType + " queued");
        }

        // ----------------------------------------------------------- 微灵命令桥
        private JObject BuildCommand(int type, JObject data)
        {
            return this.BuildCommand(type, data, this._lastClientToken);
        }

        private JObject BuildQueryCommand(int type, JObject data)
        {
            JObject body = this.BuildCommand(type, data);
            body["getData"] = new JValue(true);
            return body;
        }
        private JObject BuildCommand(int type, JObject data, string clientToken)
        {
            JObject body = new JObject();
            body["client"] = IdToken(clientToken);
            body["type"] = new JValue(type);
            body["data"] = data == null ? new JObject() : data;
            return body;
        }

        private CommandExecResult InvokeCommand(JObject body)
        {
            if (this._onCommand == null)
            {
                throw new Exception("微灵命令处理器未初始化");
            }
            try
            {
                Task<CommandExecResult> task = this._onCommand(body);
                if (task == null)
                {
                    this.Log("command returned null task: " + Short(body.ToString(Formatting.None)));
                    return null;
                }
                if (!task.Wait(ActionTimeoutMs))
                {
                    this.Log("command timeout: " + Short(body.ToString(Formatting.None)));
                    return null;
                }
                if (task.IsFaulted)
                {
                    this.Log("command faulted: " + Flatten(task.Exception));
                    return null;
                }
                if (task.IsCanceled)
                {
                    this.Log("command canceled");
                    return null;
                }
                return task.Result;
            }
            catch (Exception ex)
            {
                this.Log("command invoke error: " + Flatten(ex));
                return null;
            }
        }

        private static JObject ResultData(CommandExecResult result)
        {
            if (result == null || !result.Ok)
            {
                return null;
            }
            object data = result.Data;
            if (data == null)
            {
                return null;
            }
            JObject obj = data as JObject;
            if (obj != null)
            {
                return obj;
            }
            try
            {
                return JObject.FromObject(data);
            }
            catch (Exception)
            {
                return null;
            }
        }

        private JToken ResultMessageId(CommandExecResult result)
        {
            JObject data = ResultData(result);
            if (data != null)
            {
                string value = PickString(data, "server_id", "message_id", "local_id", "msg_id");
                if (!string.IsNullOrEmpty(value))
                {
                    return IdToken(value);
                }
            }
            return new JValue(this.NextMessageIdLong());
        }

        // ---------------------------------------------------------- 状态与识别
        private bool IsPersonalChannel()
        {
            return this._channel != null && this._channel.IndexOf("personal", StringComparison.OrdinalIgnoreCase) >= 0;
        }

        private string ResolveSelfId()
        {
            if (!string.IsNullOrEmpty(this._selfId))
            {
                return this._selfId;
            }
            // 测试/离线场景可通过环境变量关闭 HTTP 探测（默认开启）；
            // 避免测试机上有真实微灵实例在 8060 时被误取真实 self_id。
            if (Environment.GetEnvironmentVariable("ONEBOT_DISABLE_HTTP_SELFID_PROBE") == "1")
            {
                return null;
            }
            if (this._userWanted && (DateTime.UtcNow - this._lastSelfIdProbeUtc).TotalMilliseconds >= 15000)
            {
                this._lastSelfIdProbeUtc = DateTime.UtcNow;
                string probed = this.QuerySelfIdViaHttp();
                if (!string.IsNullOrEmpty(probed))
                {
                    this._selfId = probed;
                    this.Log("self_id resolved via HTTP: " + probed);
                    return probed;
                }
            }
            return null;
        }

        private string QuerySelfIdViaHttp()
        {
            if (this.IsPersonalChannel())
            {
                return null;
            }
            try
            {
                HttpWebRequest request = (HttpWebRequest)WebRequest.Create("http://127.0.0.1:8060/api/Client/ExecCommand");
                request.Method = "POST";
                request.ContentType = "application/json; charset=utf-8";
                request.Timeout = 2500;
                request.ReadWriteTimeout = 2500;
                request.Proxy = null;
                byte[] payload = Encoding.UTF8.GetBytes("{\"client\":1,\"type\":11035,\"getData\":true,\"data\":{}}");
                request.ContentLength = payload.Length;
                using (Stream stream = request.GetRequestStream())
                {
                    stream.Write(payload, 0, payload.Length);
                }
                using (HttpWebResponse response = (HttpWebResponse)request.GetResponse())
                using (Stream responseStream = response.GetResponseStream())
                using (StreamReader reader = new StreamReader(responseStream, Encoding.UTF8))
                {
                    string text = reader.ReadToEnd();
                    JObject json = JObject.Parse(text);
                    JObject data = json["data"] as JObject;
                    if (data != null)
                    {
                        string userId = PickString(data, "user_id", "account");
                        if (!string.IsNullOrEmpty(userId))
                        {
                            return userId;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                this.Log("HTTP self_id probe failed: " + ex.Message);
            }
            return null;
        }

        private string BuildUrl()
        {
            string raw = this.GetUrlSafe();
            if (string.IsNullOrEmpty(raw))
            {
                if (this.IsPersonalChannel())
                {
                    this.LogOnce("personal ws url 未配置，个人微信通道不会连接 OneBot");
                    return null;
                }
                raw = "ws://127.0.0.1:6200/ws";
                this.LogOnce("企微 WS 地址为空，使用默认 " + raw);
            }
            raw = raw.Trim();
            if (raw.StartsWith("http://", StringComparison.OrdinalIgnoreCase))
            {
                raw = "ws://" + raw.Substring(7);
            }
            else if (raw.StartsWith("https://", StringComparison.OrdinalIgnoreCase))
            {
                raw = "wss://" + raw.Substring(8);
            }
            else if (!raw.StartsWith("ws://", StringComparison.OrdinalIgnoreCase) && !raw.StartsWith("wss://", StringComparison.OrdinalIgnoreCase))
            {
                raw = "ws://" + raw;
            }
            Uri uri;
            if (!Uri.TryCreate(raw, UriKind.Absolute, out uri))
            {
                this.LogOnce("非法的 WS 地址: " + raw);
                return null;
            }
            string path = uri.AbsolutePath;
            if (string.IsNullOrEmpty(path) || path == "/")
            {
                path = "/ws";
            }
            string url = uri.Scheme + "://" + uri.Authority + path;
            if (!string.IsNullOrEmpty(uri.Query))
            {
                url = url + uri.Query;
            }
            return url;
        }

        private string GetUrlSafe()
        {
            try
            {
                return this._getUrl == null ? null : this._getUrl();
            }
            catch (Exception ex)
            {
                this.Log("getUrl callback failed: " + ex.Message);
                return null;
            }
        }

        private string GetTokenSafe()
        {
            try
            {
                return this._getToken == null ? null : this._getToken();
            }
            catch (Exception ex)
            {
                this.Log("getToken callback failed: " + ex.Message);
                return null;
            }
        }

        private string GetClientNameSafe()
        {
            try
            {
                if (this._clientName != null)
                {
                    return this._clientName;
                }
                this._clientName = this._getClientName == null ? "" : this._getClientName();
                return this._clientName;
            }
            catch (Exception)
            {
                return "";
            }
        }

        private void RaiseStatus()
        {
            Action handler = this.StatusChanged;
            if (handler == null)
            {
                return;
            }
            try
            {
                handler();
            }
            catch (Exception ex)
            {
                this.Log("StatusChanged handler error: " + Flatten(ex));
            }
        }

        private void CloseSocketQuietly()
        {
            ClientWebSocket ws = this._socket;
            this._connected = false;
            this._socket = null;
            if (ws == null)
            {
                return;
            }
            try
            {
                ws.Abort();
            }
            catch (Exception)
            {
            }
            try
            {
                ws.Dispose();
            }
            catch (Exception)
            {
            }
            this.Log("socket closed");
            this.SignalSend();
        }

        private void SleepInterruptible(int milliseconds)
        {
            int remaining = milliseconds;
            while (remaining > 0 && this._userWanted)
            {
                int slice = remaining > 200 ? 200 : remaining;
                Thread.Sleep(slice);
                remaining = remaining - slice;
            }
        }

        // ------------------------------------------------------------- 工具方法
        private void Log(string message)
        {
            string line = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff", CultureInfo.InvariantCulture) + " " + this._logPrefix + message;
            try
            {
                lock (this._logLock)
                {
                    string path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "onebot_v11.log");
                    try
                    {
                        if (File.Exists(path) && new FileInfo(path).Length > 5L * 1024L * 1024L)
                        {
                            string oldPath = path + ".1";
                            if (File.Exists(oldPath))
                            {
                                File.Delete(oldPath);
                            }
                            File.Move(path, oldPath);
                        }
                    }
                    catch (Exception)
                    {
                    }
                    File.AppendAllText(path, line + Environment.NewLine, Encoding.UTF8);
                }
            }
            catch (Exception)
            {
            }
        }

        private void LogOnce(string message)
        {
            if (this._ignoredTypes.ContainsKey(-999999))
            {
                return;
            }
            this._ignoredTypes[-999999] = true;
            this.Log(message);
        }

        private void CacheMessage(string messageId, string json)
        {
            if (string.IsNullOrEmpty(messageId))
            {
                return;
            }
            lock (this._cacheLock)
            {
                if (this._msgCache.ContainsKey(messageId))
                {
                    return;
                }
                this._msgCache[messageId] = json;
                this._msgCacheOrder.Enqueue(messageId);
                while (this._msgCacheOrder.Count > MessageCacheLimit)
                {
                    string oldest = this._msgCacheOrder.Dequeue();
                    this._msgCache.Remove(oldest);
                }
            }
        }

        private void CaptureState(JObject evt)
        {
            try
            {
                JObject data = evt["data"] as JObject;
                int type = PickInt(evt, "type", -1);
                int client = PickInt(evt, "client", 0);
                if (client != 0)
                {
                    this._lastClientToken = client.ToString(CultureInfo.InvariantCulture);
                }
                string robotId = PickString(evt, "robot_id");
                if (!string.IsNullOrEmpty(robotId))
                {
                    this._selfId = robotId;
                }
                if (data == null)
                {
                    return;
                }
                if (type == 11026 || type == 11179)
                {
                    string userId = PickString(data, "user_id");
                    if (!string.IsNullOrEmpty(userId))
                    {
                        this._selfId = userId;
                    }
                    string nickname = PickString(data, "username", "name");
                    if (!string.IsNullOrEmpty(nickname))
                    {
                        this._nickname = nickname;
                    }
                }
                else if (type == 11027)
                {
                    this.Log("logout event received; clearing self_id");
                    this._selfId = null;
                }
            }
            catch (Exception ex)
            {
                this.Log("CaptureState error: " + ex.Message);
            }
        }

        private bool ParseConversation(string conversationId, out string kind, out string peer)
        {
            kind = null;
            peer = null;
            if (string.IsNullOrEmpty(conversationId))
            {
                return false;
            }
            if (conversationId.StartsWith("R:", StringComparison.Ordinal))
            {
                kind = "group";
                peer = conversationId.Substring(2);
                return peer.Length > 0;
            }
            if (conversationId.StartsWith("S:", StringComparison.Ordinal))
            {
                kind = "private";
                string body = conversationId.Substring(2);
                int index = body.LastIndexOf('_');
                peer = index > 0 ? body.Substring(0, index) : body;
                return peer.Length > 0;
            }
            return false;
        }

        private string ParsePeerFromPrivate(string conversationId)
        {
            string kind;
            string peer;
            if (this.ParseConversation(conversationId, out kind, out peer) && kind == "private")
            {
                return peer;
            }
            return null;
        }

        private string LookupClient(string conversationId)
        {
            lock (this._cacheLock)
            {
                string token;
                if (!string.IsNullOrEmpty(conversationId) && this._convClients.TryGetValue(conversationId, out token) && !string.IsNullOrEmpty(token))
                {
                    return token;
                }
                return this._lastClientToken;
            }
        }

        private string NextMessageIdString()
        {
            return this.NextMessageIdLong().ToString(CultureInfo.InvariantCulture);
        }

        private long NextMessageIdLong()
        {
            lock (this._cacheLock)
            {
                this._nextMessageId = this._nextMessageId + 1;
                return this._nextMessageId;
            }
        }

        private void AddTextSegment(JArray segments, string text)
        {
            if (text == null)
            {
                return;
            }
            JObject segment = new JObject();
            segment["type"] = new JValue("text");
            JObject data = new JObject();
            data["text"] = new JValue(text);
            segment["data"] = data;
            segments.Add(segment);
        }

        private void AddAtSegment(JArray segments, string userId)
        {
            long parsed;
            if (!long.TryParse(userId, NumberStyles.Integer, CultureInfo.InvariantCulture, out parsed))
            {
                return;
            }
            JObject segment = new JObject();
            segment["type"] = new JValue("at");
            JObject data = new JObject();
            data["qq"] = new JValue(userId);
            segment["data"] = data;
            segments.Add(segment);
        }

        private void AddMediaSegment(JArray segments, string type, string file, string url)
        {
            JObject segment = new JObject();
            segment["type"] = new JValue(type);
            JObject data = new JObject();
            data["file"] = new JValue(file == null ? "" : file);
            if (!string.IsNullOrEmpty(url))
            {
                data["url"] = new JValue(url);
            }
            segment["data"] = data;
            segments.Add(segment);
        }

        private static JArray FindFirstArray(JObject data)
        {
            if (data == null)
            {
                return null;
            }
            IEnumerator<JToken> values = data.Values().GetEnumerator();
            while (values.MoveNext())
            {
                JArray array = values.Current as JArray;
                if (array != null)
                {
                    return array;
                }
            }
            return null;
        }

        private static string SegmentsToPlainText(JArray segments)
        {
            if (segments == null)
            {
                return "";
            }
            StringBuilder builder = new StringBuilder();
            for (int i = 0; i < segments.Count; i++)
            {
                JObject segment = segments[i] as JObject;
                if (segment == null)
                {
                    continue;
                }
                string type = PickString(segment, "type");
                JObject data = segment["data"] as JObject;
                if (type == "text")
                {
                    builder.Append(PickString(data, "text"));
                }
                else if (type == "at")
                {
                    builder.Append("@");
                    builder.Append(PickString(data, "qq"));
                }
            }
            return builder.ToString();
        }

        private static bool ScalarEquals(JToken token, string value)
        {
            if (token == null)
            {
                return string.IsNullOrEmpty(value);
            }
            string text = token.Type == JTokenType.String ? (string)token : token.ToString(Formatting.None);
            return text == value;
        }

        private static string StripConversationPrefix(string conversationId, string prefix)
        {
            if (string.IsNullOrEmpty(conversationId))
            {
                return null;
            }
            if (conversationId.StartsWith(prefix, StringComparison.Ordinal))
            {
                return conversationId.Substring(prefix.Length);
            }
            return conversationId;
        }

        private static bool IsIntegerToken(JToken token)
        {
            return token != null && (token.Type == JTokenType.Integer || token.Type == JTokenType.Float);
        }

        private static string PickString(JObject obj, string key)
        {
            if (obj == null)
            {
                return null;
            }
            JToken token = obj[key];
            if (token == null || token.Type == JTokenType.Null || token.Type == JTokenType.Undefined)
            {
                return null;
            }
            if (token.Type == JTokenType.String)
            {
                return (string)token;
            }
            return token.ToString(Formatting.None);
        }

        private static string PickString(JObject obj, string key1, string key2)
        {
            string value = PickString(obj, key1);
            if (!string.IsNullOrEmpty(value))
            {
                return value;
            }
            return PickString(obj, key2);
        }

        private static string PickString(JObject obj, string key1, string key2, string key3)
        {
            string value = PickString(obj, key1, key2);
            if (!string.IsNullOrEmpty(value))
            {
                return value;
            }
            return PickString(obj, key3);
        }

        private static string PickString(JObject obj, string key1, string key2, string key3, string key4)
        {
            string value = PickString(obj, key1, key2, key3);
            if (!string.IsNullOrEmpty(value))
            {
                return value;
            }
            return PickString(obj, key4);
        }

        private static int PickInt(JObject obj, string key, int defaultValue)
        {
            if (obj == null)
            {
                return defaultValue;
            }
            JToken token = obj[key];
            if (token == null)
            {
                return defaultValue;
            }
            if (token.Type == JTokenType.Integer)
            {
                try
                {
                    return (int)token;
                }
                catch (Exception)
                {
                    return defaultValue;
                }
            }
            int parsed;
            if (int.TryParse(token.ToString(), NumberStyles.Integer, CultureInfo.InvariantCulture, out parsed))
            {
                return parsed;
            }
            return defaultValue;
        }

        private static long PickLong(JObject obj, string key, long defaultValue)
        {
            if (obj == null)
            {
                return defaultValue;
            }
            JToken token = obj[key];
            if (token == null)
            {
                return defaultValue;
            }
            if (token.Type == JTokenType.Integer)
            {
                try
                {
                    return (long)token;
                }
                catch (Exception)
                {
                    return defaultValue;
                }
            }
            long parsed;
            if (long.TryParse(token.ToString(), NumberStyles.Integer, CultureInfo.InvariantCulture, out parsed))
            {
                return parsed;
            }
            return defaultValue;
        }

        private static bool PickBool(JObject obj, string key, bool defaultValue)
        {
            if (obj == null)
            {
                return defaultValue;
            }
            JToken token = obj[key];
            if (token == null)
            {
                return defaultValue;
            }
            if (token.Type == JTokenType.Boolean)
            {
                return (bool)token;
            }
            bool parsed;
            if (bool.TryParse(token.ToString(), out parsed))
            {
                return parsed;
            }
            return defaultValue;
        }

        private static double PickDouble(JObject obj, string key1, string key2)
        {
            if (obj == null)
            {
                return 0.0;
            }
            JToken token = obj[key1];
            if (token == null)
            {
                token = obj[key2];
            }
            if (token == null)
            {
                return 0.0;
            }
            if (token.Type == JTokenType.Float || token.Type == JTokenType.Integer)
            {
                try
                {
                    return (double)token;
                }
                catch (Exception)
                {
                    return 0.0;
                }
            }
            double parsed;
            if (double.TryParse(token.ToString(), NumberStyles.Float, CultureInfo.InvariantCulture, out parsed))
            {
                return parsed;
            }
            return 0.0;
        }

        private static double PickDouble(JObject obj, string key1, string key2, string key3)
        {
            if (obj == null)
            {
                return 0.0;
            }
            JToken token = obj[key1];
            if (token == null)
            {
                token = obj[key2];
            }
            if (token == null)
            {
                token = obj[key3];
            }
            if (token == null)
            {
                return 0.0;
            }
            if (token.Type == JTokenType.Float || token.Type == JTokenType.Integer)
            {
                try
                {
                    return (double)token;
                }
                catch (Exception)
                {
                    return 0.0;
                }
            }
            double parsed;
            if (double.TryParse(token.ToString(), NumberStyles.Float, CultureInfo.InvariantCulture, out parsed))
            {
                return parsed;
            }
            return 0.0;
        }

        private static JToken IdToken(string value)
        {
            if (string.IsNullOrEmpty(value))
            {
                return JValue.CreateNull();
            }
            long parsed;
            if (long.TryParse(value, NumberStyles.Integer, CultureInfo.InvariantCulture, out parsed))
            {
                return new JValue(parsed);
            }
            return new JValue(value);
        }

        private static long NowUnix()
        {
            DateTime epoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
            return (long)(DateTime.UtcNow - epoch).TotalSeconds;
        }

        private static string RedactUrl(string url)
        {
            if (string.IsNullOrEmpty(url))
            {
                return "";
            }
            int index = url.IndexOf("token=", StringComparison.OrdinalIgnoreCase);
            if (index < 0)
            {
                return url;
            }
            return url.Substring(0, index) + "token=***";
        }

        private static string Short(string text)
        {
            if (string.IsNullOrEmpty(text))
            {
                return "";
            }
            if (text.Length <= 300)
            {
                return text;
            }
            return text.Substring(0, 300) + "...";
        }

        private static string Flatten(Exception exception)
        {
            if (exception == null)
            {
                return "unknown error";
            }
            string message = exception.GetType().Name + ": " + exception.Message;
            Exception inner = exception.InnerException;
            while (inner != null)
            {
                message = message + " <- " + inner.GetType().Name + ": " + inner.Message;
                inner = inner.InnerException;
            }
            return message;
        }
    }
}
