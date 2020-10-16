using Format;
using System;
using System.Timers;

namespace MainProgram
{
    /// <summary>
    /// Functions used for database operation.This class will upload data every 1s.
    /// </summary>
    internal class DataBaseUtils
    {
        private DbInfo Info;
        private string dataString;
        private static Timer aTimer;
        private bool isEnable;
        private readonly object o = new object();

        public DataBaseUtils(DbInfo dbInfo)
        {
            Info = dbInfo;
            dataString = "";
            // Create a timer with a 1 second interval.
            aTimer = new Timer(1000);
            // Hook up the Elapsed event for the timer.
            aTimer.Elapsed += Upload;
            aTimer.AutoReset = true;
            aTimer.Enabled = true;
            isEnable = true;
        }
        /// <summary>
        /// Add data to data list.
        /// </summary>
        /// <param name="dataPackage">Decoded data package</param>
        public void PostData(DataPackage dataPackage)
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

        /// <summary>
        /// Upload the data list to database and empty the data list if upload success.
        /// </summary>
        /// <param name="source"></param>
        /// <param name="e"></param>
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
            string auth = Info.needAuth ? ("&u=" + Info.UserName + "&p=" + Info.PassWord) : "";
            string url = Info.DataBaseAddress + "/write?db=" + Info.DataBaseName + "&precision=ms" + auth;//Dedabase name is WSN. Time precision is mill seconds.
            var res = WebAPIUtil.HttpPost(url, data);
            if (res == null && data.Length < 1000000)//limit maximum data length
            {
                dataString = data + dataString;
            }
        }

        /// <summary>
        /// Convert the decoded data package to InfluxDB Line protocol with time stamp.
        /// </summary>
        /// <param name="dataPackage">Decoded data package</param>
        /// <returns>InfluxDB Line protocol string</returns>
        private string InfluxDBStringBuilder(DataPackage dataPackage)
        {
            string timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds().ToString();//Unix UTC time for InfluxDB
            string result = Info.MeasurementName + ",";
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