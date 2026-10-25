$ErrorActionPreference="Stop"
Add-Type -AssemblyName System.Windows.Forms,System.Drawing
[Windows.Forms.Application]::EnableVisualStyles()
$a=[Reflection.Assembly]::LoadFrom((Join-Path $PWD 'DeepSeekBalance\build\DeepSeekBalance.exe'));$t=$a.GetType('DeepSeekBalance.BalancePanel');$m=$a.GetType('DeepSeekBalance.PanelAppearance');$flags=[Reflection.BindingFlags]'Instance,NonPublic';$load=$m.GetMethod('Load',[type[]]@([string]));$file=Join-Path $PWD 'DeepSeekBalance\build\appearance-ui-qa.json';$model=$load.Invoke($null,[object[]]@([string]$file));$p=[Activator]::CreateInstance($t,[object[]]@([Action]{},[Action]{}));$t.GetField('appearance',$flags).SetValue($p,$model)
try {
 $p.Show();[Windows.Forms.Application]::DoEvents();$t.GetMethod('OpenAppearance').Invoke($p,@()) | Out-Null
 $theme=$t.GetField('theme',$flags).GetValue($p);$theme.SelectedIndex=1
 $saved=$load.Invoke($null,[object[]]@([string]$file));if(-not $m.GetField('Light').GetValue($saved)){throw 'Theme switch not saved'};'PASS: Theme switch applies and saves'
 $slider=$t.GetField('transparency',$flags).GetValue($p);$slider.Value=72.5
 $label=$t.GetField('transparencyText',$flags).GetValue($p);if(-not $label.Text.Contains('72.5')){throw 'Slider preview not updated'};'PASS: Fractional slider updates live'
 $slider.GetType().GetMethod('OnKeyDown',$flags).Invoke($slider,[object[]]@([Windows.Forms.KeyEventArgs]::new([Windows.Forms.Keys]::Right))) | Out-Null
 $saved=$load.Invoke($null,[object[]]@([string]$file));if([Math]::Abs([double]$m.GetField('Transparency').GetValue($saved)-72.6) -gt 0.001){throw 'Fractional value not persisted'};'PASS: Slider commit survives reload'
 $view=$t.GetField('appearanceView',$flags).GetValue($p);($view.Controls | Where-Object {$_.Text -eq '返回'}).PerformClick();if($view.Visible -or -not $p.Visible){throw 'Return behavior failed'};'PASS: Return restores balance panel'
}finally{$p.Dispose();if(Test-Path -LiteralPath $file){Remove-Item -LiteralPath $file}}