using System;
using System.Collections.Generic;
using System.Timers;
using System.Windows;

namespace GUI
{
    class SensorWatcher
    {
        private List<SensorInfo> sensorInfos;
        private static Timer aTimer;
        public void SetTimer(List<SensorInfo> sensors)
        {
            // Create a timer with a two second interval.
            aTimer = new Timer(10000);
            // Hook up the Elapsed event for the timer. 
            aTimer.Elapsed += CheckSensorStatus;
            aTimer.AutoReset = true;
            aTimer.Enabled = true;
            sensorInfos = sensors;
        }

        private void CheckSensorStatus(Object source, ElapsedEventArgs e)
        {
            sensorInfos.ForEach(delegate (SensorInfo sensor)
            {
                Application.Current.Dispatcher.Invoke(() => //Use invoke
                {
                    sensor.SensorData.CheckStatus();
                });
            });
        }
    }
}
