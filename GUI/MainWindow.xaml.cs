using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using LiveCharts;
using LiveCharts.Wpf;
using LiveCharts.Geared;
using System.IO.Ports;
using System.ComponentModel;

namespace GUI
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {
        public SensorData sensorData { get; set; }
        public GearedValues<double> Values { get; set; }

        private int databu;
        private int datacoun;
        public MainWindow()
        {
            InitializeComponent();
            sensorData = new SensorData();
            Values = new GearedValues<double> { };
            databu = 0;
            datacoun = 0;
            DataContext = this;
            Status.DataContext = sensorData;
            InitCOM("COM5");
        }

        private void SelectionChanged(object sender, RoutedPropertyChangedEventArgs<Object> e)
        {
            //Perform actions when SelectedItem changes
            MessageBox.Show(sensorData.Pressure);
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
            if (datacoun < 10)
            {
                databu += sensorData.PressureValue;
                datacoun++;
            }
            else
            {
                Values.Add(((double)databu / datacoun / 65536d + 0.095) / 0.009);
                databu = 0;
                datacoun = 0;
            }
            //MessageBox.Show(sensorData.Pressure);
        }
        //

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

        private void button_Click(object sender, RoutedEventArgs e)
        {

        }
    }
}
