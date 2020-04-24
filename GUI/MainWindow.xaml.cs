using System;
using System.IO.Ports;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using SciChart.Charting.Model.DataSeries;
using SciChart.Charting.Visuals;
using SciChart.Charting.ChartModifiers;
using System.Threading;


namespace GUI
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {
        public SensorData sensorData { get; set; }

        public string[] PortListData { get; set; }
        public Format.PlotControl PlotControl { get; set; }

        public MainWindow()
        {
            PlotControl = new Format.PlotControl();
            InitializeComponent();

            // Create the chart surface
            //sciChartSurface = new SciChartSurface();

            // Create the X and Y Axis
            //var xAxis = new NumericAxis() { AxisTitle = "Time" };
            //var yAxis = new NumericAxis() { AxisTitle = "Value" };

            //sciChartSurface.XAxis = xAxis;
            //sciChartSurface.YAxis = yAxis;
            // Instantiate the ViewportManager here
            //double windowSize = 1000.0;
            LineSeries.DataSeries = new XyDataSeries<DateTime, double>();
            LineSeries.DataSeries.SeriesName = "Pressure";
            // Specify Interactivity Modifiers
            //sciChartSurface.ChartModifier = new ModifierGroup(new RubberBandXyZoomModifier(), new ZoomExtentsModifier());
            PortListData = SerialPort.GetPortNames();
            sensorData = new SensorData();
            //Values = new ChartValues<double> { };
            //PlotControl = new Format.PlotControl();
            PlotControl.Scale = 50;
            DataContext = this;
            sciChartSurface.DataContext = PlotControl;
            sll.DataContext = PlotControl;
            lll.DataContext = PlotControl;
            Status.DataContext = sensorData;

            //InitCOM("COM3");
        }

        private void SelectionChanged(object sender, RoutedPropertyChangedEventArgs<Object> e)
        {
            //Perform actions when SelectedItem changes
            //MessageBox.Show(sciChartSurface.ViewportManager.);

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
                sensorData.Pressure1mLine.Append(DateTime.Now, (sensorData.Pressure / 65536d + 0.095) / 0.009);
                PlotControl.RefreshLimit(DateTime.Now);
                //if (sciChartSurface.ZoomState == ZoomStates.AtExtents)
                //{
                //    PlotControl.RefreshLimit(DateTime.Now);
                //    sciChartSurface.XAxis.VisibleRange = new SciChart.Data.Model.DateRange(PlotControl.Min, PlotControl.Max);
                //}
                LineSeries.DataSeries = sensorData.Pressure1mLine;
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
        }

        private void Label_MouseDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            //System.Diagnostics.Process.Start("Explorer.exe", @"/select,C:\mylog.log");
        }
    }
}