# 微灵 1.11.0 主程序 → OneBot v11 客户端（shim）替换补丁说明

- 执行人：patch-engineer（team lead: lead）
- 任务：team task-3
- 日期：2026-11-12（13:25 生成最终 exe）
- 工作目录：`D:\wecom-ling`

## 1. 输入 / 输出

| 项 | 路径 | 大小 | SHA256 |
|---|---|---|---|
| 原程序（只读，未修改） | `D:\wecom-ling\微灵1.11.0_免卡密.exe` | 51,472,896 B | `4F2954A7149B318B4A69F7D407302EAED67A2911F27084C289DD35BF0F5BA24A` |
| 原程序备份 | `D:\wecom-ling\backup\微灵1.11.0_免卡密.orig.exe` | 51,472,896 B | 同上（未动） |
| shim 输入（最终版） | `D:\wecom-ling\onebot\output\OneBotShim.dll` | 47,104 B | `CFAE60EAF91947D0176D096B28994776FC97B33CD141C649FE058C110DA2FE93`（mtime 13:21:15） |
| **改造产物** | `D:\wecom-ling\onebot\output\微灵-OneBotV11-1.11.0.exe` | **51,417,600 B** | **`55EBAF294092476813ADF416D8121D0707EE8C44631EC42C680AA426F2EC9861`**（mtime 13:25:30） |

- 程序集身份未变：`WeComSever, Version=1.11.0.0, Culture=neutral, PublicKeyToken=null`。
- 输出比原程序小 55,296 字节：全部 4 个内嵌资源名称/长度逐一致（见 §5），差异来自删除 80 个旧方法与 8 个旧嵌套类型后的元数据/方法体布局变化，不是资源丢失。
- 原程序在本工具运行前后各校验一次 SHA256，均与任务给定值一致；原文件与 backup 未写入。

## 2. 环境与依赖

- Windows（PowerShell 7 驱动脚本；补丁工具本身为 .NET Framework 4.x 控制台程序）。
- 编译器：`C:\Windows\Microsoft.NET\Framework\v4.0.30319\csc.exe`（C#5）。
- IL 库：**dnlib 4.5.0.0（net45 版，PublicKeyToken=50e96378b6e77999）**
  - 来源：`D:\wecom-ling\onebot\lib\dnlib\lib\net45\dnlib.dll`
  - build.ps1 会自动复制到工具目录 `D:\wecom-ling\onebot\tools\dnlib.dll`（运行时必须与 PatchOneBot.exe 同目录）。
  - 注：先前尝试的 Mono.Cecil 0.11.6 无法忠实重写该混淆程序集（纯 round-trip 产物无法加载），已废弃，工具以 dnlib 版为准。

## 3. 实现要点（`PatchOneBot.cs`，dnlib）

1. 读取原 exe 与 shim（`ModuleDefMD.Load`），递归建立目标模块/ shim 模块的 FullName → TypeDef 索引。
2. 写前预检：扫描全部存活类型，确认没有其它类型引用将被删除的成员/嵌套类型（本程序集结果：528 个类型，问题引用 0）。
3. 目标类型 `WeComConsoleServer.Core.WxWebSocketClient`：
   - 保留 10 个原有 MethodDef（**只替换方法体**，保持属性/事件访问器与 `IDisposable` 接线不变）：`.ctor`、`Start`、`Stop`、`TrySendEvent`、`Dispose`、`add_StatusChanged`、`remove_StatusChanged`、`get_IsConnected`、`get_StatusText`、`get_StatusBackground`；
   - 保留字段 `StatusChanged`（System.Action）与 3 个只读属性定义、事件 `StatusChanged` 定义、嵌套类型 `CommandHandler`（含原 .ctor/Invoke/BeginInvoke/EndInvoke 与混淆器附加的 `.cctor`，签名原样保留）；
   - 删除其余 80 个方法（含 `.cctor`）、19 个旧字段、8 个旧嵌套类型（`PendingSend`、`<>c__DisplayClass33_0`、`<>c__DisplayClass39_0`、`<ConnectOnce>d__36`、`<ExecuteCommand>d__40`、`<ReadUntilClose>d__38`、`<ReceiveLoop>d__35`、`<SendFile>d__42`）。
4. 按 shim `OneBotShim.WxSocketClient` 重建：
   - 字段 53 个（`StatusChanged` 复用目标原字段；shim 其余 51 个新增，其中 11 个 literal const 带 Constant；shim 顶层 `<PrivateImplementationDetails>{319771B8-...}` 的静态字段 `$$method0x600000f-1 : Dictionary<string,int>` 迁移到目标类型上）；
   - 方法 97 个（10 个保留方法按 名称+签名 匹配，87 个新建 MethodDef + 参数定义）；方法与字段的名称/签名/访问修饰/参数名与 shim 一致，方法体整体从 shim 克隆（共 8,978 条 IL 指令）。
5. 操作数重映射：
   - 类型：shim TypeDef/TypeRef → 目标类型（`WxSocketClient`→目标类型）；`DefinitionAssembly == WeComSever` 的 TypeRef → 目标模块同名 TypeDef；其它外部类型 → `Importer`；递归处理 `GenericInstSig/SZArraySig/ArraySig/ByRefSig/PtrSig/PinnedSig/CModReqdSig/CModOptSig/GenericVar/GenericMVar` 等。
   - 成员引用：shim 内部方法/字段 → 目标成员；`WeComSever` 内部成员按 名称+签名 映射；外部 MemberRef/MethodSpec 用目标模块签名重建（共 995 个外来成员引用走外部重建路径，均为 mscorlib/System/Newtonsoft 成员）。
   - 方法体：两阶段克隆（先建空操作数指令，再填操作数）；分支/switch 用旧→新指令字典；局部变量按 Index、参数按 Parameter.Index 映射；异常处理器四端点 + CatchType 重映射；MaxStack/InitLocals 复制。
6. **垃圾 IL 归一化**：原混淆 exe 中有 3,752 条 `call` 指令的元数据 token 指向不存在的表（原始字节为故意注入的无效 ASCII），分布在 3,752 个方法（`<Module>` 的 .cctor 及 `<>f__AnonymousTypeN` 的成员）中，每处都位于 `br.s` 之后、为不可达死代码。经可达性分析（含 EH 边界检查）确认 **可达 0、异常处理边界 0** 后，原地替换为 `nop`（指令数不变）。不归一化则 dnlib 严格写出会报 `Instruction operand is null`。
7. 写出：`new ModuleWriterOptions(module)`，显式使用严格 logger `DummyLogger.ThrowModuleWriterExceptionOnErrorInstance`；先写临时文件再覆盖目标。
8. 回读校验（工具第 10 节）全部通过后才打印 `PATCH_OK`。

## 4. 复现命令

```powershell
# 1) 编译（并运行）补丁工具：把 dnlib.dll 复制到 tools，再 csc 编译，Push-Location tools 运行
powershell -ExecutionPolicy Bypass -File D:\wecom-ling\onebot\tools\build.ps1

# 仅编译：
powershell -ExecutionPolicy Bypass -File D:\wecom-ling\onebot\tools\build.ps1 -NoRun

# 等价的底层命令：
#   Copy-Item D:\wecom-ling\onebot\lib\dnlib\lib\net45\dnlib.dll D:\wecom-ling\onebot\tools\dnlib.dll -Force
#   & 'C:\Windows\Microsoft.NET\Framework\v4.0.30319\csc.exe' /nologo /target:exe /out:D:\wecom-ling\onebot\tools\PatchOneBot.exe `
#       /r:'C:\Windows\Microsoft.NET\Framework\v4.0.30319\netstandard.dll' /r:D:\wecom-ling\onebot\tools\dnlib.dll `
#       D:\wecom-ling\onebot\tools\PatchOneBot.cs

# 2) 单独运行（必须在 tools 目录，dnlib.dll 同目录），本次完整日志见 patch_run.log
cd D:\wecom-ling\onebot\tools
.\PatchOneBot.exe 2>&1 | Tee-Object -FilePath patch_run.log

# 3) 32 位进程加载校验（日志见 check_output_32.log）
C:\Windows\SysWOW64\WindowsPowerShell\v1.0\powershell.exe -NoProfile -ExecutionPolicy Bypass `
    -File D:\wecom-ling\onebot\tools\check_output_32.ps1

# 4) 哈希复核
Get-FileHash 'D:\wecom-ling\微灵1.11.0_免卡密.exe','D:\wecom-ling\onebot\output\微灵-OneBotV11-1.11.0.exe' -Algorithm SHA256
```

## 5. 校验结果（逐项）

### 5.1 原程序完整性（PASS）
- 运行前：`4F2954A7149B318B4A69F7D407302EAED67A2911F27084C289DD35BF0F5BA24A`，与任务给定值一致。
- 运行后再次校验：同值（原 exe 未被修改）；backup 目录原备份未被触碰。

### 5.2 输出程序集（dnlib 回读，全部 PASS）
- 可加载：`WeComSever, Version=1.11.0.0, Culture=neutral, PublicKeyToken=null`（程序集名/版本/公钥令牌未变）。
- `typeDefs(all)=529`（原 537 − 删除的 8 个嵌套类型）。
- 4 个 EmbeddedResource 名称 + 长度完全一致：
  - `h4ujvQyHvlSaXGW2mq.hq2XF22b2u8xpqkKGN` = 256
  - `5kmEuTbVfrZIGsLpQ5.YN6uKuxR0C0Vj4TRmA` = 49,442,954
  - `cSDKENemDwhxmHBtcK.B5B03efYDED1hvX8TY` = 4,552
  - `Vddcg0PBc9s1Y40Ht5.F0ANIOEy9fAwW7eIKk` = 50,232
- 目标类型：methods=97（10 保留 + 87 新建）、fields=53、props=3（IsConnected/StatusText/StatusBackground）、events=1（StatusChanged）、nested=1（CommandHandler）、ifaces=1（System.IDisposable）。
- 97 个方法的名称/签名/方法体指令数与 shim 逐一一致（含 10 个保留方法）。
- 模块共 5,042 个方法，RVA 非 0 的 4,204 个均有指令；无 null 操作数指令。
- 无 `OneBotShim` 程序集引用/类型引用残留。
- `CommandHandler` 成员数与签名与原程序一致（5 个，含原委托 .cctor）。

### 5.3 32 位进程加载校验（PASS）
命令见 §4-3，日志 `check_output_32.log`：

```
LOADED dep: Newtonsoft.Json, Version=13.0.0.0, ...        (从 D:\wecom-ling\onebot\lib\Newtonsoft.Json.dll 预加载依赖)
LoadFile REFUSED (environment zone policy; 原 exe 同样被拒): NotSupportedException
LOADED(Load(byte[])): WeComSever, Version=1.11.0.0, Culture=neutral, PublicKeyToken=null
TYPE: WeComConsoleServer.Core.WxWebSocketClient public=True sealed=True ifaces=1 (IDisposable)
methods=96 ctors=1 typeInit=no => dnlib methods=97
fields=53 props=3 events=1
nested=1 (CommandHandler)
CommandHandler methods=3 ctors=2 typeInit=yes => dnlib 5
props: IsConnected,StatusBackground,StatusText
events: StatusChanged
CHECK_STRUCT=PASS
JIT target-methods ok=97 fail=0 (total=97)
CHECK_32BIT=PASS
```

- 说明：`[Reflection.Assembly]::LoadFile(path)` 对本机**原 exe 与输出 exe 同样**返回 `NotSupportedException: ... loadFromRemoteSources ...`（本机 zone 安全策略，非产物问题，已用原 exe 作对照复现）。因此改用 `[Reflection.Assembly]::Load([IO.File]::ReadAllBytes(path))`（不丢程序集内容，加载的是同一 PE 镜像，且不受 zone 限制）。GetType 成功、成员全部可列出。
- 追加验证：对目标类型全部 96 方法 + 1 构造器逐一执行 `RuntimeHelpers.PrepareMethod`（强制 JIT 解码全部 IL 并解析 token）：**97/97 成功，0 失败**——说明目标类型方法体 IL 合法、可 JIT。

### 5.4 shim 字符串证据（补充，PASS）
在输出 exe 的 UTF-16 用户字符串堆中检出 shim 专属字符串，而原 exe 中均不存在：

| 字符串 | 原 exe | 输出 exe |
|---|---|---|
| `send_private_msg` | 未找到 | FOUND @0x17FB78D |
| `OneBot` | 未找到 | FOUND @0x17FB469 |
| `get_login_info` | 未找到 | FOUND @0x17FB7B6 |
| `send_msg` | 未找到 | FOUND @0x17FB7AD |
| `OneBot v11` | 未找到 | FOUND @0x17FB9B6 |

用于证明 shim 方法体（而非仅类型表面）确实进入了输出程序集。

## 6. 未验证项 / 风险

1. **未启动 GUI、未连接真实 AstrBot/微信**：仅完成“加载 + 反射 + JIT”级验证；实际 WebSocket 连接、OneBot v11 HTTP/WS 协议行为、事件分发、文件发送等端到端功能需要在真实环境实测（本次任务明确不启动 GUI）。
2. **3,752 条不可达垃圾 IL 被替换为 nop**：这些异常 token 为混淆器注入的反编译保护；已证明其不可达且非异常处理边界，替换不改变可达语义。但若未来有代码通过反射/动态方式（如 `RuntimeHelpers.PrepareMethod` 或运行 `＜Module＞::.cctor` 的特定分支）走到这些位置，行为与原始 exe 不同（原 exe 这些位置若被执行本身也是非法 IL）。
3. **995 个外部 MemberRef 由工具重建**（mscorlib / System / Newtonsoft.Json 13.0.0.0 的 ctor、Delegate.Combine/Remove、Interlocked.CompareExchange<T> 等）：按名称+签名重建，语义等价；未逐一做运行期调用验证。
4. 32 位校验用 `Assembly.Load(byte[])` 替代 `LoadFile`（后者被本机 zone 策略拒绝，原 exe 同样被拒）；未验证 `ReflectionOnlyLoadFrom` 路径。
5. 输出 exe 尚未做数字签名（与原程序一致，无强签名）。
6. 若 shim 源码/二进制再次更新，需要重跑 `PatchOneBot.exe` 重新生成输出（工具会重新读取当前 `onebot\output\OneBotShim.dll`）。

## 7. 工作区改动范围

- 只写：`D:\wecom-ling\onebot\tools\`（工具、探针、日志、本说明）与 `D:\wecom-ling\onebot\output\`（输出 exe；shim 由 lead/其他人产出，未修改）。
- 未修改：原 exe、`D:\wecom-ling\backup\`、`config.ini`、`onebot\src\`、`onebot\tests\`。

## 8. 工具目录文件清单（关键）

| 文件 | 说明 |
|---|---|
| `PatchOneBot.cs` / `PatchOneBot.exe` | 移植工具源码/可执行（dnlib 4.5.0） |
| `build.ps1` | 复制 dnlib + csc 编译 + 运行（`-NoRun` 仅编译） |
| `patch_run.log` | 最终一次运行的完整输出（136 行，PATCH_OK） |
| `check_output_32.ps1` / `check_output_32.log` | 32 位 PowerShell 加载 + 反射 + JIT 校验脚本/日志 |
| `dnlib.dll` | dnlib 4.5.0.0（net45），运行时必须与 exe 同目录 |
| `Dump*.cs`、`BodyProbe.cs`、`ForceWrite.cs`、`ProbeRaw.cs`、`ProbeReach.cs`、`CHProbe.cs`、`Reflect*.cs`、`DnProbe.cs`、`RoundTrip.cs`、`CecilErr.cs`、`ResProbe.cs` 等 | 探查/取证探针（只读用途；Mono.Cecil 阶段留下的参考工具，最终补丁不使用） |

---

## 9. 加法式策略（最终方案，2026-11-12 14:09 起）

> **本文件第 1–5 节描述的是早期「删除式 + Cecil 往返」方案，其中 97/97 JIT、输出 51,417,600 B、SHA256 55EBAF29… 等结论已被本轮迭代取代，仅作过程记录。**

### 9.1 最终产物
| 项目 | 值 |
|---|---|
| 输出 exe | `D:\wecom-ling\onebot\output\微灵-OneBotV11-1.11.0.exe`（51,488,768 B） |
| SHA256 | `66B6143C21000C0A0CA5A3F62DFFE465EA7D1A3941635362E487D16146A5D648` |
| 根目录交付副本 | `D:\wecom-ling\微灵-OneBotV11-1.11.0.exe`（同 SHA256） |
| 输入 shim | `onebot\output\OneBotShim.dll`（47,104 B，SHA256 `CFAE60EA…FE93`） |
| 原 exe（未改） | SHA256 `4F2954A7…5BA24A`（51,472,896 B） |

### 9.2 策略变更
- **不再删除**任何原类型/字段/方法/嵌套类型（删除式会因 `PendingSend` 等 TypeDef 仍被原 TypeSpec 引用而写失败，且破坏混淆器依赖的布局）。
- 只替换 `WeComConsoleServer.Core.WxWebSocketClient` 的 10 个公开成员方法体（`.ctor` / `Start` / `Stop` / `TrySendEvent` / `Dispose` / `add_StatusChanged` / `remove_StatusChanged` / `get_IsConnected` / `get_StatusText` / `get_StatusBackground`），事件字段 `StatusChanged` 复用。
- shim 其余 87 个方法、52 个字段以 `ob_` 前缀**追加**；原类型保留 90 个旧方法、20 个旧字段。
- writer 必须：`MetadataFlags.PreserveAll` + `DummyLogger.NoThrowInstance`。

### 9.3 为什么必须 PreserveAll（关键教训）
dnlib 默认重写会改变 #Strings/#US/#Blob 堆偏移与签名尾数据；微灵混淆器把这些布局信息用于运行期字符串解密/初始化。普通 round-trip（含 Cecil、dnlib 默认、仅保留堆偏移的 0x7800/0xF800 flags）产出的 exe 启动即触发 CLR FailFast（`Infinite recursion during resource lookup within mscorlib ... Arg_NullReferenceException`，隐藏 `Assert Failure` 对话框，退出码 0x80131623）。
对照实验：`PreserveAll` round-trip 可正常启动；`PreserveAll` + 追加 `ob_probeField/ob_probeMethod` 也可正常启动 → 确认「PreserveAll + 加法式」可行。

### 9.4 最终校验（本机实测）
1. 输出回读：types=537（与原版一致）、resources=4（256 / 49,442,954 / 4,552 / 50,232 字节）、目标类型 methods=177（90 原有 + 87 新）、fields=72、nested=9、props=3、events=1、`ob_` 方法 87、10 个保留成员名齐全；assemblyRefs 16→15（伪造 `mscorlib 65535.…` 被 writer 丢弃，实测无影响）。
2. GUI 实跑：出现「微灵」主窗口、自动释放 `dll/` 9 文件、创建 `logs/`、写 `onebot_v11.log`；`titles=[微灵] dll=True logs=True shimlog=True`。
3. 真实 AstrBot 联调：配 8060 stub 提供 self_id 后 `self_id resolved via HTTP` → `connected: ws://127.0.0.1:6200/ws` → `lifecycle.connect`；AstrBot 日志同秒出现 `aiocqhttp(OneBot v11) 适配器已连接。`
4. 原 exe 与 `backup\` 副本 SHA256 前后未变。

### 9.5 复现命令
```
powershell -NoProfile -ExecutionPolicy Bypass -File D:\wecom-ling\onebot\tools\build.ps1
```
（build.ps1 复制 dnlib.dll、csc 编译 PatchOneBot.cs、运行生成输出 exe；`-NoRun` 仅编译。）

## 10. 交付后迭代（2026-11-13，最终构建）

本文件 §9 的 66B6143C… 为首次交付构建；现场反馈后 shim 经 4 轮迭代，最终部署构建为：

| 项目 | 最终值 |
|---|---|
| shim | `onebot\output\OneBotShim.dll` 53,760 B，SHA256 `4F0D7EA2372B0BF7109C6F6AEC87D5D6E41A523313B8264B0D93ECC6F46FD714` |
| 输出 exe | `onebot\output\微灵-OneBotV11-1.11.0.exe` 51,494,912 B，SHA256 `3A54DF0E9B44889796B7A2E806018F963E97847F1B01A31869BBE633BB96DED8` |
| 根目录 / 安装目录 | `D:\wecom-ling\微灵-OneBotV11-1.11.0.exe`、`D:\微灵OneBotV11\微灵-OneBotV11-1.11.0.exe`（同一 SHA256） |
| 原 exe | SHA256 `4F2954A7…5BA24A` 未变 |

迭代要点：
1. 入站 11041 保留指向自己的 at 段（否则 AstrBot 不触发回复）。
2. 查询指令（11038/11036/11039/11040）改为**顶层** `getData:true`；11039 参数用 `user_id`（微灵要求 ID 字符串、limit 1–200、q 1–128 UTF-8 字节）。
3. 出站段支持 `reply`（忽略）与 `face`/`at all` 忽略；新增 `get_group_member_list`（11040+getData）与 `get_group_msg_history`（空数组）。
4. 出站 image/video/file/gif：先落地 `<程序目录>\onebot_media_tmp\` 临时文件（支持 base64://、http(s)、file://、绝对路径），再按 11030/11067/11031/11070 逐条发送，最后发文本；record 不支持。
5. 入站段修正：11045 file→`{name,file,url}`、11046→location、11047→share、11050→contact、11048→image；11200 未映射消息按 content/at_list 映射，`read_receipt` 过滤。
6. **移除 HTTP BaseAddress 补丁**：8060 起不来的真因是运行目录被安全策略限制（见 REPORT §九）；原版从可信目录运行 `http://0.0.0.0:8060/` 正常，补丁改为 `+` 反而把监听限制到 127.0.0.1，故删除。
7. 测试：62/62 PASS（P0 54 + P1 8）；线上实测图片/文件双向收发、中文发送正常。
