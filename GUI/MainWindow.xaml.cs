using System;
using System.IO.Ports;
using System.Text;
using System.Windows;
using System.Windows.Controls;
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

        public MainWindow()
        {
            PlotControl = new Format.PlotControl();
            InitializeComponent();

            SensorInfo sensorInfo = new SensorInfo();
            sensorInfo.SetInfo("Sensor1", 5001, 1, SensorInfo.Types.regular, "nothing");
            sensorInfo.SensorStatus = SensorInfo.Status.Ok;
            NodeList.Items.Add(sensorInfo);

            LineSeries.DataSeries = new XyDataSeries<DateTime, double>();
            LineSeries.DataSeries.SeriesName = "Pressure";
            PortListData = SerialPort.GetPortNames();
            sensorData = new SensorData();
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
            sensorData.GetSensorData(str);
            using (sciChartSurface.SuspendUpdates())
            {
                sensorData.PressureLine.Append(DateTime.Now, (sensorData.Pressure / 65536d + 0.095) / 0.009);
                PlotControl.RefreshLimit(DateTime.Now);
                //if (sciChartSurface.ZoomState == ZoomStates.AtExtents)
                //{
                //    PlotControl.RefreshLimit(DateTime.Now);
                //    sciChartSurface.XAxis.VisibleRange = new SciChart.Data.Model.DateRange(PlotControl.Min, PlotControl.Max);
                //}
                LineSeries.DataSeries = sensorData.PressureLine;
            }
            //MessageBox.Show(sensorData.Pressure);
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
            string setting;
            setting = settingWindow.Setting;
            MessageBox.Show(setting);
        }
    }
}