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
using System.Windows.Documents;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace GUI
{
    class DataLoger : INotifyPropertyChanged
    {
        private readonly Process p;
        private DateTime lastTime;
        private TimeSpan tokenRefreshSpan;
        private readonly List<string> dataString;
        private TimeSpan uploadSpan;
        private string projectName;
        private int fileCount;
        private delegate void uploadDelegate(string cloudPath);
        private readonly uploadDelegate upload;
        private string CloudPath;
        private string LocalPath;
        private List<string> failedFilePathList;
        private readonly Regex uploadRegex = new Regex(@"\|\s*(uploaded|skipped)\s*\|\s*1\s*\|");
        private readonly Regex listFolderRegex = new Regex(@"\|\s*(\S*)\s*\|.*");
        public string LastFileName { set; get; }
        public string LastFileTime { set; get; }
        public event PropertyChangedEventHandler PropertyChanged;
        protected void NotifyPropertyChanged([CallerMemberName] String propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        public DataLoger()
        {
            p = new Process();
            p.StartInfo.CreateNoWindow = true;
            p.StartInfo.UseShellExecute = false;
            p.StartInfo.RedirectStandardInput = true;
            p.StartInfo.RedirectStandardOutput = true;
            p.StartInfo.RedirectStandardError = true;
            p.StartInfo.FileName = "tapis";
            tokenRefreshSpan = new TimeSpan(0, 50, 0);
            dataString = new List<string>();
            uploadSpan = new TimeSpan(0, 1, 0);
            upload = new uploadDelegate(Upload);
            lastTime = DateTime.Now;
            projectName = "test";
            CloudPath = "project-2213334571396698601-242ac11a-0001-012/GUI_Test/";
            LocalPath = "C:\\Users\\sun12\\Desktop\\TEST";
            fileCount = 0;
            failedFilePathList = new List<string>();
        }
        public string Init(Format.ProgramSetting setting)
        {
            string output;
            projectName = setting.ProjectName;
            CloudPath = setting.CloudPath;
            LocalPath = setting.LocalPath;
            uploadSpan = setting._uploadSpan;
            tokenRefreshSpan = setting._tokenRefreshSpan;
            fileCount = FindFileCount(setting.LocalPath) + 1;
            output = RefreshToken();
            return output;
        }
        /// <summary>
        /// Try to create a .temp file in Local path and upload it to Cloud Path.
        /// If file is upload successfully, it will clean up the .temp file.
        /// </summary>
        /// <returns>Check result.</returns>
        public string TryUpload()
        {
            string str = DateTime.Now.ToString();
            string testFile = Convert.ToBase64String(Encoding.ASCII.GetBytes(str)).Substring(10) + ".temp"; //random file name
            using (StreamWriter writer = File.CreateText(LocalPath + "\\" + testFile))
            {
                writer.WriteLine(str);
            }
            string output = RunTapis("files upload agave://" + CloudPath + " " + LocalPath + "\\" + testFile);
            if (!uploadRegex.IsMatch(output))
            {
                return "Unable to upload";
            }
            List<string> fileList = ListFolder(CloudPath);
            if (fileList.Contains(testFile))
            {
                DeleteFile(CloudPath + "/" + testFile);
                File.Delete(LocalPath + "\\" + testFile);
            }
            return "Pass";
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
            if ((DateTime.Now - lastTime) > tokenRefreshSpan)
            {
                RefreshToken();
            }
            string filename = LocalPath + "\\" + projectName + "-" + fileCount.ToString() + ".csv";
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
            string output = RunTapis("files upload agave://" + cloudPath + " " + filename);
            if (uploadRegex.IsMatch(output))
            {
                LastFileName = Path.GetFileName(filename);
                LastFileTime = lastTime.ToString("g");
                NotifyPropertyChanged("LastFileName");
                NotifyPropertyChanged("LastFileTime");
            }
            else
            {
                failedFilePathList.Add(filename);
            }
            if (failedFilePathList.Count > 0)
            {
                List<string> successFiles = new List<string>();
                foreach (string file in failedFilePathList)
                {
                    string outputt = RunTapis("files upload agave://" + cloudPath + " " + file);
                    if (uploadRegex.IsMatch(outputt))
                    {
                        successFiles.Add(file);
                    }
                }
                foreach (string file in successFiles)
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
            if ((((DateTime.Now - lastTime) > uploadSpan) || (data == null)) && dataString.Count > 0)
            {
                //Upload("project-6284144844314644966-242ac11c-0001-012/GUI_Test/");
                upload.BeginInvoke(CloudPath, null, null);
                //upload.BeginInvoke("project-6284144844314644966-242ac11c-0001-012/GUI_Test/", null, null);
                //upload(Environment.CurrentDirectory + "\\" + filename, "project-6284144844314644966-242ac11c-0001-012/GUI_Test/");
                //Upload(Environment.CurrentDirectory + "\\" + filename, "project-6284144844314644966-242ac11c-0001-012/GUI_Test/");
            }
        }
        private string RefreshToken()
        {
            string output = RunTapis("auth tokens refresh");
            if (output == "Error")
            {
                output = RunTapis("auth tokens refresh");
            }
            return output;
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
        public List<string> ListFolder(string path)
        {
            List<string> fileList = new List<string>();
            string output = RunTapis("files list agave://" + path);
            if (output == "Error")
            {
                return fileList;
            }
            MatchCollection matchCollection = listFolderRegex.Matches(output);
            foreach (Match match in matchCollection)
            {
                fileList.Add(match.Groups[1].Value);
            }
            if (fileList.Count > 0)
            {
                fileList.RemoveAt(0); //Remove first element.| name | lastModified | length |
            }
            return fileList;
        }
        public bool DeleteFile(string path)
        {
            string output = RunTapis("files list agave://" + path);
            return uploadRegex.IsMatch(output);
        }
        /// <summary>
        /// Find the maximum suffix in given folder.
        /// </summary>
        /// <param name="path">Data file folder path.</param>
        /// <returns>Maximum suffix. Start from -1.</returns>
        private int FindFileCount(string path)
        {
            int lastFileCount = -1;
            string[] fileList = Directory.GetFiles(path);
            Regex regex = new Regex(@"^" + projectName + @"-([0-9]*).csv$");
            foreach (string file in fileList)
            {
                string fileName = Path.GetFileName(file);
                if (regex.IsMatch(fileName))
                {
                    lastFileCount = Math.Max(lastFileCount, int.Parse(regex.Match(fileName).Groups[1].Value));
                }
            }
            return lastFileCount;
        }
    }
}
