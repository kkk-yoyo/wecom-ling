# 32 位 PowerShell 校验（additive 版输出 exe）
# 用法：C:\Windows\SysWOW64\WindowsPowerShell\v1.0\powershell.exe -NoProfile -ExecutionPolicy Bypass -File check_output_32.ps1
$ErrorActionPreference = 'Stop'
$p  = 'D:\wecom-ling\onebot\output\微灵-OneBotV11-1.11.0.exe'
$nj = 'D:\wecom-ling\onebot\lib\Newtonsoft.Json.dll'
$v  = 'D:\wecom-ling\onebot\smoke\verify32'
Write-Output ('output exe size = ' + (Get-Item $p).Length)
Write-Output ('newtonsoft  size = ' + (Get-Item $nj).Length)

# 1) 目标路径 LoadFile（本机 zone 策略拒绝，原 exe 同样被拒，仅作对照记录）
try { $null = [Reflection.Assembly]::LoadFile($p); Write-Output 'LoadFile: OK' }
catch { Write-Output ('LoadFile REFUSED (environment zone policy; 原 exe 同样被拒): ' + $_.Exception.InnerException.GetType().Name) }

# 2) byte[] Load（同一 PE 镜像，不受 zone 限制）——确认可加载/可取类型/可列成员
$asm = [Reflection.Assembly]::Load([System.IO.File]::ReadAllBytes($p))
Write-Output ('LOADED(Load(byte[])): ' + $asm.FullName)
$t = $asm.GetType('WeComConsoleServer.Core.WxWebSocketClient', $false)
if ($null -eq $t) { Write-Output 'TYPE_NOT_FOUND'; exit 2 }
$ifaces = $t.GetInterfaces()
Write-Output ('TYPE: ' + $t.FullName + ' public=' + $t.IsPublic + ' sealed=' + $t.IsSealed + ' ifaces=' + $ifaces.Length + ' (' + (($ifaces | ForEach-Object { $_.Name }) -join ',') + ')')
$flags = [Reflection.BindingFlags]'Public,NonPublic,Instance,Static,DeclaredOnly'
$ms = $t.GetMethods($flags); $fs = $t.GetFields($flags); $ps = $t.GetProperties($flags); $ev = $t.GetEvents($flags)
$ctors = $t.GetConstructors([Reflection.BindingFlags]'Public,NonPublic,Instance,Static,DeclaredOnly')
$ti = $t.TypeInitializer
# 说明：dnlib 方法数 = GetMethods + 实例构造器（.cctor 会被 GetMethods 返回，且同时是 TypeInitializer，不能重复计）
$dnTotal = $ms.Length + $ctors.Length
Write-Output ('methods=' + $ms.Length + ' ctors=' + $ctors.Length + ' typeInit=' + $(if ($null -ne $ti) { 'yes' } else { 'no' }) + ' => dnlib methods=' + $dnTotal)
Write-Output ('fields=' + $fs.Length + ' props=' + $ps.Length + ' events=' + $ev.Length)
$n = $t.GetNestedTypes([Reflection.BindingFlags]'Public,NonPublic')
Write-Output ('nested=' + $n.Length + ' (' + (($n | ForEach-Object { $_.Name }) -join ',') + ')')
$ch = $null
foreach ($nt in $n) { if ($nt.Name -eq 'CommandHandler') { $ch = $nt; break } }
if ($null -eq $ch) { Write-Output 'CH_NOT_FOUND'; exit 2 }
$chm = $ch.GetMethods([Reflection.BindingFlags]'Public,NonPublic,Instance,Static,DeclaredOnly')
$chc = $ch.GetConstructors([Reflection.BindingFlags]'Public,NonPublic,Instance,Static,DeclaredOnly')
Write-Output ('CommandHandler methods=' + $chm.Length + ' ctors=' + $chc.Length + ' typeInit=' + $(if ($null -ne $ch.TypeInitializer) { 'yes' } else { 'no' }))
Write-Output ('props: ' + (($ps | ForEach-Object { $_.Name } | Sort-Object) -join ','))
Write-Output ('events: ' + (($ev | ForEach-Object { $_.Name } | Sort-Object) -join ','))

# additive 期望：177 方法（90 旧 + 87 ob_）、72 字段（20 旧 + 52 ob_）、9 嵌套、3 属性、1 事件、1 接口
$okStruct = ($dnTotal -eq 177) -and ($fs.Length -eq 72) -and ($ps.Length -eq 3) -and ($ev.Length -eq 1) -and ($n.Length -eq 9) -and ($t.IsPublic) -and ($t.IsSealed) -and ($ifaces.Length -eq 1)
Write-Output ('CHECK_STRUCT=' + $(if ($okStruct) { 'PASS' } else { 'FAIL' }))

# 3) 真实执行校验：UnsafeLoadFrom 本地副本（Newtonsoft 放同目录以便依赖解析），构造实例 + 读 3 个属性 + Stop/Dispose
New-Item -ItemType Directory -Force -Path $v | Out-Null
Copy-Item $p (Join-Path $v 'WeComSever.exe') -Force
Copy-Item $nj (Join-Path $v 'Newtonsoft.Json.dll') -Force
$execOk = $false
Push-Location $v
try {
  $a2 = [Reflection.Assembly]::UnsafeLoadFrom((Join-Path $v 'WeComSever.exe'))
  $t2 = $a2.GetType('WeComConsoleServer.Core.WxWebSocketClient', $false)
  $f2 = [Reflection.BindingFlags]'Public,NonPublic,Instance,Static,DeclaredOnly'
  $ctor = $null
  foreach ($c in $t2.GetConstructors($f2)) { if ($c.GetParameters().Length -eq 6) { $ctor = $c; break } }
  if ($null -eq $ctor) { Write-Output 'CTOR_NOT_FOUND'; exit 4 }
  Write-Output ('ctor params: ' + (($ctor.GetParameters() | ForEach-Object { $_.ParameterType.Name }) -join ','))
  $obj = $ctor.Invoke(@($null, $null, $null, $null, $null, $null))
  Write-Output ('EXEC ctor: OK type=' + $obj.GetType().FullName)
  $execOk = $true
  foreach ($pn in @('IsConnected', 'StatusText', 'StatusBackground')) {
    $pv = $t2.GetProperty($pn, $f2).GetValue($obj, $null)
    if ($pn -eq 'StatusText') { if ([string]::IsNullOrEmpty([string]$pv)) { $execOk = $false } }
    Write-Output ('EXEC ' + $pn + '=' + $(if ($null -eq $pv) { '<null>' } else { [string]$pv }))
  }
  $t2.GetMethod('Stop', $f2).Invoke($obj, $null)
  Write-Output 'EXEC Stop: OK'
  $t2.GetMethod('Dispose', $f2).Invoke($obj, $null)
  Write-Output 'EXEC Dispose: OK'
} finally { Pop-Location }
Write-Output ('CHECK_EXEC=' + $(if ($execOk) { 'PASS' } else { 'FAIL' }))
$ok = $okStruct -and $execOk
Write-Output ('CHECK_32BIT=' + $(if ($ok) { 'PASS' } else { 'FAIL' }))
