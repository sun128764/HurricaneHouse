using System;

namespace Format
{
    /// <summary>
    /// Timer series class.
    /// </summary>
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
}