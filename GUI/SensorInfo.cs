using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections.ObjectModel;
using System.Drawing;
using System.Windows.Media;
using Newtonsoft.Json;

namespace GUI
{
    public class SensorInfo
    {
        public ObservableCollection<SensorInfo> Items { get; set; }
        public enum Types { Router, Anemometer, Humidity, Regular }
        
        public enum Status { Ok, Lost, Error, Wait };

        public string Name { set; get; }
        public int NetWorkID { set; get; }
        public int SensorID { set; get; }
        public Types SensorType { set; get; }
        [JsonIgnore]
        public int TypeIndex
        {
            set
            {
                SensorType = (Types)value;
            }
            get
            {
                return (int)this.SensorType;
            }
        }
        public string MetaData { set; get; }
        [JsonIgnore]
        public Status SensorStatus { set; get; }
        [JsonIgnore]
        public SensorData SensorData = new SensorData();
        public SensorInfo()
        {
            this.Items = new ObservableCollection<SensorInfo>();
        }

    }
}
