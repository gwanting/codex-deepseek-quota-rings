using System;
using System.Drawing;
using System.Drawing.Drawing2D;

namespace DeepSeekBalance {
static class BalanceAmountRenderer {
 public static void Draw(Graphics graphics,string amount,bool muted){
  using(var family=new FontFamily("Segoe UI"))using(var path=new GraphicsPath())using(var format=new StringFormat(StringFormat.GenericTypographic)){
   path.AddString(amount,family,(int)FontStyle.Bold,24,new PointF(0,0),format);var bounds=path.GetBounds();if(bounds.Width<=0||bounds.Height<=0)return;
   float scale=Math.Min(1,Math.Min(18/bounds.Height,35/bounds.Width));
   using(var matrix=new Matrix(scale,0,0,scale,30-(bounds.Left+bounds.Width/2)*scale,30-(bounds.Top+bounds.Height/2)*scale))path.Transform(matrix);
   using(var fill=new SolidBrush(muted?Color.FromArgb(194,203,215):Color.White))graphics.FillPath(fill,path);
  }
 }
}
}
