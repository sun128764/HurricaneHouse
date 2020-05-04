using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections.ObjectModel;
using System.Drawing;
using System.Windows.Media;

namespace GUI
{
    public class SensorInfo
    {
        public ObservableCollection<SensorInfo> Items { get; set; }
        public enum Types { Router, Anemometer, Humidity, regular }
        public enum Status { Ok, Lost, Error, Wait };

        //Use auto property if there's no other useage
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

        public Status SensorStatus { set; get; }


        public void SetInfo(string name, int netWorkID, int sensorID, Types type, string metaData)
        {
            this._name = name;
            this._netWorkID = netWorkID;
            this._sensorID = sensorID;
            this._sensorType = type;
            this._metaData = metaData;
        }
        public SensorInfo()
        {
            this.Items = new ObservableCollection<SensorInfo>();
        }

    }
}
