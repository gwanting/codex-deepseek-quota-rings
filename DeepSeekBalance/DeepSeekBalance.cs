using System;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;



namespace DeepSeekBalance {
static class Program {
 [DllImport("user32.dll")]static extern bool SetProcessDpiAwarenessContext(IntPtr context);
 [STAThread]static void Main(string[] args){if(args.Length==2&&args[0]=="--check"){try{var result=DeepSeekClient.Read();File.WriteAllText(args[1],new System.Web.Script.Serialization.JavaScriptSerializer().Serialize(new{result.Total,result.Granted,result.ToppedUp,result.Currency,result.Fetched,result.Available}));}catch(Exception ex){File.WriteAllText(args[1],new System.Web.Script.Serialization.JavaScriptSerializer().Serialize(new{error=ex.Message}));Environment.ExitCode=1;}return;}try{SetProcessDpiAwarenessContext(new IntPtr(-4));}catch{}Application.EnableVisualStyles();Application.SetCompatibleTextRenderingDefault(false);bool fresh;using(var mutex=new Mutex(true,"Local\\Wanting.DeepSeekBalance",out fresh)){if(!fresh)return;Application.Run(new Bar());}}
}
class Bar:Form {
 BalanceSnapshot snapshot;BalancePanel details;BalanceReference reference=new BalanceReference(Path.Combine(AppDomain.CurrentDomain.BaseDirectory,"balance-reference.json"));bool busy,closing;string error;ToolTip tip=new ToolTip();System.Windows.Forms.Timer refresh,position;DateTime fetched;string statusText;IntPtr panelHandle,mouseHook;MouseHookCallback mouseCallback;bool ringPressed,ringRightPressed;
 [StructLayout(LayoutKind.Sequential)]public struct Rect{public int Left,Top,Right,Bottom;}
 delegate bool EnumCallback(IntPtr handle,IntPtr param);
 [DllImport("user32.dll",CharSet=CharSet.Unicode)]static extern IntPtr FindWindow(string cls,string title);
 [DllImport("user32.dll",CharSet=CharSet.Unicode)]static extern IntPtr FindWindowEx(IntPtr parent,IntPtr after,string cls,string title);
 [DllImport("user32.dll")]static extern bool GetWindowRect(IntPtr handle,out Rect rect);
 [DllImport("user32.dll")]static extern bool SetWindowPos(IntPtr handle,IntPtr after,int x,int y,int width,int height,uint flags);
 [DllImport("user32.dll")]static extern bool EnumWindows(EnumCallback callback,IntPtr param);
 [DllImport("user32.dll")]static extern uint GetWindowThreadProcessId(IntPtr handle,out uint processId);
 [DllImport("user32.dll",CharSet=CharSet.Unicode)]static extern int GetWindowText(IntPtr handle,StringBuilder text,int count);
 [DllImport("user32.dll")]static extern bool ShowWindow(IntPtr handle,int command);
 [DllImport("user32.dll")]static extern bool IsWindowVisible(IntPtr handle);
 [DllImport("user32.dll")]static extern bool SetForegroundWindow(IntPtr handle);
 delegate IntPtr MouseHookCallback(int code,IntPtr message,IntPtr data);
 [StructLayout(LayoutKind.Sequential)]struct MouseEvent{public NativePoint Point;public uint MouseData,Flags,Time;public IntPtr ExtraInfo;}
 [DllImport("user32.dll")]static extern IntPtr SetWindowsHookEx(int id,MouseHookCallback callback,IntPtr module,uint thread);
 [DllImport("user32.dll")]static extern bool UnhookWindowsHookEx(IntPtr hook);
 [DllImport("user32.dll")]static extern IntPtr CallNextHookEx(IntPtr hook,int code,IntPtr message,IntPtr data);
 [DllImport("kernel32.dll",CharSet=CharSet.Unicode)]static extern IntPtr GetModuleHandle(string name);
 [DllImport("user32.dll")]static extern IntPtr WindowFromPoint(NativePoint point);
 [StructLayout(LayoutKind.Sequential)]struct NativePoint{public int X,Y;public NativePoint(int x,int y){X=x;Y=y;}}
 [StructLayout(LayoutKind.Sequential)]struct NativeSize{public int Width,Height;public NativeSize(int w,int h){Width=w;Height=h;}}
 [StructLayout(LayoutKind.Sequential,Pack=1)]struct Blend{public byte Op,Flags,Alpha,Format;}
 [DllImport("user32.dll")]static extern IntPtr GetDC(IntPtr handle);
 [DllImport("user32.dll")]static extern int ReleaseDC(IntPtr handle,IntPtr dc);
 [DllImport("gdi32.dll")]static extern IntPtr CreateCompatibleDC(IntPtr dc);
 [DllImport("gdi32.dll")]static extern IntPtr SelectObject(IntPtr dc,IntPtr obj);
 [DllImport("gdi32.dll")]static extern bool DeleteObject(IntPtr obj);
 [DllImport("gdi32.dll")]static extern bool DeleteDC(IntPtr dc);
 [DllImport("user32.dll")]static extern bool UpdateLayeredWindow(IntPtr window,IntPtr screen,ref NativePoint destination,ref NativeSize size,IntPtr source,ref NativePoint origin,uint key,ref Blend blend,uint flags);
 protected override bool ShowWithoutActivation{get{return true;}}
 protected override CreateParams CreateParams{get{var p=base.CreateParams;p.ExStyle|=0x08000000|0x80|0x80000;return p;}}
 public Bar(){Text="DeepSeek 余额任务栏";AutoScaleMode=AutoScaleMode.None;StartPosition=FormStartPosition.Manual;FormBorderStyle=FormBorderStyle.None;ShowInTaskbar=false;TopMost=true;DoubleBuffered=true;ClientSize=new Size(60,60);BackColor=Color.FromArgb(25,32,40);Cursor=Cursors.Hand;using(var outline=new GraphicsPath()){outline.AddEllipse(0,0,60,60);Region=new Region(outline);}tip.SetToolTip(this,"DeepSeek 账户余额\n正在读取…");
  var menu=new ContextMenuStrip{Font=new Font("Microsoft YaHei UI",11)};menu.Items.Add("打开 DeepSeek 余额详情",null,(s,e)=>OpenPanel());menu.Items.Add("刷新余额",null,(s,e)=>Fetch());menu.Items.Add("设置满圈金额…",null,(s,e)=>SetReference());menu.Items.Add("以当前余额重新设为满圈",null,(s,e)=>{if(Current()){reference.Set(snapshot.Total,snapshot.Currency,true);UpdateStatus();Render();}});menu.Items.Add("退出 DeepSeek 余额",null,(s,e)=>Close());ContextMenuStrip=menu;
  refresh=new System.Windows.Forms.Timer{Interval=60000};refresh.Tick+=(s,e)=>Fetch();refresh.Start();position=new System.Windows.Forms.Timer{Interval=1000};position.Tick+=(s,e)=>{Place();UpdateStatus();Render();};position.Start();Shown+=(s,e)=>{Place();Render();Fetch();mouseCallback=OnMouse;mouseHook=SetWindowsHookEx(14,mouseCallback,GetModuleHandle(null),0);};MouseEnter+=(s,e)=>{if(snapshot==null||DateTime.Now-fetched>TimeSpan.FromSeconds(5))Fetch();};FormClosed+=(s,e)=>{closing=true;if(details!=null)details.Dispose();if(mouseHook!=IntPtr.Zero)UnhookWindowsHookEx(mouseHook);refresh.Dispose();position.Dispose();tip.Dispose();};
 }
 protected override void WndProc(ref Message m){if(m.Msg==0x21){m.Result=new IntPtr(3);return;}if(m.Msg==0x202){long bits=m.LParam.ToInt64();var p=new Point((short)(bits&65535),(short)((bits>>16)&65535));if(ClientRectangle.Contains(p))BeginInvoke(new Action(OpenPanel));m.Result=IntPtr.Zero;return;}base.WndProc(ref m);}
 void Place(){var task=FindWindow("Shell_TrayWnd",null);Rect t,n;int x,y;if(task!=IntPtr.Zero&&GetWindowRect(task,out t)){var tray=FindWindowEx(task,IntPtr.Zero,"TrayNotifyWnd",null);int right=GetWindowRect(tray,out n)?n.Left-10:t.Right-300;Rect codex;var neighbor=FindWindow(null,"Codex 双额度任务栏");if(neighbor!=IntPtr.Zero&&GetWindowRect(neighbor,out codex))right=codex.Left-8;x=Math.Max(t.Left+10,right-Width);y=t.Top+Math.Max(0,(t.Bottom-t.Top-Height)/2);}else{var r=Screen.PrimaryScreen.WorkingArea;x=r.Right-Width-12;y=r.Bottom-Height-8;}SetWindowPos(Handle,new IntPtr(-1),x,y,0,0,0x11);}
 bool Current(){return snapshot!=null&&snapshot.Current;}
 void UpdateStatus(){bool valid=Current();string state=busy?"正在查询":error!=null?error:valid?"已更新":"余额未知";AccessibleName="DeepSeek 余额 "+(valid?snapshot.Unit+snapshot.Amount:"未知")+"；"+state;string text="DeepSeek 账户余额："+(valid?snapshot.Unit+snapshot.Amount:"未知")+"\n充值余额："+(valid?snapshot.ToppedUp.ToString("0.00"):"未知")+"\n赠金余额："+(valid?snapshot.Granted.ToString("0.00"):"未知")+"\n"+PricingPeriod.Description(DateTimeOffset.UtcNow)+"\n外环：余额进度（峰红／谷绿）；内环：余额预警\n"+state+(snapshot==null?"":"；上次成功 "+snapshot.Fetched.LocalDateTime.ToString("HH:mm:ss"))+(valid&&snapshot.Currency=="CNY"?"\n"+BalanceAlert.Description(snapshot.Total):"")+(!String.IsNullOrEmpty(reference.Currency)?"\n满圈基准："+reference.Amount.ToString("0.00")+" "+reference.Currency:"")+(reference.SaveError==null?"":"\n"+reference.SaveError)+"\n点击查看详情；右键可设置满圈金额";if(text!=statusText){statusText=text;tip.SetToolTip(this,text);}if(details!=null)details.UpdateBalance(snapshot,error,busy);}
 async void Fetch(){if(busy||closing)return;busy=true;refresh.Stop();UpdateStatus();try{var result=await Task.Run(()=>DeepSeekClient.Read());if(closing)return;snapshot=result;reference.Observe(result);fetched=result.Fetched.LocalDateTime;error=null;}catch(Exception ex){if(!closing)error=ex.Message;}finally{busy=false;if(!closing){UpdateStatus();Render();refresh.Interval=error==null?60000:15000;refresh.Start();}}}
 protected override void OnPaint(PaintEventArgs e){Render();}
 void Render(){if(!IsHandleCreated||closing)return;using(var bitmap=new Bitmap(Width,Height,System.Drawing.Imaging.PixelFormat.Format32bppPArgb)){using(var g=Graphics.FromImage(bitmap)){g.Clear(Color.Transparent);g.SmoothingMode=SmoothingMode.AntiAlias;using(var hitSurface=new SolidBrush(Color.FromArgb(1,0,0,0)))g.FillEllipse(hitSurface,0,0,Width,Height);g.TextRenderingHint=System.Drawing.Text.TextRenderingHint.AntiAliasGridFit;DrawRing(g,new RectangleF(4,4,52,52),3.5f,reference.Percent(snapshot),PricingPeriod.ColorAt(DateTimeOffset.UtcNow));DrawRing(g,new RectangleF(9,9,42,42),2f,Current()?100m:(decimal?)null,Current()&&snapshot.Currency=="CNY"?BalanceAlert.ColorAt(snapshot.Total):Color.FromArgb(150,163,180));DrawPercentage(g);if(error!=null||(snapshot!=null&&!Current()))using(var warning=new SolidBrush(Color.FromArgb(244,183,94)))g.FillEllipse(warning,47,5,6,6);}var screen=GetDC(IntPtr.Zero);var memory=CreateCompatibleDC(screen);var image=bitmap.GetHbitmap(Color.FromArgb(0));var old=SelectObject(memory,image);try{var destination=new NativePoint(Left,Top);var origin=new NativePoint(0,0);var size=new NativeSize(Width,Height);var blend=new Blend{Alpha=255,Format=1};UpdateLayeredWindow(Handle,screen,ref destination,ref size,memory,ref origin,0,ref blend,2);}finally{SelectObject(memory,old);DeleteObject(image);DeleteDC(memory);ReleaseDC(IntPtr.Zero,screen);}}}
 void DrawPercentage(Graphics g){BalanceAmountRenderer.Draw(g,Current()?snapshot.Amount:"--",error!=null||!Current());}
 void DrawRing(Graphics g,RectangleF bounds,float thickness,decimal? percent,Color color){using(var track=new Pen(Color.FromArgb(65,127,143,162),thickness))g.DrawEllipse(track,bounds);if(!percent.HasValue)return;float sweep=(float)Math.Max(0,Math.Min(360,percent.Value*3.6m));if(sweep<=0)return;using(var arc=new Pen(color,thickness)){arc.StartCap=LineCap.Round;arc.EndCap=LineCap.Round;if(sweep>=359.99f)g.DrawEllipse(arc,bounds);else g.DrawArc(arc,bounds,-90,sweep);}}
 bool InsideCircle(NativePoint point){double x=point.X-Left-Width/2.0,y=point.Y-Top-Height/2.0;return x*x+y*y<=Width*Width/4.0;}
 IntPtr OnMouse(int code,IntPtr message,IntPtr data){if(code>=0&&!closing){int kind=message.ToInt32();var point=((MouseEvent)Marshal.PtrToStructure(data,typeof(MouseEvent))).Point;if(kind==0x204&&InsideCircle(point)){ringRightPressed=true;return new IntPtr(1);}if(kind==0x205&&ringRightPressed){ringRightPressed=false;if(InsideCircle(point))try{BeginInvoke(new Action(()=>ContextMenuStrip.Show(new Point(point.X,point.Y))));}catch(InvalidOperationException){}return new IntPtr(1);}if(kind==0x201&&InsideCircle(point)){ringPressed=true;return new IntPtr(1);}if(kind==0x202&&ringPressed){ringPressed=false;if(InsideCircle(point))try{BeginInvoke(new Action(OpenPanel));}catch(InvalidOperationException){}return new IntPtr(1);}if(panelHandle!=IntPtr.Zero&&(kind==0x201||kind==0x204||kind==0x207))try{BeginInvoke(new Action(()=>DismissOutside(point)));}catch(InvalidOperationException){}}return CallNextHookEx(mouseHook,code,message,data);}
 void DismissOutside(NativePoint point){if(closing||panelHandle==IntPtr.Zero||!IsWindowVisible(panelHandle))return;if(Region!=null&&Region.IsVisible(point.X-Left,point.Y-Top))return;Rect panelBounds;if(GetWindowRect(panelHandle,out panelBounds)&&point.X>=panelBounds.Left&&point.X<panelBounds.Right&&point.Y>=panelBounds.Top&&point.Y<panelBounds.Bottom)return;uint panelPid,targetPid;GetWindowThreadProcessId(panelHandle,out panelPid);GetWindowThreadProcessId(WindowFromPoint(point),out targetPid);if(targetPid==panelPid)return;ShowWindow(panelHandle,0);}
 IntPtr FindPanel(){return details==null?IntPtr.Zero:details.Handle;}
 void ShowPanel(){if(details==null)return;var area=Screen.FromRectangle(Bounds).WorkingArea;details.Location=new Point(Math.Max(area.Left+8,Math.Min(Left+(Width-details.Width)/2,area.Right-details.Width-8)),Math.Max(area.Top+8,Top-details.Height-8));if(!details.Visible)details.Show();panelHandle=details.Handle;}
 void SetReference(){using(var dialog=new Form{Text="设置满圈金额",ClientSize=new Size(330,175),FormBorderStyle=FormBorderStyle.FixedDialog,MaximizeBox=false,MinimizeBox=false,StartPosition=FormStartPosition.CenterScreen,Font=new Font("Microsoft YaHei UI",16,FontStyle.Regular,GraphicsUnit.Pixel)}){var label=new Label{Text="外环满圈（100%）对应的金额",Bounds=new Rectangle(18,18,295,30)};var input=new NumericUpDown{DecimalPlaces=2,Minimum=0.01m,Maximum=1000000000m,Value=Math.Max(0.01m,Math.Min(1000000000m,reference.Amount)),Bounds=new Rectangle(20,57,286,32),ThousandsSeparator=true};var save=new Button{Text="保存",Bounds=new Rectangle(212,115,94,34),DialogResult=DialogResult.OK};dialog.Controls.AddRange(new Control[]{label,input,save});dialog.AcceptButton=save;if(dialog.ShowDialog()==DialogResult.OK){reference.Set(input.Value,Current()?snapshot.Currency:reference.Currency??"CNY",false);UpdateStatus();Render();}}}
 void OpenPanel(){if(details==null)details=new BalancePanel(Fetch);UpdateStatus();ShowPanel();Fetch();}
}
}