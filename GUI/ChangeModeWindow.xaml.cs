using System.Windows;

namespace GUI
{
    /// <summary>
    /// ChangeModeWindow.xaml 的交互逻辑
    /// </summary>
    public partial class ChangeModeWindow : Window
    {
        public ChangeModeWindow()
        {
            InitializeComponent();
        }

        private void Confirm_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            ModeName.Text = "";
            Close();
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            //ModeName.Text = "";
        }
    }
}