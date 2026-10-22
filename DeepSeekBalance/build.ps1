$ErrorActionPreference='Stop'
$output=Join-Path $PSScriptRoot 'build'
New-Item -ItemType Directory -Path $output -Force | Out-Null
& 'C:\Windows\Microsoft.NET\Framework64\v4.0.30319\csc.exe' /nologo /target:winexe /main:DeepSeekBalance.Program /platform:x64 /optimize+ "/win32icon:$PSScriptRoot\DeepSeekBalance.ico" /reference:System.Windows.Forms.dll /reference:System.Drawing.dll /reference:System.Web.Extensions.dll /reference:System.Core.dll /reference:System.Security.dll "/out:$output\DeepSeekBalance.exe" (Join-Path $PSScriptRoot 'DeepSeekBalance.cs') (Join-Path $PSScriptRoot 'DeepSeekClient.cs') (Join-Path $PSScriptRoot 'BalancePanel.cs') (Join-Path $PSScriptRoot 'PricingPeriod.cs') (Join-Path $PSScriptRoot 'BalanceAlert.cs') (Join-Path $PSScriptRoot 'BalanceReference.cs') (Join-Path $PSScriptRoot 'BalanceAmountRenderer.cs')
if($LASTEXITCODE -ne 0){throw 'Build failed'}
Write-Output "Built: $output\DeepSeekBalance.exe"
