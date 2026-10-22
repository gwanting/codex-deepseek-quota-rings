using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace DeepSeekBalance {
sealed class BalancePanel:Form {
 Label total=new Label(),top=new Label(),grant=new Label(),state=new Label();Button refresh=new Button();
 protected override bool ShowWithoutActivation{get{return true;}}
 public BalancePanel(Action fetch){Text="DeepSeek 余额详情";FormBorderStyle=FormBorderStyle.None;ShowInTaskbar=false;TopMost=true;AutoScaleMode=AutoScaleMode.None;ClientSize=new Size(370,302);BackColor=Color.FromArgb(24,32,45);ForeColor=Color.White;Font=new Font("Microsoft YaHei UI",18,FontStyle.Regular,GraphicsUnit.Pixel);using(var p=new GraphicsPath()){p.AddArc(0,0,28,28,180,90);p.AddArc(342,0,28,28,270,90);p.AddArc(342,274,28,28,0,90);p.AddArc(0,274,28,28,90,90);p.CloseFigure();Region=new Region(p);}
  var title=new Label{Text="DeepSeek 账户余额",Bounds=new Rectangle(22,18,320,38),Font=new Font("Microsoft YaHei UI",23,FontStyle.Bold,GraphicsUnit.Pixel)};Controls.Add(title);
  total.Bounds=new Rectangle(22,64,326,54);total.Font=new Font("Segoe UI",36,FontStyle.Bold,GraphicsUnit.Pixel);Controls.Add(total);
  top.Bounds=new Rectangle(24,130,320,30);top.ForeColor=Color.FromArgb(88,205,255);Controls.Add(top);grant.Bounds=new Rectangle(24,166,320,30);grant.ForeColor=Color.FromArgb(178,157,255);Controls.Add(grant);
  state.Bounds=new Rectangle(24,205,320,40);state.Font=new Font("Microsoft YaHei UI",15,FontStyle.Regular,GraphicsUnit.Pixel);state.ForeColor=Color.FromArgb(178,191,207);Controls.Add(state);
  refresh.Font=new Font("Microsoft YaHei UI",17,FontStyle.Regular,GraphicsUnit.Pixel);refresh.Text="刷新余额";refresh.Bounds=new Rectangle(232,250,112,35);refresh.FlatStyle=FlatStyle.Flat;refresh.ForeColor=Color.White;refresh.Click+=(s,e)=>fetch();Controls.Add(refresh);
 }
 public void UpdateBalance(BalanceSnapshot value,string error,bool busy){bool valid=value!=null&&value.Current;total.Text=valid?value.Unit+" "+value.Amount:"—";top.Text="充值余额  "+(valid?value.Unit+" "+value.ToppedUp.ToString("0.00"):"未知");grant.Text="赠金余额  "+(valid?value.Unit+" "+value.Granted.ToString("0.00"):"未知");state.Text=busy?"正在查询官方余额…":error!=null?error+"\n"+(value==null?"":"上次成功 "+value.Fetched.LocalDateTime.ToString("HH:mm:ss")):value==null?"正在读取…":"官方余额 · 更新 "+value.Fetched.LocalDateTime.ToString("HH:mm:ss");refresh.Enabled=!busy;}
}
}
