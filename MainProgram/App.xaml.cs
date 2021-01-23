using System.Diagnostics;
using System.Threading;
using System.Windows;

namespace MainProgram
{
    /// <summary>
    /// App.xaml 的交互逻辑
    /// </summary>
    public partial class App : Application
    {
        public App()
        {
            ThreadPool.QueueUserWorkItem((object state) =>
            {
                //WebMap.HttpGet("http://api.ipstack.com/check?access_key=c4358b1d5570b8a0fdc733e18c1045c6");
                WebAPIUtil.HttpGet("https://api.ipgeolocation.io/ipgeo?apiKey=01d110a5e710445b91306d8d3345657e");
            }, null);
            // Set this code once in App.xaml.cs or application startup
            SciChart.Charting.Visuals.SciChartSurface.SetRuntimeLicenseKey("WSiYNV5ed+wsx8mTgyFl3zER8MjbGOtnxTU/tt6phukwzhotytEJ0BLdzrUbqHHbBP0ZkBYJIRYT7sVSeNdb/SclGIj8ETTMuPXk75cxYcr4YdWcG19kcvbHWDdZjHjNIibbADkgQG0NBG7ZoutL3KNZZ6jh+TeakVI4NKh8m39XvHGYsZ2q1qEEhsclLhIlGdPiA1nmn/Rvl1w/z/fj7K+ZX00ft/zmAoT/kTBDGeqdKFlb2S272M7TsMkamGLyAjs/b02DcEcJzO4cNgd4F+NKg50/39Gclebg4rhr/jkSobaSKCxYWGz5gz3zcu0Kbn278f0oJdXAPc/jDvROryve5HU4e0axAkQrUeZ1aBfy0XElQL1en+g375hHQlWAh/ebGCS14H/Ma+4oPUp87KeNBd8fpdw0RzxDejw7tjknq56l0ytVl4YNZ8tRMsoJreQLUOx1G9R1QtRm4BjlLGJ4S3ROT6x2QeIiBODdwoCOu/UNPG76AsH4Gx+bCI2ucPxlOpWv3rvAdqFf5YkO9VKT");
        }
        protected override void OnStartup(StartupEventArgs e)
        {
            //e.Args为命令行参数
            //Do something
            
        }
    }
}