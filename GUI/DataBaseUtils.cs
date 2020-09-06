using System;
using System.Timers;

namespace GUI
{
    internal class DataBaseUtils
    {
        private string DataBaseAddress;
        private string userName;
        private string passWord;
        private bool needAuth;
        private string dataString;
        private static Timer aTimer;
        private bool isEnable;
        private readonly object o = new object();

        public DataBaseUtils(string Address, bool NeedPassword = false, string User = "", string PassWord = "")
        {
            DataBaseAddress = Address;
            needAuth = NeedPassword;
            userName = User;
            passWord = PassWord;
            dataString = "";
            // Create a timer with a 1 second interval.
            aTimer = new Timer(1000);
            // Hook up the Elapsed event for the timer.
            aTimer.Elapsed += Upload;
            aTimer.AutoReset = true;
            aTimer.Enabled = true;
            isEnable = true;
        }

        public void PostData(Format.DataPackage dataPackage)
        {
            if (!isEnable) return;
            lock (o)
            {
                dataString += InfluxDBStringBuilder(dataPackage) + "\n";
            }
        }

        public void EnableUpload()
        {
            if (aTimer == null) return;
            isEnable = true;
        }

        public void DisableUpload()
        {
            if (aTimer == null) return;
            isEnable = false;
        }

        private void Upload(Object source, ElapsedEventArgs e)
        {
            int length = dataString.Length;
            if (length < 1) return;
            string data;
            lock (o)
            {
                data = dataString;
                dataString = "";
            }
            string auth = needAuth ? ("&u=" + userName + "&p=" + passWord) : "";
            string url = DataBaseAddress + "/write?db=WSN&precision=ms" + auth;//Dedabase name is WSN. Time precision is mill seconds.
            var res = WebAPIUtil.HttpPost(url, data);
            if (res == null)
            {
                dataString = data + dataString;
            }
        }

        private string InfluxDBStringBuilder(Format.DataPackage dataPackage)
        {
            string timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds().ToString();//Unix UTC time for InfluxDB
            string result = "WSN,";
            result += "SensorID=" + dataPackage.SensorID + " ";
            switch (dataPackage.SensorTYpe)
            {
                case 2:
                    result += "WindSpeed=" + (dataPackage.WindSpeed / 65536d * 3.3 * (57.6 + 150) / 57.6 * 20).ToString("F3") + ",";
                    result += "WindDirection=" + (dataPackage.WindDirection / 65536d * 3.3 * (57.6 + 150) / 57.6 * 72).ToString("F2") + ",";
                    break;

                case 4:
                    result += "Pressure=" + ((dataPackage.Pressure / 65536d + 0.095) / 0.009 * 10).ToString("F3") + ",";
                    result += "Battery=" + (dataPackage.Battery / 65536d * 3.3 * 2).ToString("F3") + ",";
                    break;

                default:
                    break;
            }
            result += "Temperature=" + (((dataPackage.Temperature / 65536d * 3.3 - 0.05) / 0.01 * 1.8) + 32).ToString("F3") + " ";
            result += timestamp;
            return result;
        }
    }
}