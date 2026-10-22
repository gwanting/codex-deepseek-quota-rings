$ErrorActionPreference = 'Stop'
$output = Join-Path $PSScriptRoot 'build'
if (-not (Test-Path -LiteralPath (Join-Path $output 'backend\CodexMeter.exe'))) { throw 'CodexMeter backend is missing; run build.ps1 first.' }
& 'C:\Windows\Microsoft.NET\Framework64\v4.0.30319\csc.exe' /nologo /target:winexe /main:DualQuotaBar.Program /optimize+ /platform:x64 /reference:System.Windows.Forms.dll /reference:System.Drawing.dll /reference:System.Web.Extensions.dll /reference:System.Core.dll /reference:C:\Windows\Microsoft.NET\Framework64\v4.0.30319\WPF\UIAutomationClient.dll /reference:C:\Windows\Microsoft.NET\Framework64\v4.0.30319\WPF\UIAutomationTypes.dll /reference:C:\Windows\Microsoft.NET\Framework64\v4.0.30319\WPF\WindowsBase.dll "/out:$output\DualQuotaBar.exe" (Join-Path $PSScriptRoot 'QuotaData.cs') (Join-Path $PSScriptRoot 'DualQuotaBar.cs') (Join-Path $PSScriptRoot 'LocalQuotaFeed.cs')
if ($LASTEXITCODE -ne 0) { throw 'Build failed' }
Write-Output "Built: $output\DualQuotaBar.exe"

& 'C:\Windows\Microsoft.NET\Framework64\v4.0.30319\csc.exe' /nologo /target:winexe /optimize+ /platform:x64 /reference:System.Windows.Forms.dll "/out:$output\CodexUsageLauncher.exe" (Join-Path $PSScriptRoot 'CodexUsageLauncher.cs')
if ($LASTEXITCODE -ne 0) { throw 'Launcher build failed' }
