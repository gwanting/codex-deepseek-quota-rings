using System;
using System.Drawing;
using System.Diagnostics;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace DeepSeekBalance {
sealed class BalancePanel:Form {
 Label total=new Label(),top=new Label(),grant=new Label(),state=new Label(),todayCost=new Label(),todayTokens=new Label(),usageState=new Label();Button refresh=new Button();
 protected override bool ShowWithoutActivation{get{return true;}}
 public BalancePanel(Action fetch,Action login){Text="DeepSeek 余额详情";FormBorderStyle=FormBorderStyle.None;ShowInTaskbar=false;TopMost=true;AutoScaleMode=AutoScaleMode.None;ClientSize=new Size(370,392);BackColor=Color.FromArgb(24,32,45);ForeColor=Color.White;Font=new Font("Microsoft YaHei UI",18,FontStyle.Regular,GraphicsUnit.Pixel);using(var p=new GraphicsPath()){p.AddArc(0,0,28,28,180,90);p.AddArc(342,0,28,28,270,90);p.AddArc(342,364,28,28,0,90);p.AddArc(0,364,28,28,90,90);p.CloseFigure();Region=new Region(p);}
  var title=new Label{Text="DeepSeek 账户余额",Bounds=new Rectangle(22,18,320,38),Font=new Font("Microsoft YaHei UI",23,FontStyle.Bold,GraphicsUnit.Pixel)};Controls.Add(title);
  total.Bounds=new Rectangle(22,64,326,54);total.Font=new Font("Segoe UI",36,FontStyle.Bold,GraphicsUnit.Pixel);Controls.Add(total);
  top.Bounds=new Rectangle(24,130,320,30);top.ForeColor=Color.FromArgb(88,205,255);Controls.Add(top);grant.Bounds=new Rectangle(24,166,320,30);grant.ForeColor=Color.FromArgb(178,157,255);Controls.Add(grant);
  todayCost.Bounds=new Rectangle(24,205,320,30);Controls.Add(todayCost);todayTokens.Bounds=new Rectangle(24,239,320,30);Controls.Add(todayTokens);
  usageState.Bounds=new Rectangle(24,274,320,24);usageState.Font=new Font("Microsoft YaHei UI",13,FontStyle.Regular,GraphicsUnit.Pixel);usageState.ForeColor=Color.FromArgb(178,191,207);Controls.Add(usageState);
  state.Bounds=new Rectangle(24,303,320,38);state.Font=new Font("Microsoft YaHei UI",14,FontStyle.Regular,GraphicsUnit.Pixel);state.ForeColor=Color.FromArgb(178,191,207);Controls.Add(state);
  refresh.Font=new Font("Microsoft YaHei UI",16,FontStyle.Regular,GraphicsUnit.Pixel);refresh.Text="刷新";refresh.Bounds=new Rectangle(248,346,96,32);refresh.FlatStyle=FlatStyle.Flat;refresh.ForeColor=Color.White;refresh.Click+=(s,e)=>fetch();Controls.Add(refresh);
  var signIn=new Button{Text="登录用量",Bounds=new Rectangle(136,346,96,32),FlatStyle=FlatStyle.Flat,Font=new Font("Microsoft YaHei UI",16,FontStyle.Regular,GraphicsUnit.Pixel)};signIn.Click+=(s,e)=>login();Controls.Add(signIn);
  var recharge=new Button{Text="去充值",Bounds=new Rectangle(24,346,96,32),FlatStyle=FlatStyle.Flat,BackColor=Color.FromArgb(36,114,146),ForeColor=Color.White,Font=new Font("Microsoft YaHei UI",16,FontStyle.Regular,GraphicsUnit.Pixel),Cursor=Cursors.Hand};recharge.Click+=(s,e)=>{try{Process.Start(new ProcessStartInfo("https://platform.deepseek.com/top_up"){UseShellExecute=true});}catch(Exception){MessageBox.Show(this,"无法打开浏览器，请访问 https://platform.deepseek.com/top_up 充值。","打开充值页面失败",MessageBoxButtons.OK,MessageBoxIcon.Information);}};Controls.Add(recharge);
 }
 public void UpdateBalance(BalanceSnapshot value,string error,bool busy,PlatformUsage usage){bool valid=value!=null&&value.Current;total.Text=valid?value.Unit+" "+value.Amount:"—";top.Text="充值余额  "+(valid?value.Unit+" "+value.ToppedUp.ToString("0.00"):"未知");grant.Text="赠金余额  "+(valid?value.Unit+" "+value.Granted.ToString("0.00"):"未知");state.Text=busy?"正在查询官方余额…":error!=null?error+"\n"+(value==null?"":"上次成功 "+value.Fetched.LocalDateTime.ToString("HH:mm:ss")):value==null?"正在读取…":"余额更新 "+value.Fetched.LocalDateTime.ToString("HH:mm:ss");refresh.Enabled=!busy;
  bool known=usage!=null&&usage.Current;string unit=known?(usage.Currency=="CNY"?"¥":usage.Currency=="USD"?"$":usage.Currency+" "):"";todayCost.Text="今日消费  "+(known?unit+usage.Cost.ToString("0.00"):"—");todayTokens.Text="今日 Token  "+(known?usage.TokenText:"—");usageState.Text=known?"官方账户全量 · 北京时间 · "+usage.Fetched.ToOffset(TimeSpan.FromHours(8)).ToString("HH:mm"):usage==null?"点击“登录用量”接入账户统计":usage.Error??"用量已过期，正在刷新";
 }
}
}
