using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.RegularExpressions;

namespace MainProgram
{
    /// <summary>
    /// Save data to local disk and upload to DesignSafe.
    /// </summary>
    internal class DataLoger : INotifyPropertyChanged
    {
        private static readonly NLog.Logger Logger = NLog.LogManager.GetCurrentClassLogger();

        private readonly Process p;
        private DateTime lastTime;
        private TimeSpan tokenRefreshSpan;
        private readonly List<string> dataString;
        private TimeSpan uploadSpan;
        private string projectName;
        private int fileCount;
        public bool enableUpload;
        public bool isPass;

        private delegate void uploadDelegate(string cloudPath);

        private readonly uploadDelegate upload;
        private string CloudPath;
        private string LocalPath;
        private List<string> failedFilePathList;
        private readonly Regex uploadRegex = new Regex(@"\|\s*(uploaded|skipped)\s*\|\s*1\s*\|");
        private readonly Regex listFolderRegex = new Regex(@"\|\s*(\S*)\s*\|.*");
        private readonly object o = new object();
        public string LastFileName { set; get; }
        public string LastFileTime { set; get; }

        public event PropertyChangedEventHandler PropertyChanged;

        private string _outputString;

        public string OutputString
        {
            set
            {
                if (value != _outputString)
                {
                    _outputString = value;
                    NotifyPropertyChanged();
                }
            }
            get
            {
                return _outputString;
            }
        }

        private int _pBar;

        public int PBar
        {
            set
            {
                if (value != _pBar)
                {
                    _pBar = value;
                    NotifyPropertyChanged();
                }
            }
            get
            {
                return _pBar;
            }
        }

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
            OutputString = "";
            isPass = true;
            enableUpload = true;
        }

        /// <summary>
        /// Initialize the logger.User must set up Tapis and create token first.
        /// </summary>
        /// <param name="setting">Program setting</param>
        /// <returns>Tapis output</returns>
        public string Init(Format.ProgramSetting setting)
        {
            OutputString += "Start initialing upload module...";
            PBar = 10;
            string output;
            projectName = setting.ProjectName;
            CloudPath = setting.CloudPath;
            LocalPath = setting.LocalPath;
            uploadSpan = setting._uploadSpan;
            tokenRefreshSpan = setting._tokenRefreshSpan;
            fileCount = FindFileCount(setting.LocalPath) + 1;
            output = RefreshToken();
            if (output == "Error")
            {
                OutputString += output + Environment.NewLine;
            }
            if (output.Contains("access_token"))
            {
                OutputString += "Success" + Environment.NewLine;
                PBar = 40;
            }
            else
            {
                OutputString += "Fail" + Environment.NewLine;
                isPass = false;
            }
            return output;
        }

        /// <summary>
        /// Try to create a .temp file in Local path and upload it to Cloud Path.
        /// If file is upload successfully, it will clean up the .temp file.
        /// </summary>
        /// <returns>Check result.</returns>
        public string TryUpload()
        {
            if (!isPass)
            {
                return "Failed";
            }
            OutputString += "Start uploading test..." + Environment.NewLine;
            PBar = 45;
            string str = DateTime.Now.ToString();
            string testFile = Convert.ToBase64String(Encoding.ASCII.GetBytes(str)).Substring(10) + ".temp"; //random file name
            OutputString += "Creating .temp file:" + LocalPath + "\\" + testFile + Environment.NewLine;
            PBar = 50;
            using (StreamWriter writer = File.CreateText(LocalPath + "\\" + testFile))
            {
                writer.WriteLine(str);
            }
            OutputString += "Uploading to:" + "agave://" + CloudPath + Environment.NewLine;
            PBar = 55;
            string output = RunTapis("files upload agave://" + CloudPath + " " + LocalPath + "\\" + testFile);
            if (!uploadRegex.IsMatch(output))
            {
                OutputString += "Unable to upload";
                isPass = false;
                return "Unable to upload";
            }
            OutputString += "Checking clould path folder...";
            PBar = 80;
            List<string> fileList = ListFolder(CloudPath);
            if (fileList.Contains(testFile))
            {
                OutputString += "Success" + Environment.NewLine;
                PBar = 90;
                DeleteFile(CloudPath + "/" + testFile);
                File.Delete(LocalPath + "\\" + testFile);
            }
            OutputString += "Validation finished." + Environment.NewLine;
            PBar = 100;
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
                List<string> writeData = new List<string>();
                lock (o)
                {
                    writeData.AddRange(dataString);
                    dataString.Clear();
                }
                foreach (string t in writeData)
                {
                    writer.WriteLine(t);
                }
            }
            fileCount++;
            //lastTime = DateTime.Now;
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
            if (!enableUpload) return;
            if (data == null || data.Length < 10) return;
            lock (o)
            {
                this.dataString.Add(data);
            }
            if ((((DateTime.Now - lastTime) > uploadSpan) || (data == null)) && dataString.Count > 0)
            {
                lastTime = DateTime.Now;
                upload.BeginInvoke(CloudPath, null, null);
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

        public void ClearData()
        {
            dataString.Clear();
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
            catch (SystemException ex)
            {
                Logger.Error(ex,"Cannot run tapis");
                return "Error";
            }
        }

        /// <summary>
        /// List all files in DesignSafe data saving path
        /// </summary>
        /// <param name="path">DesignSafe path</param>
        /// <returns>File name list</returns>
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

        /// <summary>
        /// Delete file in DesignSafe
        /// </summary>
        /// <param name="path">File path</param>
        /// <returns>Result</returns>
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