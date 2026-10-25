using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace DeepSeekBalance {
// Only the material sits in this window. Foreground text and controls remain opaque.
sealed class PanelBackdrop:Form {
 [StructLayout(LayoutKind.Sequential)]struct Margins{public int Left,Right,Top,Bottom;}
 [StructLayout(LayoutKind.Sequential)]struct Accent{public int State,Flags,Color,Animation;}
 [StructLayout(LayoutKind.Sequential)]struct Composition{public int Attribute;public IntPtr Data;public int Size;}
 [DllImport("dwmapi.dll")]static extern int DwmSetWindowAttribute(IntPtr window,int attribute,ref int value,int size);
 [DllImport("dwmapi.dll")]static extern int DwmExtendFrameIntoClientArea(IntPtr window,ref Margins margins);
 [DllImport("user32.dll")]static extern int SetWindowCompositionAttribute(IntPtr window,ref Composition data);
 [DllImport("user32.dll")]static extern bool SetLayeredWindowAttributes(IntPtr window,uint key,byte alpha,uint flags);
 [DllImport("user32.dll")]static extern bool SetWindowPos(IntPtr window,IntPtr after,int x,int y,int width,int height,uint flags);
 bool light;
 int tint;
 byte alpha;
 public bool AcrylicEnabled{get;private set;}
 public event Action<Point> BackgroundMouseDown;
 public event Action<Point> BackgroundMouseMove;
 protected override bool ShowWithoutActivation{get{return true;}}
 protected override CreateParams CreateParams{get{var value=base.CreateParams;value.ExStyle|=0x08000000|0x80|0x80000;return value;}}
 public PanelBackdrop(){Text="DeepSeek 面板背景";FormBorderStyle=FormBorderStyle.None;ShowInTaskbar=false;TopMost=true;BackColor=Color.Black;StartPosition=FormStartPosition.Manual;AutoScaleMode=AutoScaleMode.None;}
 public void Configure(bool isLight,int color,byte opacity,Rectangle bounds){bool changed=light!=isLight||tint!=color||alpha!=opacity;light=isLight;tint=color;alpha=opacity;if(Bounds!=bounds)Bounds=bounds;if(!IsHandleCreated){IntPtr unused=Handle;}else if(changed)Apply();}
 protected override void OnHandleCreated(EventArgs e){base.OnHandleCreated(e);Apply();}
 void Apply(){
  try{int dark=light?0:1,rounded=2,border=unchecked((int)0xFFFFFFFE);DwmSetWindowAttribute(Handle,20,ref dark,4);DwmSetWindowAttribute(Handle,33,ref rounded,4);DwmSetWindowAttribute(Handle,34,ref border,4);var policy=new Accent{State=4,Flags=0,Color=tint};int size=Marshal.SizeOf(policy);IntPtr memory=Marshal.AllocHGlobal(size);try{Marshal.StructureToPtr(policy,memory,false);var data=new Composition{Attribute=19,Data=memory,Size=size};AcrylicEnabled=SetWindowCompositionAttribute(Handle,ref data)!=0;}finally{Marshal.FreeHGlobal(memory);}var margins=new Margins{Left=-1,Right=-1,Top=-1,Bottom=-1};DwmExtendFrameIntoClientArea(Handle,ref margins);}catch{AcrylicEnabled=false;BackColor=light?Color.FromArgb(245,248,252):Color.FromArgb(24,32,45);}
  SetLayeredWindowAttributes(Handle,0,alpha,2);
 }
 public void PlaceBehind(IntPtr foreground){if(alpha==0){Hide();return;}if(!Visible)Show();SetWindowPos(Handle,foreground,Left,Top,Width,Height,0x10|0x200);}
 public void PreviewOpacity(byte value){alpha=value;if(IsHandleCreated)SetLayeredWindowAttributes(Handle,0,alpha,2);if(alpha==0)Hide();}
 protected override void OnMouseDown(MouseEventArgs e){base.OnMouseDown(e);if(e.Button==MouseButtons.Left&&BackgroundMouseDown!=null)BackgroundMouseDown(PointToScreen(e.Location));}
 protected override void OnMouseMove(MouseEventArgs e){base.OnMouseMove(e);if(BackgroundMouseMove!=null)BackgroundMouseMove(PointToScreen(e.Location));}
 protected override void OnSizeChanged(EventArgs e){base.OnSizeChanged(e);if(Width<28||Height<28)return;using(var path=new GraphicsPath()){path.AddArc(0,0,28,28,180,90);path.AddArc(Width-28,0,28,28,270,90);path.AddArc(Width-28,Height-28,28,28,0,90);path.AddArc(0,Height-28,28,28,90,90);path.CloseFigure();Region=new Region(path);}}
}
}
