using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
using System.Collections.Generic;
using System.Linq;
using SciChart.Charting.Model.DataSeries;
namespace GUI
{
    public class SensorData : INotifyPropertyChanged
    {
        private enum Type { Temprature, Battery, Pressure, WindSpeed, WindDirection, Huminity };
        // These fields hold the values for the public properties.
        private int TemperatureValue;
        private int BatteryLevelValue;
        public int PressureValue;
        private int WindSpeedValue;
        private int WindDirectionValue;
        private int HuminityValue;
        private int StatusValue;


        private const double RefVol = 3.3;
        private const int BitDepth = 16;
        private static readonly object locker = new object();
        public bool isSI = true;

        public List<Format.TimeSeries> Pressure1m = new List<Format.TimeSeries>();
        public List<Format.TimeSeries> Pressure5m = new List<Format.TimeSeries>();
        public List<Format.TimeSeries> Pressure30m = new List<Format.TimeSeries>();
        public List<Format.TimeSeries> Huminity5m = new List<Format.TimeSeries>();
        public List<Format.TimeSeries> Temperature5m = new List<Format.TimeSeries>();

        public XyDataSeries<DateTime, double> PressureLine = new XyDataSeries<DateTime, double>();
        public XyDataSeries<DateTime, double> Pressure1mLine = new XyDataSeries<DateTime, double>();
        public XyDataSeries<DateTime, double> Pressure5mLine = new XyDataSeries<DateTime, double>();

        public event PropertyChangedEventHandler PropertyChanged;

        // This method is called by the Set accessor of each property.
        // The CallerMemberName attribute that is applied to the optional propertyName
        // parameter causes the property name of the caller to be substituted as an argument.
        protected void NotifyPropertyChanged([CallerMemberName] String propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        #region Real Time Data
        public string Temperature
        {
            get
            {
                return ((this.TemperatureValue / 65536d * 3.3 - 0.5) / 0.01).ToString("F3") + "ºC";
            }
            set
            {
                if (int.Parse(value) != this.TemperatureValue)
                {
                    this.TemperatureValue = int.Parse(value);
                    NotifyPropertyChanged();
                }
            }
        }
        /// <summary>
        /// The battery voltage. Needs to be calibrated. R1 and R3.
        /// </summary>
        public string BatteryLevel
        {
            get
            {
                return ConvertToString(this.BatteryLevelValue, Type.Battery);
            }
            set
            {
                if (int.Parse(value) != this.BatteryLevelValue)
                {
                    this.BatteryLevelValue = int.Parse(value);
                    NotifyPropertyChanged();
                }
            }
        }

        public string Pressure
        {
            get
            {
                return ConvertToString(this.PressureValue, Type.Pressure);
            }
            set
            {
                if (int.Parse(value) != this.PressureValue)
                {
                    this.PressureValue = int.Parse(value);
                    NotifyPropertyChanged();
                }
            }
        }

        public string WindSpeed
        {
            get
            {
                return ConvertToString(this.WindSpeedValue, Type.WindSpeed);
            }
            set
            {
                if (int.Parse(value) != this.WindSpeedValue)
                {
                    this.WindSpeedValue = int.Parse(value);
                    NotifyPropertyChanged();
                }
            }
        }
        public string WindDirection
        {
            get
            {
                return ConvertToString(this.WindDirectionValue, Type.WindDirection);
            }
            set
            {
                if (int.Parse(value) != this.WindDirectionValue)
                {
                    this.WindDirectionValue = int.Parse(value);
                    NotifyPropertyChanged();
                }
            }
        }
        public string Huminity
        {
            get
            {
                return ConvertToString(this.HuminityValue, Type.Huminity);
            }
            set
            {
                if (int.Parse(value) != this.HuminityValue)
                {
                    this.HuminityValue = int.Parse(value);
                    NotifyPropertyChanged();
                }
            }
        }
        #endregion
        #region Average Data
        public string PressureAvg1m
        {
            get
            {
                lock (locker)
                {
                    if (this.Pressure1m.Count < 1) return "0";
                    else return ConvertToString((int)this.Pressure1m.Average(t => t.Value), Type.Pressure);
                }

            }
        }

        #endregion
        //public string Pressure1mAvg
        //{
        //    get
        //    {
        //        return Pressure1m.Average(t => t.Value);
        //    }
        //}
        /// <summary>
        /// Convert ADC reading to actual value; SI or Eng unit is determined by SensorData.isSI;
        /// </summary>
        /// <param name="value">ADC reading</param>
        /// <param name="datatype">The type of ADC reading</param>
        /// <param name="depth">The bit depth of ADC</param>
        /// <returns>String of the data with unit name;</returns>
        private string ConvertToString(int value, Type datatype)
        {
            double voltage = (double)value / (2 << (BitDepth - 1)) * RefVol;
            double res;
            switch (datatype)
            {
                case Type.Temprature:
                    res = (voltage - 0.5) / 0.01;
                    if (isSI) return res.ToString("F3") + "ºC";
                    else return ((res - 32) / 1.8).ToString("F3") + "ºF";
                case Type.Battery:
                    return (voltage * 2).ToString("F3") + "V";
                case Type.Pressure:
                    res = (voltage / RefVol + 0.095) / 0.009;
                    if (isSI) return res.ToString("F3") + "kPa";
                    else return (res * 0.145037738).ToString("F3") + "PSI";
                case Type.WindSpeed:
                    return "N/A";
                case Type.WindDirection:
                    return "N/A";
                case Type.Huminity:
                    return "N/A";
                default:
                    return "error";
            }
        }
        public void GetSensorData(string s)
        {
            Regex regex = new Regex(@"-?[0-9]\d*");
            MatchCollection match = regex.Matches(s);
            if (match.Count == 5)
            {
                DateTime time = DateTime.Now;
                int i = 0;
                Temperature = match[i++].Value;
                BatteryLevel = match[i++].Value;
                Pressure = match[i++].Value;
                WindSpeed = match[i++].Value;
                Huminity = match[i++].Value;
                lock (locker)
                {
                    AddData(ref Pressure1m, time, PressureValue, -1);
                    NotifyPropertyChanged("PressureAvg1m");
                    AddData(ref Pressure5m, time, PressureValue, -5);
                    AddData(ref Pressure30m, time, PressureValue, -30);
                    AddData(ref Temperature5m, time, TemperatureValue, -5);
                    AddData(ref Huminity5m, time, HuminityValue, -5);
                }

            }
        }
        private void AddData(ref List<Format.TimeSeries> series, DateTime time, int value, double interval)
        {
            series.Add(new Format.TimeSeries(time, value));
            List<int> remove = new List<int>();
            foreach (Format.TimeSeries series1 in series)
            {
                if (series1.DateTime < time.AddMinutes(interval)) remove.Add(series.IndexOf(series1));
            }
            foreach (int i in remove) series.RemoveAt(i);
        }

    }
}