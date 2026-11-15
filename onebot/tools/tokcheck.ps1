function Dump($p,$label){
  $a=[Reflection.Assembly]::Load([System.IO.File]::ReadAllBytes($p))
  $t=$a.GetType('WeComConsoleServer.Core.WxWebSocketClient',$false)
  $f=[Reflection.BindingFlags]'Public,NonPublic,Instance,Static,DeclaredOnly'
  $ms=$t.GetMethods($f); $ct=$t.GetConstructors($f)
  Write-Output ("$label : GetMethods=" + $ms.Length + " ctors=" + $ct.Length)
  $cts=$t.TypeInitializer
  Write-Output ("  cctor=" + ($null -ne $cts))
  $sorted = $ms | Sort-Object { $_.MetadataToken }
  Write-Output ("  firstM=" + $sorted[0].Name + " tok=0x" + $sorted[0].MetadataToken.ToString('X8') + "  lastM=" + $sorted[$sorted.Count-1].Name + " tok=0x" + $sorted[$sorted.Count-1].MetadataToken.ToString('X8'))
  Write-Output ("  ctor tok=0x" + $ct[0].MetadataToken.ToString('X8'))
  $ch=$t.GetNestedType('CommandHandler',[Reflection.BindingFlags]'Public,NonPublic')
  $chm=$ch.GetMethods($f); $chc=$ch.GetConstructors($f)
  Write-Output ("  CH GetMethods=" + $chm.Length + " ctors=" + $chc.Length + " firstTok=0x" + $(if($chm.Length){($chm | Sort-Object { $_.MetadataToken })[0].MetadataToken.ToString('X8')}else{'na'}))
}
Dump 'D:\wecom-ling\微灵1.11.0_免卡密.exe' 'ORIG'
Dump 'D:\wecom-ling\onebot\smoke\rt6\WeComSever.exe' 'RT6'