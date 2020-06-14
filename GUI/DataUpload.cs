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
            p.StartInfo.CreateNoWindow = true;
            p.StartInfo.UseShellExecute = false;
            p.StartInfo.RedirectStandardInput = true;
            p.StartInfo.RedirectStandardOutput = true;
            p.StartInfo.RedirectStandardError = true;
            p.StartInfo.FileName = "tapis";
            tokenRefreshTime = new TimeSpan(0, 50, 0);
        }
        public void Init()
        {
            RunTapis("auth tokens create");
            SetTimer(5000);
            lastTime = DateTime.Now;
        }

        public bool CheckEnv()
        {
            string output = RunTapis("--help");
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
            RunTapis("files upload agave://" + cloudPath + " " + filePath);
        }
        private void refreshToken()
        {
            string output = RunTapis("auth tokens refresh");
            if (output == "Error")
            {
                RunTapis("auth tokens refresh");
            }
        }
        /// <summary>
        /// Call Tapis.
        /// </summary>
        /// <param name="command">Arguments</param>
        /// <returns>Tapis output</returns>
        private string RunTapis(string command)
        {
            try
            {
                p.StartInfo.Arguments = command;
                p.Start();
                p.StandardInput.AutoFlush = true;
                p.StandardInput.WriteLine("exit");
                StreamReader reader = p.StandardOutput;
                string output = reader.ReadToEnd();
                reader.Close();
                p.WaitForExit();
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
