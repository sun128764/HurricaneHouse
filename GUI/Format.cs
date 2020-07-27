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
using SciChart.Charting.Model.DataSeries;
using SciChart.Charting.Visuals.Axes;
using System.Drawing.Printing;

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
        private AutoRange _autoRange;
        public AutoRange AutoRange
        {
            set
            {
                if (_autoRange != value)
                {
                    _autoRange = value;
                    NotifyPropertyChanged();
                }
            }
            get
            {
                return _autoRange;
            }
        }
        private IRange _xVisibleRange;
        public IRange XVisibleRange
        {
            get { return _xVisibleRange; }
            set
            {
                if (_xVisibleRange != value)
                {
                    _xVisibleRange = value;
                    NotifyPropertyChanged();
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
                    NotifyPropertyChanged();
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
            if (ZoomState == ZoomStates.UserZooming)
            {
                XVisibleRange = range;
                AutoRange = AutoRange.Never;
            }
            else
            {
                XVisibleRange = new DateRange(this.Min, this.Max);
                AutoRange = AutoRange.Always;
            }
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
        public int BoardTime;
        public int[] PressureList;
        public DateTime[] TimeSeries;
        public string DataString;
        public static DataPackage Decode(byte[] data)
        {
            DataPackage dataPackage = new DataPackage();
            dataPackage.TimeSeries = new DateTime[10];
            dataPackage.SensorID = data[0];
            dataPackage.SensorTYpe = data[1];
            dataPackage.Temperature = data[2] << 8;
            dataPackage.Battery = data[3] << 8;
            dataPackage.WindDirection = ((data[4] << 8) + data[5]) << 4;
            if (dataPackage.SensorTYpe == 2) dataPackage.WindSpeed = data[6] << 8;
            else dataPackage.Huminity = data[6] << 8;
            dataPackage.BoardTime = (data[7] << 24) + (data[8] << 16) + (data[9] << 8) + data[10];
            dataPackage.Pressure = ((data[11] << 8) + data[12]);
            dataPackage.Time = DateTime.Now;

            dataPackage.DataString += DateTime.Now.ToString("o");
            dataPackage.DataString += "," + "5001";
            dataPackage.DataString += "," + dataPackage.SensorID.ToString();
            dataPackage.DataString += "," + dataPackage.SensorTYpe.ToString();
            dataPackage.DataString += "," + dataPackage.BoardTime.ToString();
            dataPackage.DataString += "," + dataPackage.Temperature.ToString();
            dataPackage.DataString += "," + dataPackage.Battery.ToString();
            dataPackage.DataString += "," + dataPackage.WindSpeed.ToString();
            dataPackage.DataString += "," + dataPackage.WindDirection.ToString();
            dataPackage.DataString += "," + dataPackage.Huminity.ToString();

            dataPackage.PressureList = new int[10];
            int i = 11;
            for (int j = 0; j < 10; j++)
            {
                dataPackage.PressureList[j] = ((data[i] << 8) + data[i + 1]);
                dataPackage.TimeSeries[j] = dataPackage.Time.AddMilliseconds(100 * j);
                dataPackage.DataString += "," + dataPackage.PressureList[j].ToString();
                i += 2;
            }
            return dataPackage;
        }
    }
    public class ProgramSetting
    {
        public string ProjectName { set; get; }
        public string CloudPath { set; get; }
        public string LocalPath { set; get; }
        public string ProjectLocation { set; get; }
        public string SensorConfPath { set; get; }
        public TimeSpan _uploadSpan;
        public string UploadSpan
        {
            set
            {
                _uploadSpan = TimeSpan.FromMinutes(Math.Max(1, int.Parse(value)));
            }
            get
            {
                return _uploadSpan.TotalMinutes.ToString("F0");
            }
        }
        public TimeSpan _tokenRefreshSpan;
        public string TokenRefreshSpan
        {
            set
            {
                _tokenRefreshSpan = TimeSpan.FromMinutes(Math.Max(60, int.Parse(value)));
            }
            get
            {
                return _tokenRefreshSpan.TotalMinutes.ToString("F0");
            }
        }
        public string Username { set; get; }
        public string Password { set; get; }
        public string PortName { set; get; }
        public int BaudRate { set; get; }
    }
    public class IpStackApi
    {
        public string ip { get; set; }
        public string type { get; set; }
        public string continent_code { get; set; }
        public string continent_name { get; set; }
        public string country_code { get; set; }
        public string country_name { get; set; }
        public string region_code { get; set; }
        public string region_name { get; set; }
        public string city { get; set; }
        public string zip { get; set; }
        public float latitude { get; set; }
        public float longitude { get; set; }
        public Location location { get; set; }

        public class Location
        {
            public int geoname_id { get; set; }
            public string capital { get; set; }
            public Language[] languages { get; set; }
            public string country_flag { get; set; }
            public string country_flag_emoji { get; set; }
            public string country_flag_emoji_unicode { get; set; }
            public string calling_code { get; set; }
            public bool is_eu { get; set; }
        }

        public class Language
        {
            public string code { get; set; }
            public string name { get; set; }
            public string native { get; set; }
        }
    }
    public class IpgeoLocationApi
    {
        public string ip { get; set; }
        public string continent_code { get; set; }
        public string continent_name { get; set; }
        public string country_code2 { get; set; }
        public string country_code3 { get; set; }
        public string country_name { get; set; }
        public string country_capital { get; set; }
        public string state_prov { get; set; }
        public string district { get; set; }
        public string city { get; set; }
        public string zipcode { get; set; }
        public double latitude { get; set; }
        public double longitude { get; set; }
        public bool is_eu { get; set; }
        public string calling_code { get; set; }
        public string country_tld { get; set; }
        public string languages { get; set; }
        public string country_flag { get; set; }
        public string geoname_id { get; set; }
        public string isp { get; set; }
        public string connection_type { get; set; }
        public string organization { get; set; }
        public Currency currency { get; set; }
        public Time_Zone time_zone { get; set; }

        public class Currency
        {
            public string code { get; set; }
            public string name { get; set; }
            public string symbol { get; set; }
        }

        public class Time_Zone
        {
            public string name { get; set; }
            public int offset { get; set; }
            public string current_time { get; set; }
            public float current_time_unix { get; set; }
            public bool is_dst { get; set; }
            public int dst_savings { get; set; }
        }

    }
}
