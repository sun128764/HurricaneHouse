using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace GUI
{
    class JsonTools
    {
        public static string ListToJson(List<SensorInfo> sensorInfos)
        {
            return JsonConvert.SerializeObject(sensorInfos);
        }
    }
}
