using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using LiveCharts;
using LiveCharts.Configurations;



namespace Format
{
    public class TimeSeries
    {
        public DateTime DateTime;
        public double Value;
        public TimeSeries(DateTime time, double value)
        {
            DateTime = time;
            Value = value;
        }

    }

    public class PlotControl : INotifyPropertyChanged
    {
        public Func<double, string> DateTimeFormatter { get; set; }
        public double AxisStep { get; set; }
        public double AxisUnit { get; set; }
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
            set { if (this._Scale != value) { this._Scale = value; this.Max = DateTime.Now.Ticks; this.Min = DateTime.Now.AddMinutes(-value * 0.25 - 5).Ticks; NotifyPropertyChanged(); } }
        }
        public long Max
        {
            get { return this._max.Ticks; }
            set { DateTime dateTime = new DateTime(value); if (this._max != dateTime) { this._max = dateTime; NotifyPropertyChanged(); } }
        }
        public long Min
        {
            get { return this._min.Ticks; }
            set { DateTime dateTime = new DateTime(value); if (this._min != dateTime) { this._min = dateTime; NotifyPropertyChanged(); } }
        }
        public void RefreshLimit(DateTime dateTime)
        {
            this.Max = dateTime.Ticks;
            this.Min = dateTime.AddMinutes(-Scale * 0.25 - 5).Ticks;
        }
        public PlotControl()
        {
            //lets set how to display the X Labels
            DateTimeFormatter = value => new DateTime((long)value).ToString("mm:ss");

            //AxisStep forces the distance between each separator in the X axis
            AxisStep = TimeSpan.FromSeconds(1).Ticks;
            //AxisUnit forces lets the axis know that we are plotting seconds
            //this is not always necessary, but it can prevent wrong labeling
            AxisUnit = TimeSpan.TicksPerSecond;
        }

    }
}
