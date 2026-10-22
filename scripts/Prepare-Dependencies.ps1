[CmdletBinding()]
param([switch]$Offline)
$ErrorActionPreference='Stop'
$root=Split-Path -Parent $PSScriptRoot
$manifest=Get-Content -LiteralPath (Join-Path $root 'third_party\dependencies.json') -Raw | ConvertFrom-Json
$cache=Join-Path $root '.dependencies'
$codexbar=Join-Path $root 'third_party\Win-CodexBar\CodexBar.exe'
$meter=Join-Path $root 'DualQuotaBar\build\backend\CodexMeter.exe'
function Confirm-Hash([string]$Path,[string]$Expected){if((Get-FileHash -LiteralPath $Path -Algorithm SHA256).Hash -ne $Expected){throw "Dependency checksum mismatch: $Path"}}
New-Item -ItemType Directory -Path $cache,(Split-Path $codexbar),(Split-Path $meter) -Force | Out-Null
if(-not (Test-Path -LiteralPath $codexbar)){
 if($Offline){throw 'Win-CodexBar dependency is missing. Run without -Offline to download.'}
 Invoke-WebRequest -Uri $manifest.codexbar.url -OutFile $codexbar
}
Confirm-Hash $codexbar $manifest.codexbar.sha256
if(-not (Test-Path -LiteralPath $meter)){
 if($Offline){throw 'CodexMeter dependency is missing. Run without -Offline to download.'}
 $zip=Join-Path $cache 'CodexMeter-v0.1.0.zip'
 Invoke-WebRequest -Uri $manifest.codexmeter.url -OutFile $zip
 $unpacked=Join-Path $cache ('CodexMeter-'+[Guid]::NewGuid().ToString('N'))
 Expand-Archive -LiteralPath $zip -DestinationPath $unpacked
 $binary=Get-ChildItem -LiteralPath $unpacked -Filter 'CodexMeter.exe' -File -Recurse | Select-Object -First 1
 if(-not $binary){throw 'CodexMeter archive does not contain its executable.'}
 Confirm-Hash $binary.FullName $manifest.codexmeter.sha256
 Copy-Item -LiteralPath $binary.FullName -Destination $meter
}
Confirm-Hash $meter $manifest.codexmeter.sha256
Copy-Item -LiteralPath (Join-Path $root 'third_party\CodexMeter.LICENSE') -Destination (Join-Path (Split-Path $meter) 'LICENSE') -Force
Copy-Item -LiteralPath (Join-Path $root 'third_party\Win-CodexBar.LICENSE') -Destination (Join-Path (Split-Path $codexbar) 'LICENSE') -Force
Write-Output 'Pinned dependencies verified.'
