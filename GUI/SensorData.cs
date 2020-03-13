using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
using System.Collections.Generic;
namespace GUI
{
    public class SensorData : INotifyPropertyChanged
    {
        // These fields hold the values for the public properties.
        private int TemperatureValue;
        private int BatteryLevelValue;
        public int PressureValue;
        private int WindSpeedValue;
        private int HuminityValue;
        public List<Format.TimeSeries> Pressure1m;
        public List<Format.TimeSeries> Pressure5m;
        public List<Format.TimeSeries> Pressure30m;
        public List<Format.TimeSeries> Huminity5m;
        public List<Format.TimeSeries> Temperature5m;

        public event PropertyChangedEventHandler PropertyChanged;

        // This method is called by the Set accessor of each property.
        // The CallerMemberName attribute that is applied to the optional propertyName
        // parameter causes the property name of the caller to be substituted as an argument.
        protected void NotifyPropertyChanged([CallerMemberName] String propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

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

        public string BatteryLevel
        {
            get
            {
                return (this.BatteryLevelValue / 65536d * 3.3).ToString("F3") + "V";
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
                return ((this.PressureValue / 65536d + 0.095) / 0.009).ToString("F3") + "kPa";
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
                return this.WindSpeedValue.ToString() + "Mph";
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

        public string Huminity
        {
            get
            {
                return this.HuminityValue.ToString() + "kg/m^3";
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
                AddData(ref Pressure1m, time, PressureValue, -1);
                AddData(ref Pressure5m, time, PressureValue, -5);
                AddData(ref Pressure30m, time, PressureValue, -30);
                AddData(ref Temperature5m, time, TemperatureValue, -5);
                AddData(ref Huminity5m, time, HuminityValue, -5);
            }
        }
        private void AddData(ref List<Format.TimeSeries> series,DateTime time, double value, double interval)
        {
            series.Add(new Format.TimeSeries(time, value));
            foreach(Format.TimeSeries series1 in series)
            {
                if (series1.DateTime < time.AddMinutes(interval)) series.Remove(series1);
            }
        }

    }
}