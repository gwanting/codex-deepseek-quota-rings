using System;
using DeepSeekBalance;
static class PricingChecks {
 static void Check(string value,PricingBand expected){if(PricingPeriod.Get(DateTimeOffset.Parse(value))!=expected)throw new Exception(value);Console.WriteLine("PASS: "+value+" "+expected);}
 static void Main(){
  Check("2026-10-22T08:59:59+08:00",PricingBand.OffPeak);Check("2026-10-22T09:00:00+08:00",PricingBand.Peak);
  Check("2026-10-22T11:59:59+08:00",PricingBand.Peak);Check("2026-10-22T12:00:00+08:00",PricingBand.OffPeak);
  Check("2026-10-22T13:59:59+08:00",PricingBand.OffPeak);Check("2026-10-22T14:00:00+08:00",PricingBand.Peak);
  Check("2026-10-22T17:59:59+08:00",PricingBand.Peak);Check("2026-10-22T18:00:00+08:00",PricingBand.OffPeak);
  Check("2026-10-24T10:00:00+08:00",PricingBand.OffPeak);Check("2026-10-10T10:00:00+08:00",PricingBand.OffPeak);
  Check("2026-10-01T10:00:00+08:00",PricingBand.OffPeak);Check("2026-10-22T01:00:00Z",PricingBand.Peak);
  Check("2027-01-01T10:00:00+08:00",PricingBand.Unknown);
 }
}
