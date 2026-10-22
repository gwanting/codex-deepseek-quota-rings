using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Web.Script.Serialization;

namespace DeepSeekBalance {
sealed class BalanceSnapshot {
 public decimal Total,Granted,ToppedUp;public string Currency;public DateTimeOffset Fetched;public bool Available;
 public decimal TopShare{get{return Total>0?100*ToppedUp/Total:0;}}
 public decimal GrantShare{get{return Total>0?100*Granted/Total:0;}}
 public string Amount{get{return Total.ToString("0.00",CultureInfo.InvariantCulture);}}
 public string Unit{get{return Currency=="CNY"?"¥":Currency=="USD"?"$":Currency;}}
 public bool Current{get{return DateTimeOffset.Now-Fetched<TimeSpan.FromMinutes(3);}}
 public static BalanceSnapshot Parse(string json){
  var doc=DeepSeekClient.Json(json);var list=doc["balance_infos"] as IEnumerable;if(list==null)throw new InvalidDataException("余额响应缺少金额");BalanceSnapshot selected=null;
  foreach(var raw in list){var item=raw as Dictionary<string,object>;if(item==null)continue;var value=new BalanceSnapshot{Currency=Convert.ToString(item["currency"]),Total=Money(item,"total_balance"),Granted=Money(item,"granted_balance"),ToppedUp=Money(item,"topped_up_balance"),Available=Convert.ToBoolean(doc["is_available"]),Fetched=DateTimeOffset.Now};if(value.Currency=="CNY")return value;if(selected==null)selected=value;}
  if(selected==null)throw new InvalidDataException("未返回可用的余额币种");return selected;
 }
 static decimal Money(Dictionary<string,object> item,string key){decimal value;if(!decimal.TryParse(Convert.ToString(item[key],CultureInfo.InvariantCulture),NumberStyles.Number,CultureInfo.InvariantCulture,out value)||value<0)throw new InvalidDataException("余额金额格式无效");return value;}
}
static class DeepSeekClient {
 public static Dictionary<string,object> Json(string text){return new JavaScriptSerializer().Deserialize<Dictionary<string,object>>(text);}
 public static BalanceSnapshot Read(){
  var path=Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),"CodexBar","api_keys.json");
  if(!File.Exists(path))throw new InvalidOperationException("未找到 Win-CodexBar 已保存的 API 密钥");
  var document=Json(File.ReadAllText(path));object raw;
  if(document.TryGetValue("format",out raw)&&Convert.ToString(raw)=="codexbar.secure-file"){
   if(Convert.ToString(document["protection"])!="windows-dpapi-user")throw new InvalidOperationException("不支持的密钥存储格式");
   var plain=ProtectedData.Unprotect(Convert.FromBase64String(Convert.ToString(document["payload"])),null,DataProtectionScope.CurrentUser);try{document=Json(Encoding.UTF8.GetString(plain));}finally{Array.Clear(plain,0,plain.Length);}
  }
  var keys=document["keys"] as Dictionary<string,object>;if(keys==null||!keys.TryGetValue("deepseek",out raw))throw new InvalidOperationException("请先在 Win-CodexBar 中保存 DeepSeek API 密钥");
  var entry=raw as Dictionary<string,object>;string key=entry==null?null:Convert.ToString(entry["api_key"]);if(String.IsNullOrWhiteSpace(key))throw new InvalidOperationException("DeepSeek API 密钥为空");
  ServicePointManager.SecurityProtocol=SecurityProtocolType.Tls12;
  var request=(HttpWebRequest)WebRequest.Create("https://api.deepseek.com/user/balance");request.Method="GET";request.AllowAutoRedirect=false;request.Timeout=15000;request.ReadWriteTimeout=15000;request.Proxy=WebRequest.GetSystemWebProxy();request.Headers[HttpRequestHeader.Authorization]="Bearer "+key;request.UserAgent="DeepSeekBalance/1.0";
  try{using(var response=(HttpWebResponse)request.GetResponse())using(var reader=new StreamReader(response.GetResponseStream())){if(response.StatusCode!=HttpStatusCode.OK)throw new InvalidOperationException("DeepSeek 未返回余额");return BalanceSnapshot.Parse(reader.ReadToEnd());}}
  catch(WebException ex){var response=ex.Response as HttpWebResponse;if(response!=null){int status=(int)response.StatusCode;response.Dispose();throw new InvalidOperationException("DeepSeek 余额查询失败（HTTP "+status+"）");}throw new InvalidOperationException(ex.Status==WebExceptionStatus.Timeout?"余额查询超时":"无法连接 DeepSeek 余额接口");}
 }
}
}
