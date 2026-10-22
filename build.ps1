[CmdletBinding()]
param([switch]$Offline)
$ErrorActionPreference='Stop'
& (Join-Path $PSScriptRoot 'scripts\Prepare-Dependencies.ps1') -Offline:$Offline
& (Join-Path $PSScriptRoot 'DualQuotaBar\build.ps1')
& (Join-Path $PSScriptRoot 'DeepSeekBalance\build-platform.ps1') -Offline:$Offline
& (Join-Path $PSScriptRoot 'DeepSeekBalance\build.ps1')
$reference=Join-Path $PSScriptRoot 'DeepSeekBalance\build\balance-reference.json'
if(-not (Test-Path -LiteralPath $reference)){
 Copy-Item -LiteralPath (Join-Path $PSScriptRoot 'DeepSeekBalance\balance-reference.example.json') -Destination $reference
}
Write-Output 'Build complete. Run Start.cmd to launch all components.'
