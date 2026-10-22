using System;
using System.Collections.Concurrent;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using QuotaData;

namespace DualQuotaBar {
sealed class LocalQuotaFeed:IDisposable {
 readonly ConcurrentDictionary<string,byte> pending=new ConcurrentDictionary<string,byte>(StringComparer.OrdinalIgnoreCase);
 readonly FileSystemWatcher watcher;Task<Snapshot> work;Snapshot latest;
 public LocalQuotaFeed(){
  var home=Environment.GetEnvironmentVariable("CODEX_HOME");if(String.IsNullOrEmpty(home))home=Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),".codex");
  var root=Path.Combine(home,"sessions");if(!Directory.Exists(root))return;
  watcher=new FileSystemWatcher(root,"*.jsonl"){IncludeSubdirectories=true,NotifyFilter=NotifyFilters.LastWrite|NotifyFilters.Size|NotifyFilters.FileName};
  watcher.Changed+=(s,e)=>Queue(e.FullPath);watcher.Created+=(s,e)=>Queue(e.FullPath);watcher.Renamed+=(s,e)=>Queue(e.FullPath);watcher.EnableRaisingEvents=true;
  Task.Run(()=>{try{foreach(var path in Directory.EnumerateFiles(root,"*.jsonl",SearchOption.AllDirectories))if(File.GetLastWriteTimeUtc(path)>DateTime.UtcNow.AddSeconds(-90))Queue(path);}catch(IOException){}catch(UnauthorizedAccessException){}});
 }
 void Queue(string path){pending[path]=0;}
 public Snapshot Poll(){
  if(work!=null&&work.IsCompleted){if(work.Status==TaskStatus.RanToCompletion&&work.Result!=null)latest=Snapshot.Merge(latest,work.Result);work=null;}
  if(work==null&&!pending.IsEmpty)work=Task.Run(()=>ReadPending());return latest;
 }
 Snapshot ReadPending(){Snapshot newest=null;foreach(var item in pending){byte unused;if(!pending.TryRemove(item.Key,out unused))continue;try{using(var stream=new FileStream(item.Key,FileMode.Open,FileAccess.Read,FileShare.ReadWrite|FileShare.Delete)){long start=Math.Max(0,stream.Length-512*1024);stream.Seek(start,SeekOrigin.Begin);using(var reader=new StreamReader(stream,Encoding.UTF8)){if(start>0)reader.ReadLine();string line;while((line=reader.ReadLine())!=null){if(line.IndexOf("\"rate_limits\"",StringComparison.Ordinal)<0)continue;try{var candidate=Snapshot.FromQuotaEvent(line);if(candidate!=null&&candidate.Fetched<=DateTimeOffset.Now.AddSeconds(5)&&DateTimeOffset.Now-candidate.Fetched<=TimeSpan.FromSeconds(90))newest=Snapshot.Merge(newest,candidate);}catch(ArgumentException){}catch(InvalidCastException){}catch(InvalidOperationException){}}}}}catch(IOException){}catch(UnauthorizedAccessException){}}return newest;}
 public void Dispose(){if(watcher!=null)watcher.Dispose();}
}
}
