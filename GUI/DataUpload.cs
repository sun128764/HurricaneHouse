using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using System.IO;
using System.Timers;

namespace GUI
{
    class DataUpload
    {
        private string fileName;
        private readonly Process p;
        private DateTime lastTime;
        private TimeSpan tokenRefreshTime;
        private Timer timer;
        public DataUpload()
        {
            p = new Process();
            p.StartInfo.CreateNoWindow = true;         // 不创建新窗口    
            p.StartInfo.UseShellExecute = false;       //不启用shell启动进程  
            p.StartInfo.RedirectStandardInput = true;  // 重定向输入    
            p.StartInfo.RedirectStandardOutput = true; // 重定向标准输出    
            p.StartInfo.RedirectStandardError = true;  // 重定向错误输出  
            p.StartInfo.FileName = "cmd.exe";
            tokenRefreshTime = new TimeSpan(0, 50, 0);
        }
        public void Init()
        {
            runCMD("tapis auth tokens create");
            SetTimer(5000);
            lastTime = DateTime.Now;
        }

        public bool CheckEnv()
        {
            string output = runCMD("tapis --help");
            if (output.Contains("Tapis CLI"))
            {
                return true;
            }
            else
            {
                return false;
            }
        }
        /// <summary>
        /// Upload local to DesignSafe-CI by tapis files upload comand
        /// </summary>
        /// <param name="filePath">Local file path</param>
        /// <param name="cloudPath">Cloud destination path.(i.e. project-6284144844314644966-242ac11c-0001-012/GUI_Test/)</param>
        public void Upload(string filePath, string cloudPath)
        {
            if ((DateTime.Now - lastTime) > tokenRefreshTime)
            {
                refreshToken();
            }
            runCMD("tapis files upload agave://" + cloudPath + " " + filePath);
        }
        private void refreshToken()
        {
            string output = runCMD("tapis auth tokens refresh");
            if (output == "Error")
            {
                runCMD("tapis auth tokens refresh");
            }
        }
        private string runCMD(string command)
        {
            try
            {
                p.Start();
                p.StandardInput.WriteLine(command);
                p.StandardInput.AutoFlush = true;
                p.StandardInput.WriteLine("exit");
                StreamReader reader = p.StandardOutput;
                string output = reader.ReadToEnd(); //获取错误信息到error
                reader.Close(); //close进程
                p.WaitForExit();  //等待程序执行完退出进程
                p.Close();
                return output;
            }
            catch (SystemException)
            {
                return "Error";
            }
        }
        private void SetTimer(double milli)
        {
            // Create a timer with a two second interval.
            timer = new System.Timers.Timer(milli);
            // Hook up the Elapsed event for the timer. 
            timer.Elapsed += OnTimedEvent;
            timer.AutoReset = true;
            timer.Enabled = true;
        }
        private void OnTimedEvent(Object source, ElapsedEventArgs e)
        {

        }
    }
}
