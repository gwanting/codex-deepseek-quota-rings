using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;
using System.Runtime.InteropServices;
using System.Web.Script.Serialization;
using System.Windows.Forms;

namespace DeepSeekBalance {
sealed class FrostedLabel:Label {
 protected override void OnPaint(PaintEventArgs e){e.Graphics.CompositingMode=CompositingMode.SourceCopy;e.Graphics.TextRenderingHint=System.Drawing.Text.TextRenderingHint.AntiAliasGridFit;using(var brush=new SolidBrush(ForeColor))using(var format=new StringFormat()){format.Alignment=TextAlign==ContentAlignment.TopRight?StringAlignment.Far:StringAlignment.Near;e.Graphics.DrawString(Text,Font,brush,new RectangleF(0,0,Width,Height),format);}}
}
sealed class PanelAppearance {
 public bool Light;
 public double Transparency=60;
 public string SaveError;
 readonly string path;
 PanelAppearance(string file){path=file;}
 public static PanelAppearance Load(){return Load(Path.Combine(PlatformUsage.Folder,"appearance.json"));}
 public static PanelAppearance Load(string file){var value=new PanelAppearance(file);try{var data=new JavaScriptSerializer().Deserialize<Dictionary<string,object>>(File.ReadAllText(file));value.Light=Convert.ToBoolean(data["light"]);double number=Convert.ToDouble(data["transparency"]);if(!Double.IsNaN(number)&&!Double.IsInfinity(number))value.Transparency=Math.Max(0,Math.Min(100,number));}catch{}return value;}
 public int Tint{get{if(Transparency>=100)return 0;int alpha=Math.Max(1,(int)BackgroundAlpha);var color=Light?Color.FromArgb(245,248,252):Color.FromArgb(24,32,45);return unchecked((int)(((uint)alpha<<24)|((uint)color.B<<16)|((uint)color.G<<8)|color.R));}}
 public byte BackgroundAlpha{get{return (byte)Math.Max(0m,Math.Min(255m,Math.Round(255m*(100m-(decimal)Transparency)/100m,0,MidpointRounding.AwayFromZero)));}}
 public void Save(){try{Directory.CreateDirectory(Path.GetDirectoryName(path));File.WriteAllText(path,new JavaScriptSerializer().Serialize(new{light=Light,transparency=Math.Round(Transparency,1)}));SaveError=null;}catch{SaveError="设置未保存，请检查目录写入权限";}}
}

sealed class TransparencySlider:Control {
 [DllImport("user32.dll")]static extern short GetAsyncKeyState(int key);
 [DllImport("user32.dll")]static extern bool GetCursorPos(out NativePoint point);
 [StructLayout(LayoutKind.Sequential)]struct NativePoint{public int X,Y;}
 readonly Timer dragTimer=new Timer{Interval=16};
 bool dragging;
 double value;
 public event Action ValueChanged,ValueCommitted;
 public Color TrackColor=Color.FromArgb(105,122,143),AccentColor=Color.FromArgb(54,183,218);
 public double Value{get{return value;}set{double next=Math.Round(Math.Max(0,Math.Min(100,value)),1);if(Math.Abs(this.value-next)<0.001)return;this.value=next;Invalidate();if(ValueChanged!=null)ValueChanged();}}
 public TransparencySlider(){SetStyle(ControlStyles.UserPaint|ControlStyles.AllPaintingInWmPaint|ControlStyles.OptimizedDoubleBuffer|ControlStyles.SupportsTransparentBackColor|ControlStyles.Selectable,true);BackColor=Color.Transparent;TabStop=true;Cursor=Cursors.Hand;AccessibleName="背景透明度";AccessibleRole=AccessibleRole.Slider;dragTimer.Tick+=(s,e)=>TrackDrag();}
 protected override void OnPaint(PaintEventArgs e){base.OnPaint(e);var g=e.Graphics;g.SmoothingMode=SmoothingMode.AntiAlias;float y=Height/2f,x=10+(Width-20)*(float)(value/100);using(var track=new Pen(TrackColor,5)){track.StartCap=track.EndCap=LineCap.Round;g.DrawLine(track,10,y,Width-10,y);}if(x>10)using(var active=new Pen(AccentColor,5)){active.StartCap=active.EndCap=LineCap.Round;g.DrawLine(active,10,y,x,y);}using(var fill=new SolidBrush(AccentColor))g.FillEllipse(fill,x-8,y-8,16,16);if(Focused)using(var focus=new Pen(AccentColor,1))g.DrawEllipse(focus,x-11,y-11,22,22);}
 void SetFromMouse(int x){Value=100.0*(x-10)/Math.Max(1,Width-20);}
 protected override void OnMouseDown(MouseEventArgs e){base.OnMouseDown(e);if(e.Button==MouseButtons.Left){dragging=true;Focus();Capture=true;SetFromMouse(e.X);dragTimer.Start();}}
 protected override void OnMouseMove(MouseEventArgs e){base.OnMouseMove(e);if(dragging)SetFromMouse(e.X);}
 protected override void OnMouseUp(MouseEventArgs e){base.OnMouseUp(e);if(e.Button==MouseButtons.Left&&dragging){SetFromMouse(e.X);EndDrag();}}
 void TrackDrag(){if(!dragging){dragTimer.Stop();return;}NativePoint point;if(GetCursorPos(out point))SetFromMouse(PointToClient(new Point(point.X,point.Y)).X);if((GetAsyncKeyState(1)&0x8000)==0)EndDrag();}
 void EndDrag(){if(!dragging)return;dragging=false;dragTimer.Stop();Capture=false;if(ValueCommitted!=null)ValueCommitted();}
 protected override void OnVisibleChanged(EventArgs e){base.OnVisibleChanged(e);if(!Visible)EndDrag();}
 protected override void Dispose(bool disposing){if(disposing){dragging=false;dragTimer.Stop();dragTimer.Dispose();}base.Dispose(disposing);}
 protected override bool IsInputKey(Keys key){return key==Keys.Left||key==Keys.Right||key==Keys.Up||key==Keys.Down||key==Keys.Home||key==Keys.End||base.IsInputKey(key);}
 protected override void OnKeyDown(KeyEventArgs e){base.OnKeyDown(e);if(e.KeyCode==Keys.Left||e.KeyCode==Keys.Down)Value-=0.1;else if(e.KeyCode==Keys.Right||e.KeyCode==Keys.Up)Value+=0.1;else if(e.KeyCode==Keys.Home)Value=0;else if(e.KeyCode==Keys.End)Value=100;else return;e.Handled=true;if(ValueCommitted!=null)ValueCommitted();}
 protected override void OnMouseWheel(MouseEventArgs e){base.OnMouseWheel(e);Value+=e.Delta/120.0;if(ValueCommitted!=null)ValueCommitted();}
 protected override void OnGotFocus(EventArgs e){base.OnGotFocus(e);Invalidate();}
 protected override void OnLostFocus(EventArgs e){base.OnLostFocus(e);Invalidate();}
}
}
