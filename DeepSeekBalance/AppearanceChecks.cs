using System;
using System.IO;
using DeepSeekBalance;
static class AppearanceChecks {
 static void Check(string name,bool value){if(!value)throw new Exception(name);Console.WriteLine("PASS: "+name);}
 static void Main(){string folder=Path.Combine(Path.GetTempPath(),"DeepSeekAppearance-"+Guid.NewGuid().ToString("N"));string path=Path.Combine(folder,"appearance.json");try{
  var value=PanelAppearance.Load(path);Check("Default matches existing dark transparency",!value.Light&&value.Transparency==60);value.Light=true;value.Transparency=81.3;value.Save();Check("Settings saved",value.SaveError==null);var reopened=PanelAppearance.Load(path);Check("Theme and fractional transparency survive restart",reopened.Light&&reopened.Transparency==81.3);
  File.WriteAllText(path,"{\"light\":false,\"transparency\":150}");Check("Out-of-range transparency is clamped",PanelAppearance.Load(path).Transparency==100);File.WriteAllText(path,"bad json");Check("Corrupt settings use readable defaults",PanelAppearance.Load(path).Transparency==60);
  value.Transparency=100;Check("Maximum transparency has zero tint",value.Tint==0);value.Transparency=99.9;Check("Below maximum retains acrylic tint compatibility",((uint)value.Tint>>24)==1);value.Transparency=0;Check("Minimum transparency is an opaque tint",((uint)value.Tint>>24)==255);
  value.Transparency=99;Check("99 percent leaves about one percent background",value.BackgroundAlpha==3);value.Transparency=100;Check("100 percent leaves zero background",value.BackgroundAlpha==0);value.Transparency=95;Check("95 percent remains close to clear",value.BackgroundAlpha==13);
 }finally{if(File.Exists(path))File.Delete(path);if(Directory.Exists(folder))Directory.Delete(folder);}}
}
