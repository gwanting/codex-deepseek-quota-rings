using System;
using System.Drawing;
using DeepSeekBalance;
static class AlertChecks {
 static void Check(decimal value,int r,int g,int b){if(BalanceAlert.ColorAt(value).ToArgb()!=Color.FromArgb(r,g,b).ToArgb())throw new Exception(value.ToString());Console.WriteLine("PASS: "+value);}
 static void Main(){Check(0,244,77,86);Check(4.99m,244,77,86);Check(5,255,203,65);Check(9.99m,255,203,65);Check(10,66,170,255);Check(20,66,170,255);Check(20.01m,45,215,139);}
}
