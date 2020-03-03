using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using System.IO.Ports;

namespace DataCollectDaemon
{
    public class Tools
    {
        /// <summary>
        /// Read the sensor list form txt file.
        /// </summary>
        /// <param name="path">The path of txt file.</param>
        /// <returns>A list of sensor info(name,address).</returns>
        static public List<DataFormat.SensorInfo> ReadSensorList(string path)
        {
            Regex inputPattern = new Regex(@"(\d+)\s+(\d+)");
            try
            {
                string[] lines = System.IO.File.ReadAllLines(path);
                List<DataFormat.SensorInfo> sensorList = new List<DataFormat.SensorInfo>();
                int totalNum = int.Parse(lines[0]);
                for (int i = 1; i < totalNum; i++)
                {
                    MatchCollection matches = inputPattern.Matches(lines[i]);
                    if (matches.Count != 2) throw new Exception("Input format doesn't match");
                    string name = matches[0].Value;
                    string address = matches[1].Value;
                    sensorList.Add(new DataFormat.SensorInfo(name, address));
                }
                return sensorList;
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                return null;
            }
        }
        public class SerialTools
        {
            

            static bool isPort() 
            {
                string[] str = SerialPort.GetPortNames();
                if (str == null)
                {
                    return false;
                }
                else return true;
            }

            

        }
    }
}
