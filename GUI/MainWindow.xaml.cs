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
        public SensorData sensorData { get; set; }
        public string[] PortListData { get; set; }
        public Format.PlotControl PlotControl { get; set; }
        public List<SensorInfo> SensorInfos { set; get; }
        public XyDataSeries<DateTime, double> DispalySeries;

        public MainWindow()
        {
            InitializeComponent();
            PlotControl = new Format.PlotControl();
            SensorInfos = new List<SensorInfo>();
            SensorInfo sensorInfo = new SensorInfo() { Name = "New Sensor", NetWorkID = 1000, SensorID = 1, SensorStatus = SensorInfo.Status.Ok };
            SensorInfos.Add(sensorInfo);
            NodeList.Items.Refresh();
            PortListData = SerialPort.GetPortNames();
            sensorData = SensorInfos[0].SensorData;
            PlotControl.Scale = 5;
            DataContext = this;
            sciChartSurface.DataContext = PlotControl;
            sll.DataContext = PlotControl;
            lll.DataContext = PlotControl;
            Status.DataContext = sensorData;
        }

        public SerialPort serialPort;//串口对象类

        public bool InitCOM(string PortName)
        {
            serialPort = new SerialPort(PortName, 9600, Parity.None, 8, StopBits.One);
            serialPort.DataReceived += new SerialDataReceivedEventHandler(serialPort_DataReceived);//DataReceived事件委托
            serialPort.ReceivedBytesThreshold = 1;
            serialPort.RtsEnable = true;
            return OpenPort();//串口打开
        }

        /// 数据接收事件
        private void serialPort_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            // Thread.Sleep(2000);
            //serialPort.Read(readBuffer, 0, readBuffer.Length);
            string str = serialPort.ReadLine();
            Format.DataPackage dataPackage = Format.DataPackage.Decode(str);
            SensorInfo sensorInfo = SensorInfos.Find(x => x.NetWorkID == dataPackage.NetworkID || x.SensorID == dataPackage.SensorID);
            if (sensorInfo != null)
            {
                sensorInfo.SensorData.GetSensorData(dataPackage);
                using (sciChartSurface.SuspendUpdates())
                {
                    sensorInfo.SensorData.PressureLine.Append(DateTime.Now, (sensorInfo.SensorData.Pressure / 65536d + 0.095) / 0.009);
                    PlotControl.RefreshLimit(DateTime.Now);
                    LineSeries.DataSeries = sensorInfo.SensorData.PressureLine;
                }
            }
            //sensorData.GetSensorData(dataPackage);
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
    }
}