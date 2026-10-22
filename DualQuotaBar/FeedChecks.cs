using System;
using System.IO;
using System.Threading;
using System.Web.Script.Serialization;
using DualQuotaBar;
using QuotaData;

static class FeedChecks {
 static string Event(double used,DateTimeOffset time,string bucket){return new JavaScriptSerializer().Serialize(new{timestamp=time.ToString("o"),type="event_msg",payload=new{type="token_count",rate_limits=new{limit_id=bucket,plan_type="plus",primary=new{used_percent=used,window_minutes=300,resets_at=(time.AddHours(1)-new DateTimeOffset(1970,1,1,0,0,0,TimeSpan.Zero)).TotalSeconds},secondary=new{used_percent=71,window_minutes=10080}}}});}
 static void Check(string name,bool condition){if(!condition)throw new Exception(name);Console.WriteLine("PASS: "+name);}
 static Snapshot Wait(LocalQuotaFeed feed,double remaining){var until=DateTime.UtcNow.AddSeconds(5);while(DateTime.UtcNow<until){var result=feed.Poll();if(result!=null&&result.Windows.Find(w=>w.Minutes==300).Remaining==remaining)return result;Thread.Sleep(50);}throw new Exception("Quota event did not propagate");}
 static void Main(){
  var now=DateTimeOffset.UtcNow;
  var parsed=Snapshot.FromQuotaEvent(Event(80,now,"codex"));
  Check("App event maps used to remaining",parsed.Windows.Find(w=>w.Minutes==300).Remaining==20);
  Check("Other quota bucket is ignored",Snapshot.FromQuotaEvent(Event(80,now,"other"))==null);
  Check("Unrelated event is ignored",Snapshot.FromQuotaEvent("{\"type\":\"response_item\"}")==null);
  Check("Event timestamp is preserved",parsed.Fetched==now);
  var laterStale=Snapshot.FromQuotaEvent(Event(80,now.AddSeconds(1),"codex"));var actual=Snapshot.FromQuotaEvent(Event(81,now,"codex"));
  laterStale.Windows[0].Reset=actual.Windows[0].Reset;
  Check("Repeated older quota cannot increase remaining within same window",Snapshot.Merge(actual,laterStale).Windows[0].Remaining==19);
  laterStale.Windows[0].Reset=now.AddHours(6);laterStale.Windows[0].Remaining=100;
  Check("A new quota window can restore remaining",Snapshot.Merge(actual,laterStale).Windows[0].Remaining==100);
  var root=Path.Combine(Path.GetTempPath(),"DualQuotaFeed-check-"+Guid.NewGuid().ToString("N"));Directory.CreateDirectory(Path.Combine(root,"sessions"));Environment.SetEnvironmentVariable("CODEX_HOME",root);
  var path=Path.Combine(root,"sessions","quota.jsonl");File.WriteAllText(path,Event(80,now,"codex")+Environment.NewLine);
  using(var feed=new LocalQuotaFeed()){
   Wait(feed,20);Check("Existing fresh event initializes display",true);
   File.AppendAllText(path,Event(81,DateTimeOffset.UtcNow,"codex")+Environment.NewLine);
   Wait(feed,19);Check("File watcher propagates a new quota event",true);
   File.AppendAllText(path,Event(10,now.AddMinutes(-10),"codex")+Environment.NewLine);
   Thread.Sleep(200);for(int i=0;i<10;i++){feed.Poll();Thread.Sleep(50);}Check("Old event cannot roll back display",feed.Poll().Windows.Find(w=>w.Minutes==300).Remaining==19);
  }
 }
}
