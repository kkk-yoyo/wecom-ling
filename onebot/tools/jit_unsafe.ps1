$ErrorActionPreference='Stop'
$p='D:\wecom-ling\onebot\output\微灵-OneBotV11-1.11.0.exe'
$a=[Reflection.Assembly]::UnsafeLoadFrom($p)
Write-Output ('LOADED(UnsafeLoadFrom): ' + $a.FullName)
$t=$a.GetType('WeComConsoleServer.Core.WxWebSocketClient',$false)
$f=[Reflection.BindingFlags]'Public,NonPublic,Instance,Static,DeclaredOnly'
$ms=$t.GetMethods($f); $ct=$t.GetConstructors([Reflection.BindingFlags]'Public,NonPublic,Instance,Static,DeclaredOnly')
$all=@(); foreach($m in $ms){$all+=$m}; foreach($c in $ct){$all+=$c}
$ok=0;$fail=0;$first=''
foreach($m in $all){ try { [Runtime.CompilerServices.RuntimeHelpers]::PrepareMethod($m.MethodHandle); $ok++ } catch { $fail++; if($first -eq ''){$first=$m.Name+' :: '+$_.Exception.GetType().Name+' :: '+$_.Exception.Message} } }
Write-Output ('JIT ok=' + $ok + ' fail=' + $fail + ' total=' + $all.Length)
if($first -ne ''){Write-Output ('FIRST: ' + $first)}