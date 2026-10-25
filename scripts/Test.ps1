$ErrorActionPreference='Stop'
$root=Split-Path -Parent $PSScriptRoot
$output=Join-Path $root '.dependencies\checks'
New-Item -ItemType Directory -Path $output -Force | Out-Null
$compiler=Join-Path $env:SystemRoot 'Microsoft.NET\Framework64\v4.0.30319\csc.exe'
$sets=@(
 @{name='SynchronizationChecks';folder='DualQuotaBar';files=@('SynchronizationChecks.cs','QuotaData.cs')},
 @{name='FeedChecks';folder='DualQuotaBar';files=@('FeedChecks.cs','LocalQuotaFeed.cs','QuotaData.cs')},
 @{name='BalanceChecks';folder='DeepSeekBalance';files=@('BalanceChecks.cs','DeepSeekClient.cs')},
 @{name='PricingChecks';folder='DeepSeekBalance';files=@('PricingChecks.cs','PricingPeriod.cs')},
 @{name='AlertChecks';folder='DeepSeekBalance';files=@('AlertChecks.cs','BalanceAlert.cs')},
 @{name='ReferenceChecks';folder='DeepSeekBalance';files=@('ReferenceChecks.cs','DeepSeekClient.cs','BalanceReference.cs')},
 @{name='PlatformUsageChecks';folder='DeepSeekBalance';files=@('PlatformUsageChecks.cs','PlatformUsage.cs')},
 @{name='AppearanceChecks';folder='DeepSeekBalance';files=@('AppearanceChecks.cs','PanelAppearance.cs','PlatformUsage.cs')}
)
foreach($set in $sets){
 $exe=Join-Path $output ($set.name+'.exe')
 $files=$set.files | ForEach-Object {Join-Path (Join-Path $root $set.folder) $_}
 & $compiler /nologo /target:exe /platform:x64 /reference:System.Windows.Forms.dll /reference:System.Drawing.dll /reference:System.Web.Extensions.dll /reference:System.Core.dll /reference:System.Security.dll "/main:$($set.name)" "/out:$exe" @files
 if($LASTEXITCODE -ne 0){throw "Check build failed: $($set.name)"}
 & $exe
 if($LASTEXITCODE -ne 0){throw "Check failed: $($set.name)"}
}
if(Get-Command node -ErrorAction SilentlyContinue){
 & node (Join-Path $root 'DeepSeekBalance\PlatformQueryChecks.js')
 if($LASTEXITCODE -ne 0){throw 'Platform query checks failed'}
}else{Write-Warning 'Node.js is unavailable; browser-query checks skipped.'}
