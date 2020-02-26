using System;
using System.Collections.Generic;
using System.Text;

namespace DataCollectDaemon
{
    public class DataFormat
    {

        public class SensorInfo
        {
            public string Address { get; }
            public string Name { get; }
            public SensorInfo(string name, string address)
            {
                Address = address;
                Name = name;
            }
        }
        
    }
}
