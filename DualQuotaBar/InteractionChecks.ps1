Add-Type @"
using System;using System.Text;using System.Runtime.InteropServices;
public class RingClickQA{
 [DllImport("user32.dll")]public static extern IntPtr SetThreadDpiAwarenessContext(IntPtr c);
 public delegate bool CB(IntPtr h,IntPtr p);
 [StructLayout(LayoutKind.Sequential)]public struct P{public int X,Y;public P(int x,int y){X=x;Y=y;}}
 [StructLayout(LayoutKind.Sequential)]public struct R{public int L,T,Right,B;}
 [DllImport("user32.dll")]public static extern bool EnumWindows(CB c,IntPtr p);
 [DllImport("user32.dll",CharSet=CharSet.Unicode)]public static extern int GetWindowText(IntPtr h,StringBuilder s,int n);
 [DllImport("user32.dll")]public static extern bool GetWindowRect(IntPtr h,out R r);
 [DllImport("user32.dll")]public static extern bool IsWindowVisible(IntPtr h);
 [DllImport("user32.dll")]public static extern IntPtr WindowFromPoint(P p);
 [DllImport("user32.dll")]public static extern bool GetCursorPos(out P p);
 [DllImport("user32.dll")]public static extern bool SetCursorPos(int x,int y);
 [DllImport("user32.dll")]public static extern void mouse_event(uint flags,uint x,uint y,uint data,UIntPtr extra);
 [DllImport("user32.dll")]public static extern bool ShowWindow(IntPtr h,int n);
 [DllImport("user32.dll",CharSet=CharSet.Unicode)]public static extern IntPtr FindWindow(string c,string t);
}
"@
function Find-Window([string]$title){$script:found=[IntPtr]::Zero;[void][RingClickQA]::EnumWindows({param($h,$p)$text=[Text.StringBuilder]::new(256);[void][RingClickQA]::GetWindowText($h,$text,256);if($text.ToString() -eq $title){$script:found=$h};return $true},[IntPtr]::Zero);return $script:found}
function Click([int]$x,[int]$y){[void][RingClickQA]::SetCursorPos($x,$y);[RingClickQA]::mouse_event(2,0,0,0,[UIntPtr]::Zero);[RingClickQA]::mouse_event(4,0,0,0,[UIntPtr]::Zero)}
function Check([string]$name,[bool]$value){if(-not $value){throw "FAIL: $name"};Write-Output "PASS: $name"}
[void][RingClickQA]::SetThreadDpiAwarenessContext([IntPtr](-4))
$original=[RingClickQA+P]::new(0,0);[void][RingClickQA]::GetCursorPos([ref]$original)
try{
 $bar=Find-Window 'Codex 双额度任务栏';Check 'Ring window exists' ($bar -ne [IntPtr]::Zero)
 $r=[RingClickQA+R]::new();[void][RingClickQA]::GetWindowRect($bar,[ref]$r)

 $panel=Find-Window 'CodexBar';if($panel -ne [IntPtr]::Zero){[void][RingClickQA]::ShowWindow($panel,0)}
 Click ($r.L+30) ($r.T+10);Start-Sleep -Milliseconds 2500
 $panel=Find-Window 'CodexBar';Check 'Transparent ring gap opens panel' ($panel -ne [IntPtr]::Zero -and [RingClickQA]::IsWindowVisible($panel))
 $pr=[RingClickQA+R]::new();[void][RingClickQA]::GetWindowRect($panel,[ref]$pr)
 Click ($pr.L+15) ($pr.T+100);Start-Sleep -Milliseconds 300;Check 'Inside panel stays open' ([RingClickQA]::IsWindowVisible($panel))
 Click ($r.L+30) ($r.T+30);Start-Sleep -Milliseconds 300;Check 'Center click keeps panel open' ([RingClickQA]::IsWindowVisible($panel))
 $tray=[RingClickQA]::FindWindow('Shell_TrayWnd',$null);$tr=[RingClickQA+R]::new();[void][RingClickQA]::GetWindowRect($tray,[ref]$tr)
 Click ($tr.L+2) ($tr.T+2);Start-Sleep -Milliseconds 350;Check 'Outside click dismisses panel' (-not [RingClickQA]::IsWindowVisible($panel))
 [void][RingClickQA]::GetWindowRect($bar,[ref]$r);Click ($r.L+30) ($r.T+22);Start-Sleep -Milliseconds 1500;Check 'Transparent center opens panel' ([RingClickQA]::IsWindowVisible($panel))
 Click ($tr.L+2) ($tr.T+2);Start-Sleep -Milliseconds 200
 foreach($offset in @(@(30,5),@(5,30),@(55,30),@(30,55))){[void][RingClickQA]::GetWindowRect($bar,[ref]$r);Click ($r.L+$offset[0]) ($r.T+$offset[1]);Start-Sleep -Milliseconds 350;Check ('Outer edge '+($offset -join ',')) ([RingClickQA]::IsWindowVisible($panel));Click ($tr.L+2) ($tr.T+2);Start-Sleep -Milliseconds 200}
}finally{[void][RingClickQA]::SetCursorPos($original.X,$original.Y)}