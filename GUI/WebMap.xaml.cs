using Microsoft.Maps.MapControl.WPF;
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

namespace GUI
{
    /// <summary>
    /// WebMap.xaml 的交互逻辑
    /// </summary>
    public partial class WebMap : Window
    {
        public WebMap()
        {
            InitializeComponent();
        }

        private void myMap_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            // Disables the default mouse double-click action.
            e.Handled = true;

            //Get the mouse click coordinates
            Point mousePosition = e.GetPosition(this);

            //Convert the mouse coordinates to a locatoin on the map
            Location pinLocation = myMap.ViewportPointToLocation(mousePosition);

            // The pushpin to add to the map.
            Pushpin pin = new Pushpin();
            pin.Location = pinLocation;

            // Adds the pushpin to the map.
            LocationStr.Text = pinLocation.ToString();
            myMap.Children.Clear();
            myMap.Children.Add(pin);
        }
        private void Toggle_Map(object sender, RoutedEventArgs e)
        {
            if (myMap.Mode.ToString() == "Microsoft.Maps.MapControl.WPF.RoadMode")
            {
                //Set the map mode to Aerial with labels
                myMap.Mode = new AerialMode(true);
            }
            if (myMap.Mode.ToString() == "Microsoft.Maps.MapControl.WPF.AerialMode")
            {
                //Set the map mode to RoadMode
                myMap.Mode = new RoadMode();
            }
        }
        private void Finish_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
        private void Cancle_Click(object sender, RoutedEventArgs e)
        {
            LocationStr.Text = "";
            Close();
        }
    }
}
