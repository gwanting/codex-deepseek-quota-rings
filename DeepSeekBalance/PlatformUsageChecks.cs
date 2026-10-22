using System;
using DeepSeekBalance;
static class PlatformUsageChecks {
 static void Check(string name,bool value){if(!value)throw new Exception(name);Console.WriteLine("PASS: "+name);}
 static string Wrap(string body){return "{\"code\":0,\"data\":{\"biz_code\":0,\"biz_data\":"+body+"}}";}
 static void Main(){
  var now=DateTimeOffset.UtcNow;var local=now.ToOffset(TimeSpan.FromHours(8));long start=(long)(new DateTimeOffset(local.Date,TimeSpan.FromHours(8))-new DateTimeOffset(1970,1,1,0,0,0,TimeSpan.Zero)).TotalSeconds;
  string item="{\"time\":"+start+",\"usage\":{\"PROMPT_CACHE_HIT_TOKEN\":\"10\",\"PROMPT_CACHE_MISS_TOKEN\":\"20\",\"RESPONSE_TOKEN\":\"30\",\"REQUEST\":\"4\"}}";
  string old=item.Replace(start.ToString(),(start-86400).ToString());string tomorrow=item.Replace(start.ToString(),(start+86400).ToString());
  var amount=Wrap("{\"series\":[{\"api_key\":{\"name\":\"test-a\"},\"buckets\":["+item+","+old+","+tomorrow+"]},{\"buckets\":["+item+"]}]}");
  var costs=Wrap("{\"data\":[{\"currency\":\"CNY\",\"series\":[{\"buckets\":[{\"time\":"+start+",\"cost\":\"0.015\"},{\"time\":"+(start-86400)+",\"cost\":\"9\"}]},{\"buckets\":[{\"time\":"+start+",\"cost\":\"0.025\"}]}]}]}");
  var value=PlatformUsage.Parse(amount,costs,now);Check("All keys combined without counting REQUEST",value.Tokens==120);Check("Only today's cost; no price estimate",value.Cost==0.040m&&value.Currency=="CNY");Check("Beijing day and fresh timestamp",value.Day==local.ToString("yyyy-MM-dd")&&value.Current);
  var withFunctions=costs.Replace("\"currency\":\"CNY\",","\"currency\":\"CNY\",\"function_series\":[{\"buckets\":[{\"time\":"+start+",\"cost\":\"0.06\"}]}],");Check("Official function charges included",PlatformUsage.Parse(amount,withFunctions,now).Cost==0.100m);
  value.Fetched=now.AddMinutes(-6);Check("Stale stats are hidden",!value.Current);value.Fetched=now;value.Day=local.AddDays(-1).ToString("yyyy-MM-dd");Check("Yesterday stats are hidden",!value.Current);
  var midnight=PlatformUsage.Parse(amount,costs,new DateTimeOffset(local.Date,TimeSpan.FromHours(8)));Check("Inclusive Beijing midnight",midnight.Tokens==120);
  bool rejected=false;try{PlatformUsage.Parse("{\"code\":40003}",costs,now);}catch{rejected=true;}Check("Expired login is not zero usage",rejected);
  rejected=false;try{PlatformUsage.Parse(Wrap("{}"),costs,now);}catch{rejected=true;}Check("Unknown schema is not zero usage",rejected);
  var zero=PlatformUsage.Parse(Wrap("{\"series\":[]}"),Wrap("{\"data\":[{\"currency\":\"CNY\",\"series\":[]}]}"),now);Check("Explicit empty series is valid zero",zero.Tokens==0&&zero.Cost==0);
  var serializer=new System.Web.Script.Serialization.JavaScriptSerializer();var saved=PlatformUsage.ParseSaved(serializer.Serialize(new{value.Day,value.Currency,value.Cost,value.Tokens,Fetched=now.ToString("o"),Error=(string)null}));Check("Cached timestamp roundtrip",saved.Fetched==now&&saved.Tokens==120);
 }
}
