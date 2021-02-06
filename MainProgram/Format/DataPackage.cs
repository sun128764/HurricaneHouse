using Force.Crc32;
using System;

namespace Format
{
    /// <summary>
    /// The format of data package sent from sensors via serial port.
    /// </summary>
    public class DataPackage
    {
        public int NetworkID;
        public int SensorID;
        public int SensorTYpe;
        public DateTime Time;
        public int Pressure;
        public int Temperature;
        public int Battery;
        public int WindSpeed;
        public int WindDirection;
        public int Huminity;
        public int BoardTime;
        public int[] PressureList;
        public DateTime[] TimeSeries;
        public string DataString;
        public bool passCrc32;
        /// <summary>
        /// Decode the data array from serial port and convert to Data Package.
        /// </summary>
        /// <param name="data">Received byte array</param>
        /// <returns>Data package</returns>
        public static DataPackage Decode(byte[] data)
        {
            DataPackage dataPackage = new DataPackage();
            dataPackage.TimeSeries = new DateTime[10];
            dataPackage.SensorID = data[0];
            dataPackage.SensorTYpe = data[1];
            dataPackage.Temperature = data[2] << 8;
            dataPackage.Battery = data[3] << 8;
            if (dataPackage.SensorTYpe == 2)
            {
                dataPackage.WindSpeed = ((data[4] << 8) + data[5]);
                dataPackage.WindDirection = ((data[6] << 8) + data[7]);
            }
            else if (dataPackage.SensorTYpe == 3)
            {
                dataPackage.Huminity = ((data[6] << 8) + data[7]);
            }
                //dataPackage.Huminity = 0;
            dataPackage.BoardTime = (data[8] << 24) + (data[9] << 16) + (data[10] << 8) + data[11];
            dataPackage.Pressure = ((data[12] << 8) + data[13]);
            dataPackage.Time = DateTime.Now;

            dataPackage.DataString += DateTime.Now.ToString("o");
            dataPackage.DataString += "," + "5001";
            dataPackage.DataString += "," + dataPackage.SensorID.ToString();
            dataPackage.DataString += "," + dataPackage.SensorTYpe.ToString();
            dataPackage.DataString += "," + dataPackage.BoardTime.ToString();
            dataPackage.DataString += "," + dataPackage.Temperature.ToString();
            dataPackage.DataString += "," + dataPackage.Battery.ToString();
            dataPackage.DataString += "," + dataPackage.WindSpeed.ToString();
            dataPackage.DataString += "," + dataPackage.WindDirection.ToString();
            dataPackage.DataString += "," + dataPackage.Huminity.ToString();
            dataPackage.passCrc32 = Crc32Algorithm.IsValidWithCrcAtEnd(data);
            if (dataPackage.SensorTYpe != 4)
            {
                return dataPackage;
            }
          

            dataPackage.PressureList = new int[10];
            int i = 12;
            for (int j = 0; j < 10; j++)
            {
                dataPackage.PressureList[j] = ((data[i] << 8) + data[i + 1]);
                dataPackage.TimeSeries[j] = dataPackage.Time.AddMilliseconds(100 * j);
                dataPackage.DataString += "," + dataPackage.PressureList[j].ToString();
                i += 2;
            }
            //byte[] input = new byte[36];
            //data.CopyTo(input, 0);
            //Crc32Algorithm.ComputeAndWriteToEnd(input); // CRC32 local test
            return dataPackage;
        }
    }
}