using System;
using System.Drawing;
using System.Diagnostics;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using System.Runtime.InteropServices;

namespace DeepSeekBalance {
sealed class BalancePanel:Form {
 [StructLayout(LayoutKind.Sequential)]struct GlassMargins{public int Left,Right,Top,Bottom;}
 [DllImport("dwmapi.dll")]static extern int DwmSetWindowAttribute(IntPtr window,int attribute,ref int value,int size);
 [DllImport("dwmapi.dll")]static extern int DwmExtendFrameIntoClientArea(IntPtr window,ref GlassMargins margins);
 [StructLayout(LayoutKind.Sequential)]struct AccentPolicy{public int State,Flags,Color,Animation;}
 [StructLayout(LayoutKind.Sequential)]struct CompositionData{public int Attribute;public IntPtr Data;public int Size;}
 [DllImport("user32.dll")]static extern int SetWindowCompositionAttribute(IntPtr window,ref CompositionData data);
 [DllImport("user32.dll")]static extern IntPtr SendMessage(IntPtr window,int message,IntPtr wparam,IntPtr lparam);
 bool glass;
 readonly PanelAppearance appearance=PanelAppearance.Load();
 Panel appearanceView;ComboBox theme;TransparencySlider transparency;Label transparencyText,savedText;bool syncing;
 PanelMotion motion;
 PanelBackdrop backdrop;bool suppressBackdrop;
 public bool IsOpen{get{return motion==null?Visible:motion.TargetVisible;}}
 public bool IsPanelShown{get{return Visible||(motion!=null&&motion.Running);}}
 public void ShowAnimated(Point anchor){try{if(motion==null)motion=new PanelMotion(this,CaptureMotionFrame,SyncBackdrop);motion.Show(anchor);}catch{if(motion!=null)motion.Dispose();motion=null;Opacity=1;ApplyAppearance();Show();}}
 public void HideAnimated(Point anchor){try{if(motion==null){Hide();return;}motion.Hide(anchor);}catch{if(motion!=null)motion.Dispose();motion=null;Hide();}}
 Bitmap CaptureMotionFrame(){
  bool wasVisible=Visible;double originalOpacity=Opacity;Bitmap raw=null;
  suppressBackdrop=true;try{if(!wasVisible){Opacity=0;Show();}raw=new Bitmap(Width,Height,System.Drawing.Imaging.PixelFormat.Format24bppRgb);DrawToBitmap(raw,new Rectangle(0,0,Width,Height));}finally{if(!wasVisible)Hide();Opacity=originalOpacity;suppressBackdrop=false;SyncBackdrop();}
  try{raw.MakeTransparent(Color.Black);var frame=new Bitmap(Width,Height,System.Drawing.Imaging.PixelFormat.Format32bppPArgb);using(var graphics=Graphics.FromImage(frame)){
   graphics.Clear(Color.Transparent);graphics.SmoothingMode=SmoothingMode.AntiAlias;
   if(appearance.Transparency<100){int alpha=appearance.BackgroundAlpha;Color tint=appearance.Light?Color.FromArgb(alpha,245,248,252):Color.FromArgb(alpha,24,32,45);using(var shape=new GraphicsPath()){shape.AddArc(0,0,28,28,180,90);shape.AddArc(Width-28,0,28,28,270,90);shape.AddArc(Width-28,Height-28,28,28,0,90);shape.AddArc(0,Height-28,28,28,90,90);shape.CloseFigure();using(var fill=new SolidBrush(tint))graphics.FillPath(fill,shape);}}
   graphics.DrawImageUnscaled(raw,0,0);
  }return frame;}finally{raw.Dispose();}
 }
 protected override void Dispose(bool disposing){if(disposing){if(motion!=null){motion.Dispose();motion=null;}if(backdrop!=null){backdrop.Dispose();backdrop=null;}}base.Dispose(disposing);}
 protected override void OnVisibleChanged(EventArgs e){base.OnVisibleChanged(e);SyncBackdrop();}
 protected override void OnLocationChanged(EventArgs e){base.OnLocationChanged(e);SyncBackdrop();}
 protected override void OnSizeChanged(EventArgs e){base.OnSizeChanged(e);SyncBackdrop();}
 void SyncBackdrop(){if(backdrop==null||backdrop.IsDisposed)return;if(!Visible||suppressBackdrop||appearance.BackgroundAlpha==0){backdrop.Hide();return;}backdrop.Configure(appearance.Light,appearance.Tint,appearance.BackgroundAlpha,Bounds);backdrop.PlaceBehind(Handle);}
 void ForwardBackdropInput(Point screen){Control hit=this;while(true){var next=hit.GetChildAtPoint(hit.PointToClient(screen),GetChildAtPointSkip.Invisible);if(next==null)break;hit=next;}if(!(hit is TransparencySlider))return;var point=hit.PointToClient(screen);SendMessage(hit.Handle,0x201,new IntPtr(1),new IntPtr((point.X&65535)|((point.Y&65535)<<16)));}
 protected override void OnHandleCreated(EventArgs e){base.OnHandleCreated(e);ApplyAppearance();}
 void ApplyAppearance(){
  // Keep the foreground opaque; fade the complete material in a separate window.
  TransparencyKey=Color.Black;
  if(IsHandleCreated)try{int dark=appearance.Light?0:1,rounded=2,border=unchecked((int)0xFFFFFFFE);DwmSetWindowAttribute(Handle,20,ref dark,4);DwmSetWindowAttribute(Handle,33,ref rounded,4);DwmSetWindowAttribute(Handle,34,ref border,4);var margins=new GlassMargins();var accent=new AccentPolicy();int size=Marshal.SizeOf(accent);IntPtr memory=Marshal.AllocHGlobal(size);try{Marshal.StructureToPtr(accent,memory,false);var data=new CompositionData{Attribute=19,Data=memory,Size=size};SetWindowCompositionAttribute(Handle,ref data);}finally{Marshal.FreeHGlobal(memory);}DwmExtendFrameIntoClientArea(Handle,ref margins);}catch{}
  if(backdrop==null&&!Disposing&&!IsDisposed){backdrop=new PanelBackdrop();backdrop.BackgroundMouseDown+=ForwardBackdropInput;}
  if(backdrop!=null&&!backdrop.IsDisposed&&IsHandleCreated)backdrop.Configure(appearance.Light,appearance.Tint,appearance.BackgroundAlpha,Bounds);
  glass=backdrop!=null&&backdrop.AcrylicEnabled;BackColor=Color.Black;ForeColor=appearance.Light?Color.FromArgb(24,35,49):Color.White;StyleChildren(this);SyncBackdrop();
  top.ForeColor=appearance.Light?Color.FromArgb(0,92,136):Color.FromArgb(88,205,255);grant.ForeColor=appearance.Light?Color.FromArgb(100,64,171):Color.FromArgb(178,157,255);state.ForeColor=usageState.ForeColor=appearance.Light?Color.FromArgb(69,83,103):Color.FromArgb(178,191,207);
  if(transparency!=null){transparency.TrackColor=appearance.Light?Color.FromArgb(136,152,171):Color.FromArgb(105,122,143);transparency.Invalidate();transparencyText.Text="背景透明度  "+appearance.Transparency.ToString("0.#")+"%";}
  Invalidate(true);
 }
 void StyleChildren(Control root){foreach(Control control in root.Controls){control.ForeColor=ForeColor;var label=control as Label;if(label!=null){label.BackColor=Color.Transparent;label.UseCompatibleTextRendering=glass;}var panel=control as Panel;if(panel!=null)panel.BackColor=Color.Transparent;var button=control as Button;if(button!=null){button.UseVisualStyleBackColor=false;button.BackColor=appearance.Light?Color.FromArgb(231,239,247):Color.FromArgb(32,46,62);button.FlatAppearance.BorderColor=appearance.Light?Color.FromArgb(119,143,165):Color.FromArgb(127,151,175);button.FlatAppearance.MouseOverBackColor=appearance.Light?Color.FromArgb(211,228,241):Color.FromArgb(43,66,88);}var combo=control as ComboBox;if(combo!=null)combo.BackColor=appearance.Light?Color.FromArgb(242,247,251):Color.FromArgb(32,46,62);if(control.HasChildren)StyleChildren(control);}}
 void SaveAppearance(){appearance.Save();if(savedText!=null){savedText.Text=appearance.SaveError??"已自动保存";savedText.ForeColor=appearance.SaveError==null?ForeColor:Color.FromArgb(230,95,95);}}
 void PreviewTransparency(){appearance.Transparency=transparency.Value;transparencyText.Text="背景透明度  "+appearance.Transparency.ToString("0.#")+"%";if(backdrop==null){ApplyAppearance();return;}backdrop.PreviewOpacity(appearance.BackgroundAlpha);if(Visible)backdrop.PlaceBehind(Handle);}
 void CommitTransparency(){ApplyAppearance();SaveAppearance();}
 public void OpenAppearance(){
  if(appearanceView==null){appearanceView=new Panel{Dock=DockStyle.Fill,BackColor=Color.Transparent};var heading=new FrostedLabel{Text="外观设置",Bounds=new Rectangle(22,18,240,38),Font=new Font("Microsoft YaHei UI",23,FontStyle.Bold,GraphicsUnit.Pixel)};var back=new Button{Text="返回",Bounds=new Rectangle(286,23,60,30),FlatStyle=FlatStyle.Flat,Font=new Font("Microsoft YaHei UI",15,FontStyle.Regular,GraphicsUnit.Pixel)};back.Click+=(s,e)=>appearanceView.Hide();
   var mode=new FrostedLabel{Text="背景模式",Bounds=new Rectangle(24,83,320,30)};theme=new ComboBox{DropDownStyle=ComboBoxStyle.DropDownList,Bounds=new Rectangle(24,119,320,34)};theme.DrawMode=DrawMode.OwnerDrawFixed;theme.ItemHeight=25;theme.DrawItem+=(s,e)=>{if(e.Index<0)return;e.Graphics.CompositingMode=CompositingMode.SourceCopy;e.Graphics.TextRenderingHint=System.Drawing.Text.TextRenderingHint.AntiAliasGridFit;Color background=appearance.Light?Color.FromArgb(242,247,251):Color.FromArgb(32,46,62);using(var fill=new SolidBrush(background))e.Graphics.FillRectangle(fill,e.Bounds);using(var brush=new SolidBrush(ForeColor))e.Graphics.DrawString(Convert.ToString(theme.Items[e.Index]),theme.Font,brush,e.Bounds.Left+5,e.Bounds.Top+2);};theme.Items.AddRange(new object[]{"深色磨砂","浅色磨砂"});theme.SelectedIndex=appearance.Light?1:0;
   transparencyText=new FrostedLabel{Bounds=new Rectangle(24,185,320,30)};transparency=new TransparencySlider{Bounds=new Rectangle(24,226,320,40),Value=appearance.Transparency};
   var low=new FrostedLabel{Text="不透明",Bounds=new Rectangle(24,269,100,24),Font=new Font("Microsoft YaHei UI",13,FontStyle.Regular,GraphicsUnit.Pixel)};var high=new FrostedLabel{Text="全透明",Bounds=new Rectangle(283,269,61,24),TextAlign=ContentAlignment.TopRight,Font=low.Font};var hint=new FrostedLabel{Text="100% 时关闭磨砂与底色，仅保留内容。\n设置自动保存，下次启动继续使用。",Bounds=new Rectangle(24,309,320,45),Font=low.Font};savedText=new FrostedLabel{Bounds=new Rectangle(24,360,320,24),Font=low.Font};
   low.Cursor=high.Cursor=Cursors.Hand;low.Click+=(s,e)=>{transparency.Value=0;CommitTransparency();};high.Click+=(s,e)=>{transparency.Value=100;CommitTransparency();};appearanceView.Controls.AddRange(new Control[]{heading,back,mode,theme,transparencyText,transparency,low,high,hint,savedText});Controls.Add(appearanceView);
   theme.SelectedIndexChanged+=(s,e)=>{if(syncing)return;appearance.Light=theme.SelectedIndex==1;ApplyAppearance();SaveAppearance();};transparency.ValueChanged+=PreviewTransparency;transparency.ValueCommitted+=CommitTransparency;
  }
  syncing=true;theme.SelectedIndex=appearance.Light?1:0;transparency.Value=appearance.Transparency;syncing=false;ApplyAppearance();appearanceView.Show();appearanceView.BringToFront();
 }
 Label total=new FrostedLabel(),top=new FrostedLabel(),grant=new FrostedLabel(),state=new FrostedLabel(),todayCost=new FrostedLabel(),todayTokens=new FrostedLabel(),usageState=new FrostedLabel();Button refresh=new Button();
 protected override bool ShowWithoutActivation{get{return true;}}
 public BalancePanel(Action fetch,Action login){Text="DeepSeek 余额详情";FormBorderStyle=FormBorderStyle.None;ShowInTaskbar=false;TopMost=true;StartPosition=FormStartPosition.Manual;AutoScaleMode=AutoScaleMode.None;ClientSize=new Size(370,392);BackColor=Color.FromArgb(24,32,45);ForeColor=Color.White;Font=new Font("Microsoft YaHei UI",18,FontStyle.Regular,GraphicsUnit.Pixel);using(var p=new GraphicsPath()){p.AddArc(0,0,28,28,180,90);p.AddArc(342,0,28,28,270,90);p.AddArc(342,364,28,28,0,90);p.AddArc(0,364,28,28,90,90);p.CloseFigure();Region=new Region(p);}
  var title=new FrostedLabel{Text="DeepSeek 账户余额",Bounds=new Rectangle(22,18,268,38),Font=new Font("Microsoft YaHei UI",23,FontStyle.Bold,GraphicsUnit.Pixel)};Controls.Add(title);
  var settings=new Button{Text="设置",Bounds=new Rectangle(294,23,52,30),FlatStyle=FlatStyle.Flat,Font=new Font("Microsoft YaHei UI",14,FontStyle.Regular,GraphicsUnit.Pixel)};settings.Click+=(s,e)=>OpenAppearance();Controls.Add(settings);
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
