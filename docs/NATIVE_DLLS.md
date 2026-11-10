# 原生组件说明（NATIVE_DLLS）

微灵的 hook 能力全部在包内两个 x86 原生 DLL 中，程序集的 `WeComConsoleServer.Core.WeComHelper` 只是 P/Invoke 封装。

## 1. 文件

| 文件 | 大小 | 角色 | SHA256 |
| --- | --- | --- | --- |
| `Loader.dll` | 5,771,776 B | 注入器 / socket 宿主。主程序 `LoadLibrary` 后按**固定偏移**取函数指针 | `CE4557BF449FD53078F2EAA9BECBF43B6F2BF40F16199E1A4BD6088AB233A65A` |
| `Helper_5.0.0.6008.dll` | 10,254,848 B | 注入到 `WXWork.exe` 的 hook 核心，版本绑定企业微信 5.0.0.6008 | `038E16A76F6F29E4BB41A811585F027D5506AA1A50E3CD67381A38DCFDB58438` |

两者均未签名；`Loader.dll` 不是通过导出表调用的，而是 `LoadLibrary` + `base + offset`，因此**替换/升级 Loader.dll 会直接破坏兼容性**。

## 2. Loader.dll 函数偏移

`WeComHelper.cs` 中的实际调用（`IntPtr.Add(ModuleHandle, offset)`）：

| 偏移 | 封装方法 | 原生签名（C# 委托） | 说明 |
| --- | --- | --- | --- |
| `19216` | `InitializeWeChatSocket` | `bool InitWeChatSocket(AcceptCb, RecvCb, CloseCb)` | 初始化 socket 回调。`AcceptCb(int dwClientId)` / `RecvCb(int dwClientId, string message, int dwSize)` / `CloseCb(int dwClientId)` |
| `20096` | `OpenWechatMutexTwo` | `void InjectWeChatMultiOpen(string dllPath, string weChatExePath)` | 多开：以 `Helper_5.0.0.6008.dll` + 企微 exe 启动一个新实例 |
| `20688` | `InjectWeChatPid` | `void InjectWeChatPid(int pid, string dllPath)` | 向指定 `WXWork.exe` 进程注入 helper dll |
| `22528` | `SendMessage` / `SendMessageSyncWithLock` / `SendMessageSyncByPath` | `bool WxSendMessage(int client, string message)` | 把 JSON 指令交给对应 client 的 hook；`false` 表示发出失败 |

行为要点：

- `client` 编号来自 socket 的 `AcceptCb(dwClientId)`，每注入一个企微实例 +1（从 1 开始）。
- 指令字符串在传入前会被 `EncodeNonAsciiForDll` 转义：所有非 ASCII 字符变成 `\uXXXX`（原生侧按转义 JSON 解析）。
- 回调 `RecvCb` 可能被同步请求消费（`getData=true`）；只有未被消费的事件才广播到 TCP/WS/回调。
- `CloseCb` 表示该 client 的 socket 断开（企微退出/被杀）。

## 3. Helper_5.0.0.6008.dll

- 由 `Loader.dll` 注入到 `WXWork.exe` 进程内，完成消息收发、登录态、联系人/群操作等全部 hook 逻辑。
- **版本强绑定**：只匹配企业微信 5.0.0.6008 的特征。换其它客户端版本会注入失败、崩溃或 hook 无效（我们实测 4.0.0.6024 / 5.0.3.6005 / 5.0.12.6018 / 5.0.13.6005 均不可用）。
- 主程序通过 `AppDomain.CurrentDomain.BaseDirectory + "Helper_5.0.0.6008.dll"` 定位文件，因此该 DLL 必须与 `微灵.exe`（或编译产物）**放在同一目录**。
- 事件以 JSON 字符串从 DLL socket 回到主程序，格式见 `docs/PROTOCOL.md`。

## 4. 内嵌资源

`微灵.exe` 用 Costura.Fody 把以下托管依赖压缩进资源（启动时 `Costura.AssemblyLoader` 解压并挂到 `AssemblyResolve`）：

- `Newtonsoft.Json`、`Newtonsoft.Json.Bson`
- `System.Web.Http`、`System.Web.Http.SelfHost`、`System.Net.Http.Formatting`
- `Spectre.Console`（含多语言资源）、`System.Buffers`、`System.Memory`、`System.Numerics.Vectors`、`System.Runtime.CompilerServices.Unsafe`、`System.Threading.Tasks.Extensions`
- 另有两个直接内嵌的 exe：`WeComSever.nps.npc.exe`（nps 内网穿透客户端）、`beego.exe`（Go 辅助组件）

反编译产物里保留了这些 `costura.*.compressed` 资源与 `mainwindow.baml` / `cleanersettingswindow.baml`，可用于重建工程（见 `docs/BUILD.md`）。

## 5. 重打包注意

- 若你要基于反编译源码重新编译 `WeComSever.exe`，必须把 `Loader.dll` / `Helper_5.0.0.6008.dll` 复制到输出目录；否则 `WeComHelper` 静态构造会 `LoadLibrary` 失败。
- 输出必须是 **x86（Prefer32Bit）**，因为原生 DLL 是 32 位。
- 以管理员权限运行，注入需要相应进程权限。
- 本项目不对原生二进制做任何修改；如需支持其它企微版本，需要自行分析对应版本的 Helper dll（属独立逆向工作）。
