# 微灵 / WeComSever —— 企业微信 PC Hook 逆向工程与开源实现

本仓库是闭源工具「微灵」（内部程序集名 `WeComSever`）的**反向工程整理版**：反编译源码、协议文档、原生注入组件与 OneBot v11 适配层。

原工具通过注入企业微信 PC 客户端（**仅适配企业微信 5.0.0.6008 x86**），实现以**本人账号身份**收发消息，并对外提供 HTTP / WebSocket / TCP 三套指令与事件通道。原作者已停止维护；本仓库用于技术研究与学习。

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
├── src/WeComSever/          # ilspycmd 反编译的完整源码（.NET Framework 4.7.2 / WPF）
├── native/
│   ├── bin/Loader.dll       # 注入器（x86 原生）
│   ├── bin/Helper_5.0.0.6008.dll  # hook 核心（版本绑定 5.0.0.6008）
│   └── README.md            # 原生组件接口说明
├── dist/微灵.exe            # 原始二进制（v1.0.1.0，未修改），用于对照/直接运行
├── bridge/                  # OneBot v11 适配器（微灵 ⇄ AstrBot 等）
├── docs/
│   ├── PROTOCOL.md          # 指令与事件协议完整说明
│   ├── ARCHITECTURE.md      # 架构与启动流程
│   ├── NATIVE_DLLS.md       # Loader.dll / Helper dll 接口
│   ├── BUILD.md             # 构建说明（含已知问题）
│   └── original/            # 原作者提供的 API 文档（docx/txt）
├── examples/config.ini      # 微灵配置样例
└── tools/decompile.ps1      # 用 ilspycmd 复现反编译
```

## 快速体验

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

## 协议速查

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

## 构建

当前 `src/WeComSever/` 是**反编译产物**：`.cs` 完整可读，但 WPF 的 XAML 已被编译成 BAML，反编译工具无法直接还原，因此不能开箱即编译（浏览器构建见 [`docs/BUILD.md`](docs/BUILD.md)）。
替代方案：直接使用 `dist/微灵.exe`（原始二进制）配合 `bridge/`。

## 授权与致谢

- 原始程序「微灵」/ `WeComSever` 版权归原作者所有。原作者已停止维护并以二进制形式提供本套资料；本仓库对其进行逆向整理。
- **若原作者对公开本项目有异议，请提交 issue，我们会立即移除相关内容。**
- 本仓库新增的文档、桥接层代码以 MIT 协议开源；反编译源码与原生二进制在作者授权前仅作研究参考，不构成再分发授权。
- 致谢：原作者；ilspycmd / ICSharpCode.Decompiler；OneBot v11 与 AstrBot 社区。
