[CmdletBinding()]
param([switch]$Offline)
$ErrorActionPreference='Stop'
$version='1.0.4258.31'
$sdk=Join-Path $PSScriptRoot ('.dependencies\WebView2-'+$version)
$archive=Join-Path $PSScriptRoot ('.dependencies\WebView2-'+$version+'.zip')
$expected='56f7f4b8bf9aee4b8efefbbdd4f67d5f74ebd1b100ed0806da71bf76af481aa9'
if(-not (Test-Path -LiteralPath $archive)){
 if($Offline){throw 'WebView2 SDK missing. Run build-platform.ps1 online once.'}
 New-Item -ItemType Directory -Force -Path (Split-Path $archive) | Out-Null
 Invoke-WebRequest "https://api.nuget.org/v3-flatcontainer/microsoft.web.webview2/$version/microsoft.web.webview2.$version.nupkg" -OutFile $archive
}
if((Get-FileHash -LiteralPath $archive -Algorithm SHA256).Hash.ToLower() -ne $expected){throw 'WebView2 SDK checksum mismatch'}
if(-not (Test-Path -LiteralPath (Join-Path $sdk 'lib\net462\Microsoft.Web.WebView2.Core.dll'))){Expand-Archive -LiteralPath $archive -DestinationPath $sdk}
$output=Join-Path $PSScriptRoot 'build\platform'
New-Item -ItemType Directory -Force -Path $output | Out-Null
foreach($file in @('lib\net462\Microsoft.Web.WebView2.Core.dll','lib\net462\Microsoft.Web.WebView2.WinForms.dll','runtimes\win-x64\native\WebView2Loader.dll','LICENSE.txt','NOTICE.txt')){Copy-Item -LiteralPath (Join-Path $sdk $file) -Destination $output -Force}
Copy-Item -LiteralPath (Join-Path $PSScriptRoot 'PlatformUsageQuery.js') -Destination $output -Force
$compiler=Join-Path $env:SystemRoot 'Microsoft.NET\Framework64\v4.0.30319\csc.exe'
& $compiler /nologo /target:winexe /platform:x64 /main:DeepSeekBalance.PlatformBrowserProgram /reference:System.Windows.Forms.dll /reference:System.Drawing.dll /reference:System.Web.Extensions.dll /reference:System.Core.dll "/reference:$output\Microsoft.Web.WebView2.Core.dll" "/reference:$output\Microsoft.Web.WebView2.WinForms.dll" "/out:$output\DeepSeekPlatformUsage.exe" (Join-Path $PSScriptRoot 'PlatformUsage.cs') (Join-Path $PSScriptRoot 'PlatformUsageBrowser.cs')
if($LASTEXITCODE -ne 0){throw 'Platform login helper build failed'}
Write-Output "Built: $output\DeepSeekPlatformUsage.exe"
