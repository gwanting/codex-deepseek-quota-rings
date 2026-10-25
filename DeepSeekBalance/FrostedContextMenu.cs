using System;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace DeepSeekBalance {
sealed class FrostedContextMenu:ContextMenuStrip {
 [DllImport("user32.dll")]static extern bool SetLayeredWindowAttributes(IntPtr window,uint key,byte alpha,uint flags);
 readonly GlassMenuRenderer glassRenderer=new GlassMenuRenderer();
 PanelBackdrop backdrop;
 PanelAppearance appearance;
 public FrostedContextMenu(){
  Text="DeepSeek 操作菜单";AccessibleName="DeepSeek 操作菜单";
  AutoClose=false;ShowImageMargin=false;ShowCheckMargin=false;DropShadowEnabled=false;
  BackColor=Color.Black;Font=new Font("Microsoft YaHei UI",18,FontStyle.Regular,GraphicsUnit.Pixel);
  Padding=new Padding(8);Renderer=glassRenderer;
  ItemAdded+=(s,e)=>e.Item.Padding=new Padding(10,6,12,6);
  ItemClicked+=(s,e)=>Close();
 }
 protected override CreateParams CreateParams{get{var value=base.CreateParams;value.ExStyle|=0x80000;return value;}}
 protected override void OnHandleCreated(EventArgs e){base.OnHandleCreated(e);SetLayeredWindowAttributes(Handle,0,255,1);}
 protected override void OnOpening(CancelEventArgs e){ApplyAppearance(PanelAppearance.Load());base.OnOpening(e);}
 public void ApplyAppearance(PanelAppearance value){appearance=value;glassRenderer.Light=value.Light;ForeColor=value.Light?Color.FromArgb(24,35,49):Color.White;foreach(ToolStripItem item in Items)item.ForeColor=ForeColor;Invalidate();SyncBackdrop();}
 protected override void OnOpened(EventArgs e){base.OnOpened(e);SyncBackdrop();}
 protected override void OnClosed(ToolStripDropDownClosedEventArgs e){if(backdrop!=null)backdrop.Hide();base.OnClosed(e);}
 protected override void OnLocationChanged(EventArgs e){base.OnLocationChanged(e);SyncBackdrop();}
 protected override void OnSizeChanged(EventArgs e){base.OnSizeChanged(e);if(Width>=28&&Height>=28)using(var path=new GraphicsPath()){path.AddArc(0,0,28,28,180,90);path.AddArc(Width-28,0,28,28,270,90);path.AddArc(Width-28,Height-28,28,28,0,90);path.AddArc(0,Height-28,28,28,90,90);path.CloseFigure();Region=new Region(path);}SyncBackdrop();}
 void SyncBackdrop(){
  if(!Visible||appearance==null){if(backdrop!=null)backdrop.Hide();return;}
  if(backdrop==null){backdrop=new PanelBackdrop();backdrop.Text="DeepSeek 菜单背景";backdrop.BackgroundMouseDown+=ClickBackground;backdrop.BackgroundMouseMove+=HoverBackground;}
  backdrop.Configure(appearance.Light,appearance.Tint,appearance.BackgroundAlpha,Bounds);
  backdrop.PlaceBehind(Handle);
 }
 void ClickBackground(Point screen){if(!Visible)return;var item=GetItemAt(PointToClient(screen));if(item!=null&&item.Enabled&&item.Available){item.PerformClick();Close();}}
 void HoverBackground(Point screen){if(!Visible)return;var item=GetItemAt(PointToClient(screen));if(item!=null&&item.Enabled)item.Select();}
 protected override bool ProcessCmdKey(ref Message message,Keys key){if(key==Keys.Escape){Close();return true;}return base.ProcessCmdKey(ref message,key);}
 protected override void Dispose(bool disposing){if(disposing&&backdrop!=null){backdrop.Dispose();backdrop=null;}base.Dispose(disposing);}
}

sealed class GlassMenuRenderer:ToolStripRenderer {
 public bool Light;
 protected override void OnRenderToolStripBackground(ToolStripRenderEventArgs e){e.Graphics.Clear(Color.Black);}
 protected override void OnRenderToolStripBorder(ToolStripRenderEventArgs e){}
 protected override void OnRenderImageMargin(ToolStripRenderEventArgs e){}
 protected override void OnRenderMenuItemBackground(ToolStripItemRenderEventArgs e){
  if(!e.Item.Selected||!e.Item.Enabled)return;
  var bounds=new Rectangle(2,1,Math.Max(1,e.Item.Width-4),Math.Max(1,e.Item.Height-2));
  using(var brush=new SolidBrush(Light?Color.FromArgb(217,233,245):Color.FromArgb(38,65,84))){e.Graphics.SmoothingMode=SmoothingMode.AntiAlias;using(var path=new GraphicsPath()){int r=10;path.AddArc(bounds.Left,bounds.Top,r,r,180,90);path.AddArc(bounds.Right-r,bounds.Top,r,r,270,90);path.AddArc(bounds.Right-r,bounds.Bottom-r,r,r,0,90);path.AddArc(bounds.Left,bounds.Bottom-r,r,r,90,90);path.CloseFigure();e.Graphics.FillPath(brush,path);}}
 }
 protected override void OnRenderItemText(ToolStripItemTextRenderEventArgs e){
  e.Graphics.CompositingMode=CompositingMode.SourceCopy;
  e.Graphics.TextRenderingHint=System.Drawing.Text.TextRenderingHint.AntiAliasGridFit;
  using(var brush=new SolidBrush(e.Item.Enabled?(Light?Color.FromArgb(24,35,49):Color.White):Color.FromArgb(128,140,151)))
  using(var format=new StringFormat{LineAlignment=StringAlignment.Center,HotkeyPrefix=System.Drawing.Text.HotkeyPrefix.Hide})e.Graphics.DrawString(e.Text,e.TextFont,brush,e.TextRectangle,format);
 }
}
}
