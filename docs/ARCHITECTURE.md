# 架构说明（ARCHITECTURE）

## 1. 组成

```
微灵.exe (WeComSever.exe, .NET Framework 4.7.2 WPF, x86)
│
├─ GUI (WPF)                    WeComSever.MainWindow / CleanerSettingsWindow
│    ├─ 企业微信页：HTTP 服务 / TCP 服务 / 客户端列表 / 回调地址
│    ├─ 内网穿透页：内置 nps 客户端（npc.exe）
│    └─ 工具页
│
├─ HTTP API 8060                Robot.Config.WebServer + WebApiConfig + WebApiFactory
│    └─ WebServerConsole.Controller.ClientController   (OpenWeCom/InjectAllWeCom/ReInitSocket/ExecCommand/…)
│
├─ WebSocket 8061               Robot.Config.WebSocketServer  (HttpListener, 固定前缀 http://*:8061/message/)
│
├─ TCP 9090                     WeComSever.Network.TcpServer
│
├─ 原生桥                      WeComConsoleServer.Core.WeComHelper
│    ├─ LoadLibrary("Loader.dll") + GetProcAddress(基址+偏移)
│    └─ 把指令 JSON 交给 ServerSendMessage，回调经 NotifyStaticMessage 回流
│
├─ 配置/工具                  Robot.Utils.ConfigManager / Logger、Robot.Common.R / MessageTypeMapper / CustomException
│
├─ 嵌入式组件                  Costura.Fody 打包（Newtonsoft.Json、System.Web.Http(.SelfHost)、Spectre.Console…）
│    ├─ WeComSever.nps.npc.exe  （nps 内网穿透客户端，内嵌资源）
│    └─ beego.exe               （Go 辅助组件，内嵌资源）
│
└─ 定时清理                    WeComSever.Utils.CronHelper（默认每天 03:00 清 temp 目录）
```

## 2. 启动流程

```
MainWindow 构造
  → 创建日志控制台窗口（WinApi.CreateLogConsole("微灵 运行日志")）
  → 读取 config.ini（ConfigManager）
  → 检测管理员权限（UAC 提权）
  → 禁用企业微信自动更新
  → WeComHelper 静态构造：
        LoadLibrary("Loader.dll")
        InitSocket(回调地址)
        遍历 WXWork.exe 进程注入（InjectWeChatPid）
  → 启动 HTTP 服务（8060，WebApiFactory → SelfHost）
  → 启动 WebSocket（8061，HttpListener）
  → 启动 TCP（9090）
  → 主线程进入 GUI 消息循环
```

运行期事件流：

```
WXWork 进程 (Helper_5.0.0.6008.dll)
   └─(socket 回调)→ WeComHelper.NotifyStaticMessage(json)
        ├─→ TcpServer.BroadcastAsync(json)            # 9090
        ├─→ WebSocketServer.SendMessageToAllClients   # 8061
        └─→ POST CallbackUrl                          # config.ini
```

指令流：

```
bridge / 用户
   ├─(HTTP POST /api/Client/ExecCommand)→ ClientController.ExecCommand
   │        → WeComHelper.SendMessage(client, json)       # 异步
   │        → WeComHelper.SendMessageSyncWithLock(...)    # getData=true，最长 60s
   ├─(WS 8061 文本帧)→ WebSocketServer → WeComHelper.SendMessage
   └─(TCP 9090 行 JSON)→ TcpServer     → WeComHelper.SendMessage
```

## 3. 关键类

| 类 | 位置 | 职责 |
| --- | --- | --- |
| `MainWindow` | `WeComSever/MainWindow.cs` | GUI 与全局状态：客户端列表、登录信息、服务启停、注入、定时器 |
| `WeComClient` | `WeComSever.Models/WeComClient.cs` | 一个被注入的企微实例：`ClientId`、`WeChatId`、`IsLoggedIn`、`Pid` 等 |
| `ClientController` | `WebServerConsole.Controller/ClientController.cs` | HTTP API 入口（Web API 控制器） |
| `WebSocketServer` | `Robot.Config/WebSocketServer.cs` | 8061 服务与广播 |
| `TcpServer` | `WeComSever.Network/TcpServer.cs` | 9090 服务与广播 |
| `WebServer` / `WebApiFactory` / `WebApiConfig` | `Robot.Config/*` | SelfHost 启动、端口绑定 `http://0.0.0.0:{Port}/`、路由 |
| `WeComHelper` | `WeComConsoleServer.Core/WeComHelper.cs` | 原生 DLL 桥、登录事件处理、同步/异步发送、下载 pending |
| `ConfigManager` | `Robot.Utils/ConfigManager.cs` | config.ini 读写 |
| `CronHelper` / `CronPreset` | `WeComSever.Utils/*` | 定时清理（默认 `0 3 * * *`） |
| `R<T>` | `Robot.Common/R.cs` | API 统一返回 `{code,msg,data,time}` |
| `AssemblyLoader` | `Costura/AssemblyLoader.cs` | 启动时解压内嵌依赖程序集 |

## 4. config.ini

程序目录下 `config.ini`（GBK/ANSI，键名大小写不敏感）：

```ini
# WeComServer配置文件
Port=8060            ; HTTP API 端口
CallbackUrl=         ; 事件回调 HTTP 地址（可空；只接受 http:// 或 https://）
TcpPort=9090         ; TCP 事件端口（0 = 关闭）
NpcServer=           ; nps 服务器地址（内网穿透，可空）
NpcVkey=             ; nps 客户端验证密钥
CleanerFolderPath=   ; 定时清理目录
CleanerCron=0 3 * * *; 清理计划
CleanerEnabled=1     ; 是否启用清理
CleanerLastCleanTime=; 上次清理时间（程序写入）
```

改端口后需重启微灵；改 `CallbackUrl` 在 GUI「企业微信」页保存即可即时生效。

## 5. 客户端实例与多开

- 每个 `WXWork.exe` 进程注入成功后注册一个 `WeComClient`，`ClientId` 从 1 递增。
- GUI 客户端列表显示 微信ID/在线状态；右键可刷新授权/复制信息。
- 「启动新实例」= `Loader.dll` 的 `MultiOpen`（偏移 20096），用于同一台机器开多个企微账号。
- 同一 `client` 的所有指令都路由到该实例的注入 socket，因此 bridge 里必须先记住 `client` 编号（登录事件里带）。

## 6. 已知限制

- 只支持企业微信 **5.0.0.6008 x86**；其它版本注入即崩或 hook 不生效。
- 登录态依赖 PC 客户端在线；手机端退出会导致 11027 并清空登录信息。
- 事件推送没有重试/去重，bridge 端要自行容错。
- 反编译源码中的 XAML 资源是 BAML，当前不能直接编译出 GUI（见 BUILD.md）。
