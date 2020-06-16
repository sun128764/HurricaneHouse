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

namespace GUI
{
    /// <summary>
    /// Wizard.xaml 的交互逻辑
    /// </summary>
    public partial class Wizard : Window
    {
        public Format.ProgramSetting ProgramSetting { set; get; }
        private string[] portList { set; get; }
        private bool isCreate;
        private string savePath;
        public Wizard()
        {
            InitializeComponent();
            ProgramSetting = new Format.ProgramSetting();
            DataContext = ProgramSetting;
            RefreshPort_Click(null, null);
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
            SaveFileDialog saveFileDialog = new SaveFileDialog
            {
                Filter = "Setting files (*.json)|*.json",
                AddExtension = true,
                OverwritePrompt = true
            };
            if (saveFileDialog.ShowDialog() == true)
            {
                savePath = saveFileDialog.FileName;
            }
            isCreate = true;
        }
        private void RefreshPort_Click(object sender, RoutedEventArgs e)
        {
            portList = SerialPort.GetPortNames();
        }
    }
}
