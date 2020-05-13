﻿using System;
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
using Newtonsoft.Json;

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
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "Setting files (*.json)|*.json";
            if (openFileDialog.ShowDialog() == true)
            {
                Setting = File.ReadAllText(openFileDialog.FileName);
                SensorInfos.Clear();
                SensorInfos.AddRange(JsonConvert.DeserializeObject<List<SensorInfo>>(Setting));
                SensorList.Items.Refresh();
            }
        }

        private void SaveBtn_Click(object sender, RoutedEventArgs e)
        {
            string output = JsonConvert.SerializeObject(SensorInfos, Formatting.Indented);
            SaveFileDialog saveFileDialog = new SaveFileDialog();
            saveFileDialog.Filter = "Setting files (*.json)|*.json";
            saveFileDialog.AddExtension = true;
            saveFileDialog.OverwritePrompt = true;
            if (saveFileDialog.ShowDialog() == true)
            {
                File.WriteAllText(saveFileDialog.FileName, output);
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

        private void FinishBtn_Click(object sender, RoutedEventArgs e)
        {

        }

        private void Metadata_TextChanged(object sender, TextChangedEventArgs e)
        {

        }
    }
}