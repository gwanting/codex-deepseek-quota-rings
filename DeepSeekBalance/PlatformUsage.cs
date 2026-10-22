using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Web.Script.Serialization;

namespace DeepSeekBalance {
sealed class PlatformUsage {
 public string Day,Currency,Error;
 public decimal Cost;
 public long Tokens;
 public string TokenText{get{decimal count=Tokens;if(count>=100000000m)return (count/100000000m).ToString("0.00",CultureInfo.InvariantCulture)+"亿";if(count>=10000000m)return (count/10000000m).ToString("0.00",CultureInfo.InvariantCulture)+"千万";if(count>=1000000m)return (count/1000000m).ToString("0.00",CultureInfo.InvariantCulture)+"百万";return Tokens.ToString("N0",CultureInfo.InvariantCulture);}}
 public DateTimeOffset Fetched;
 public bool Current{get{return Error==null&&Day==DateTimeOffset.UtcNow.ToOffset(TimeSpan.FromHours(8)).ToString("yyyy-MM-dd")&&DateTimeOffset.UtcNow-Fetched<TimeSpan.FromMinutes(5);}}
 public static string Folder{get{return Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),"DeepSeekQuotaUsage");}}
 public static string CachePath{get{return Path.Combine(Folder,"today.json");}}
 static Dictionary<string,object> Object(object value){var result=value as Dictionary<string,object>;if(result==null)throw new InvalidDataException("平台用量格式已变化");return result;}
 static IEnumerable List(object value){if(value is string||!(value is IEnumerable))throw new InvalidDataException("平台用量格式已变化");return (IEnumerable)value;}
 static Dictionary<string,object> Data(string json){var root=Object(new JavaScriptSerializer().DeserializeObject(json));if(Convert.ToInt32(root["code"])!=0)throw new InvalidDataException("平台登录已失效");var data=Object(root["data"]);if(Convert.ToInt32(data["biz_code"])!=0)throw new InvalidDataException("平台登录已失效或用量暂不可用");return Object(data["biz_data"]);}
 public static PlatformUsage Parse(string amountJson,string costJson,DateTimeOffset now){
  var local=now.ToOffset(TimeSpan.FromHours(8));var start=new DateTimeOffset(local.Date,TimeSpan.FromHours(8));long first=(long)(start-new DateTimeOffset(1970,1,1,0,0,0,TimeSpan.Zero)).TotalSeconds,last=first+86400;
  var result=new PlatformUsage{Day=local.ToString("yyyy-MM-dd"),Fetched=now};
  foreach(var raw in List(Data(amountJson)["series"]))foreach(var bucketRaw in List(Object(raw)["buckets"])){
   var bucket=Object(bucketRaw);long time=Convert.ToInt64(bucket["time"],CultureInfo.InvariantCulture);if(time<first||time>=last)continue;var usage=Object(bucket["usage"]);
   foreach(string key in new[]{"PROMPT_CACHE_HIT_TOKEN","PROMPT_CACHE_MISS_TOKEN","RESPONSE_TOKEN"}){long count=Convert.ToInt64(usage[key],CultureInfo.InvariantCulture);if(count<0)throw new InvalidDataException("平台 Token 无效");result.Tokens=checked(result.Tokens+count);}
  }
  foreach(var raw in List(Data(costJson)["data"])){
   var currencyData=Object(raw);string currency=Convert.ToString(currencyData["currency"]);if(result.Currency!=null&&result.Currency!=currency)throw new InvalidDataException("多币种消费需要分别显示");result.Currency=currency;
   result.Cost+=SumCost(currencyData["series"],first,last);object functions;if(currencyData.TryGetValue("function_series",out functions))result.Cost+=SumCost(functions,first,last);
  }
  if(String.IsNullOrEmpty(result.Currency))throw new InvalidDataException("平台未返回消费币种");return result;
 }
 static decimal SumCost(object rows,long first,long last){decimal total=0;foreach(var series in List(rows))foreach(var bucketRaw in List(Object(series)["buckets"])){var bucket=Object(bucketRaw);long time=Convert.ToInt64(bucket["time"],CultureInfo.InvariantCulture);if(time<first||time>=last)continue;decimal cost=Convert.ToDecimal(bucket["cost"],CultureInfo.InvariantCulture);if(cost<0)throw new InvalidDataException("平台费用无效");total+=cost;}return total;}
 public static PlatformUsage ParseSaved(string json){var data=new JavaScriptSerializer().Deserialize<Dictionary<string,object>>(json);return new PlatformUsage{Day=Convert.ToString(data["Day"]),Currency=Convert.ToString(data["Currency"]),Cost=Convert.ToDecimal(data["Cost"],CultureInfo.InvariantCulture),Tokens=Convert.ToInt64(data["Tokens"],CultureInfo.InvariantCulture),Fetched=DateTimeOffset.Parse(Convert.ToString(data["Fetched"]),CultureInfo.InvariantCulture),Error=data["Error"]==null?null:Convert.ToString(data["Error"])};}
 public static PlatformUsage Read(){try{return ParseSaved(File.ReadAllText(CachePath));}catch{return null;}}
 public void Save(){Directory.CreateDirectory(Folder);var json=new JavaScriptSerializer().Serialize(new{Day,Currency,Cost,Tokens,Fetched=Fetched.ToString("o"),Error});var temp=CachePath+"."+Guid.NewGuid().ToString("N")+".tmp";try{File.WriteAllText(temp,json);if(File.Exists(CachePath)){try{File.Replace(temp,CachePath,null,true);}catch(Exception error){if(!(error is IOException)&&!(error is UnauthorizedAccessException))throw;using(var writer=new StreamWriter(new FileStream(CachePath,FileMode.Create,FileAccess.Write,FileShare.None))){writer.Write(json);}}}else File.Move(temp,CachePath);}finally{if(File.Exists(temp))File.Delete(temp);}}
}
}
