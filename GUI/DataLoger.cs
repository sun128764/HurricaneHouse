using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using System.IO;
using System.Timers;
using System.Text.RegularExpressions;
using SciChart.Core.Extensions;

namespace GUI
{
    class DataLoger
    {
        private string fileName;
        private readonly Process p;
        private DateTime lastTime;
        private TimeSpan tokenRefreshTime;
        private Timer timer;
        private readonly List<string> dataString;
        private TimeSpan uploadSpan;
        private string projectName;
        private int fileCount;
        private delegate void uploadDelegate(string cloudPath);
        private readonly uploadDelegate upload;
        private readonly string CloudPath = "project-2213334571396698601-242ac11a-0001-012/GUI_Test/";
        private Format.ProgramSetting programSetting;
        private List<string> failedFilePathList;
        private readonly Regex regex = new Regex(@"\|\s*(uploaded|skipped)\s*\|\s*1\s*\|");
        public DataLoger()
        {
            p = new Process();
            p.StartInfo.CreateNoWindow = true;
            p.StartInfo.UseShellExecute = false;
            p.StartInfo.RedirectStandardInput = true;
            p.StartInfo.RedirectStandardOutput = true;
            p.StartInfo.RedirectStandardError = true;
            p.StartInfo.FileName = "tapis";
            tokenRefreshTime = new TimeSpan(0, 50, 0);
            dataString = new List<string>();
            uploadSpan = new TimeSpan(0, 1, 0);
            upload = new uploadDelegate(Upload);
            lastTime = DateTime.Now;
            projectName = "test";
            fileCount = 0;
            failedFilePathList = new List<string>();
        }
        public void Init()
        {
            RunTapis("auth tokens create");
            SetTimer(5000);
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
        public void Upload(string cloudPath)
        {
            if ((DateTime.Now - lastTime) > tokenRefreshTime)
            {
                refreshToken();
            }
            string filename = projectName + "-" + fileCount.ToString() + ".csv";
            using (StreamWriter writer = File.CreateText(filename))
            {
                writer.WriteLine("Base computer time stamp(UTC), Network ID, Board ID, Type," +
                    " Sensor local time stamp, Temperature, Battery, Wind Speed, Wind Direction, " +
                    "Humidity, Pressure 1, Pressure 2, Pressure 3, Pressure 4, Pressure 5," +
                    " Pressure 6, Pressure 7, Pressure 8, Pressure 9, Pressure 10");
                foreach (string t in dataString)
                {
                    writer.WriteLine(t);
                }
            }
            dataString.Clear();
            fileCount++;
            lastTime = DateTime.Now;
            string output = RunTapis("files upload agave://" + cloudPath + " " + Environment.CurrentDirectory + "\\" + filename);
            if (!regex.IsMatch(output))
            {
                failedFilePathList.Add(Environment.CurrentDirectory + "\\" + filename);
            }
            if (failedFilePathList.Count > 0)
            {
                List<string> successFiles = new List<string>();
                foreach(string file in failedFilePathList)
                {
                    string outputt = RunTapis("files upload agave://" + cloudPath + " " + file);
                    if (regex.IsMatch(outputt))
                    {
                        successFiles.Add(file);
                    }
                }
                foreach(string file in successFiles)
                {
                    failedFilePathList.Remove(file);
                }
            }
        }
        /// <summary>
        /// Add data to data buffer. Auto upload to DesignSafe. Use Null input to enforce upload.
        /// </summary>
        /// <param name="data">Sensor data. Use Null to enforce upload</param>
        public void AddData(string data)
        {
            this.dataString.Add(data);
            if ((((DateTime.Now - lastTime) > uploadSpan)||(data == null)) && dataString.Count > 0)
            {
                //Upload("project-6284144844314644966-242ac11c-0001-012/GUI_Test/");
                upload.BeginInvoke(CloudPath, null, null);
                //upload.BeginInvoke("project-6284144844314644966-242ac11c-0001-012/GUI_Test/", null, null);
                //upload(Environment.CurrentDirectory + "\\" + filename, "project-6284144844314644966-242ac11c-0001-012/GUI_Test/");
                //Upload(Environment.CurrentDirectory + "\\" + filename, "project-6284144844314644966-242ac11c-0001-012/GUI_Test/");
            }
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
