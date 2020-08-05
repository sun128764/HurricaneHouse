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
            string url = DataBaseAddress + "/write?db=WSN";
            WebMap.HttpPost(url, InfluxDBStringBuilder(dataPackage));
        }
        private string InfluxDBStringBuilder(Format.DataPackage dataPackage)
        {
            string result = "";
            result += "Pressure,";
            result += "SensorID=" + dataPackage.SensorID + " ";
            result += "Value=" + ((dataPackage.Pressure / 65536d + 0.095) / 0.009 * 10).ToString("F3") + " ";
            result += "\n";
            result += "Battery,";
            result += "SensorID=" + dataPackage.SensorID + " ";
            result += "Value=" + (dataPackage.Battery / 65536d * 3.3 * 2).ToString("F3") + " ";
            result += "\n";
            result += "Temperature,";
            result += "SensorID=" + dataPackage.SensorID + " ";
            result += "Value=" + (((dataPackage.Temperature / 65536d * 3.3 - 0.05) / 0.01 * 1.8) + 32).ToString("F3") + " ";
            return result;
        }
    }
}
