$p = $args[0]
$a=[Reflection.Assembly]::Load([System.IO.File]::ReadAllBytes($p))
$t=$a.GetType('WeComConsoleServer.Core.WxWebSocketClient',$false)
$f=[Reflection.BindingFlags]'Public,NonPublic,Instance,Static,DeclaredOnly'
$ms=$t.GetMethods($f); $ct=$t.GetConstructors($f); $ti=$t.TypeInitializer
Write-Output ("FILE=" + $p)
Write-Output ("methods=" + $ms.Length + " ctors=" + $ct.Length + " ti=" + ($null -ne $ti))
$toks = @()
foreach($m in $ms){ $toks += ('{0:X8} {1}' -f $m.MetadataToken, $m.Name) }
foreach($c in $ct){ $toks += ('{0:X8} {1}' -f $c.MetadataToken, $c.Name) }
if($null -ne $ti){ $toks += ('{0:X8} .cctor' -f $ti.MetadataToken) }
$sorted = $toks | Sort-Object
Write-Output ("TOKENS_MIN=" + $sorted[0] + "  TOKENS_MAX=" + $sorted[$sorted.Count-1])
Write-Output ("---token list---")
$sorted | ForEach-Object { Write-Output $_ }