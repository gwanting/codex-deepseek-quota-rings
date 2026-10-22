using System;
using System.Collections.Generic;
using System.Drawing;

namespace DeepSeekBalance {
enum PricingBand {OffPeak,Peak,Unknown}
sealed class PricingCountdown {
 public DateTimeOffset Start,End,Now;
 public PricingBand Next;
 public decimal Percent{get{return (decimal)Math.Max(0,Math.Min(100,100*(End-Now).TotalSeconds/(End-Start).TotalSeconds));}}
 public TimeSpan Remaining{get{return End-Now;}}
 public string Description{get{var left=Remaining;return "距"+(Next==PricingBand.Peak?"峰期":"谷期")+"还有 "+((int)left.TotalHours).ToString("00")+":"+left.Minutes.ToString("00")+":"+left.Seconds.ToString("00");}}
}
static class PricingPeriod {
 static readonly HashSet<DateTime> Holidays=CreateHolidays();
 static HashSet<DateTime> CreateHolidays(){var dates=new HashSet<DateTime>();Add(dates,1,1,3);Add(dates,2,15,23);Add(dates,4,4,6);Add(dates,5,1,5);Add(dates,6,19,21);Add(dates,9,25,27);Add(dates,10,1,7);return dates;}
 static void Add(HashSet<DateTime> dates,int month,int first,int last){for(int day=first;day<=last;day++)dates.Add(new DateTime(2026,month,day));}
 public static PricingBand Get(DateTimeOffset instant){
  var beijing=instant.ToOffset(TimeSpan.FromHours(8));var day=beijing.Date;var time=beijing.TimeOfDay;
  if(day.DayOfWeek==DayOfWeek.Saturday||day.DayOfWeek==DayOfWeek.Sunday)return PricingBand.OffPeak;
  if(!((time>=TimeSpan.FromHours(9)&&time<TimeSpan.FromHours(12))||(time>=TimeSpan.FromHours(14)&&time<TimeSpan.FromHours(18))))return PricingBand.OffPeak;
  if(day.Year!=2026)return PricingBand.Unknown;
  return Holidays.Contains(day)?PricingBand.OffPeak:PricingBand.Peak;
 }
 public static Color ColorAt(DateTimeOffset time){var band=Get(time);return band==PricingBand.Peak?Color.FromArgb(244,77,86):band==PricingBand.OffPeak?Color.FromArgb(45,215,139):Color.FromArgb(150,163,180);}
 public static PricingCountdown Countdown(DateTimeOffset instant){
  var now=instant.ToOffset(TimeSpan.FromHours(8));var band=Get(now);if(band==PricingBand.Unknown)return null;
  DateTimeOffset? start=null,end=null;PricingBand next=PricingBand.Unknown;
  // Find actual band changes; midnight, weekends and holidays do not reset a continuous valley.
  for(int day=-32;day<=32;day++){
   foreach(int hour in new[]{0,9,12,14,18}){
   var boundary=new DateTimeOffset(now.Date.AddDays(day).AddHours(hour),TimeSpan.FromHours(8));
   var before=Get(boundary.AddTicks(-1));var after=Get(boundary);if(before==after)continue;
   if(boundary<=now){start=boundary;}else{end=boundary;next=after;break;}
   }
   if(end.HasValue)break;
  }
  if(!start.HasValue||!end.HasValue||next==PricingBand.Unknown)return null;
  return new PricingCountdown{Start=start.Value,End=end.Value,Now=now,Next=next};
 }
 public static string Description(DateTimeOffset time){var band=Get(time);return band==PricingBand.Peak?"高峰时段（北京时间）":band==PricingBand.OffPeak?"谷期／空闲时段（北京时间）":"峰谷时段待确认：节假日表需更新";}
}
}
