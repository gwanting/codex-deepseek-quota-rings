[CmdletBinding()]
param([string]$Version='v0.1.0')
$ErrorActionPreference='Stop'
if($Version -notmatch '^v[0-9]+\.[0-9]+\.[0-9]+$'){throw 'Version must be vX.Y.Z.'}
$root=Split-Path -Parent $PSScriptRoot
$dist=Join-Path $root 'dist'
$name='codex-deepseek-quota-rings-'+$Version+'-windows-x64'
$package=Join-Path $dist $name
if(Test-Path -LiteralPath $package){throw 'Package directory exists; use a new version or inspect it first.'}
$files=@(
 'README.md','LICENSE','THIRD_PARTY_NOTICES.md','Start.cmd','install.ps1',
 'DualQuotaBar\build\DualQuotaBar.exe','DualQuotaBar\build\CodexUsageLauncher.exe',
 'DualQuotaBar\build\backend\CodexMeter.exe','DualQuotaBar\build\backend\LICENSE',
 'DeepSeekBalance\build\DeepSeekBalance.exe',
 'third_party\Win-CodexBar\CodexBar.exe','third_party\Win-CodexBar\LICENSE',
 'third_party\CodexMeter.LICENSE','third_party\Win-CodexBar.LICENSE','third_party\dependencies.json'
)
foreach($file in $files){
 $source=Join-Path $root $file
 if(-not (Test-Path -LiteralPath $source)){throw "Package input missing: $file"}
}
foreach($file in $files){$target=Join-Path $package $file;New-Item -ItemType Directory -Path (Split-Path $target) -Force | Out-Null;Copy-Item -LiteralPath (Join-Path $root $file) -Destination $target}
# Only the documented example is distributed, never the user's runtime settings.
Copy-Item -LiteralPath (Join-Path $root 'DeepSeekBalance\balance-reference.example.json') -Destination (Join-Path $package 'DeepSeekBalance\build\balance-reference.json')
$hashes=Get-ChildItem -LiteralPath $package -File -Recurse | Sort-Object FullName | ForEach-Object {((Get-FileHash -LiteralPath $_.FullName -Algorithm SHA256).Hash.ToLower()+'  '+$_.FullName.Substring($package.Length+1).Replace('\','/'))}
[IO.File]::WriteAllLines((Join-Path $package 'SHA256SUMS.txt'),$hashes,[Text.UTF8Encoding]::new($false))
$archive=Join-Path $dist ($name+'.zip')
Compress-Archive -Path (Join-Path $package '*') -DestinationPath $archive
$hash=(Get-FileHash -LiteralPath $archive -Algorithm SHA256).Hash.ToLower()
[IO.File]::WriteAllText(($archive+'.sha256'),($hash+'  '+[IO.Path]::GetFileName($archive)+[Environment]::NewLine),[Text.UTF8Encoding]::new($false))
Write-Output $archive
