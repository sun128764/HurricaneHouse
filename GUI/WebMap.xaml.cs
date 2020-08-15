using Microsoft.Maps.MapControl.WPF;
using Newtonsoft.Json;
using System;
using System.IO;
using System.Net;
using System.Text;
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
                var res = HttpGet("https://api.ipgeolocation.io/ipgeo?apiKey=01d110a5e710445b91306d8d3345657e");
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
        public static string HttpGet(string url)
        {
            try
            {
                //ServicePointManager.ServerCertificateValidationCallback = new RemoteCertificateValidationCallback(CheckValidationResult);
                //Encoding encoding = Encoding.UTF8;
                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
                request.Method = "GET";
                request.Accept = "text/html, application/xhtml+xml, */*";
                request.ContentType = "application/json";

                HttpWebResponse response = (HttpWebResponse)request.GetResponse();
                using (StreamReader reader = new StreamReader(response.GetResponseStream(), Encoding.UTF8))
                {
                    return reader.ReadToEnd();
                }
            }
            catch (Exception)
            {
                return null;
            }
        }
        public static string HttpPost(string url, string body)
        {
            try
            {
                //ServicePointManager.ServerCertificateValidationCallback = new RemoteCertificateValidationCallback(CheckValidationResult);
                Encoding encoding = Encoding.UTF8;
                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
                request.Method = "POST";
                request.Accept = "text/html, application/xhtml+xml, */*";
                request.ContentType = "application/json";

                byte[] buffer = encoding.GetBytes(body);
                request.ContentLength = buffer.Length;
                request.GetRequestStream().Write(buffer, 0, buffer.Length);
                HttpWebResponse response = (HttpWebResponse)request.GetResponse();
                using (StreamReader reader = new StreamReader(response.GetResponseStream(), Encoding.UTF8))
                {
                    return reader.ReadToEnd();
                }
            }
            catch (Exception)
            {
                return null;
            }
        }
    }
}
