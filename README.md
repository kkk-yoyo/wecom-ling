# 微灵 / WeComSever —— 企业微信 PC Hook 逆向工程与开源实现

本仓库是闭源工具「微灵」（内部程序集名 `WeComSever`）的**反向工程整理版**：反编译源码、协议文档、原生注入组件与 OneBot v11 适配层。

原工具通过注入企业微信 PC 客户端，实现以**本人账号身份**收发消息，并对外提供指令与事件通道。原作者已停止维护；本仓库用于技术研究与学习。

**支持的微灵 / 企业微信版本：**

| 微灵版本 | 企业微信版本 | 说明 |
| --- | --- | --- |
| v1.0.1.0（旧版） | 5.0.0.6008 x86 | 原始版本，反编译源码 + HTTP 桥接层（`bridge/`） |
| v1.11.0（改造版） | **5.0.13.6005 x86** | OneBot v11 改造工程（`onebot/`），内置反向 WS 客户端，无需外挂桥接 |

> ⚠️ **风险提示（务必阅读）**
>
> PC hook 属于“非官方客户端”，腾讯侧有检测：登录时可能弹出
> 「该账号涉嫌使用外挂、非官方客户端或模拟器，请进行身份验证后重新登录」，
> 并触发风控、封号或强制身份核验。**请仅使用测试账号做技术研究，不要用于生产/商业场景，账号损失自负。**
> 本项目与腾讯公司无关，也不代表任何合规使用许可。

---

## 仓库结构

```
wecom-ling/
├── src/WeComSever/              # ilspycmd 反编译源码（v1.0.1.0，.NET Framework 4.7.2 / WPF）
├── native/bin/                  # 原生注入组件（Loader.dll / Helper_5.0.0.6008.dll）
├── bridge/                      # OneBot v11 HTTP 桥接层（Python，适配 v1.0.1.0 旧版）
├── docs/                        # 协议文档、架构说明、原作者 API 文档
├── tools/                       # 反编译脚本
│
├── onebot/                      # ★ OneBot v11 改造工程（适配微灵 1.11.0）
│   ├── src/WxSocketClient.cs    #   OneBot v11 反向 WS 客户端 shim（C# 5，2600+ 行）
│   ├── tools/PatchOneBot.cs     #   IL 移植工具（dnlib 4.5.0，加法式策略 + PreserveAll）
│   ├── tools/build.ps1           #   一键编译 shim + 重打补丁
│   ├── tests/                    #   62 用例 Mock 测试（Python + C# TestHost）
│   ├── plugin_ai_approval/       #   AstrBot AI 审批插件（astrbot_plugin_ai_approval）
│   ├── analysis/                 #   1.11.0 逆向分析产物（反编译 .cs + 探针脚本）
│   ├── docs/                     #   对接文档生成脚本
│   └── REPORT.md                 #   改造报告（SHA256、原理、验证证据、已知限制）
│
├── dist/                        # 原始二进制说明
└── .gitignore
```

## 快速体验

### 方式一：微灵 1.11.0 + OneBot v11（推荐）

1. 安装企业微信 **5.0.13.6005 x86**。
2. 构建（或下载）改造版 `微灵-OneBotV11-1.11.0.exe`，放到**不受安全软件限制的目录**。
3. 同目录建 `config.ini`：

   ```ini
   Port=8060
   WeComVersion=5.0.13.6005
   WeComUseWebSocket=true
   WxWsAutoConnect=true
   WxWsUrl=ws://127.0.0.1:6200
   WxWsToken=your-token
   ```

4. 以管理员身份运行，登录企业微信，OneBot 实现端（AstrBot 等）显示「适配器已连接」即成功。
5. 详细部署见 [`onebot/REPORT.md`](onebot/REPORT.md) 与对接文档。

### 方式二：微灵 v1.0.1.0 + HTTP 桥接（旧版）

1. 安装企业微信 **5.0.0.6008 x86**（官方历史版本包，见 `docs/BUILD.md`）。
2. 把 `native/bin/Loader.dll`、`native/bin/Helper_5.0.0.6008.dll`、`dist/微灵.exe` 放到**同一个目录**。
3. 在该目录建 `config.ini`（参考 `examples/config.ini`）：

   ```ini
   Port=8060
   CallbackUrl=http://127.0.0.1:8899/wecom/callback
   TcpPort=9090
   ```

4. 以**管理员身份**运行 `微灵.exe`，会自动拉起企业微信并注入，日志窗口出现「接口就绪 11024」后用手机企业微信扫码登录。
5. 启动桥接层（需要 Python 3.10+ 与 `aiohttp`）：

   ```bash
   pip install aiohttp
   python bridge/ling_bridge.py --config bridge/config.json
   ```

   OneBot 服务端（如 AstrBot 的 aiocqhttp 反向 WS）监听 `127.0.0.1:6200`。

## 协议速查（微灵 v1.0.1.0 原生协议）

| 通道 | 地址 | 用途 |
| --- | --- | --- |
| HTTP | `POST http://127.0.0.1:8060/api/Client/ExecCommand` | 下发指令 |
| HTTP | `GET /api/Client/OpenWeCom`、`/InjectAllWeCom`、`/ReInitSocket` | 拉起/注入/重连 |
| WebSocket | `ws://127.0.0.1:8061/message/` | 下发指令 + 事件广播 |
| TCP | `127.0.0.1:9090` | 事件广播（备用） |
| 回调 | `config.ini` 的 `CallbackUrl` | 事件推送（HTTP POST） |

下发一条文本消息：

```json
POST /api/Client/ExecCommand
{"client": 1, "type": 11029,
 "data": {"conversation_id": "S:1688851111111111_1688855837869858", "content": "hi"}}
```

事件推送格式（`CallbackUrl` / TCP / WS 广播一致）：

```json
{"client": 1, "type": 11041,
 "data": {"conversation_id": "R:10855764690307218", "sender": "1688857075806028",
          "sender_name": "张三", "content": "hello", "at_list": [], "send_time": "1690000000"}}
```

完整指令/事件表见 [`docs/PROTOCOL.md`](docs/PROTOCOL.md)。

## OneBot v11 改造（微灵 1.11.0）

在原仓库（v1.0.1.0 反编译）基础上，新增了 **微灵 1.11.0 的 OneBot v11 协议改造工程**（`onebot/` 目录）。

### 做了什么

将微灵 1.11.0 自研 WebSocket 协议（`{id,action,payload}` + hello/ping/pong + WXFL 二进制分片）**替换为标准 OneBot v11 反向 WebSocket**，使微灵可以直接对接 AstrBot / NapCat / LLOneBot 等 OneBot v11 实现，**无需外挂桥接层**。

- 适配企业微信 **5.0.13.6005 x86**（1.11.0 版本绑定的客户端版本）。
- 原程序 exe 不修改（SHA256 前后一致），改造产物为独立 exe。
- 使用 dnlib IL 补丁（加法式策略 + `MetadataFlags.PreserveAll`），只替换 `WxWebSocketClient` 类的 10 个公开方法体，其余全部保留。
- 微灵其余功能（企微注入、HTTP 8060 API、事件回调、UI、个微 TCP 桥等）原样保留。

### 消息能力

| 方向 | 支持 |
| --- | --- |
| **入站** | 文本/@、图片、视频、动图、文件、位置、链接、名片、撤回通知、好友申请、群成员变动；11200 未映射消息兜底；已读上报过滤 |
| **出站** | 文本、@（群内）、图片/视频/文件/动图（base64://、http(s)://、file://、绝对路径四种来源）；reply/face 段静默忽略 |
| **查询** | get_login_info / get_status / get_version_info / get_group_list / get_friend_list / get_group_info / get_group_member_info / get_group_member_list / get_stranger_info / get_msg / can_send_image / can_send_record |

### 快速构建

```powershell
# 1. 编译 shim（需要 .NET Framework 4.x csc + Newtonsoft.Json 13.0.0）
cd onebot\tools
.\build.ps1

# 2. 输出：onebot\output\微灵-OneBotV11-1.11.0.exe
```

### 验证

- Mock 测试 **62/62 PASS**
- 真实 AstrBot 4.29 联调通过（反向 WS 握手 + 事件双向 + 动作应答）
- 改造报告见 [`onebot/REPORT.md`](onebot/REPORT.md)

### AI 审批插件

附带 AstrBot 插件 `astrbot_plugin_ai_approval`（`onebot/plugin_ai_approval/`）：员工在群/私聊提交请假/报销/外出申请，插件按规则自动通过或转管理员提醒，JSONL 审计日志。

---

## 构建

当前 `src/WeComSever/` 是**反编译产物**：`.cs` 完整可读，但 WPF 的 XAML 已被编译成 BAML，反编译工具无法直接还原，因此不能开箱即编译（浏览器构建见 [`docs/BUILD.md`](docs/BUILD.md)）。
替代方案：直接使用 `dist/微灵.exe`（原始二进制）配合 `bridge/`。

## 授权与致谢

- 原始程序「微灵」/ `WeComSever` 版权归原作者所有。原作者已停止维护并以二进制形式提供本套资料；本仓库对其进行逆向整理。
- **若原作者对公开本项目有异议，请提交 issue，我们会立即移除相关内容。**
- 本仓库新增的文档、桥接层代码以 MIT 协议开源；反编译源码与原生二进制在作者授权前仅作研究参考，不构成再分发授权。
- 致谢：原作者；ilspycmd / ICSharpCode.Decompiler；OneBot v11 与 AstrBot 社区。
