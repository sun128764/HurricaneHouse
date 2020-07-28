using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;

namespace GUI
{
    /// <summary>
    /// URLReader.xaml 的交互逻辑
    /// </summary>
    public partial class URLReader : Window, INotifyPropertyChanged
    {
        //Example URL
        //https://www.designsafe-ci.org/data/browser/projects/2213334571396698601-242ac11a-0001-012//GUI_Test?query_string=
        //Example Cloud path
        //project-6284144844314644966-242ac11c-0001-012/GUI_Test/
        private Regex Regex = new Regex(@"^(https://)?www.designsafe-ci.org/data/browser/(projects.*?)\?query_string=\s?$");
        public event PropertyChangedEventHandler PropertyChanged;

        // This method is called by the Set accessor of each property.
        // The CallerMemberName attribute that is applied to the optional propertyName
        // parameter causes the property name of the caller to be substituted as an argument.
        protected void NotifyPropertyChanged([CallerMemberName] String propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private string _uRL;
        private string _cloudPath;
        public string URL
        {
            set
            {
                if (value != _uRL)
                {
                    _uRL = value;
                    NotifyPropertyChanged();
                }
            }
            get
            {
                return _uRL;
            }
        }
        public string CloudPath
        {
            set
            {
                if (value != _cloudPath)
                {
                    _cloudPath = value;
                    NotifyPropertyChanged();
                }
            }
            get
            {
                return _cloudPath;
            }
        }
        private bool isFinish;
        public URLReader()
        {
            InitializeComponent();
            DataContext = this;
            URL = "";
            CloudPath = "";
            isFinish = false;
        }

        private void URL_TextChanged(object sender, TextChangedEventArgs e)
        {
            TextBox textBox = sender as TextBox;
            if (Regex.IsMatch(textBox.Text))
            {
                CloudPath = Regex.Match(textBox.Text).Groups[2].Value.Replace("projects/", "projects-").Replace("//", "/") + "/";
            }
        }
        private void Finish_Click(object sender, RoutedEventArgs e)
        {
            isFinish = true;
            Close();
        }
        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            isFinish = true;
            Close();
        }
        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (!isFinish)
            {
                CloudPath = "";
            }
        }
    }
}
