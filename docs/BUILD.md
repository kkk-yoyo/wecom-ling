# 构建说明（BUILD）

## 1. 现状

`src/WeComSever/` 是 `ilspycmd 9.1.0.7988` 对 `dist/微灵.exe` 的**反编译产物**：

- ✅ 所有 C# 源码完整（62 个文件中的 .cs 部分，含资源与 `.csproj`）
- ❌ WPF 的 `MainWindow.xaml` / `CleanerSettingsWindow.xaml` 在编译时已转成 BAML（`mainwindow.baml` / `cleanersettingswindow.baml`），ILSpy 命令行工具没有 BAML 反编译器，因此**缺少 XAML 文件，`dotnet build` / `msbuild` 会失败**
- ✅ `.csproj`（`src/WeComSever/微灵.csproj`）保留了原工程配置：`net472`、`WinExe`、`UseWPF`、`Prefer32Bit`、Costura 资源、程序集引用列表

因此目前有两种使用方式：

1. **直接用原始二进制**：`dist/微灵.exe` + `native/bin/*.dll` + `config.ini`（推荐，功能完整）。
2. **作为源码研究**：阅读/修改 C# 代码，XAML 需按下节还原后再编译。

## 2. 环境要求

- Windows 10/11 x64（目标是 x86 进程）
- Visual Studio 2022（含 ".NET 桌面开发" 工作负载）或 VS 2022 Build Tools + .NET Framework 4.7.2 目标包
- .NET Framework 4.7.2 Runtime（运行）
- NuGet 访问（还原 `Newtonsoft.Json`、`System.Web.Http`、`System.Web.Http.SelfHost`、`Spectre.Console`、`Microsoft.AspNet.WebApi.Client` 等）

本机若只装了 Build Tools，可用：

```
C:\Program Files (x86)\Microsoft Visual Studio\2022\BuildTools\MSBuild\Current\Bin\MSBuild.exe
```

## 3. 还原 XAML（编译前必须）

`mainwindow.baml` 是 WPF 的二进制 XAML。可选做法：

1. **ILSpy GUI（推荐）**：下载 ILSpy 桌面版 → 打开 `dist/微灵.exe` → 右键程序集「Save Code…」→ 勾选 “Decompile BAML to XAML” → 用导出的 `MainWindow.xaml` / `CleanerSettingsWindow.xaml` 覆盖 `src/WeComSever/` 下同名资源位置。
2. 手工重构：参照 `MainWindow.cs` 的字段与 `InitializeComponent()`，重写 XAML（工作量大，但可控）。
3. 反编译器中转：把 BAML 交给支持 BAML 的第三方工具（如 dnSpy + BAML 插件）导出。

> 注：`ilspycmd` 9.1 的 NuGet 包里 `ICSharpCode.Decompiler.dll` / `ICSharpCode.ILSpyX.dll` 均**不含 Baml 类型**（已实测为 0），所以只能走 ILSpy GUI。

## 4. 编译步骤（XAML 还原后）

1. 编辑 `src/WeComSever/微灵.csproj`：
   - 把 `<Reference Include="Newtonsoft.Json" />` 等改成 `PackageReference`（或补 `HintPath` 指向 NuGet 缓存），否则无法解析；
   - `AssemblyName` 可保持不变（`WeComSever`）；
   - 确认 `<EmbeddedResource>` 中引用的 baml/costura 文件实际存在。
2. 还原并编译：

   ```bat
   msbuild "src\WeComSever\微灵.csproj" /restore /p:Configuration=Release /p:Platform=x86
   ```

3. 把 `native/bin/Loader.dll`、`native/bin/Helper_5.0.0.6008.dll` 复制到输出目录。
4. 在输出目录写 `config.ini`（参考 `examples/config.ini`），以管理员身份运行。

## 5. 已知问题清单（欢迎 PR）

- [ ] 还原 `MainWindow.xaml` / `CleanerSettingsWindow.xaml`（当前最大阻塞）
- [ ] 把 csproj 的 `Reference` 迁移为 `PackageReference`，提供可复现的 NuGet 依赖版本
- [ ] 无 GUI 的 headless 服务端（去掉 `Application.Current` / `MainWindow` 静态依赖，便于服务器部署）
- [ ] bridge：图片/文件消息的 CDN 下载（type 11170/11171）→ OneBot `image`/`file` 段
- [ ] bridge：群成员/群列表 action 的完整实现
- [ ] 移除对 `CronHelper` 清理逻辑中 `MessageBox` 的 UI 依赖（headless 需要）

## 6. 反编译复现

`tools/decompile.ps1` 是当时使用的完整流程（ilspycmd 走 NuGet 镜像）：

```powershell
# 1. 安装 ilspycmd（example: 9.1.0.7988）
$url = 'https://api.nuget.org/v3-flatcontainer/ilspycmd/9.1.0.7988/ilspycmd.9.1.0.7988.nupkg'
Invoke-WebRequest $url -OutFile "$env:TEMP\ilspycmd.nupkg"
Expand-Archive "$env:TEMP\ilspycmd.nupkg" -DestinationPath "$env:TEMP\ilspycmd"
# 2. 运行（本机只有 dotnet runtime 10，需要 Major roll-forward）
$env:DOTNET_ROLL_FORWARD = 'Major'
dotnet "$env:TEMP\ilspycmd\tools\net8.0\any\ilspycmd.dll" -p -o .\src\WeComSever dist\微灵.exe
```

## 7. 客户端版本

必须在目标机安装企业微信 **5.0.0.6008 x86**。历史版本官方地址（当年可用）：

```
https://dldir1.qq.com/wework/work_weixin/WeCom_5.0.0.6008.exe
```

安装包是 NSIS 自解压，可用 7-Zip 以 `-tnsis` 解包（普通 `7z x` 会报 `Headers Error`）：

```bat
7z x -tnsis WeCom_5.0.0.6008.exe -oD:\WeCom_5.0.0.6008
```

解包后把 payload 放到独立目录，并写入注册表让系统使用该客户端（这是我们实测可行的做法）：

```
HKCU\Software\Tencent\WXWork
  Version    = 5.0.0.6008
  Executable = <安装目录>\WXWork.exe
  WeMeetDir  = <安装目录>\5.0.0.6008\WeMeet
```

并删除安装目录里的 `WXWork.exe.new` / `Uninstall.exe.new`、把 `WXWorkUpgrader` 改名，阻止自动升级到新版本（新版本注入不了）。

## 8. 运行注意

- 必须**管理员权限**（UAC 提权后启动）。
- 只运行一个企微主进程：多开请用 GUI「启动新实例」或 `Loader.dll` 的多开函数。
- 账号风险：5.0.0.6008 属老版本，登录新账号很容易触发腾讯风控（外挂/非官方客户端提示），**建议只用测试号**。
