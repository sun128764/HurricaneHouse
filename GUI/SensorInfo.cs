using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GUI
{
    class SensorInfo
    {
        public enum Types { Router, Anemometer, Humidity, regular }

        private string _name;
        private int _netWorkID;
        private int _sensorID;
        private Types _sensorType;
        private string _metaData;

        public string Name => _name;
        public int NetWorkID => _netWorkID;
        public int SensorID => _sensorID;
        public Types SensorType => _sensorType;
        public string MetaData => _metaData;

        public void SetInfo(string name, int netWorkID, int sensorID,Types type, string metaData)
        {
            this._name = name;
            this._netWorkID = netWorkID;
            this._sensorID = sensorID;
            this._sensorType = type;
            this._metaData = metaData;
        }
    }
}
