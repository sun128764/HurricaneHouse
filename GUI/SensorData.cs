using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace GUI
{
    public class SensorData:INotifyPropertyChanged
    {
        public float BatteryLevel { get; set; }

        public event PropertyChangedEventHandler PropertyChanged;

    }
}
