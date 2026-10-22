using System;
using QuotaData;

static class SynchronizationChecks {
 static void Check(string name,bool condition){if(!condition)throw new Exception(name);Console.WriteLine("PASS: "+name);}
 static void Main(){
  var now=DateTimeOffset.Parse("2030-01-01T00:00:00Z");
  var snapshot=Snapshot.Parse(@"{""FetchedAt"":""2030-01-01T00:00:00Z"",""Plan"":""plus"",""Windows"":[{""BucketId"":""other"",""DurationMinutes"":300,""UsedPercent"":2},{""BucketId"":""codex"",""DurationMinutes"":300,""UsedPercent"":69,""ResetsAt"":""2030-01-01T01:00:00Z""},{""BucketId"":""codex"",""DurationMinutes"":10080,""UsedPercent"":40,""ResetsAt"":""2030-01-07T00:00:00Z""}]}");
  var shortWindow=snapshot.Windows.Find(w=>w.Minutes==300);
  Check("Codex bucket only",snapshot.Windows.Count==2);
  Check("69 used means 31 remaining",shortWindow.Percent=="31%");
  Check("Weekly remains independent",snapshot.Windows.Find(w=>w.Minutes==10080).Percent=="60%");
  Check("Fresh result is visible",snapshot.IsCurrent(shortWindow,now.AddSeconds(89)));
  Check("Expired result is hidden",!snapshot.IsCurrent(shortWindow,now.AddSeconds(91)));
  shortWindow.Reset=now.AddSeconds(20);
  Check("Reset invalidates old quota",!snapshot.IsCurrent(shortWindow,now.AddSeconds(20)));
  Check("Unknown quota stays unknown",!snapshot.IsCurrent(null,now));
  shortWindow.Reset=now.AddHours(1);shortWindow.Remaining=0;
  Check("Zero remaining is valid",snapshot.IsCurrent(shortWindow,now));
 }
}
