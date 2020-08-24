using Newtonsoft.Json;

namespace GUI
{
    public class SensorInfo
    {
        public enum Types { Router, Anemometer, Humidity, Regular }

        public string Name { set; get; }
        public int NetWorkID { set; get; }
        public int SensorID { set; get; }

        public Types SensorType
        {
            set
            {
                this.SensorData.SensorType = value;
            }
            get
            {
                return this.SensorData.SensorType;
            }
        }

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
        public SensorData SensorData { set; get; }

        public SensorInfo()
        {
            SensorData = new SensorData() { Status = SensorData.StatusValue.Lost };
        }
    }
}