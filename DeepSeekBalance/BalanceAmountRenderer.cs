using System;
using System.Drawing;

namespace DeepSeekBalance {
static class BalanceAmountRenderer {
 public static void Draw(Graphics graphics,decimal? balance,bool muted){
  string amount=balance.HasValue?(balance.Value<1?balance.Value.ToString("0.00",System.Globalization.CultureInfo.InvariantCulture):decimal.Truncate(balance.Value).ToString("0",System.Globalization.CultureInfo.InvariantCulture)):"--";
  using(var format=new StringFormat(StringFormat.GenericTypographic)){
   format.Alignment=StringAlignment.Center;format.LineAlignment=StringAlignment.Center;
   using(var font=new Font("Segoe UI",amount.Length>2?15:19,FontStyle.Bold,GraphicsUnit.Pixel))
   using(var brush=new SolidBrush(muted?Color.FromArgb(151,163,174):Color.FromArgb(240,247,253)))graphics.DrawString(amount,font,brush,new RectangleF(16,18,28,24),format);
  }
 }
}
}
