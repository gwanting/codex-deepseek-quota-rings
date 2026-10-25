$ErrorActionPreference='Stop';Add-Type -AssemblyName System.Windows.Forms,System.Drawing
Add-Type @"
using System;using System.Runtime.InteropServices;public static class SliderDragNative{
[StructLayout(LayoutKind.Sequential)]public struct P{public int X,Y;}
[DllImport("user32.dll")]public static extern bool SetProcessDpiAwarenessContext(IntPtr c);
[DllImport("user32.dll")]public static extern bool SetCursorPos(int x,int y);
[DllImport("user32.dll")]public static extern bool GetCursorPos(out P p);
[DllImport("user32.dll")]public static extern short GetAsyncKeyState(int key);
[DllImport("user32.dll")]public static extern void mouse_event(uint f,uint x,uint y,uint d,UIntPtr e);
}
"@
[void][SliderDragNative]::SetProcessDpiAwarenessContext([IntPtr](-4));[Windows.Forms.Application]::EnableVisualStyles();$file=Join-Path $PWD 'DeepSeekBalance\build\slider-drag-qa.json';$exe='build';$a=[Reflection.Assembly]::LoadFrom((Join-Path $PWD ('DeepSeekBalance\'+$exe+'\DeepSeekBalance.exe')));$t=$a.GetType('DeepSeekBalance.BalancePanel');$modelType=$a.GetType('DeepSeekBalance.PanelAppearance');$f=[Reflection.BindingFlags]'Instance,NonPublic';$model=$modelType.GetMethod('Load',[type[]]@([string])).Invoke($null,[object[]]@([string]$file));$modelType.GetField('Transparency').SetValue($model,[double]50);$p=[Activator]::CreateInstance($t,[object[]]@([Action]{},[Action]{}));$t.GetField('appearance',$f).SetValue($p,$model);$p.Location=[Drawing.Point]::new(700,300);$saved=[SliderDragNative+P]::new();[void][SliderDragNative]::GetCursorPos([ref]$saved)
function Pump([int]$ms){$s=[Diagnostics.Stopwatch]::StartNew();while($s.ElapsedMilliseconds-lt $ms){[Windows.Forms.Application]::DoEvents();Start-Sleep -Milliseconds 5}}
function MovePercent([double]$v){$point=$slider.PointToScreen([Drawing.Point]::new([int](10+($slider.Width-20)*$v/100),20));[void][SliderDragNative]::SetCursorPos($point.X,$point.Y);Pump 100}
try{
 $p.Show();$p.OpenAppearance();Pump 150;$slider=$t.GetField('transparency',$f).GetValue($p)
 foreach($start in @(50,15)){
 $slider.Value=50;MovePercent $start;[SliderDragNative]::mouse_event(2,0,0,0,[UIntPtr]::Zero);Pump 80;Write-Output ('START '+$start+' value='+$slider.Value+' captured='+$slider.Capture+' left='+[SliderDragNative]::GetAsyncKeyState(1)+' dragging='+$slider.GetType().GetField('dragging',$f).GetValue($slider))
 if($start -eq 15){$slider.Capture=$false};foreach($target in @(25,65,100,80,0,40)){MovePercent $target;if([Math]::Abs($slider.Value-$target) -gt 0.2){throw ('Drag failed at '+$target+' actual '+$slider.Value)};Write-Output ('DRAG target='+$target+' actual='+$slider.Value+' captured='+$slider.Capture+' left='+[SliderDragNative]::GetAsyncKeyState(1)+' dragging='+$slider.GetType().GetField('dragging',$f).GetValue($slider))}
 [SliderDragNative]::mouse_event(4,0,0,0,[UIntPtr]::Zero);Pump 100;Write-Output ('RELEASE value='+$slider.Value);$committed=$modelType.GetMethod('Load',[type[]]@([string])).Invoke($null,[object[]]@([string]$file));if([Math]::Abs($modelType.GetField('Transparency').GetValue($committed)-40) -gt 0.2){throw 'Final drag value not saved'};$last=$slider.Value;MovePercent 85;if($slider.Value -ne $last){throw 'Slider keeps moving after release'};if($slider.GetType().GetField('dragTimer',$f).GetValue($slider).Enabled){throw 'Drag timer still running after release'};Write-Output 'PASS: release saves value once and stops tracking'
 }
}finally{[SliderDragNative]::mouse_event(4,0,0,0,[UIntPtr]::Zero);[void][SliderDragNative]::SetCursorPos($saved.X,$saved.Y);$p.Dispose();if(Test-Path -LiteralPath $file){Remove-Item -LiteralPath $file}}