using System;
using System.Globalization;
using System.IO;
using System.Web.Script.Serialization;

namespace DeepSeekBalance {
sealed class BalanceReference {
 readonly string path;public decimal Amount;public string Currency;public bool Automatic=true;public string SaveError;
 public BalanceReference(string file){path=file;try{if(!File.Exists(path))return;var doc=DeepSeekClient.Json(File.ReadAllText(path));decimal amount;if(!decimal.TryParse(Convert.ToString(doc["amount"],CultureInfo.InvariantCulture),NumberStyles.Number,CultureInfo.InvariantCulture,out amount)||amount<0)return;Amount=amount;Currency=Convert.ToString(doc["currency"]);Automatic=Convert.ToBoolean(doc["automatic"]);}catch(IOException){}catch(UnauthorizedAccessException){}catch(ArgumentException){}catch(InvalidOperationException){}catch(System.Collections.Generic.KeyNotFoundException){}}
 public void Observe(BalanceSnapshot value){if(String.IsNullOrEmpty(Currency)||Currency!=value.Currency){Set(value.Total,value.Currency,true);return;}if(Automatic&&value.Total>Amount)Set(value.Total,value.Currency,true);}
 public void Set(decimal amount,string currency,bool automatic){if(amount<0||(!automatic&&amount==0))throw new ArgumentOutOfRangeException("amount");Amount=amount;Currency=currency;Automatic=automatic;SaveError=null;try{File.WriteAllText(path,new JavaScriptSerializer().Serialize(new{amount=Amount,currency=Currency,automatic=Automatic}));}catch(IOException){SaveError="满圈基准未能保存";}catch(UnauthorizedAccessException){SaveError="满圈基准未能保存";}}
 public decimal? Percent(BalanceSnapshot value){if(value==null||!value.Current||value.Currency!=Currency)return null;if(Amount==0)return value.Total==0?0m:(decimal?)null;return Math.Max(0m,Math.Min(100m,value.Total/Amount*100m));}
}
}
