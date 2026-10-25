using System;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace DeepSeekBalance {
sealed class PanelMotion:IDisposable {
 readonly Form owner;
 readonly Func<Bitmap> capture;
 readonly Action restore;
 readonly Timer timer=new Timer{Interval=15};
 readonly Stopwatch clock=new Stopwatch();
 MotionSurface surface;
 Rectangle from,to,fullBounds;
 Point anchor;
 bool opening,disposed;
 double duration;
 byte startAlpha;
 public bool Running{get{return timer.Enabled;}}
 public bool TargetVisible{get{return Running?opening:owner.Visible;}}
 public PanelMotion(Form form,Func<Bitmap> snapshot,Action refresh){owner=form;capture=snapshot;restore=refresh;timer.Tick+=(s,e)=>Tick();}
 static Rectangle Small(Rectangle full,Point center){int w=Math.Max(1,(int)Math.Round(full.Width*0.16)),h=Math.Max(1,(int)Math.Round(full.Height*0.16));return new Rectangle(center.X-w/2,center.Y-h/2,w,h);}
 public void Show(Point origin){
  if(disposed)return;if(Running&&opening)return;if(owner.Visible&&!Running)return;
  anchor=origin;fullBounds=owner.Bounds;
  Rectangle start=surface==null?Small(fullBounds,anchor):surface.Bounds;
  if(surface==null)surface=new MotionSurface(capture());
  owner.Hide();Start(start,fullBounds,true);
 }
 public void Hide(Point origin){
  if(disposed||Running&&!opening)return;if(!owner.Visible&&!Running)return;
  anchor=origin;fullBounds=owner.Bounds;
  Rectangle start=surface==null?fullBounds:surface.Bounds;
  if(surface==null)surface=new MotionSurface(capture());
  if(!surface.HasFrame)surface.Render(start,255);surface.Show();owner.Hide();Start(start,Small(fullBounds,anchor),false);
 }
 void Start(Rectangle start,Rectangle end,bool show){from=start;to=end;opening=show;startAlpha=surface.HasFrame?surface.CurrentAlpha:show?(byte)35:(byte)255;duration=show?210:160;clock.Restart();surface.Render(from,startAlpha);if(!surface.Visible)surface.Show();timer.Start();}
 void Tick(){
  if(disposed||owner.IsDisposed){Dispose();return;}
  double fraction=Math.Min(1,clock.Elapsed.TotalMilliseconds/duration),ease=opening?1-Math.Pow(1-fraction,3):fraction*fraction;
  var bounds=new Rectangle(Mix(from.X,to.X,ease),Mix(from.Y,to.Y,ease),Math.Max(1,Mix(from.Width,to.Width,ease)),Math.Max(1,Mix(from.Height,to.Height,ease)));
  surface.Render(bounds,(byte)Math.Max(0,Math.Min(255,startAlpha+((opening?255:0)-startAlpha)*ease)));
  if(fraction<1)return;
  timer.Stop();clock.Stop();
  if(opening){owner.Bounds=fullBounds;restore();owner.Show();owner.Update();}
  surface.Dispose();surface=null;
 }
 static int Mix(int a,int b,double amount){return (int)Math.Round(a+(b-a)*amount);}
 public void Dispose(){if(disposed)return;disposed=true;timer.Stop();timer.Dispose();if(surface!=null){surface.Dispose();surface=null;}}
}

// A temporary, non-activating bitmap surface scales the whole panel, including text.
// It never changes the live controls' sizes, fonts or hit-test rectangles.
sealed class MotionSurface:Form {
 readonly Bitmap snapshot;
 public bool HasFrame{get;private set;}
 public byte CurrentAlpha{get;private set;}
 [StructLayout(LayoutKind.Sequential)]struct NativePoint{public int X,Y;public NativePoint(int x,int y){X=x;Y=y;}}
 [StructLayout(LayoutKind.Sequential)]struct NativeSize{public int Width,Height;public NativeSize(int width,int height){Width=width;Height=height;}}
 [StructLayout(LayoutKind.Sequential,Pack=1)]struct Blend{public byte Operation,Flags,Alpha,Format;}
 [DllImport("user32.dll")]static extern IntPtr GetDC(IntPtr window);
 [DllImport("user32.dll")]static extern int ReleaseDC(IntPtr window,IntPtr dc);
 [DllImport("gdi32.dll")]static extern IntPtr CreateCompatibleDC(IntPtr dc);
 [DllImport("gdi32.dll")]static extern IntPtr SelectObject(IntPtr dc,IntPtr item);
 [DllImport("gdi32.dll")]static extern bool DeleteObject(IntPtr item);
 [DllImport("gdi32.dll")]static extern bool DeleteDC(IntPtr dc);
 [DllImport("user32.dll")]static extern bool UpdateLayeredWindow(IntPtr window,IntPtr screen,ref NativePoint destination,ref NativeSize size,IntPtr source,ref NativePoint origin,uint key,ref Blend blend,uint flags);
 protected override bool ShowWithoutActivation{get{return true;}}
 protected override CreateParams CreateParams{get{var p=base.CreateParams;p.ExStyle|=0x08000000|0x80|0x80000|0x20;return p;}}
 public MotionSurface(Bitmap image){snapshot=image;Text="DeepSeek 面板动画";FormBorderStyle=FormBorderStyle.None;ShowInTaskbar=false;TopMost=true;StartPosition=FormStartPosition.Manual;}
 public void Render(Rectangle bounds,byte alpha){
  HasFrame=true;CurrentAlpha=alpha;
  Bounds=bounds;using(var frame=new Bitmap(bounds.Width,bounds.Height,PixelFormat.Format32bppPArgb)){
   using(var graphics=Graphics.FromImage(frame)){graphics.Clear(Color.Transparent);graphics.InterpolationMode=InterpolationMode.HighQualityBicubic;graphics.PixelOffsetMode=PixelOffsetMode.HighQuality;using(var attributes=new ImageAttributes()){attributes.SetWrapMode(WrapMode.TileFlipXY);graphics.DrawImage(snapshot,new Rectangle(0,0,frame.Width,frame.Height),0,0,snapshot.Width,snapshot.Height,GraphicsUnit.Pixel,attributes);}}
   IntPtr screen=GetDC(IntPtr.Zero),memory=CreateCompatibleDC(screen),bitmap=frame.GetHbitmap(Color.FromArgb(0)),old=SelectObject(memory,bitmap);
   try{var destination=new NativePoint(bounds.X,bounds.Y);var origin=new NativePoint(0,0);var size=new NativeSize(bounds.Width,bounds.Height);var blend=new Blend{Alpha=alpha,Format=1};UpdateLayeredWindow(Handle,screen,ref destination,ref size,memory,ref origin,0,ref blend,2);}finally{SelectObject(memory,old);DeleteObject(bitmap);DeleteDC(memory);ReleaseDC(IntPtr.Zero,screen);}
  }
 }
 protected override void Dispose(bool disposing){if(disposing)snapshot.Dispose();base.Dispose(disposing);}
}
}
