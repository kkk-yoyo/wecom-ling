# 反编译复现脚本：微灵.exe -> C# 源码
#
# 用法（PowerShell 5.1+ / 7+，需要已安装 .NET 8+ 运行时）：
#   .\tools\decompile.ps1
#   .\tools\decompile.ps1 -Exe "D:\path\to\微灵.exe" -OutDir "D:\wecom-ling\src\WeComSever"
#
# 说明：
#   - 从 nuget.org 下载 ilspycmd 9.1.0.7988（含 ICSharpCode.Decompiler）；
#   - 用 DOTNET_ROLL_FORWARD=Major 让 net8.0 的 ilspycmd 跑在更高版本的 dotnet 上；
#   - 若已下载过，可加 -NoDownload 跳过下载。
param(
    [string]$Exe = "D:\download\lanzou\wecom\微灵.exe",
    [string]$OutDir = "$PSScriptRoot\..\src\WeComSever",
    [string]$CacheDir = "$PSScriptRoot\..\tools\ilspycmd",
    [switch]$NoDownload
)

$ErrorActionPreference = "Stop"
$version = "9.1.0.7988"
$pkgUrl = "https://api.nuget.org/v3-flatcontainer/ilspycmd/$version/ilspycmd.$version.nupkg"
$pkgZip = Join-Path $CacheDir "ilspycmd.$version.zip"
$pkgDir = Join-Path $CacheDir "pkg"
$ilspy = Join-Path $pkgDir "tools\net8.0\any\ilspycmd.dll"

if (-not (Test-Path -LiteralPath $Exe)) {
    throw "找不到目标 exe: $Exe"
}

if (-not (Test-Path -LiteralPath $ilspy)) {
    if ($NoDownload) { throw "ilspycmd 不存在且指定了 -NoDownload: $ilspy" }
    New-Item -ItemType Directory -Force -Path $CacheDir | Out-Null
    Write-Host "下载 ilspycmd $version ..."
    Invoke-WebRequest -Uri $pkgUrl -OutFile $pkgZip -UseBasicParsing
    if (Test-Path -LiteralPath $pkgDir) { Remove-Item -LiteralPath $pkgDir -Recurse -Force }
    Expand-Archive -LiteralPath $pkgZip -DestinationPath $pkgDir -Force
    if (-not (Test-Path -LiteralPath $ilspy)) { throw "解包后仍找不到 ilspycmd.dll" }
    Write-Host "ilspycmd 已就绪: $ilspy"
}

if (-not (Test-Path -LiteralPath $OutDir)) {
    New-Item -ItemType Directory -Force -Path $OutDir | Out-Null
}

$env:DOTNET_ROLL_FORWARD = "Major"
Write-Host "开始反编译: $Exe"
Write-Host "输出目录:   $OutDir"
& dotnet $ilspy -p -o $OutDir $Exe
if ($LASTEXITCODE -ne 0) { throw "ilspycmd 退出码 $LASTEXITCODE" }

$count = (Get-ChildItem -LiteralPath $OutDir -Recurse -File | Measure-Object).Count
Write-Host "完成，共 $count 个文件"
Write-Host "提示：BAML 资源（*.baml）无法用 ilspycmd 还原为 XAML，见 docs/BUILD.md"
