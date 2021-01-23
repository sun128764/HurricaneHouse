using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Timers;

namespace MainProgram
{
    /// <summary>
    /// Set the state of the sensor network.
    /// </summary>
    internal class StateManager : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        // This method is called by the Set accessor of each property.
        // The CallerMemberName attribute that is applied to the optional propertyName
        // parameter causes the property name of the caller to be substituted as an argument.
        protected void NotifyPropertyChanged([CallerMemberName] String propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public StateManager(DataLoger dataLoger, SerialCOM serialCOM, List<DataBaseUtils> dataBaseUtils, MainWindow mainWindow, State state)
        {
            DataLoger = dataLoger;
            SerialCOM = serialCOM;
            DataBaseUtilsList = dataBaseUtils;
            MainWindow = mainWindow;
            // Create a timer with a 1 second interval.
            aTimer = new Timer(1000);
            // Hook up the Elapsed event for the timer.
            aTimer.Elapsed += SendSleepCommand;
            aTimer.AutoReset = true;
            aTimer.Enabled = false;
            _state = state;
            SetRecord();
        }

        public enum State { Recording, Sleep, NoUpload }

        private readonly DataLoger DataLoger;
        private readonly SerialCOM SerialCOM;
        private readonly List<DataBaseUtils> DataBaseUtilsList;
        private readonly MainWindow MainWindow;
        private static Timer aTimer;

        public void SetState(string str)
        {
            switch (str)
            {
                case "Normal recording":
                    SensorState = State.Recording;
                    break;

                case "No uploading":
                    SensorState = State.NoUpload;
                    break;

                case "Sleep mode":
                    SensorState = State.Sleep;
                    break;

                default:
                    break;
            }
        }

        private State _state;

        public State SensorState
        {
            set
            {
                if (value == _state)
                {
                    return;
                }
                _state = value;
                switch (value)
                {
                    case State.NoUpload:
                        SetNoUpload();
                        break;

                    case State.Recording:
                        SetRecord();
                        break;

                    case State.Sleep:
                        SetSleep();
                        break;

                    default:
                        break;
                }
                NotifyPropertyChanged();
            }
            get
            {
                return _state;
            }
        }

        private void SetRecord()
        {
            aTimer.Stop();
            MainWindow.isCollecting = true;
            DataLoger.enableUpload = true;
            DataBaseUtilsList?.ForEach(p => p.EnableUpload());
        }

        private void SetNoUpload()
        {
            aTimer.Stop();
            MainWindow.isCollecting = true;
            DataLoger.enableUpload = false;
            DataLoger.ClearData();
            DataBaseUtilsList?.ForEach(p => p.DisableUpload());
        }

        private void SetSleep()
        {
            SetNoUpload();
            aTimer.Start();
        }

        private void SendSleepCommand(Object source, ElapsedEventArgs e)
        {
            SerialCOM.SendCommand("Sleep");
        }
    }
}