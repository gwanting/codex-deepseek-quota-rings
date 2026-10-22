[CmdletBinding(SupportsShouldProcess)]
param([switch]$Start)
$ErrorActionPreference='Stop'
$launcher=Join-Path $PSScriptRoot 'DualQuotaBar\build\CodexUsageLauncher.exe'
if(-not (Test-Path -LiteralPath $launcher)){throw 'Build is missing. Run build.ps1 or use the portable release.'}
$shortcut=Join-Path ([Environment]::GetFolderPath('Desktop')) 'Codex 用量.lnk'
if($PSCmdlet.ShouldProcess($shortcut,'Create unified Codex and DeepSeek shortcut')){
 $link=(New-Object -ComObject WScript.Shell).CreateShortcut($shortcut)
 $link.TargetPath=$launcher
 $link.WorkingDirectory=Split-Path $launcher
 $link.IconLocation=Join-Path $PSScriptRoot 'third_party\Win-CodexBar\CodexBar.exe,0'
 $link.Description='Codex 用量与 DeepSeek 余额'
 $link.Save()
}
if($Start -and $PSCmdlet.ShouldProcess($launcher,'Start quota views')){Start-Process -FilePath $launcher -WorkingDirectory (Split-Path $launcher)}
