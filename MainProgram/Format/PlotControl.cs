﻿using SciChart.Charting.Visuals;
using SciChart.Charting.Visuals.Axes;
using SciChart.Data.Model;
using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace Format
{
    /// <summary>
    /// Control of SciChart.Manage the range of x and y axis.
    /// </summary>
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
}