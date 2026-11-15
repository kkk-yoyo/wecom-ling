$p='D:\wecom-ling\onebot\smoke\rt6\WeComSever.exe'
$a=[Reflection.Assembly]::Load([System.IO.File]::ReadAllBytes($p))
Write-Output ('asm=' + $a.FullName)
$t=$a.GetType('WeComConsoleServer.Core.WxWebSocketClient',$false)
$f=[Reflection.BindingFlags]'Public,NonPublic,Instance,Static,DeclaredOnly'
$ms=$t.GetMethods($f); $fs=$t.GetFields($f)
Write-Output ('methods=' + $ms.Length + ' fields=' + $fs.Length)
$probe=$ms | Where-Object { $_.Name -eq 'ob_probeMethod' }
Write-Output ('ob_probeMethod present=' + ($null -ne $probe))
$fprobe=$fs | Where-Object { $_.Name -eq 'ob_probeField' }
Write-Output ('ob_probeField present=' + ($null -ne $fprobe))
# 检查是否有其它类型包含 ob_probeMethod（CLR 视角按 ranges 归属）
$all=$a.GetTypes()
$owner=@(); foreach($ty in $all){ foreach($m in $ty.GetMethods($f)){ if($m.Name -eq 'ob_probeMethod'){ $owner += $ty.FullName } } }
Write-Output ('CLR-side owners of ob_probeMethod: ' + ($owner -join ';'))