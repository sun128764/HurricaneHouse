using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GUI
{
    class DataBaseUtils
    {
        public string DataBaseAddress;
        public void PostData(Format.DataPackage dataPackage)
        {
            string url = DataBaseAddress + "/write?db=WSN&precision=ms";//Dedabase name is WSN. Time precision is mill seconds.
            WebAPIUtil.HttpPost(url, InfluxDBStringBuilder(dataPackage));
        }
        private string InfluxDBStringBuilder(Format.DataPackage dataPackage)
        {
            string timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds().ToString()+"\n";//Unix UTC time for InfluxDB
            string result = "WSN,";
            result += "SensorID=" + dataPackage.SensorID + " ";
            switch (dataPackage.SensorTYpe)
            {
                case 2:
                    result += "WindSpeed="+(dataPackage.WindSpeed / 65536d * 3.3 * (57.6 + 150) / 57.6 * 20).ToString("F3") + ",";
                    result += "WindDirection="+(dataPackage.WindDirection / 65536d * 3.3 * (57.6 + 150) / 57.6 * 72).ToString("F2") + ",";
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
