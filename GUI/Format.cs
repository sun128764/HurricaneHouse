using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel;
using System.Runtime.CompilerServices;



namespace Format
{
    public class TimeSeries
    {
        public DateTime DateTime;
        public double Value;
        public TimeSeries(DateTime time,double value)
        {
            DateTime = time;
            Value = value;
        }

    }

    public class PlotControl : INotifyPropertyChanged
    {
        //public int Scale { set; get; }
        // These fields hold the values for the public properties.
        private int _Scale;
        public event PropertyChangedEventHandler PropertyChanged;
        private DateTime _max;
        private DateTime _min;
        // This method is called by the Set accessor of each property.
        // The CallerMemberName attribute that is applied to the optional propertyName
        // parameter causes the property name of the caller to be substituted as an argument.
        protected void NotifyPropertyChanged([CallerMemberName] String propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        public int Scale
        {
            get { return this._Scale; }
            set { if (this._Scale != value) { this._Scale = value; NotifyPropertyChanged(); } }
        }
        public DateTime Max
        {
            get { return this._max; }
            set { if (this._max != value) { this._max = value; NotifyPropertyChanged(); } }
        }
        public DateTime Min
        {
            get { return this._min; }
            set { if (this._min != value) { this._min = value;NotifyPropertyChanged(); } }
        }
    }
}
