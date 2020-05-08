using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using SciChart.Data.Model;
using SciChart.Charting.Visuals;
using System.Text.RegularExpressions;

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
        private int _scale;
        public event PropertyChangedEventHandler PropertyChanged;
        private DateTime _max;
        private DateTime _min;

        private IRange _xVisibleRange;
        public IRange XVisibleRange
        {
            get { return _xVisibleRange; }
            set
            {
                if (_xVisibleRange != value)
                {
                    _xVisibleRange = value;
                    NotifyPropertyChanged("XVisibleRange");
                }
            }
        }

        private ZoomStates _zoomeState;
        public ZoomStates ZoomState
        {
            get { return _zoomeState; }
            set
            {
                if (_zoomeState != value)
                {
                    _zoomeState = value;
                    NotifyPropertyChanged("ZoomState");
                }
            }
        }
        // This method is called by the Set accessor of each property.
        // The CallerMemberName attribute that is applied to the optional propertyName
        // parameter causes the property name of the caller to be substituted as an argument.
        protected void NotifyPropertyChanged([CallerMemberName] String propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        public int Scale
        {
            get { return this._scale; }
            set { if (this._scale != value) { this._scale = value; this.Max = DateTime.Now; this.Min = DateTime.Now.AddMinutes(-value); NotifyPropertyChanged(); } }
        }
        public DateTime Max
        {
            get { return this._max; }
            set { DateTime dateTime = value; if (this._max != dateTime) { this._max = dateTime; NotifyPropertyChanged(); } }
        }
        public DateTime Min
        {
            get { return this._min; }
            set { DateTime dateTime = value; if (this._min != dateTime) { this._min = dateTime; NotifyPropertyChanged(); } }
        }
        public DateRange Limit
        {
            get { return new DateRange(Min, Max); }
        }
        public void RefreshLimit(DateTime dateTime)
        {
            this.Max = dateTime;
            this.Min = dateTime.AddMinutes(-Scale);
            IRange range = XVisibleRange;
            if (ZoomState == ZoomStates.UserZooming) XVisibleRange = range;
            else XVisibleRange = new DateRange(this.Min, this.Max);
        }
    }

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
        public static DataPackage Decode(String data)
        {
            DataPackage dataPackage = new DataPackage();
            Regex regex = new Regex(@"-?[0-9]\d*");
            MatchCollection match = regex.Matches(data);
            if (match.Count == 8)
            {
                dataPackage.Time = DateTime.Now;
                int i = 0;
                dataPackage.NetworkID = int.Parse(match[i++].Value);
                dataPackage.SensorID = int.Parse(match[i++].Value);
                dataPackage.SensorTYpe = int.Parse(match[i++].Value);
                dataPackage.Temperature = int.Parse(match[i++].Value);
                dataPackage.Battery = int.Parse(match[i++].Value);
                dataPackage.Pressure = int.Parse(match[i++].Value);
                dataPackage.WindSpeed = int.Parse(match[i++].Value);
                dataPackage.Huminity = int.Parse(match[i++].Value);
            }
            else dataPackage = null;
            return dataPackage;
        }
    }
}
