$ErrorActionPreference='Stop'
$dir='D:\wecom-ling\onebot\smoke\verify32'
$a=[Reflection.Assembly]::UnsafeLoadFrom((Join-Path $dir 'WeComSever.exe'))
$t=$a.GetType('WeComConsoleServer.Core.WxWebSocketClient',$false)
$f=[Reflection.BindingFlags]'Public,NonPublic,Instance,Static,DeclaredOnly'
$ctor=$null
foreach($c in $t.GetConstructors($f)){ $ps=$c.GetParameters(); if($ps.Length -eq 6){ $ctor=$c; break } }
Write-Output ('ctor found=' + ($null -ne $ctor) + ' params=' + (($ctor.GetParameters() | ForEach-Object { $_.ParameterType.Name }) -join ','))
$obj=$ctor.Invoke(@($null,$null,$null,$null,$null,$null))
Write-Output ('CONSTRUCTED type=' + $obj.GetType().FullName)
foreach($pn in @('IsConnected','StatusText','StatusBackground')){ $p=$t.GetProperty($pn,$f); $v=$p.GetValue($obj,$null); Write-Output ($pn + '=' + $(if($null -eq $v){'<null>'}else{[string]$v})) }
$mStop=$t.GetMethod('Stop',$f); $mStop.Invoke($obj,$null); Write-Output 'STOP_OK'
$mDispose=$t.GetMethod('Dispose',$f); $mDispose.Invoke($obj,$null); Write-Output 'DISPOSE_OK'