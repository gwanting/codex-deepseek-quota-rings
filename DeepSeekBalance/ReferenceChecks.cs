using System;
using System.IO;
using DeepSeekBalance;
static class ReferenceChecks {
 static BalanceSnapshot Balance(decimal total){return new BalanceSnapshot{Total=total,Currency="CNY",Fetched=DateTimeOffset.Now};}
 static void Check(string name,bool condition){if(!condition)throw new Exception(name);Console.WriteLine("PASS: "+name);}
 static void Main(){var file=Path.Combine(Path.GetTempPath(),"DeepSeek-reference-"+Guid.NewGuid().ToString("N")+".json");var reference=new BalanceReference(file);reference.Observe(Balance(20));Check("First balance establishes full ring",reference.Percent(Balance(20))==100);reference.Observe(Balance(10));Check("Spending reduces ring rather than its denominator",reference.Percent(Balance(10))==50);reference=new BalanceReference(file);Check("Reference survives restart",reference.Percent(Balance(10))==50);reference.Set(100,"CNY",false);reference.Observe(Balance(20));Check("Manual reference is respected",reference.Percent(Balance(20))==20);Check("Zero balance is an empty ring",reference.Percent(Balance(0))==0);reference.Set(20,"CNY",true);reference.Observe(Balance(30));Check("New higher balance updates automatic reference",reference.Percent(Balance(15))==50);var expired=Balance(15);expired.Fetched=DateTimeOffset.Now.AddMinutes(-4);Check("Expired data does not show a progress",reference.Percent(expired)==null);}
}
