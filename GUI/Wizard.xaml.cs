using Microsoft.Win32;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Ports;
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
using Xceed.Wpf.Toolkit;

namespace GUI
{
    /// <summary>
    /// Wizard.xaml 的交互逻辑
    /// </summary>
    public partial class Wizard : Window
    {
        public Format.ProgramSetting ProgramSetting { set; get; }
        public string[] PortList { set; get; }
        private bool isCreate;
        private string savePath;
        public Wizard()
        {
            InitializeComponent();
            ProgramSetting = new Format.ProgramSetting();
            DataContext = ProgramSetting;
            RefreshPort_Click(null, null);
            BaudRateBox.ItemsSource = new string[] { "9600" };
            DataBitsBox.ItemsSource = new string[] { "7", "8" };
            ParityBox.ItemsSource = new string[] { "None", "Even", "Mark", "Odd", "Space" };
            StopBitsBox.ItemsSource = new string[] { "1", "2" };
            WizardWindow.Finish += WizardWindow_Finish;
        }

        private void WizardWindow_Finish(object sender, Xceed.Wpf.Toolkit.Core.CancelRoutedEventArgs e)
        {
            if (isCreate)
            {
                string setting = JsonConvert.SerializeObject(ProgramSetting, Formatting.Indented);
                SaveFileDialog saveFileDialog = new SaveFileDialog
                {
                    Filter = "Setting files (*.json)|*.json",
                    AddExtension = true,
                    OverwritePrompt = true
                };
                if (saveFileDialog.ShowDialog() == true)
                {
                    File.WriteAllText(saveFileDialog.FileName, setting);
                }
            }
            throw new NotImplementedException();
        }

        private void Read_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog
            {
                Filter = "Setting files (*.json)|*.json"
            };
            if (openFileDialog.ShowDialog() == true)
            {
                var setting = File.ReadAllText(openFileDialog.FileName);
                ProgramSetting = JsonConvert.DeserializeObject<Format.ProgramSetting>(setting);
            }
            isCreate = false;
        }
        private void Create_Click(object sender, RoutedEventArgs e)
        {
            isCreate = true;
            WizardWindow.CurrentPage = CloudSetting;
        }
        private void RefreshPort_Click(object sender, RoutedEventArgs e)
        {
            PortList = SerialPort.GetPortNames();
            PortListBox.ItemsSource = PortList;
            PortListBox.SelectedIndex = 0;
        }
        private void BrowseLocalPath_Click(object sender, RoutedEventArgs e)
        {

        }
    }
}
