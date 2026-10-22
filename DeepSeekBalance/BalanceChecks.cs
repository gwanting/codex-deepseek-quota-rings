using System;
using DeepSeekBalance;
static class BalanceChecks {
 static void Check(string name,bool value){if(!value)throw new Exception(name);Console.WriteLine("PASS: "+name);}
 static void Main(){
  var value=BalanceSnapshot.Parse("{\"is_available\":true,\"balance_infos\":[{\"currency\":\"CNY\",\"total_balance\":\"2.01\",\"granted_balance\":\"0.00\",\"topped_up_balance\":\"2.01\"}]}");
  Check("Exact two-decimal amount",value.Amount=="2.01");Check("Recharge share",value.TopShare==100);Check("No grant share",value.GrantShare==0);Check("CNY symbol",value.Unit=="¥");
  value=BalanceSnapshot.Parse("{\"is_available\":false,\"balance_infos\":[{\"currency\":\"CNY\",\"total_balance\":\"0\",\"granted_balance\":\"0\",\"topped_up_balance\":\"0\"}]}");Check("Zero balance remains valid",value.Amount=="0.00"&&value.Current&&!value.Available);Check("Zero has no divided-by-zero",value.TopShare==0&&value.GrantShare==0);
  value.Fetched=DateTimeOffset.Now.AddMinutes(-4);Check("Expired balance is hidden",!value.Current);
  bool rejected=false;try{BalanceSnapshot.Parse("{\"is_available\":true,\"balance_infos\":[]}");}catch(System.IO.InvalidDataException){rejected=true;}Check("Missing amount is unknown",rejected);
 }
}
