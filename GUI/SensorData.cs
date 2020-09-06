using SciChart.Charting.Model.DataSeries;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;

namespace GUI
{
    public class SensorData : INotifyPropertyChanged
    {
        private enum Type { Temprature, Battery, Pressure, WindSpeed, WindDirection, Huminity };

        public enum StatusValue { Ok, Lost, Error, Wait };

        private readonly string[] windName = new string[16] { "N", "NNE", "NE", "ENE", "E", "ESE", "SE", "SSE", "S", "SSW", "SW", "WSW", "W", "WNW", "NW", "NNW" };

        // These fields hold the values for the public properties.
        private int _temperature;

        private int _batteryLevel;
        private int _pressure;
        private int _windSpeed;
        private int _windDirection;
        private int _huminity;
        private StatusValue _status;

        public SensorInfo.Types SensorType { get; set; }

        private const double RefVol = 3.3;
        private const int BitDepth = 16;
        private TimeSpan WaitTime = new TimeSpan(0, 0, 10);
        private TimeSpan ErrorTime = new TimeSpan(0, 1, 0);
        private TimeSpan LostTime = new TimeSpan(1, 0, 0);
        private static readonly object locker = new object();
        private static readonly object Tlocker = new object();
        public bool isSI = true;
        private DateTime lastUpdate;

        public Format.PlotControl PlotControl = new Format.PlotControl() { Scale = 5 };
        public List<Format.TimeSeries> Pressure3s = new List<Format.TimeSeries>();
        public List<Format.TimeSeries> Pressure5m = new List<Format.TimeSeries>();
        public List<Format.TimeSeries> Pressure30m = new List<Format.TimeSeries>();
        public List<Format.TimeSeries> Huminity5m = new List<Format.TimeSeries>();
        public List<Format.TimeSeries> Temperature5m = new List<Format.TimeSeries>();

        public XyDataSeries<DateTime, double> PressureLine = new XyDataSeries<DateTime, double>() { SeriesName = "Pressure", AcceptsUnsortedData = true };
        public XyDataSeries<double, double> WindPlot = new XyDataSeries<double, double>();

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
                return ConvertToString(this._temperature, Type.Temprature);
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

        public double WindScale
        {
            get
            {
                return 1;
            }
        }

        public string WindDirectionString
        {
            get
            {
                return ConvertToString(this._windDirection, Type.WindDirection);
            }
        }

        public double WindAngle
        {
            get
            {
                double direction = (double)this._windDirection / (2 << (BitDepth - 1)) * RefVol * (57.6 + 150) / 57.6 * 72;
                if (SensorType == SensorInfo.Types.Anemometer) return direction;
                //if (SensorType == SensorInfo.Types.Anemometer) return (double)this._windDirection / (2 << (BitDepth - 1)) * RefVol * 57.6 / (57.6 + 150) * 20;
                else return direction;
            }
        }

        public string HuminityString
        {
            get
            {
                return ConvertToString(this._huminity, Type.Huminity);
            }
        }

        #endregion Data String

        #region Real Time Data

        public int Temperature
        {
            get
            {
                lock (Tlocker)
                {
                    return this._temperature;
                }
            }
            set
            {
                if (value != this._temperature)
                {
                    lock (Tlocker)
                    {
                        this._temperature = value;
                    }
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

        public string BatteryString
        {
            get
            {
                double voltage = (ConvertToDouble(this.BatteryLevel, Type.Battery) - 3.6) / (4.2 - 3.6) * 100.0;
                if (ConvertToDouble(this.BatteryLevel, Type.Battery) < 3.2) return "N/A";
                else return voltage.ToString("F0") + "%";
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
                    NotifyPropertyChanged("WindAngle");
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

        public StatusValue Status
        {
            get
            {
                return _status;
            }
            set
            {
                if (value != _status)
                {
                    this._status = value;
                    NotifyPropertyChanged();
                }
            }
        }

        #endregion Real Time Data

        #region Average Data

        public string PressureAvg3s
        {
            get
            {
                lock (locker)
                {
                    if (this.Pressure3s.Count < 1) return "0";
                    else return ConvertToString((int)Pressure3s.Average(t => t.Value), Type.Pressure);
                }
            }
        }

        public string PressureMin3s
        {
            get
            {
                lock (locker)
                {
                    if (this.Pressure3s.Count < 1) return "0";
                    else return ConvertToString((int)Pressure3s.Min(t => t.Value), Type.Pressure);
                }
            }
        }

        public string PressureMax3s
        {
            get
            {
                lock (locker)
                {
                    if (this.Pressure3s.Count < 1) return "0";
                    else return ConvertToString((int)Pressure3s.Max(t => t.Value), Type.Pressure);
                }
            }
        }

        public string PressureAvg5m
        {
            get
            {
                lock (locker)
                {
                    if (this.Pressure5m.Count < 1) return "0";
                    else return ConvertToString((int)Pressure5m.Average(t => t.Value), Type.Pressure);
                }
            }
        }

        public string PressureMin5m
        {
            get
            {
                lock (locker)
                {
                    if (this.Pressure5m.Count < 1) return "0";
                    else return ConvertToString((int)Pressure5m.Min(t => t.Value), Type.Pressure);
                }
            }
        }

        public string PressureMax5m
        {
            get
            {
                lock (locker)
                {
                    if (this.Pressure5m.Count < 1) return "0";
                    else return ConvertToString((int)Pressure5m.Max(t => t.Value), Type.Pressure);
                }
            }
        }

        #endregion Average Data

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
                    //if (isSI) return res.ToString("F1") + "ºC";
                    //else return (res * 1.8 + 32).ToString("F1") + "ºF";
                    return (res * 1.8 + 32).ToString("F2") + "ºF";

                case Type.Battery:
                    if (voltage < 0.2) return "0V";
                    return (voltage * 2).ToString("F2") + "V";

                case Type.Pressure:
                    res = (voltage / RefVol + 0.095) / 0.009 * 10;
                    if (isSI) return res.ToString("F2"); //+ "mBar";
                    else return (res * 0.145037738).ToString("F2") + "PSI";
                case Type.WindSpeed:
                    if (SensorType == SensorInfo.Types.Anemometer) return (voltage * (57.6 + 150) / 57.6 * 20).ToString("F2") + "m/s";
                    else return "N/A";
                case Type.WindDirection:
                    double direction = (voltage * (57.6 + 150) / 57.6 * 72);
                    string name = windName[(int)(direction % 360 / 22.5)];
                    if (SensorType == SensorInfo.Types.Anemometer) return name + " " + direction.ToString("F0") + "º";
                    else return "N/A";
                case Type.Huminity:
                    double sRH = (voltage / RefVol - 0.1515) / 0.00636;
                    double temp = ((Temperature / (2 << (BitDepth - 1)) * RefVol) - 0.5) / 0.01;
                    double tRH = sRH / (1.0546 - 0.00216 * temp);
                    if (SensorType == SensorInfo.Types.Humidity) return tRH.ToString("F0") + "%RH";
                    else return "N/A";
                default:
                    return "error";
            }
        }

        private double ConvertToDouble(int value, Type type)
        {
            double voltage = (double)value / (2 << (BitDepth - 1)) * RefVol;
            double res;
            switch (type)
            {
                case Type.Temprature:
                    res = (voltage - 0.5) / 0.01;
                    if (isSI) return res;
                    else return (res * 1.8 + 32);
                case Type.Battery:
                    return (voltage * 2);

                case Type.Pressure:
                    res = (voltage / RefVol + 0.095) / 0.009 * 10;
                    if (isSI) return res; //+ "mBar";
                    else return (res * 0.145037738);
                case Type.WindSpeed:
                    if (SensorType == SensorInfo.Types.Anemometer) return (voltage * (57.6 + 150) / 57.6 * 20);
                    else return 0;
                case Type.WindDirection:
                    double direction = (voltage * (57.6 + 150) / 57.6 * 72);
                    if (SensorType == SensorInfo.Types.Anemometer) return direction;
                    else return 0;
                case Type.Huminity:
                    double sRH = (voltage / RefVol - 0.1515) / 0.00636;
                    double temp = ((Temperature / (2 << (BitDepth - 1)) * RefVol) - 0.5) / 0.01;
                    double tRH = sRH / (1.0546 - 0.00216 * temp);
                    if (SensorType == SensorInfo.Types.Humidity) return tRH;
                    else return 0;
                default:
                    return 0;
            }
        }

        public void GetSensorData(Format.DataPackage package)
        {
            Temperature = package.Temperature;
            BatteryLevel = package.Battery;
            Pressure = package.Pressure;
            WindSpeed = package.WindSpeed;
            WindDirection = package.WindDirection;
            Huminity = package.Huminity;
            NotifyPropertyChanged("BatteryString");
            Status = StatusValue.Ok;
            lastUpdate = DateTime.Now;
            if (this.SensorType == SensorInfo.Types.Anemometer)
            {
                WindPlot.Clear();
                WindPlot.Append(ConvertToDouble(WindDirection, Type.WindDirection), ConvertToDouble(WindSpeed, Type.WindSpeed));
                return;
            }

            double[] pressureL = new double[10];
            for (int i = 0; i < 10; i++)
            {
                pressureL[i] = ConvertToDouble(package.PressureList[i], Type.Pressure);
            }
            PressureLine.Append(package.TimeSeries, pressureL);
            lock (locker)
            {
                AddData(ref Pressure3s, package.TimeSeries, package.PressureList, -3);
                NotifyPropertyChanged("PressureAvg3s");
                NotifyPropertyChanged("PressureMax3s");
                NotifyPropertyChanged("PressureMin3s");
                AddData(ref Pressure5m, package.TimeSeries, package.PressureList, -300);
                NotifyPropertyChanged("PressureAvg5m");
                NotifyPropertyChanged("PressureMax5m");
                NotifyPropertyChanged("PressureMin5m");
            }
        }

        /// <summary>
        /// Add data point to list and remove old values.
        /// </summary>
        /// <param name="series">Series to add</param>
        /// <param name="time">Data time</param>
        /// <param name="value">Value to add</param>
        /// <param name="interval">Time interval in second</param>
        private void AddData(ref List<Format.TimeSeries> series, DateTime[] time, int[] value, double interval)
        {
            if (time.Length == value.Length)
            {
                for (int i = 0; i < time.Length; i++)
                {
                    series.Add(new Format.TimeSeries(time[i], value[i]));
                }
            }
            series.RemoveAll(t => t.DateTime < time[time.Length - 1].AddSeconds(interval));
        }

        public void CheckStatus()
        {
            if (DateTime.Now - lastUpdate > WaitTime)
            {
                Status = StatusValue.Wait;
            }
            if (DateTime.Now - lastUpdate > ErrorTime)
            {
                Status = StatusValue.Error;
            }
            if (DateTime.Now - lastUpdate > LostTime)
            {
                Status = StatusValue.Lost;
            }
        }
    }
}