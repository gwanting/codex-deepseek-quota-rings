[CmdletBinding()]
param()
$ErrorActionPreference='Stop'
$root=Split-Path -Parent $PSScriptRoot
$powershell=Join-Path $env:SystemRoot 'System32\WindowsPowerShell\v1.0\powershell.exe'
# These checks show disposable windows and move/restore the cursor. Run on an idle desktop.
$checks=@('AppearanceUiChecks.ps1','LinearOpacityChecks.ps1','MotionUiChecks.ps1','MenuUiChecks.ps1','SliderDragUiChecks.ps1')
Push-Location $root
try {
 foreach($name in $checks){
  & $powershell -NoProfile -ExecutionPolicy Bypass -File (Join-Path $root ('tests\ui\'+$name))
  if($LASTEXITCODE -ne 0){throw "UI check failed: $name"}
 }
}finally{Pop-Location}
