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
using System.Windows.Shapes;
using Microsoft.Win32;
using System.IO;

namespace GUI
{
    /// <summary>
    /// SettingMaker.xaml 的交互逻辑
    /// </summary>
    public partial class SettingMaker : Window
    {
        public string Setting;
        public List<SensorInfo> SensorInfos { set; get; }
        public SettingMaker()
        {
            InitializeComponent();
            DataContext = this;
            SensorInfos = new List<SensorInfo>();
            SensorInfo sensorInfo = new SensorInfo();
            sensorInfo.SetInfo("Sensor1", 5001, 1, SensorInfo.Types.regular, "nothing");
            SensorInfos.Add(sensorInfo);
        }

        private void ExitBtn_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void CancelBtn_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void OpenBtn_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            if (openFileDialog.ShowDialog() == true)
            {
                Setting = File.ReadAllText(openFileDialog.FileName);
            }
        }

        private void SaveBtn_Click(object sender, RoutedEventArgs e)
        {

        }

        private void AddBtn_Click(object sender, RoutedEventArgs e)
        {

        }

        private void DelBtn_Click(object sender, RoutedEventArgs e)
        {

        }

        private void FinishBtn_Click(object sender, RoutedEventArgs e)
        {

        }
    }
}
