# build.ps1 -- compile TestHost.exe (x86, C# 5) and stage the runtime files into bin\
$ErrorActionPreference = 'Stop'

$TestsDir = $PSScriptRoot
$Csc      = 'C:\Windows\Microsoft.NET\Framework\v4.0.30319\csc.exe'
$LibDir   = Join-Path $TestsDir 'lib'
$BinDir   = Join-Path $TestsDir 'bin'

if (-not (Test-Path $Csc)) { throw "csc.exe not found: $Csc" }

New-Item -ItemType Directory -Force $BinDir | Out-Null

# Always test the newest compiled shim: refresh the lib copy from onebot\output.
$OutputDll = Join-Path (Split-Path $TestsDir -Parent) 'output\OneBotShim.dll'
if (Test-Path $OutputDll) {
    Copy-Item $OutputDll (Join-Path $LibDir 'OneBotShim.dll') -Force
    $stamp = (Get-Item $OutputDll).LastWriteTime.ToString('yyyy-MM-dd HH:mm:ss')
    Write-Host ("refreshed lib\OneBotShim.dll from {0} ({1} bytes, {2})" -f $OutputDll, (Get-Item $OutputDll).Length, $stamp)
} else {
    Write-Host "WARNING: $OutputDll not found, keeping existing lib\OneBotShim.dll"
}

# The shim's AssemblyRef name is WeComSever: the original exe must sit next to TestHost.exe.
foreach ($f in @('WeComSever.exe', 'OneBotShim.dll', 'Newtonsoft.Json.dll')) {
    $src = Join-Path $LibDir $f
    if (-not (Test-Path $src)) { throw "missing lib file: $src" }
    Copy-Item $src $BinDir -Force
}

$srcFile = Join-Path $TestsDir 'TestHost.cs'
$outExe  = Join-Path $BinDir 'TestHost.exe'

Write-Host "csc: $Csc"
Write-Host "compiling $srcFile -> $outExe"
& $Csc /nologo /platform:x86 /target:exe "/out:$outExe" `
    "/r:$BinDir\WeComSever.exe" "/r:$BinDir\OneBotShim.dll" "/r:$BinDir\Newtonsoft.Json.dll" `
    /r:System.dll /r:System.Core.dll /r:System.Xml.dll `
    $srcFile
if ($LASTEXITCODE -ne 0) { throw "csc failed with exit code $LASTEXITCODE" }

Write-Host ("BUILD OK -> {0} ({1} bytes)" -f $outExe, (Get-Item $outExe).Length)
exit 0
