$ErrorActionPreference='Stop'
$dir='D:\wecom-ling\onebot\smoke\verify32'
$p=Join-Path $dir 'WeComSever.exe'
$a=[Reflection.Assembly]::UnsafeLoadFrom($p)
Write-Output ('LOADED(UnsafeLoadFrom): ' + $a.FullName + ' loc=' + $a.Location)
$t=$a.GetType('WeComConsoleServer.Core.WxWebSocketClient',$false)
$f=[Reflection.BindingFlags]'Public,NonPublic,Instance,Static,DeclaredOnly'
$ms=$t.GetMethods($f); $ct=$t.GetConstructors([Reflection.BindingFlags]'Public,NonPublic,Instance,Static,DeclaredOnly')
Write-Output ('methods=' + $ms.Length + ' ctors=' + $ct.Length + ' fields=' + $t.GetFields($f).Length + ' props=' + $t.GetProperties($f).Length + ' events=' + $t.GetEvents($f).Length + ' nested=' + $t.GetNestedTypes([Reflection.BindingFlags]'Public,NonPublic').Length + ' ti=' + ($null -ne $t.TypeInitializer))
$all=@(); foreach($m in $ms){$all+=$m}; foreach($c in $ct){$all+=$c}
$ok=0;$fail=0;$first=''
foreach($m in $all){ try { [Runtime.CompilerServices.RuntimeHelpers]::PrepareMethod($m.MethodHandle); $ok++ } catch { $fail++; if($first -eq ''){$first=$m.Name+' :: '+$_.Exception.GetType().Name+' :: '+$_.Exception.Message} } }
Write-Output ('JIT ok=' + $ok + ' fail=' + $fail + ' total=' + $all.Length)
if($first -ne ''){Write-Output ('FIRST: ' + $first)}