using Microsoft.Maps.MapControl.WPF;
using Newtonsoft.Json;
using System.Threading;
using System.Windows;
using System.Windows.Input;

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
            ThreadPool.QueueUserWorkItem((object state) =>
            {
                //var res = HttpGet("http://api.ipstack.com/check?access_key=c4358b1d5570b8a0fdc733e18c1045c6");
                //Format.IpStackApi location = JsonConvert.DeserializeObject<Format.IpStackApi>(res);
                var res = WebAPIUtil.HttpGet("https://api.ipgeolocation.io/ipgeo?apiKey=01d110a5e710445b91306d8d3345657e");
                if (res == null) return; //Stop if GET method catched an excption
                Format.IpgeoLocationApi location = JsonConvert.DeserializeObject<Format.IpgeoLocationApi>(res);
                Application.Current.Dispatcher.Invoke(() => //Use invoke to refresh UI elements
                {
                    if (myMap.ZoomLevel < 7)
                    {
                        myMap.Center = new Location(location.latitude, location.longitude);
                        myMap.ZoomLevel = 13;
                    }
                });
            }, null);
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
            if (myMap.Mode.ToString() == "Microsoft.Maps.MapControl.WPF.AerialMode")
            {
                //Set the map mode to RoadMode
                myMap.Mode = new RoadMode();
            }
            else
            {
                myMap.Mode = new AerialMode(true);
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