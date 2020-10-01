using Microsoft.Win32;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.IO;
using System.Windows;

namespace MainProgram
{
    /// <summary>
    /// SettingMaker.xaml 的交互逻辑
    /// </summary>
    public partial class SettingMaker : Window
    {
        public string Setting;

        public List<SensorInfo> SensorInfos { set; get; }
        public string FilePath;

        public SettingMaker()
        {
            InitializeComponent();
            DataContext = this;
            SensorInfos = new List<SensorInfo>();
            SensorInfo sensorInfo = new SensorInfo() { Name = "New Sensor" };
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
            OpenFileDialog openFileDialog = new OpenFileDialog
            {
                Filter = "Setting files (*.json)|*.json"
            };
            if (openFileDialog.ShowDialog() == true)
            {
                Setting = File.ReadAllText(openFileDialog.FileName);
                SensorInfos.Clear();
                SensorInfos.AddRange(JsonConvert.DeserializeObject<List<SensorInfo>>(Setting));
                SensorList.Items.Refresh();
                FilePath = openFileDialog.FileName;
            }
        }

        private void SaveBtn_Click(object sender, RoutedEventArgs e)
        {
            string output = JsonConvert.SerializeObject(SensorInfos, Formatting.Indented);
            SaveFileDialog saveFileDialog = new SaveFileDialog
            {
                Filter = "Setting files (*.json)|*.json",
                AddExtension = true,
                OverwritePrompt = true
            };
            if (saveFileDialog.ShowDialog() == true)
            {
                File.WriteAllText(saveFileDialog.FileName, output);
                FilePath = saveFileDialog.FileName;
            }
        }

        private void AddBtn_Click(object sender, RoutedEventArgs e)
        {
            SensorInfo sensorInfo = new SensorInfo() { Name = "New Sensor" };
            SensorInfos.Add(sensorInfo);
            SensorList.Items.Refresh();
        }

        private void DelBtn_Click(object sender, RoutedEventArgs e)
        {
            SensorInfos.Remove(SensorList.SelectedItem as SensorInfo);
            SensorList.Items.Refresh();
        }
    }
}