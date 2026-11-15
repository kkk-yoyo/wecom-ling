# build.ps1 — 编译并运行 PatchOneBot（dnlib 4.5.0）
# 用法：powershell -ExecutionPolicy Bypass -File build.ps1 [-NoRun]
param(
    [switch]$NoRun
)

$ErrorActionPreference = 'Stop'
$tools = Split-Path -Parent $MyInvocation.MyCommand.Path
$csc = 'C:\Windows\Microsoft.NET\Framework\v4.0.30319\csc.exe'
$netstd = 'C:\Windows\Microsoft.NET\Framework\v4.0.30319\netstandard.dll'
$dnlibSrc = 'D:\wecom-ling\onebot\lib\dnlib\lib\net45\dnlib.dll'
$dnlibDst = Join-Path $tools 'dnlib.dll'

if (-not (Test-Path $csc)) { throw "csc not found: $csc" }
if (-not (Test-Path $dnlibSrc)) { throw "dnlib not found: $dnlibSrc" }

Copy-Item $dnlibSrc $dnlibDst -Force

$outExe = Join-Path $tools 'PatchOneBot.exe'
& $csc /nologo /target:exe /out:$outExe /r:$netstd /r:$dnlibDst (Join-Path $tools 'PatchOneBot.cs')
if ($LASTEXITCODE -ne 0) { throw "compile failed (exit $LASTEXITCODE)" }
Write-Host "compiled: $outExe" -ForegroundColor Green

if ($NoRun) { return }

Push-Location $tools
try {
    & $outExe
    $code = $LASTEXITCODE
} finally {
    Pop-Location
}
Write-Host "PatchOneBot exit code: $code"
if ($code -ne 0) { throw "PatchOneBot failed (exit $code)" }
