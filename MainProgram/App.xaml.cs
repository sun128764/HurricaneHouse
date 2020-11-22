using Newtonsoft.Json;
using System;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Windows;
using System.Windows.Threading;

namespace MainProgram
{
    /// <summary>
    /// App.xaml 的交互逻辑
    /// </summary>
    public partial class App : Application
    {
        private static readonly NLog.Logger Logger = NLog.LogManager.GetCurrentClassLogger();
        public App()
        {
            Current.DispatcherUnhandledException += App_DispatcherUnhandledException;
            AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;
            ThreadPool.QueueUserWorkItem((object state) =>
            {
                //WebMap.HttpGet("http://api.ipstack.com/check?access_key=c4358b1d5570b8a0fdc733e18c1045c6");
                WebAPIUtil.HttpGet("https://api.ipgeolocation.io/ipgeo?apiKey=01d110a5e710445b91306d8d3345657e");
            }, null);
            // Set this code once in App.xaml.cs or application startup
            SciChart.Charting.Visuals.SciChartSurface.SetRuntimeLicenseKey("//iyHPm7S5TmIROHMtSNDZ3aPNDr3ZKdPpDX7i3s5lNmnDFsA+g0fWBQgJLGrD6eUHBTjFul/uBiRmySEo3pH/RP3e81GiQpdqO1GG6+KjtAmPjTKvAa+ULLjfgxxIar19BQfpoNRgxtE0njKqr6w4+PFCg0zEaybGNq+w91eL/9bVo+9tYkSMdG00F/uSvuKpAGirLBj5f7PwDsvdpPrvWATZDzeQV4hZf01lEvecN2iXIlbvh4mmR6JMvodGmDhNveDrIQHXYSl+reIn1L0GIKkH6/63UXvioV5MS8Ir18mW4lLQAtQ+gdRrTgOd6UeppX4/+s/oqVzhGJgQd6nSnxr72dfr/1epVKyr5+omPsn1u8i6wNFFtRWuev0oSLpqwH4Q6um5QcH+2wXEF+bYMeb9rIJ2/Wgm2FNzjeWnN3Ale98QQpnvaQpWcaqi9VqCF/CQXkI14HCBYQktAQ/7+OjQTRSLlao6N3TUCN9SP/y6IDi7UPIBC0XC1/MrZdMTo1TZFFIp44BI70N08qTl63syK9r3/mn/2NBbbh5dv3HZ6TmJDKjiFywJVUt1Zj6hQqf/kQMSFmkfznNDrmT8WuPrbReqdzmb4hIk+/A04RpzhqL9mSxejdZrhQqhG66XT9BQ==");
        }
        private void Application_Startup(object sender, StartupEventArgs e)
        {
            Logger.Info("Program start at" + DateTime.Now.ToLongDateString());
            var mainwindow = new MainWindow();
            mainwindow.Show();
            if (e.Args.Length == 1 && e.Args[0] == "-recover")
            {
                try
                {
                    string file = File.ReadAllText(Format.ConstValues.BakFilePath);
                    Format.ProgramSetting setting = JsonConvert.DeserializeObject<Format.ProgramSetting>(file);
                    mainwindow.InitRecording(setting);
                }
                catch (Exception ex)
                {
                    Logger.Error(ex, "Can not recover the program." + ex.StackTrace);
                }
            }
        }
        //Catch unhandled exception of UI process
        private void App_DispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
        {
            Exception ex = e.Exception;
            Logger.Error(ex, "Unhandled exception of UI process." + ex.StackTrace);
            e.Handled = true;//Keep program running.
        }

        //Catch unhandled exception of UI process (such as Tapis)
        //When this method is called, the process will be stopped.
        private void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            Exception ex = e.ExceptionObject as Exception;
            Logger.Error(ex, "Unhandled exception of UI process" + ex.StackTrace); 
            Thread.Sleep(10000);
            Process.Start(Path.Combine(Environment.CurrentDirectory, "FIT Hurricane House Monitor.exe"), "-recover");
        }
    }
}