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
            DataPackage dataPackage = new DataPackage
            {
                TimeSeries = new DateTime[10],
                SensorID = data[0],
                SensorTYpe = data[1],
                Temperature = data[2] << 8,
                Battery = data[3] << 8,
                BoardTime = (data[8] << 24) + (data[9] << 16) + (data[10] << 8) + data[11],
                Pressure = ((data[12] << 8) + data[13]),
                Time = DateTime.Now,
                passCrc32 = Crc32Algorithm.IsValidWithCrcAtEnd(data)//The last 4 byte of the data package is CRC32.
            };
            if (dataPackage.SensorTYpe == 2)
            {
                dataPackage.WindSpeed = ((data[4] << 8) + data[5]);
                dataPackage.WindDirection = ((data[6] << 8) + data[7]);
            }

            dataPackage.DataString += DateTime.Now.ToString("o");
            //dataPackage.DataString += "," + "5001";
            dataPackage.DataString += "," + dataPackage.SensorID.ToString();
            dataPackage.DataString += "," + dataPackage.SensorTYpe.ToString();
            dataPackage.DataString += "," + dataPackage.BoardTime.ToString();
            dataPackage.DataString += "," + dataPackage.Temperature.ToString();
            dataPackage.DataString += "," + dataPackage.Battery.ToString();
            dataPackage.DataString += "," + dataPackage.WindSpeed.ToString();
            dataPackage.DataString += "," + dataPackage.WindDirection.ToString();
            dataPackage.DataString += "," + dataPackage.Huminity.ToString();

            // if (dataPackage.SensorTYpe == 2) return dataPackage;
            dataPackage.PressureList = new int[10];
            int i = 12;
            for (int j = 0; j < 10; j++)
            {
                dataPackage.PressureList[j] = ((data[i] << 8) + data[i + 1]);
                dataPackage.TimeSeries[j] = dataPackage.Time.AddMilliseconds(100 * j);
                dataPackage.DataString += "," + dataPackage.PressureList[j].ToString();
                i += 2;
            }
            return dataPackage;
        }
    }
}