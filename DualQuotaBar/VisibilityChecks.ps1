$ErrorActionPreference='Stop';Add-Type -AssemblyName System.Windows.Forms,System.Drawing
Add-Type @"
using System;using System.Text;using System.Runtime.InteropServices;public static class RingZQA{
public delegate bool CB(IntPtr h,IntPtr p);[DllImport("user32.dll")]public static extern bool EnumWindows(CB cb,IntPtr p);[DllImport("user32.dll",CharSet=CharSet.Unicode)]public static extern int GetWindowText(IntPtr h,StringBuilder s,int n);
[StructLayout(LayoutKind.Sequential)]public struct R{public int L,T,Right,B;}
[StructLayout(LayoutKind.Sequential)]public struct P{public int X,Y;public P(int x,int y){X=x;Y=y;}}
[DllImport("user32.dll",CharSet=CharSet.Unicode)]public static extern IntPtr FindWindow(string c,string t);
[DllImport("user32.dll")]public static extern bool GetWindowRect(IntPtr h,out R r);
[DllImport("user32.dll")]public static extern bool SetWindowPos(IntPtr h,IntPtr a,int x,int y,int w,int he,uint f);
[DllImport("user32.dll")]public static extern bool ShowWindow(IntPtr h,int n);
[DllImport("user32.dll")]public static extern bool IsWindowVisible(IntPtr h);
[DllImport("user32.dll")]public static extern IntPtr WindowFromPoint(P p);
[DllImport("user32.dll")]public static extern bool SetProcessDpiAwarenessContext(IntPtr c);
}
"@
[void][RingZQA]::SetProcessDpiAwarenessContext([IntPtr](-4));$script:h=[IntPtr]::Zero;[void][RingZQA]::EnumWindows({param($window,$arg)$title=[Text.StringBuilder]::new(256);[void][RingZQA]::GetWindowText($window,$title,256);if($title.ToString() -eq 'Codex 双额度任务栏'){$script:h=$window};return $true},[IntPtr]::Zero);if($h -eq [IntPtr]::Zero){throw 'Ring window missing'};$r=[RingZQA+R]::new();[void][RingZQA]::GetWindowRect($h,[ref]$r)
function Pump([int]$ms){$s=[Diagnostics.Stopwatch]::StartNew();while($s.ElapsedMilliseconds -lt $ms){[Windows.Forms.Application]::DoEvents();Start-Sleep -Milliseconds 10}}
$cover=[Windows.Forms.Form]::new();$cover.FormBorderStyle='None';$cover.ShowInTaskbar=$false;$cover.TopMost=$true;$cover.StartPosition='Manual';$cover.Bounds=[Drawing.Rectangle]::new($r.L,$r.T,60,60);$cover.BackColor=[Drawing.Color]::Gray
try{$cover.Show();[void][RingZQA]::SetWindowPos($cover.Handle,[IntPtr](-1),0,0,0,0,0x13);Pump 1600;$at=[RingZQA]::WindowFromPoint([RingZQA+P]::new(($r.L+30),($r.T+30)));if($at -ne $h){throw 'Ring did not restore topmost order'};'PASS: Ring returns above an overlapping topmost window';$cover.Hide();[void][RingZQA]::ShowWindow($h,0);Pump 1600;if(-not [RingZQA]::IsWindowVisible($h)){throw 'Ring stayed hidden'};'PASS: Hidden ring automatically becomes visible again'}finally{$cover.Dispose();[void][RingZQA]::SetWindowPos($h,[IntPtr](-1),0,0,0,0,0x53)}