$ErrorActionPreference='Stop'
$paths = @('D:\wecom-ling\微灵1.11.0_免卡密.exe','D:\wecom-ling\onebot\output\微灵-OneBotV11-1.11.0.exe')
foreach($p in $paths){ try { $a=[Reflection.Assembly]::LoadFile($p); Write-Output ('LOADED ' + $p + ' -> ' + $a.FullName) } catch { Write-Output ('FAILED ' + $p + ' :: ' + $_.Exception.InnerException.GetType().Name + ' :: ' + $_.Exception.InnerException.Message) } }