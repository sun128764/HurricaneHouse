using System;
using System.IO.Ports;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Collections.Generic;
using SciChart.Charting.Model.DataSeries;
using SciChart.Charting.Visuals;
using SciChart.Charting.ChartModifiers;
using System.Threading;
using System.IO;
using Newtonsoft.Json;
using System.Windows.Input;
using System.Threading.Tasks;
using SciChart.Data.Model;

namespace GUI
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {
        public string SettingPath;
        public List<SensorInfo> SensorInfos { set; get; }
        public SensorInfo SelectedSensor { set; get; }
        public List<string> datastring;
        public SensorInfo WindSensor { set; get; }
        private DataLoger dataLogger;
        public IRange FixRange => new DoubleRange(0, 360);
        public MainWindow()
        {
            InitializeComponent();
            datastring = new List<string>();
            SensorInfos = new List<SensorInfo>();
            DataContext = this;
        }
        public void InitRecording(Format.ProgramSetting setting)
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
                InitCOM(setting.PortName);
                Application.Current.Dispatcher.Invoke(() => //Use invoke to refresh UI elements
                {
                    NodeList.Items.Refresh();
                    sciChartSurface.DataContext = SelectedSensor.SensorData.PlotControl;
                    sll.DataContext = SelectedSensor.SensorData.PlotControl;
                    lll.DataContext = SelectedSensor.SensorData.PlotControl;
                    Status.DataContext = SelectedSensor.SensorData;
                    CloudStatus.DataContext = dataLogger;
                    Mouse.OverrideCursor = null;
                    Busy.IsBusy = false; //Disable busy indicator.
                });
            }, null);
        }

        public SerialPort serialPort;//Serial object

        public bool InitCOM(string PortName)
        {
            serialPort = new SerialPort(PortName, 9600, Parity.None, 8, StopBits.One);
            serialPort.DataReceived += new SerialDataReceivedEventHandler(SerialPort_DataReceived);//DataReceived event delegate
            serialPort.ReceivedBytesThreshold = 31;
            serialPort.RtsEnable = true;
            return OpenPort();
        }

        /// <summary>
        /// Data received event.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void SerialPort_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            while (serialPort.ReadByte() != 255) ;
            while (serialPort.BytesToRead < 31) ;
            byte[] data = new byte[31];
            serialPort.Read(data, 0, 31);
            Format.DataPackage dataPackage = Format.DataPackage.Decode(data);
            if (dataPackage != null)
            {
                dataLogger.AddData(dataPackage.DataString);
                SensorInfo sensorInfo = SensorInfos.Find(x => x.SensorID == dataPackage.SensorID);
                if (sensorInfo != null)
                {
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
            }
        }
        /// <summary>
        /// Open serial port
        /// </summary>
        /// <returns>If port open success.</returns>
        public bool OpenPort()
        {
            try//Avoid program crash when port open failed.
            {
                serialPort.Open();
            }
            catch { }
            if (serialPort.IsOpen)
            {
                return true;
            }
            else
            {
                MessageBox.Show("Fial to open serial port!");
                return false;
            }
        }

        /// <summary>
        /// Send command via serial port
        /// </summary>
        /// <param name="CommandString">Command string</param>
        public void SendCommand(string CommandString)
        {
            byte[] WriteBuffer = Encoding.ASCII.GetBytes(CommandString);
            serialPort.Write(WriteBuffer, 0, WriteBuffer.Length);
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

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (serialPort != null && serialPort.IsOpen)
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