using System;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Windows.Forms;

static class CodexUsageLauncher {
 [STAThread] static void Main(string[] args) {
  try {
   string codexBar=Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),"Programs","Win-CodexBar","CodexBar.exe");
   string bundled=Path.GetFullPath(Path.Combine(AppDomain.CurrentDomain.BaseDirectory,"..","..","third_party","Win-CodexBar","CodexBar.exe"));
   if(File.Exists(bundled))codexBar=bundled;
   string rings=Path.Combine(AppDomain.CurrentDomain.BaseDirectory,"DualQuotaBar.exe");
   foreach(string path in new[]{codexBar,rings})if(!File.Exists(path))throw new FileNotFoundException("找不到程序："+path);
   StartIfNeeded("CodexBar",codexBar);
   if(args.Length==0||args[0]!="--codexbar-only"){
    StartIfNeeded("DualQuotaBar",rings);
    string deepseek=Path.GetFullPath(Path.Combine(AppDomain.CurrentDomain.BaseDirectory,"..","..","DeepSeekBalance","build","DeepSeekBalance.exe"));
    if(File.Exists(deepseek))StartIfNeeded("DeepSeekBalance",deepseek);
   }
  } catch(Exception ex) {MessageBox.Show(ex.Message,"Codex 用量",MessageBoxButtons.OK,MessageBoxIcon.Error);}
 }
 static void StartIfNeeded(string name,string path) {
  foreach(var process in Process.GetProcessesByName(name))using(process){try{if(string.Equals(process.MainModule.FileName,path,StringComparison.OrdinalIgnoreCase))return;}catch(System.ComponentModel.Win32Exception){}catch(InvalidOperationException){}}
  var start=new ProcessStartInfo(path){UseShellExecute=false,CreateNoWindow=true,WorkingDirectory=Path.GetDirectoryName(path)};
  // Give both processes the Windows proxy selected by the user, including after sign-in.
  var destination=new Uri("https://chatgpt.com");var proxy=WebRequest.GetSystemWebProxy().GetProxy(destination);
  if(proxy!=null&&proxy!=destination){start.EnvironmentVariables["HTTP_PROXY"]=proxy.AbsoluteUri;start.EnvironmentVariables["HTTPS_PROXY"]=proxy.AbsoluteUri;}
  Process.Start(start);
 }
}
