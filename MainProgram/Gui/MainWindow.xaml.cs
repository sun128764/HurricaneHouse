using Format;
using Newtonsoft.Json;
using SciChart.Data.Model;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Ports;
using System.Threading;
using System.Windows;
using System.Windows.Input;

namespace MainProgram
{
    public partial class MainWindow : Window
    {
        public string SettingPath;
        public List<SensorInfo> SensorInfos { set; get; }
        public SensorInfo SelectedSensor { set; get; }
        public List<string> datastring;
        public SensorInfo WindSensor { set; get; }
        private DataLoger dataLogger;
        private SensorWatcher sensorWatcher;
        public IRange FixRange => new DoubleRange(0, 360);
        private DataBaseList dataBaseUtils;
        private SerialCOM SerialCOM;
        public bool isCollecting;
        private StateManager StateManager;

        public MainWindow()
        {
            InitializeComponent();
            System.Windows.Media.RenderOptions.ProcessRenderMode = System.Windows.Interop.RenderMode.SoftwareOnly;
            datastring = new List<string>();
            SensorInfos = new List<SensorInfo>();
            DataContext = this;
            sensorWatcher = new SensorWatcher();
            SerialCOM = new SerialCOM();
            try
            {
                DbInfoList infoList = DbInfoList.ReadDbList(Environment.CurrentDirectory + "\\database.json");
                dataBaseUtils = new DataBaseList(infoList);
            }
            catch (Exception)
            {
                //First message box will be closed automatically since a "Enter" is in the Keyboard stream for some reason.
                MessageBox.Show("Can not read setting file. Please choose correct file.");
                MessageBox.Show("Can not read setting file. Please choose correct file.");
            }
            isCollecting = true;
        }

        public void InitRecording(ProgramSetting setting)
        {
            Mouse.OverrideCursor = Cursors.Wait;
            Busy.IsBusy = true; //Enable busy indicator to bolck main window
            //Use background thread to execute initialization
            ThreadPool.QueueUserWorkItem((object state) =>
            {
                SensorInfos.Clear();
                string conf = File.ReadAllText(setting.SensorConfPath);
                SensorInfos.AddRange(JsonConvert.DeserializeObject<List<SensorInfo>>(conf));
                SelectedSensor = SensorInfos[0];
                WindSensor = SensorInfos.Find(t => t.SensorType == SensorInfo.Types.Anemometer); //Find Anemometer
                if (WindSensor == null)
                {
                    MessageBox.Show("No anemometer found in sensor list.", "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
                }
                else
                {
                    Application.Current.Dispatcher.Invoke(() => //Use invoke to refresh UI elements
                    {
                        WindInfo.DataContext = WindSensor.SensorData;
                    });
                }
                dataLogger = new DataLoger();
                string result = dataLogger.Init(setting);
                if (result == "Error") //If the Tapis is not available, abort init.
                {
                    MessageBox.Show("Unable to refresh Tapis token.Please creat token manually.");
                    return;
                }
                if (SerialCOM.IsOpen)
                {
                    SerialCOM.Close();
                }
                SerialCOM.InitCOM(setting.PortName, setting.BaudRate, new SerialDataReceivedEventHandler(SerialPort_DataReceived));
                sensorWatcher.SetTimer(SensorInfos);
                Application.Current.Dispatcher.Invoke(() => //Use invoke to refresh UI elements
                {
                    NodeList.Items.Refresh();
                    sciChartSurface.DataContext = SelectedSensor.SensorData.PlotControl;
                    sll.DataContext = SelectedSensor.SensorData.PlotControl;
                    lll.DataContext = SelectedSensor.SensorData.PlotControl;
                    Status.DataContext = SelectedSensor.SensorData;
                    CloudStatus.DataContext = dataLogger;
                    StateManager = new StateManager(dataLogger, SerialCOM, dataBaseUtils, this, StateManager.State.Recording);
                    ModeStatus.DataContext = StateManager;
                    Mouse.OverrideCursor = null;
                    Busy.IsBusy = false; //Disable busy indicator.
                });
            }, null);
        }

        /// <summary>
        /// Data received event.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void SerialPort_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            if (!isCollecting) return;
            byte[] data = new byte[36];
            try
            {
                while (SerialCOM.serialPort.ReadByte() != 255) ;
                while (SerialCOM.serialPort.BytesToRead < 36) ;
                SerialCOM.serialPort.Read(data, 0, 36);
            }
            catch
            {
                return;
            }
            DataPackage dataPackage = DataPackage.Decode(data);
            if (dataPackage == null || !dataPackage.passCrc32) return;
            dataBaseUtils?.PostData(dataPackage);
            dataLogger.AddData(dataPackage.DataString);
            SensorInfo sensorInfo = SensorInfos.Find(x => x.SensorID == dataPackage.SensorID);
            if (sensorInfo == null)
            {
                //Add undefined sensor to list.
                SensorInfo info = new SensorInfo()
                {
                    Name = "Undefind" + dataPackage.SensorID.ToString(),
                    SensorID = dataPackage.SensorID,
                    SensorType = (SensorInfo.Types)(dataPackage.SensorTYpe - 1),
                    NetWorkID = dataPackage.NetworkID,
                };
                SensorInfos.Add(info);
                Application.Current.Dispatcher.Invoke(() => //Use invoke to refresh UI elements
                {
                    NodeList.Items.Refresh();
                });
                sensorInfo = info;
                //Set as Anemometer if it is not found before.
                if (WindSensor == null && info.SensorType == SensorInfo.Types.Anemometer)
                {
                    WindSensor = info;
                }
            }
            if (sensorInfo.SensorType != (SensorInfo.Types)(dataPackage.SensorTYpe - 1)) return;
            //Add data to sensor data class and plot
            sensorInfo.SensorData.GetSensorData(dataPackage);
            if (sensorInfo == SelectedSensor)
            {
                using (sciChartSurface.SuspendUpdates())
                {
                    SelectedSensor.SensorData.PlotControl.RefreshLimit(DateTime.Now);
                    LineSeries.DataSeries = SelectedSensor.SensorData.PressureLine;
                }
            }
            if (sensorInfo == WindSensor)
            {
                using (sciChartSurface.SuspendUpdates())
                {
                    WindSeries.DataSeries = WindSensor.SensorData.WindPlot;
                }
            }
        }

        private void Label_MouseDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            SensorInfo sensor = NodeList.SelectedItem as SensorInfo;
            if (sensor != SelectedSensor)
            {
                SelectedSensor = sensor;
                Status.DataContext = SelectedSensor.SensorData;
                LineSeries.DataSeries = SelectedSensor.SensorData.PressureLine;
                sciChartSurface.DataContext = SelectedSensor.SensorData.PlotControl;
                sll.DataContext = SelectedSensor.SensorData.PlotControl;
                lll.DataContext = SelectedSensor.SensorData.PlotControl;
            }
        }

        private void SettingBtn_Click(object sender, RoutedEventArgs e)
        {
            var wizard = new Wizard();
            wizard.ShowDialog();
            Mouse.OverrideCursor = null;
            if (wizard.isFinished)
            {
                InitRecording(wizard.ProgramSetting);
            }
        }

        private void ModeBtn_Click(object sender, RoutedEventArgs e)
        {
            if (StateManager == null) return;
            var ChangeModeWindow = new ChangeModeWindow();
            ChangeModeWindow.ShowDialog();
            StateManager.SetState(ChangeModeWindow.ModeName.Text);
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (SerialCOM.IsOpen)
            {
                MessageBoxResult result = MessageBox.Show("Data recording. Do you want to exit?", "Warning", MessageBoxButton.YesNo, MessageBoxImage.Question);
                if (result == MessageBoxResult.Yes)
                {
                    dataLogger?.AddData(null);
                    e.Cancel = false;
                }
                else
                {
                    e.Cancel = true;
                }
            }
        }
    }
}