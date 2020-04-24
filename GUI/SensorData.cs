﻿using System;
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
        private int _temperature;
        private int _batteryLevel;
        private int _pressure;
        private int _windSpeed;
        private int _windDirection;
        private int _huminity;
        private int _status;

        public int SensorID { get; set; }
        public int NetworkID { get; set; }
        public int SensorType { get; set; }

        private const double RefVol = 3.3;
        private const int BitDepth = 16;
        private static readonly object locker = new object();
        public bool isSI = true;

        public List<Format.TimeSeries> Pressure1m = new List<Format.TimeSeries>();
        public List<Format.TimeSeries> Pressure5m = new List<Format.TimeSeries>();
        public List<Format.TimeSeries> Pressure30m = new List<Format.TimeSeries>();
        public List<Format.TimeSeries> Huminity5m = new List<Format.TimeSeries>();
        public List<Format.TimeSeries> Temperature5m = new List<Format.TimeSeries>();

        public XyDataSeries<DateTime, double> PressureLine = new XyDataSeries<DateTime, double>() { SeriesName = "Pressure" };

        public XyDataSeries<DateTime, double> Pressure1mLine = new XyDataSeries<DateTime, double>() { SeriesName = "Pressure1m" };
        public XyDataSeries<DateTime, double> Pressure5mLine = new XyDataSeries<DateTime, double>();

        public event PropertyChangedEventHandler PropertyChanged;

        // This method is called by the Set accessor of each property.
        // The CallerMemberName attribute that is applied to the optional propertyName
        // parameter causes the property name of the caller to be substituted as an argument.
        protected void NotifyPropertyChanged([CallerMemberName] String propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        #region Data String
        public string TemperautreString
        {
            get
            {
                return ConvertToString(this._temperature, Type.Battery);
            }
        }

        public string BatteryLevelString
        {
            get
            {
                return ConvertToString(this._batteryLevel, Type.Battery);
            }
        }
        public string PressureString
        {
            get
            {
                return ConvertToString(this._pressure, Type.Pressure);
            }
        }

        public string WindSpeedString
        {
            get
            {
                return ConvertToString(this._windSpeed, Type.WindSpeed);
            }
        }
        public string WindDirectionString
        {
            get
            {
                return ConvertToString(this._windDirection, Type.WindDirection);
            }
        }
        public string HuminityString
        {
            get
            {
                return ConvertToString(this._huminity, Type.Huminity);
            }
        }
        #endregion
        #region Real Time Data
        public int Temperature
        {
            get
            {
                return this._temperature;
            }
            set
            {
                if (value != this._temperature)
                {
                    this._temperature = value;
                    NotifyPropertyChanged("TemperautreString");
                    NotifyPropertyChanged();
                }
            }
        }
        /// <summary>
        /// The battery voltage. Needs to be calibrated. R1 and R3.
        /// </summary>
        public int BatteryLevel
        {
            get
            {
                return this._batteryLevel;
            }
            set
            {
                if (value != this._batteryLevel)
                {
                    this._batteryLevel = value;
                    NotifyPropertyChanged("BatteryLevelString");
                    NotifyPropertyChanged();
                }
            }
        }

        public int Pressure
        {
            get
            {
                return this._pressure;
            }
            set
            {
                if (value != this._pressure)
                {
                    this._pressure = value;
                    NotifyPropertyChanged("PressureString");
                    NotifyPropertyChanged();
                }
            }
        }

        public int WindSpeed
        {
            get
            {
                return this._windSpeed;
            }
            set
            {
                if (value != this._windSpeed)
                {
                    this._windSpeed = value;
                    NotifyPropertyChanged("WindSpeedString");
                    NotifyPropertyChanged();
                }
            }
        }
        public int WindDirection
        {
            get
            {
                return this._windDirection;
            }
            set
            {
                if (value != this._windDirection)
                {
                    this._windDirection = value;
                    NotifyPropertyChanged("WindDirectionString");
                    NotifyPropertyChanged();
                }
            }
        }
        public int Huminity
        {
            get
            {
                return this._huminity;
            }
            set
            {
                if (value != this._huminity)
                {
                    this._huminity = value;
                    NotifyPropertyChanged("HuminityString");
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
                    else return (res * 1.8 + 32).ToString("F3") + "ºF";
                case Type.Battery:
                    return (voltage * 2).ToString("F3") + "V";
                case Type.Pressure:
                    res = (voltage / RefVol + 0.095) / 0.009;
                    if (isSI) return res.ToString("F3") + "kPa";
                    else return (res * 0.145037738).ToString("F3") + "PSI";
                case Type.WindSpeed:
                    return (voltage * 57.6 / (57.6 + 150) * 20).ToString("F3") + "m//s";
                case Type.WindDirection:
                    return (voltage * 57.6 / (57.6 + 150) * 72).ToString("F2") + "º";
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
                Temperature = int.Parse(match[i++].Value);
                BatteryLevel = int.Parse(match[i++].Value);
                Pressure = int.Parse(match[i++].Value);
                WindSpeed = int.Parse(match[i++].Value);
                Huminity = int.Parse(match[i++].Value);
                {
                    AddData(ref Pressure1m, time, _pressure, -1);
                    NotifyPropertyChanged("PressureAvg1m");
                    AddData(ref Pressure5m, time, _pressure, -5);
                    AddData(ref Pressure30m, time, _pressure, -30);
                    AddData(ref Temperature5m, time, _temperature, -5);
                    AddData(ref Huminity5m, time, _huminity, -5);
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