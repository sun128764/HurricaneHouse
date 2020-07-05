using Newtonsoft.Json;
using System.IO;
using System.IO.Ports;
using System.Windows;
using System.Windows.Data;
using System.Windows.Forms;
using System.Windows.Controls;
using System.Text.RegularExpressions;
using System.Windows.Input;
using Xceed.Wpf.AvalonDock.Controls;
using System.Collections.Generic;
using Application = System.Windows.Application;
using Cursors = System.Windows.Input.Cursors;

namespace GUI
{
    /// <summary>
    /// Wizard.xaml 的交互逻辑
    /// </summary>
    public partial class Wizard : Window
    {
        public Format.ProgramSetting ProgramSetting { set; get; }
        private string oldProgramSettingString;
        public string[] PortList { set; get; }
        private bool isCreate;
        public bool isFinished;
        public Wizard()
        {
            InitializeComponent();
            ProgramSetting = new Format.ProgramSetting() { UploadSpan = "5", TokenRefreshSpan = "1000" };
            DataContext = ProgramSetting;
            RefreshPort_Click(null, null);
            BaudRateBox.ItemsSource = new string[] { "9600" };
            DataBitsBox.ItemsSource = new string[] { "7", "8" };
            ParityBox.ItemsSource = new string[] { "None", "Even", "Mark", "Odd", "Space" };
            StopBitsBox.ItemsSource = new string[] { "1", "2" };
            WizardWindow.Finish += WizardWindow_Finish;
            isFinished = false;
        }

        private void WizardWindow_Finish(object sender, Xceed.Wpf.Toolkit.Core.CancelRoutedEventArgs e)
        {
            string setting = JsonConvert.SerializeObject(ProgramSetting, Formatting.Indented);
            if (!isCreate && (setting != oldProgramSettingString))
            {
                MessageBoxResult boxResult = System.Windows.MessageBox.Show("Setting has been changed. Do you want to save this setting?", "Setting changed", MessageBoxButton.YesNoCancel);
                switch (boxResult)
                {
                    case MessageBoxResult.Yes:
                        isCreate = true;
                        break;
                    case MessageBoxResult.No:
                        isCreate = false;
                        break;
                    case MessageBoxResult.Cancel:
                        return;
                    default:
                        return;
                }
            }
            if (isCreate)
            {
                SaveFileDialog saveFileDialog = new SaveFileDialog
                {
                    Filter = "Setting files (*.json)|*.json",
                    AddExtension = true,
                    OverwritePrompt = true
                };
                if (saveFileDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                    File.WriteAllText(saveFileDialog.FileName, setting);
                }
            }
            isFinished = true;
        }

        private void Read_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog
            {
                Filter = "Setting files (*.json)|*.json"
            };
            if (openFileDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                oldProgramSettingString = File.ReadAllText(openFileDialog.FileName);
                try
                {
                    ProgramSetting = JsonConvert.DeserializeObject<Format.ProgramSetting>(oldProgramSettingString);
                }
                catch (JsonSerializationException)
                {
                    System.Windows.MessageBox.Show("Can not read setting file. Please choose correct file.");
                    return;
                }
            }
            isCreate = false;
            DataContext = ProgramSetting;
            WizardWindow.CurrentPage = CloudSetting;
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
            FolderBrowserDialog folderBrowserDialog = new FolderBrowserDialog
            {
                ShowNewFolderButton = true
            };
            if (folderBrowserDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                ProgramSetting.LocalPath = folderBrowserDialog.SelectedPath;
                LocalPath.GetBindingExpression(System.Windows.Controls.TextBox.TextProperty).UpdateTarget();
            }
        }
        private void BrowseCloudPath_Click(object sender, RoutedEventArgs e)
        {
            var uRLReader = new URLReader();
            uRLReader.ShowDialog();
            if (uRLReader.CloudPath.Length > 0)
            {
                pathBox.Text = uRLReader.CloudPath;
            }
        }
        private void NumberValidationTextBox(object sender, TextCompositionEventArgs e)
        {
            Regex regex = new Regex("[^0-9]+");
            e.Handled = regex.IsMatch(e.Text);
        }
        private void CheckTapis(object sender, RoutedEventArgs e)
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                Mouse.OverrideCursor = Cursors.Wait;
            });
            DataLoger dataLoger = new DataLoger();
            dataLoger.Init(ProgramSetting);
            UpChk.Text = dataLoger.TryUpload();
            Application.Current.Dispatcher.Invoke(() =>
            {
                Mouse.OverrideCursor = null;
            });
        }
        private void SensorSettingBrowse_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog
            {
                Filter = "Setting files (*.json)|*.json"
            };
            if (openFileDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                string SensorInfoString = File.ReadAllText(openFileDialog.FileName);
                try
                {
                    List<SensorInfo> sensorInfos = JsonConvert.DeserializeObject<List<SensorInfo>>(SensorInfoString);
                }
                catch (JsonSerializationException)
                {
                    System.Windows.MessageBox.Show("Can not read setting file. Please choose correct file.");
                    return;
                }
                ProgramSetting.SensorConfPath = openFileDialog.FileName;
                ConfPath.GetBindingExpression(System.Windows.Controls.TextBox.TextProperty).UpdateTarget();
            }
        }
    }
}
