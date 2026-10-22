using System;
using System.IO;
using System.Threading.Tasks;
using System.Web.Script.Serialization;
using System.Windows.Forms;
using Microsoft.Web.WebView2.Core;
using Microsoft.Web.WebView2.WinForms;

namespace DeepSeekBalance {
static class PlatformBrowserProgram {
 [STAThread]static void Main(string[] args){Application.EnableVisualStyles();Application.SetCompatibleTextRenderingDefault(false);Application.Run(new PlatformBrowser(args.Length>0&&args[0]=="--login"));}
}
sealed class PlatformBrowser:Form {
 readonly bool login;
 readonly WebView2 browser=new WebView2{Dock=DockStyle.Fill};
 readonly Label status=new Label{Dock=DockStyle.Top,Height=48,Text="请在下方官方页面登录与 API 密钥相同的账户。成功后会自动读取今日用量并关闭。",Padding=new Padding(12,8,12,0)};
 readonly Timer poll=new Timer{Interval=2500};
 readonly DateTime started=DateTime.UtcNow;
 bool busy;
 public PlatformBrowser(bool interactive){login=interactive;Text="DeepSeek 官方平台登录 · 账户用量";Width=1000;Height=780;StartPosition=FormStartPosition.CenterScreen;Controls.Add(browser);Controls.Add(status);if(!login){Opacity=0;ShowInTaskbar=false;}Shown+=async(s,e)=>await Initialize();poll.Tick+=async(s,e)=>await Query();FormClosed+=(s,e)=>poll.Dispose();}
 async Task Initialize(){try{var environment=await CoreWebView2Environment.CreateAsync(null,Path.Combine(PlatformUsage.Folder,"WebProfile"));await browser.EnsureCoreWebView2Async(environment);browser.CoreWebView2.Settings.IsPasswordAutosaveEnabled=false;browser.CoreWebView2.Settings.IsGeneralAutofillEnabled=false;browser.CoreWebView2.Settings.AreDevToolsEnabled=false;browser.CoreWebView2.NavigationCompleted+=(s,e)=>{busy=false;};browser.CoreWebView2.WebMessageReceived+=Receive;browser.CoreWebView2.NewWindowRequested+=(s,e)=>{e.Handled=true;};browser.Source=new Uri("https://platform.deepseek.com/usage");poll.Start();}catch{Fail("无法启动官方登录窗口，请检查 WebView2 Runtime");}}
 async Task Query(){
  if(!login&&(DateTime.UtcNow-started).TotalSeconds>35){Fail("官方用量查询超时，请稍后刷新");return;}if(busy||browser.CoreWebView2==null)return;busy=true;
  try{
   var now=DateTimeOffset.UtcNow;var today=now.ToOffset(TimeSpan.FromHours(8)).Date;long start=(long)(new DateTimeOffset(today,TimeSpan.FromHours(8))-new DateTimeOffset(1970,1,1,0,0,0,TimeSpan.Zero)).TotalSeconds;
   string script=File.ReadAllText(Path.Combine(AppDomain.CurrentDomain.BaseDirectory,"PlatformUsageQuery.js")).Replace("__START__",start.ToString(System.Globalization.CultureInfo.InvariantCulture)).Replace("__END__",(start+86400).ToString(System.Globalization.CultureInfo.InvariantCulture));
   await browser.ExecuteScriptAsync(script);
  }catch{busy=false;if(!login)Fail("官方用量暂不可用，请重试或重新登录");}
 }
 void Receive(object sender,CoreWebView2WebMessageReceivedEventArgs args){try{
  var uri=new Uri(args.Source);if(uri.Scheme!="https"||uri.Host!="platform.deepseek.com"||!uri.IsDefaultPort)return;
  var data=new JavaScriptSerializer{MaxJsonLength=16000000}.Deserialize<System.Collections.Generic.Dictionary<string,object>>(args.TryGetWebMessageAsString());
  if(!Convert.ToBoolean(data["ok"])){if(data.ContainsKey("login")&&Convert.ToBoolean(data["login"])){if(!login)Fail("请登录官方平台用量");}else status.Text="官方用量暂不可用，正在重试…";return;}
  var usage=PlatformUsage.Parse(Convert.ToString(data["amount"]),Convert.ToString(data["cost"]),DateTimeOffset.UtcNow);usage.Save();poll.Stop();Close();
 }catch{status.Text="已连接官方页面，用量数据暂不可读取；可稍后重试。";if(!login)Fail("官方用量暂不可用，请稍后刷新");}finally{busy=false;}}
 void Fail(string error){poll.Stop();try{new PlatformUsage{Error=error,Fetched=DateTimeOffset.UtcNow}.Save();}catch{}if(login)MessageBox.Show(this,error,"DeepSeek 用量",MessageBoxButtons.OK,MessageBoxIcon.Information);Close();}
}
}
