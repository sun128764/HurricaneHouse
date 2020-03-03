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
using System.IO.Ports;
using System.ComponentModel;

namespace GUI
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitCOM("COM5");
            SeriesCollection = new SeriesCollection
            {
                new LineSeries
                {
                    Title = "Pressure",
                    Values = new ChartValues<double>{ }
                }
            };
            YFormatter = value => value.ToString("C");

          

            DataContext = this;
        }
        public string Datanum { get; set; }

        public SeriesCollection SeriesCollection { get; set; }
        public string[] Labels { get; set; }
        public Func<double, string> YFormatter { get; set; }

        private void SelectionChanged(object sender, RoutedPropertyChangedEventArgs<Object> e)
        {
            //Perform actions when SelectedItem changes
            MessageBox.Show((e.NewValue).ToString());
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
        public delegate void HandleInterfaceUpdataDelegate(string text);
        private HandleInterfaceUpdataDelegate interfaceUpdataHandle;
        /// 数据接收事件
        private void serialPort_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            // Thread.Sleep(2000);
            byte[] readBuffer = new byte[serialPort.ReadBufferSize];
            //serialPort.Read(readBuffer, 0, readBuffer.Length);
            string str =serialPort.ReadLine(); 
            //string str = System.Text.Encoding.Default.GetString(readBuffer);
            double pre=0;
            if (str.Length > 0) pre = (int.Parse(str) / 65536d + 0.095) / 0.009;
            interfaceUpdataHandle = (x)=>
            {
                textBox.Text = x;
            };//实例化委托对象
            Dispatcher.Invoke(interfaceUpdataHandle, pre.ToString("F3")+"kPa");
            SeriesCollection[0].Values.Add(pre);

            //MessageBox.Show(str);
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
    }
}
