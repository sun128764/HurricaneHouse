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
        public MainWindow()
        {
            InitializeComponent();
            datastring = new List<string>();
            SensorInfos = new List<SensorInfo>();
            SensorInfo sensorInfo = new SensorInfo() { Name = "New Sensor1", NetWorkID = 5001, SensorID = 2, SensorStatus = SensorInfo.Status.Ok, SensorType = SensorInfo.Types.Humidity };
            SensorInfo sensorInfo2 = new SensorInfo() { Name = "New Sensor2", NetWorkID = 5001, SensorID = 1, SensorStatus = SensorInfo.Status.Ok, SensorType = SensorInfo.Types.Anemometer };
            SensorInfos.Add(sensorInfo);
            SensorInfos.Add(sensorInfo2);
            NodeList.Items.Refresh();
            PortListData = SerialPort.GetPortNames();
            SelectedSensor = SensorInfos[0];
            WindSensor = SensorInfos[1];
            DataContext = this;
            WindInfo.DataContext = WindSensor.SensorData;
            sciChartSurface.DataContext = SelectedSensor.SensorData.PlotControl;
            sll.DataContext = SelectedSensor.SensorData.PlotControl;
            lll.DataContext = SelectedSensor.SensorData.PlotControl;
            Status.DataContext = SelectedSensor.SensorData;
            
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
            while (serialPort.ReadByte() != 255);
            while (serialPort.BytesToRead < 31) ;
            byte[] data = new byte[31];
            serialPort.Read(data, 0, 31);
            Format.DataPackage dataPackage = Format.DataPackage.Decode(data);
            datastring.Add( dataPackage.DataString);
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
                    if(sensorInfo == WindSensor)
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
            SensorInfos.Clear();
            SensorInfos.AddRange(settingWindow.SensorInfos);
            NodeList.Items.Refresh();
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (datastring.Count > 0)
            {
                using (StreamWriter writer = File.CreateText("data.csv"))
                {
                    writer.WriteLine("Base computer time stamp(UTC), Network ID, Board ID, Type," +
                        " Sensor local time stamp, Temperature, Battery, Wind Speed, Wind Direction, " +
                        "Humidity, Pressure 1, Pressure 2, Pressure 3, Pressure 4, Pressure 5," +
                        " Pressure 6, Pressure 7, Pressure 8, Pressure 9, Pressure 10");
                    foreach (string t in datastring)
                    {
                        writer.WriteLine(t);
                    }
                }
            }
        }
    }
}