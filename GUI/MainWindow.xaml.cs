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


namespace GUI
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            _ = new SeriesCollection{
                new LineSeries
                {
                    Values = new ChartValues<double> { 3, 5, 7, 4 }
                    },
                new ColumnSeries
                {
                    Values = new ChartValues<decimal> { 5, 6, 2, 7 }
                }
            };
        }

        private void SelectionChanged(object sender, RoutedPropertyChangedEventArgs<Object> e)
        {
            //Perform actions when SelectedItem changes
            //MessageBox.Show(((TreeViewItem)e.NewValue).Header.ToString());
        }
    }
}
