using System;
using System.Net;
using System.IO;
using System.Windows.Media;
using System.Windows;
using System.Windows.Controls;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using System.Threading.Tasks;
using Microsoft.Maps.MapControl.WPF;
using BingMapsRESTService.Common.JSON;
using System.Runtime.Serialization.Json;
using System.Windows.Media.Imaging;
using System.Windows.Input;

namespace BingMap
{
    /// <summary>
    /// Bing Map WPF Library V1.0 By-<< Lalaji Gajera >> 
    /// 
    /// </summary>
    public class MyMap
    {
        //core map object
        public Map myMap { get; set; }
        public string MapKey { get; set; }
        private Route route;




        /// <summary>
        /// initialize Map object
        /// </summary>
        /// <param name="UserMap"></param>
        public MyMap(Map UserMap, string key)
        {
            myMap = UserMap;
            MapKey = key;

        }


        // find location by ip
        public void GetMyLocation()
        {
            try
            {
                GetLocation();
            }
            catch (SystemException err)
            {
                string errMsg = "Unable to get your location: " + err.Message.ToString();
                MessageBox.Show(errMsg);
            }
        }

        private void GetLocation()
        {
            string _urlResponse = "";
            string _ipinfodb_apikey = "_ipinfodb_apikey";
            string _mypublicipaddress = GetPublicIpAddress();
            string _urlRequest = "http://api.ipinfodb.com/v3/ip-city/?key=" + _ipinfodb_apikey + "&" + _mypublicipaddress;

            var request = (HttpWebRequest)WebRequest.Create(_urlRequest);

            request.UserAgent = "curl"; // this simulate curl linux command

            request.Method = "GET";
            using (WebResponse response = request.GetResponse())
            {
                using (var reader = new StreamReader(response.GetResponseStream()))
                {
                    _urlResponse = reader.ReadToEnd();
                }
            }

            string[] _myGeocodeInfo = _urlResponse.Split(';');

            Location myLoc = new Location(Convert.ToDouble(_myGeocodeInfo[8]), Convert.ToDouble(_myGeocodeInfo[9]));
            myMap.SetView(myLoc, Convert.ToDouble(14), Convert.ToDouble(0));

            Pushpin myPin = new Pushpin();
            MapLayer.SetPosition(myPin, myLoc);
            myMap.Children.Add(myPin);

            System.Windows.Controls.Label label = new System.Windows.Controls.Label();
            label.Content = "Here I AM!";
            label.Foreground = new SolidColorBrush(Colors.DarkBlue);
            label.Background = new SolidColorBrush(Colors.WhiteSmoke);
            label.FontSize = 30;
            MapLayer.SetPosition(label, myLoc);
            myMap.Children.Add(label);
        }


        private string GetPublicIpAddress()
        {
            var request = (HttpWebRequest)WebRequest.Create("http://ifconfig.me");

            request.UserAgent = "curl"; //simulate the linux curl command: its "cleaner"

            string publicIPAddress;

            request.Method = "GET";
            using (WebResponse response = request.GetResponse())
            {
                using (var reader = new StreamReader(response.GetResponseStream()))
                {
                    publicIPAddress = reader.ReadToEnd();
                }
            }

            return publicIPAddress.Replace("\n", "");
        }

        public void AddLabel(Location Location, string labelString)
        {

            System.Windows.Controls.Label label = new System.Windows.Controls.Label();
            label.Content = labelString;
            label.Foreground = new SolidColorBrush(Colors.White);
            label.FontSize = 25;
            MapLayer.SetPosition(label, Location);
            myMap.Children.Add(label);
        }

        public void AddPushpin(Location Location)
        {

            Pushpin pushpin = new Pushpin();
            pushpin.Location = Location;
            myMap.Children.Add(pushpin);
        }

        public void AddPushpin(double latitude, double longitude)
        {
            Location location = new Location(latitude, longitude);
            Pushpin pushpin = new Pushpin();
            pushpin.Location = location;
            myMap.Children.Add(pushpin);
        }

        //pushpin with pushpinlabel
        public void AddPushpin(Location Location, string pinLabel)
        {
            Pushpin pushpin = new Pushpin();
            pushpin.Location = Location;
            pushpin.Content = pinLabel;
            myMap.Children.Add(pushpin);
        }

        //pushpin with label
        public void AddPushpinWithLabel(Location Location, string LabelString)
        {
            AddPushpin(Location);
            AddLabel(Location, LabelString);
        }

        // Geocode an address and return a latitude and longitude
        public XmlDocument Geocode(string addressQuery)
        {
            //Create REST Services geocode request using Locations API
            string geocodeRequest = "http://dev.virtualearth.net/REST/v1/Locations/" + addressQuery + "?o=xml&key=" + MapKey;


            //Make the request and get the response
            XmlDocument geocodeResponse = GetXmlResponse(geocodeRequest);

            return (geocodeResponse);
        }
        // Submit a REST Services or Spatial Data Services request and return the response
        private XmlDocument GetXmlResponse(string requestUrl)
        {
            System.Diagnostics.Trace.WriteLine("Request URL (XML): " + requestUrl);
            HttpWebRequest request = WebRequest.Create(requestUrl) as HttpWebRequest;
            using (HttpWebResponse response = request.GetResponse() as HttpWebResponse)
            {
                if (response.StatusCode != HttpStatusCode.OK)
                    throw new Exception(String.Format("Server error (HTTP {0}: {1}).",
                    response.StatusCode,
                    response.StatusDescription));
                XmlDocument xmlDoc = new XmlDocument();
                xmlDoc.Load(response.GetResponseStream());
                return xmlDoc;
            }
        }
        public void SearchByAddress(string Address)
        {
            if (Address != "")
            {
                XmlDocument searchResponse = Geocode(Address);

                //gettng resukts
                //Get location information from geocode response 

                //Create namespace manager
                XmlNamespaceManager nsmgr = new XmlNamespaceManager(searchResponse.NameTable);
                nsmgr.AddNamespace("rest", "http://schemas.microsoft.com/search/local/ws/rest/v1");

                //Get all geocode locations in the response 
                XmlNodeList locationElements = searchResponse.SelectNodes("//rest:Location", nsmgr);
                if (locationElements.Count == 0)
                {
                    MessageBox.Show("No Results found.");
                }
                else
                {
                    //Get the geocode location points that are used for display (UsageType=Display)
                    XmlNodeList displayGeocodePoints =
                            locationElements[0].SelectNodes(".//rest:GeocodePoint/rest:UsageType[.='Display']/parent::node()", nsmgr);

                    string latitude = displayGeocodePoints[0].SelectSingleNode(".//rest:Latitude", nsmgr).InnerText;
                    string longitude = displayGeocodePoints[0].SelectSingleNode(".//rest:Longitude", nsmgr).InnerText;
                    string label = locationElements[0].SelectSingleNode(".//rest:Name", nsmgr).InnerText;
                    Location myLoc1 = new Location(Convert.ToDouble(latitude), Convert.ToDouble(longitude));
                    myMap.Center = myLoc1;
                    AddLabel(myLoc1, label);
                }
            }
        }

        public async void GetDirection(string From, string To)
        {
            myMap.Children.Clear();

            Location Location = new Location();

            if (!string.IsNullOrWhiteSpace(From))
            {

                if (!string.IsNullOrWhiteSpace(To))
                {
                    Uri routeRequest = new Uri(string.Format("http://dev.virtualearth.net/REST/V1/Routes/Driving?wp.0={0}&wp.1={1}&rpo=Points&key={2}", From, To, MapKey));
                    //Create the Request URL for the routing service

                    Response r = await GetResponse(routeRequest);



                    if (r != null &&

                        r.ResourceSets != null &&

                        r.ResourceSets.Length > 0 &&

                        r.ResourceSets[0].Resources != null &&

                        r.ResourceSets[0].Resources.Length > 0)
                    {

                        route = r.ResourceSets[0].Resources[0] as Route;



                        //Get the route line data

                        double[][] routePath = route.RoutePath.Line.Coordinates;
                        RouteLeg[] routeLabel = route.RouteLegs;
                        LocationCollection locations = new LocationCollection();



                        for (int i = 0; i < routePath.Length; i++)
                        {

                            if (routePath[i].Length >= 2)
                            {

                                locations.Add(new Location(routePath[i][0],

                                              routePath[i][1]));


                            }

                        }
                        for (int i = 0; i < routeLabel[0].ItineraryItems.Length; i++)
                        {

                            AddPushpin(routeLabel[0].ItineraryItems[i].ManeuverPoint.Coordinates[0], routeLabel[0].ItineraryItems[i].ManeuverPoint.Coordinates[1]);
                            //dont delete below line
                            // AddLabel(routeLabel[0].ItineraryItems[i].ManeuverPoint.Coordinates[0], routeLabel[0].ItineraryItems[i].ManeuverPoint.Coordinates[1], routeLabel[0].ItineraryItems[i].Instruction.Text);
                        }

                        //Create a MapPolyline of the route and add it to the map
                        addNewPolyline(locations);

                        //Add start and end pushpins

                        Pushpin start = new Pushpin()

                        {
                            // Text = "S",

                            Background = new SolidColorBrush(Colors.Green)

                        };


                        start.Content = "S";
                        myMap.Children.Add(start);

                        MapLayer.SetPosition(start,

                            new Location(route.RouteLegs[0].ActualStart.Coordinates[0],

                                route.RouteLegs[0].ActualStart.Coordinates[1]));



                        Pushpin end = new Pushpin()

                        {
                            // Text = "E",

                            Background = new SolidColorBrush(Colors.Red)

                        };

                        end.Content = "E";

                        myMap.Children.Add(end);

                        MapLayer.SetPosition(end,

                            new Location(route.RouteLegs[0].ActualEnd.Coordinates[0],

                            route.RouteLegs[0].ActualEnd.Coordinates[1]));

                        //Set the map view for the locations

                        myMap.SetView(new LocationRect(locations));


                    }

                    else
                    {

                        MessageBox.Show("No Results found.");

                    }

                }

                else
                {

                    MessageBox.Show("Invalid 'To' location.");

                }

            }

            else
            {

                MessageBox.Show("Invalid 'From' location.");

            }

        }
        public async Task<Route> GetDirectionRoute(string From, string To)
        {
            myMap.Children.Clear();

            Location Location = new Location();

            if (!string.IsNullOrWhiteSpace(From))
            {

                if (!string.IsNullOrWhiteSpace(To))
                {
                    Uri routeRequest = new Uri(string.Format("http://dev.virtualearth.net/REST/V1/Routes/Driving?wp.0={0}&wp.1={1}&rpo=Points&key={2}", From, To, MapKey));
                    //Create the Request URL for the routing service

                    Response r = await GetResponse(routeRequest);



                    if (r != null &&

                        r.ResourceSets != null &&

                        r.ResourceSets.Length > 0 &&

                        r.ResourceSets[0].Resources != null &&

                        r.ResourceSets[0].Resources.Length > 0)
                    {

                        route = r.ResourceSets[0].Resources[0] as Route;



                        //Get the route line data

                        double[][] routePath = route.RoutePath.Line.Coordinates;
                        RouteLeg[] routeLabel = route.RouteLegs;
                        LocationCollection locations = new LocationCollection();



                        for (int i = 0; i < routePath.Length; i++)
                        {

                            if (routePath[i].Length >= 2)
                            {

                                locations.Add(new Location(routePath[i][0],

                                              routePath[i][1]));


                            }

                        }
                        for (int i = 0; i < routeLabel[0].ItineraryItems.Length; i++)
                        {

                            AddPushpin(routeLabel[0].ItineraryItems[i].ManeuverPoint.Coordinates[0], routeLabel[0].ItineraryItems[i].ManeuverPoint.Coordinates[1]);
                            //dont delete below line
                            // AddLabel(routeLabel[0].ItineraryItems[i].ManeuverPoint.Coordinates[0], routeLabel[0].ItineraryItems[i].ManeuverPoint.Coordinates[1], routeLabel[0].ItineraryItems[i].Instruction.Text);
                        }

                        //Create a MapPolyline of the route and add it to the map
                        addNewPolyline(locations);

                        //Add start and end pushpins

                        Pushpin start = new Pushpin()

                        {
                            // Text = "S",

                            Background = new SolidColorBrush(Colors.Green)

                        };


                        start.Content = "S";
                        myMap.Children.Add(start);

                        MapLayer.SetPosition(start,

                            new Location(route.RouteLegs[0].ActualStart.Coordinates[0],

                                route.RouteLegs[0].ActualStart.Coordinates[1]));



                        Pushpin end = new Pushpin()

                        {
                            // Text = "E",

                            Background = new SolidColorBrush(Colors.Red)

                        };

                        end.Content = "E";

                        myMap.Children.Add(end);

                        MapLayer.SetPosition(end,

                            new Location(route.RouteLegs[0].ActualEnd.Coordinates[0],

                            route.RouteLegs[0].ActualEnd.Coordinates[1]));

                        //Set the map view for the locations

                        myMap.SetView(new LocationRect(locations));


                    }

                    else
                    {

                        MessageBox.Show("No Results found.");

                    }

                }

                else
                {

                    MessageBox.Show("Invalid 'To' location.");

                }

            }

            else
            {

                MessageBox.Show("Invalid 'From' location.");

            }

            return route;
        }

        // route end
        private async Task<Response> GetResponse(Uri uri)
        {

            System.Net.Http.HttpClient client = new System.Net.Http.HttpClient();

            var response = await client.GetAsync(uri);



            using (var stream = await response.Content.ReadAsStreamAsync())
            {

                DataContractJsonSerializer ser = new DataContractJsonSerializer(typeof(Response));

                return ser.ReadObject(stream) as Response;

            }



        }

        public void addNewPolyline(LocationCollection l)
        {
            MapPolyline polyline = new MapPolyline();
            polyline.Stroke = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Colors.Blue);
            polyline.StrokeThickness = 5;
            polyline.Opacity = 0.7;
            polyline.Locations = l;

            myMap.Children.Add(polyline);
        }

        //add Image
        public void addImageToMap(String UriString, Location location)
        {
            MapLayer imageLayer = new MapLayer();


            Image image = new Image();
            image.Height = 150;
            //Define the URI location of the image
            BitmapImage myBitmapImage = new BitmapImage();
            myBitmapImage.BeginInit();
            myBitmapImage.UriSource = new Uri(@UriString, UriKind.RelativeOrAbsolute);
            // To save significant application memory, set the DecodePixelWidth or  
            // DecodePixelHeight of the BitmapImage value of the image source to the desired 
            // height or width of the rendered image. If you don't do this, the application will 
            // cache the image as though it were rendered as its normal size rather then just 
            // the size that is displayed.
            // Note: In order to preserve aspect ratio, set DecodePixelWidth
            // or DecodePixelHeight but not both.
            //Define the image display properties
            myBitmapImage.DecodePixelHeight = 150;
            myBitmapImage.EndInit();
            image.Source = myBitmapImage;
            image.Opacity = 0.6;
            image.Stretch = System.Windows.Media.Stretch.None;


            //Center the image around the location specified
            PositionOrigin position = PositionOrigin.Center;

            //Add the image to the defined map layer
            imageLayer.AddChild(image, location, position);
            //Add the image layer to the map
            myMap.Children.Add(imageLayer);
        }



    }



}
