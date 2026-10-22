using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Script.Serialization;
using System.Windows.Forms;

namespace QuotaData {
class Window {
 public double? Remaining; public int Minutes; public DateTimeOffset? Reset; public string Name;
 public double? TimeRemaining(DateTimeOffset now) { if(!Reset.HasValue || Minutes<=0)return null; return Math.Max(0,Math.Min(100,(Reset.Value-now).TotalMinutes/Minutes*100)); }
 public string Percent { get { return Remaining.HasValue?Math.Floor(Remaining.Value).ToString("0")+"%":"--"; } }
 public string Countdown(DateTimeOffset now) { if(!Reset.HasValue)return "重置时间未知"; var t=Reset.Value-now; if(t.TotalSeconds<=0)return "已到重置时间，等待刷新确认"; return t.TotalDays>=1?String.Format("距重置 {0}天 {1}小时",(int)t.TotalDays,t.Hours):String.Format("距重置 {0}小时 {1}分钟",(int)t.TotalHours,t.Minutes); }
}
class Snapshot {
 public DateTimeOffset Fetched; public string Plan;public string Source="实时接口"; public List<Window> Windows=new List<Window>();
 public Window Focus { get {return Windows.Where(x=>x.Remaining.HasValue).OrderBy(x=>x.Remaining).FirstOrDefault();} }
 public bool IsCurrent(Window window,DateTimeOffset now){return window!=null&&window.Remaining.HasValue&&Fetched<=now.AddSeconds(5)&&now-Fetched<=TimeSpan.FromSeconds(90)&&(!window.Reset.HasValue||window.Reset.Value>now);}
 public static Snapshot Merge(Snapshot a,Snapshot b){
  if(a==null)return b;if(b==null)return a;var newer=a.Fetched>b.Fetched?a:b;var older=Object.ReferenceEquals(newer,a)?b:a;
  var result=new Snapshot{Fetched=newer.Fetched,Plan=newer.Plan,Source=newer.Source};
  foreach(var window in newer.Windows){var previous=older.Windows.Find(w=>w.Minutes==window.Minutes);var remaining=window.Remaining;if(previous!=null&&window.Reset.HasValue&&previous.Reset==window.Reset&&previous.Remaining.HasValue&&remaining.HasValue)remaining=Math.Min(remaining.Value,previous.Remaining.Value);result.Windows.Add(new Window{Remaining=remaining,Minutes=window.Minutes,Reset=window.Reset,Name=window.Name});}
  return result;
 }
 public static Snapshot Parse(string json) {
  var d=Json.Read(json); var s=new Snapshot(); s.Fetched=DateTimeOffset.Parse(Convert.ToString(d["FetchedAt"])); s.Plan=Convert.ToString(d["Plan"]);
  foreach(var raw in (IEnumerable)d["Windows"]) { var w=(Dictionary<string,object>)raw;var bucket=Json.Text(w,"BucketId");if(!String.IsNullOrEmpty(bucket)&&bucket!="codex")continue; var used=Json.Number(w,"UsedPercent"); var reset=Json.Text(w,"ResetsAt"); DateTimeOffset dt;
   s.Windows.Add(new Window { Remaining=used.HasValue?Math.Max(0,Math.Min(100,100-used.Value)):(double?)null, Minutes=(int)(Json.Number(w,"DurationMinutes")??0), Reset=DateTimeOffset.TryParse(reset,out dt)?dt:(DateTimeOffset?)null, Name=Json.Text(w,"WindowName") }); }
  return s;
 }
 public static Snapshot FromQuotaEvent(string line){
  var record=Json.Read(line);if(Json.Text(record,"type")!="event_msg")return null;
  object raw;if(!record.TryGetValue("payload",out raw))return null;var payload=raw as Dictionary<string,object>;
  if(payload==null||Json.Text(payload,"type")!="token_count"||!payload.TryGetValue("rate_limits",out raw))return null;
  var limits=raw as Dictionary<string,object>;if(limits==null||Json.Text(limits,"limit_id")!="codex")return null;
  DateTimeOffset time;if(!DateTimeOffset.TryParse(Json.Text(record,"timestamp"),out time))return null;
  var result=new Snapshot{Fetched=time,Plan=Json.Text(limits,"plan_type"),Source="Codex 应用额度事件"};
  foreach(var slot in new[]{"primary","secondary"}){if(!limits.TryGetValue(slot,out raw))continue;var quota=raw as Dictionary<string,object>;if(quota==null)continue;var used=Json.Number(quota,"used_percent");var minutes=Json.Number(quota,"window_minutes");var reset=Json.Number(quota,"resets_at");if(!used.HasValue||used.Value<0||used.Value>100||!minutes.HasValue)continue;result.Windows.Add(new Window{Remaining=100-used.Value,Minutes=(int)minutes.Value,Reset=reset.HasValue?new DateTimeOffset(1970,1,1,0,0,0,TimeSpan.Zero).AddSeconds(reset.Value):(DateTimeOffset?)null,Name=slot});}
  return result.Windows.Count==0?null:result;
 }
}
static class Json {
 public static Dictionary<string,object> Read(string s) { return new JavaScriptSerializer{MaxJsonLength=2097152}.Deserialize<Dictionary<string,object>>(s); }
 public static double? Number(Dictionary<string,object> d,string key) { object v; double n; return d.TryGetValue(key,out v)&&v!=null&&Double.TryParse(Convert.ToString(v),out n)&&!Double.IsNaN(n)&&!Double.IsInfinity(n)?n:(double?)null; }
 public static string Text(Dictionary<string,object> d,string key) {object v; return d.TryGetValue(key,out v)&&v!=null?Convert.ToString(v):null;}
}
}
