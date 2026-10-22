using System;
using DeepSeekBalance;
static class PricingChecks {
 static void Check(string value,PricingBand expected){if(PricingPeriod.Get(DateTimeOffset.Parse(value))!=expected)throw new Exception(value);Console.WriteLine("PASS: "+value+" "+expected);}
 static void Main(){
  Countdown("2026-10-22T09:00:00+08:00",100,3,PricingBand.OffPeak);
  Countdown("2026-10-22T10:30:00+08:00",50,1.5,PricingBand.OffPeak);
  Countdown("2026-10-22T12:00:00+08:00",100,2,PricingBand.Peak);
  Countdown("2026-10-22T13:00:00+08:00",50,1,PricingBand.Peak);
  Countdown("2026-10-22T16:00:00+08:00",50,2,PricingBand.OffPeak);
  Countdown("2026-10-23T18:00:00+08:00",100,63,PricingBand.Peak);
  Countdown("2026-10-25T01:30:00+08:00",50,31.5,PricingBand.Peak);
  var holiday=PricingPeriod.Countdown(DateTimeOffset.Parse("2026-10-01T10:00:00+08:00"));
  if(holiday==null||holiday.End!=DateTimeOffset.Parse("2026-10-08T09:00:00+08:00"))throw new Exception("Holiday transition");
  if(PricingPeriod.Countdown(DateTimeOffset.Parse("2027-01-01T10:00:00+08:00"))!=null)throw new Exception("Unknown calendar countdown");
  if(PricingPeriod.Countdown(DateTimeOffset.Parse("2026-12-31T18:00:00+08:00"))!=null)throw new Exception("Unknown next transition");
  Check("2026-10-22T08:59:59+08:00",PricingBand.OffPeak);Check("2026-10-22T09:00:00+08:00",PricingBand.Peak);
  Check("2026-10-22T11:59:59+08:00",PricingBand.Peak);Check("2026-10-22T12:00:00+08:00",PricingBand.OffPeak);
  Check("2026-10-22T13:59:59+08:00",PricingBand.OffPeak);Check("2026-10-22T14:00:00+08:00",PricingBand.Peak);
  Check("2026-10-22T17:59:59+08:00",PricingBand.Peak);Check("2026-10-22T18:00:00+08:00",PricingBand.OffPeak);
  Check("2026-10-24T10:00:00+08:00",PricingBand.OffPeak);Check("2026-10-10T10:00:00+08:00",PricingBand.OffPeak);
  Check("2026-10-01T10:00:00+08:00",PricingBand.OffPeak);Check("2026-10-22T01:00:00Z",PricingBand.Peak);
  Check("2027-01-01T10:00:00+08:00",PricingBand.Unknown);
 }
 static void Countdown(string value,decimal percent,double hours,PricingBand next){var result=PricingPeriod.Countdown(DateTimeOffset.Parse(value));if(result==null||Math.Abs(result.Percent-percent)>0.00001m||Math.Abs(result.Remaining.TotalHours-hours)>0.00001||result.Next!=next)throw new Exception("Countdown: "+value);Console.WriteLine("PASS countdown: "+value);}
}
