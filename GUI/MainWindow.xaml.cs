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

namespace GUI
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {
        public string SettingPath;
        public string[] PortListData { get; set; }
        public List<SensorInfo> SensorInfos { set; get; }
        public SensorInfo SelectedSensor { set; get; }
        public List<string> datastring;
        public SensorInfo WindSensor { set; get; }
        private DataLoger dataLogger;
        public MainWindow()
        {
            InitializeComponent();
            datastring = new List<string>();
            SensorInfos = new List<SensorInfo>();
            PortListData = SerialPort.GetPortNames();
            DataContext = this;
        }
        public void InitRecording(Format.ProgramSetting setting)
        {
            Mouse.OverrideCursor = Cursors.Wait;
            Busy.IsBusy = true;
            ThreadPool.QueueUserWorkItem((object state) =>
            {
                SensorInfos.Clear();
                string conf = File.ReadAllText(setting.SensorConfPath);
                SensorInfos.AddRange(JsonConvert.DeserializeObject<List<SensorInfo>>(conf));
                SelectedSensor = SensorInfos[0];
                WindSensor = SensorInfos.Find(t => t.SensorType == SensorInfo.Types.Anemometer);
                if (WindSensor == null)
                {
                    MessageBox.Show("No anemometer found in sensor list.", "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
                }
                else
                {
                    WindInfo.DataContext = WindSensor.SensorData;
                }
                dataLogger = new DataLoger();
                string result = dataLogger.Init(setting);
                if (result == "Error")
                {
                    MessageBox.Show("Unable to refresh Tapis token.Please creat token manually.");
                    return;
                }
                InitCOM(setting.PortName);
                Application.Current.Dispatcher.Invoke(() =>
                {
                    NodeList.Items.Refresh();
                    sciChartSurface.DataContext = SelectedSensor.SensorData.PlotControl;
                    sll.DataContext = SelectedSensor.SensorData.PlotControl;
                    lll.DataContext = SelectedSensor.SensorData.PlotControl;
                    Status.DataContext = SelectedSensor.SensorData;
                    Mouse.OverrideCursor = null;
                    Busy.IsBusy = false;
                });
            }, null);
        }

        public SerialPort serialPort;//串口对象类

        public bool InitCOM(string PortName)
        {
            serialPort = new SerialPort(PortName, 9600, Parity.None, 8, StopBits.One);
            serialPort.DataReceived += new SerialDataReceivedEventHandler(SerialPort_DataReceived);//DataReceived事件委托
            serialPort.ReceivedBytesThreshold = 31;
            serialPort.RtsEnable = true;
            return OpenPort();//串口打开
        }

        /// 数据接收事件
        private void SerialPort_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            // Thread.Sleep(2000);
            //serialPort.Read(readBuffer, 0, readBuffer.Length);
            while (serialPort.ReadByte() != 255) ;
            while (serialPort.BytesToRead < 31) ;
            byte[] data = new byte[31];
            serialPort.Read(data, 0, 31);
            Format.DataPackage dataPackage = Format.DataPackage.Decode(data);
            if (dataPackage != null)
            {
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
        //打开串口的方法
        public bool OpenPort()
        {
            try//这里写成异常处理的形式以免串口打不开程序崩溃
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

        //向串口发送数据
        public void SendCommand(string CommandString)
        {
            byte[] WriteBuffer = Encoding.ASCII.GetBytes(CommandString);
            serialPort.Write(WriteBuffer, 0, WriteBuffer.Length);
        }

        private void ConnectBtn_Click(object sender, RoutedEventArgs e)
        {
            ComboBox comboBox = PortList;
            if (comboBox.SelectedItem != null) InitCOM(comboBox.Text);
            Button button = sender as Button;
            if (serialPort.IsOpen) button.IsEnabled = false;
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
            //System.Diagnostics.Process.Start("Explorer.exe", @"/select,C:\mylog.log");
        }
        private void SettingBtn_Click(object sender, RoutedEventArgs e)
        {
            var settingWindow = new SettingMaker();
            settingWindow.ShowDialog();
            var wizard = new Wizard();
            wizard.ShowDialog();
            if (wizard.isFinished)
            {
                InitRecording(wizard.ProgramSetting);
            }
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            dataLogger.AddData(null);
        }
    }
}