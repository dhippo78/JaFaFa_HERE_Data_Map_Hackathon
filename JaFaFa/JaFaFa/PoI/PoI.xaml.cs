using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;

using System.Collections.ObjectModel;
using System.Xml.Linq;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using Xamarin.Essentials;
using System.Reflection;
using System.IO;
using System.Xml;
using Xamarin.Forms.Shapes;
using System.Threading.Tasks;

namespace JaFaFa.PoI
{
    // Learn more about JáFáFá by visiting https://jafafa.modecomint.com
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class PoI : ContentPage
    {
        public PoI()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Event handler for selecting a location in the list.
        /// </summary>
        /// 
        private void PoISelected(object sender, SelectedItemChangedEventArgs e)
        {            
            object selectedObject = PoILLS.SelectedItem;
            
            int selectedIndex = e.SelectedItemIndex;
        }
        void OnItemTapped(object sender, ItemTappedEventArgs e)
        {
            if (e == null) return; // has been set to null, do not 'process' tapped event

            int selectedIndex = e.ItemIndex;
                                    
            DisplayAlert("JaFaFa",
                string.Format("{0} {1} {2}\nName: {3}\nAddress: {4}\nLatitude: {5}\nLongitude: {6}",
                "Item", selectedIndex.ToString(), "Tapped",
                PoIListData[selectedIndex].PoIName, PoIListData[selectedIndex].PoIAddress,
                PoIListData[selectedIndex].PoILat, PoIListData[selectedIndex].PoILon), "Ok");

            ((ListView)sender).SelectedItem = null; // de-select the row
        }
        private void ContentPage_Appearing(object sender, EventArgs e)
        {
            base.OnAppearing();

            //#region How to load an XML file embedded resource
            string resPrefix = "JaFaFa.PoI.";
            var assembly = IntrospectionExtensions.GetTypeInfo(typeof(PoI)).Assembly;
            Stream stream = assembly.GetManifestResourceStream(resPrefix + "PoIDB.xml");

            var load = LoadPoIDatabaseMainPage(stream);
        }        
        public ObservableCollection<PoILongListData> LoadPoIDatabaseMainPage(Stream stream)
        {
            try
            {
                XDocument document = XDocument.Load(stream);

                var Query = from a in document.Descendants("POI").Elements("Amusement_Center").Elements("pt")
                            select a;
                var Query1 = from a in document.Descendants("POI").Elements("Bank").Elements("pt")
                             select a;
                var Query2 = from a in document.Descendants("POI").Elements("ATM").Elements("pt")
                             select a;
                var Query3 = from a in document.Descendants("POI").Elements("Airport").Elements("pt")
                             select a;
                var Query4 = from a in document.Descendants("POI").Elements("Building").Elements("pt")
                             select a;
                var Query5 = from a in document.Descendants("POI").Elements("Cemetery").Elements("pt")
                             select a;
                var Query6 = from a in document.Descendants("POI").Elements("Church").Elements("pt")
                             select a;
                var Query7 = from a in document.Descendants("POI").Elements("Mosque").Elements("pt")
                             select a;
                var Query8 = from a in document.Descendants("POI").Elements("Place_of_Worship").Elements("pt")
                             select a;
                var Query9 = from a in document.Descendants("POI").Elements("Golf_Course").Elements("pt")
                             select a;
                var Query10 = from a in document.Descendants("POI").Elements("Hospital").Elements("pt")
                             select a;
                var Query11 = from a in document.Descendants("POI").Elements("Landmark").Elements("pt")
                             select a;
                var Query12 = from a in document.Descendants("POI").Elements("Laundry_Services").Elements("pt")
                             select a;
                var Query13 = from a in document.Descendants("POI").Elements("Library").Elements("pt")
                             select a;
                var Query14 = from a in document.Descendants("POI").Elements("Military_Installation").Elements("pt")
                              select a;
                var Query15 = from a in document.Descendants("POI").Elements("Post_Office").Elements("pt")
                              select a;
                var Query16 = from a in document.Descendants("POI").Elements("Educational_Institution").Elements("pt")
                              select a;
                var Query17 = from a in document.Descendants("POI").Elements("Stadium").Elements("pt")
                              select a;

                int queriescount = 18;

                for (int i = 0; i < queriescount; i++)
                {
                    switch (i)
                    {
                        case 0:
                            POIQuery(Query);
                            break;
                        case 1:
                            POIQuery(Query1);
                            break;
                        case 2:
                            POIQuery(Query2);
                            break;
                        case 3:
                            POIQuery(Query3);
                            break;
                        case 4:
                            POIQuery(Query4);
                            break;
                        case 5:
                            POIQuery(Query5);
                            break;
                        case 6:
                            POIQuery(Query6);
                            break;
                        case 7:
                            POIQuery(Query7);
                            break;
                        case 8:
                            POIQuery(Query8);
                            break;
                        case 9:
                            POIQuery(Query9);
                            break;
                        case 10:
                            POIQuery(Query10);
                            break;
                        case 11:
                            POIQuery(Query11);
                            break;
                        case 12:
                            POIQuery(Query12);
                            break;
                        case 13:
                            POIQuery(Query13);
                            break;
                        case 14:
                            POIQuery(Query14);
                            break;
                        case 15:
                            POIQuery(Query15);
                            break;
                        case 16:
                            POIQuery(Query16);
                            break;
                        case 17:
                            POIQuery(Query17);
                            break;
                    }
                }

                DisplayAlert("Point of Interest Record Count", PoIListData.Count.ToString(), "Ok");
            }
            catch (Exception ex)
            {
                // For debugging                
                DisplayAlert("JaFaFa", ex.ToString(), "Ok");
            }

            PoILLS.ItemsSource = PoIListData;

            return PoIListData;
        }
        public ObservableCollection<PoILongListData> LoadPoIDatabaseGas(Stream stream)
        {
            try
            {
                XDocument document = XDocument.Load(stream);

                var Query = from a in document.Descendants("POI").Elements("Gas_Station_Rated").Elements("pt")
                            select a;
                
                POIQueryGas(Query);

                //DisplayAlert("Point of Interest Record Count", PoIListData.Count.ToString(), "Ok");
            }
            catch (Exception ex)
            {
                // For debugging                
                DisplayAlert("JaFaFa", ex.ToString(), "Ok");
            }

            //PoILLS.ItemsSource = PoIListData;

            return PoIListData;
        }
        private void POIQuery(IEnumerable<XElement> Query)
        {
            String Name;
            String Layer;
            String Street;
            String City;
            String Latitude;
            String Longitude;
            String Address;

            foreach (XElement poi in Query)
            {
                Name = poi.Attribute("name").Value;
                Layer = poi.Attribute("layer").Value;
                Street = poi.Attribute("streetdesc").Value;
                City = poi.Attribute("cityname").Value;
                Latitude = poi.Attribute("y").Value;
                Longitude = poi.Attribute("x").Value;
                Address = Street + "," + City;

                PoIListData.Add(new PoILongListData() { PoIName = Name, PoILayer = Layer, PoIAddress = Address, PoIStreet = Street,
                    PoICity = City, PoIImg = "checkered_flag_icon.png", PoILat = Latitude, PoILon = Longitude });
            }
        }
        private void POIQueryGas(IEnumerable<XElement> Query)
        {
            String Name; String Layer; String Street; String City; String Gas_Type; String Temperature; String Density;
            String Volume; String Fuel_Quality_Rating; String Fuel_Quantity_Rating; String Confidence_Factor_One;
            String Confidence_Factor_Two; String Datestamp; String Timestamp; String Latitude; String Longitude; String Address;

            foreach (XElement poi in Query)
            {
                Name = poi.Attribute("name").Value; Layer = poi.Attribute("layer").Value; Street = poi.Attribute("streetdesc").Value;
                City = poi.Attribute("cityname").Value; Gas_Type = poi.Attribute("gas_type").Value; Temperature = poi.Attribute("temperature").Value;
                Density = poi.Attribute("density").Value; Volume = poi.Attribute("volume").Value; Fuel_Quality_Rating = poi.Attribute("fuel_quality_rating").Value;
                Fuel_Quantity_Rating = poi.Attribute("fuel_quantity_rating").Value; Confidence_Factor_One = poi.Attribute("confidence_factor_one").Value;
                Confidence_Factor_Two = poi.Attribute("confidence_factor_two").Value; Datestamp = poi.Attribute("datestamp").Value;
                Timestamp = poi.Attribute("timestamp").Value; Latitude = poi.Attribute("y").Value; Longitude = poi.Attribute("x").Value;
                Address = Name + "," + Street;

                PoIListData.Add(new PoILongListData()
                {
                    PoIName = Name, PoILayer = Layer, PoIAddress = Address, PoIStreet = Street, PoICity = City,
                    PoIImg = "fillingstation.png", PoILat = Latitude, PoILon = Longitude, PoIGasType = Gas_Type,
                    PoITemperature = Temperature, PoIDensity = Density, PoIVolume = Volume, PoIFuelQualityRating = Fuel_Quality_Rating,
                    PoIFuelQuantityRating = Fuel_Quantity_Rating, PoIConfidenceFactorOne = Confidence_Factor_One,
                    PoIConfidenceFactorTwo = Confidence_Factor_Two, PoIDatestamp = Datestamp, PoITimestamp = Timestamp
                });
            }
        }
        //public Tuple<string, double, double> LoadStreamPosition(XDocument document)
        public List<GeocodeData> LoadStreamPosition(XDocument document)
        {
            var Label = "";  var Latitude = ""; var Longitude = "";

            var Query = from b in document.Descendants("address")
                        select b;

            var Query2 = from b in document.Descendants("position")
                         select b;

            var i = 0;

            foreach (XElement address in Query)
            {
                if (address.Elements("label").Count() > 0)
                {
                    Label = address.Element("label").Value;
                    
                    geocodeResultList.Add(new GeocodeData()
                    {
                        LocName = Label
                    });
                }
            }            
            foreach (XElement position in Query2)
            {
                if (position.Elements("lat").Count() > 0)
                {
                    Latitude = position.Element("lat").Value;
                    Longitude = position.Element("lng").Value;

                    geocodeResultList[i].LocLat = Latitude;
                    geocodeResultList[i].LocLon = Longitude;

                    if (i < position.Elements("lat").Count())
                        i++;
                }                
            }           

            return geocodeResultList;

            //return new Tuple<string, double, double>(Label, Convert.ToDouble(Latitude), Convert.ToDouble(Longitude));            
        }
        //public string LoadStreamMainPage(string stream)
        public string LoadStreamMainPage(XDocument document)
        {
            //XDocument document = new XDocument();

            //document = XDocument.Parse(stream);            

            //var Query = from b in document.Descendants("Address")
            //            select b;

            var Query = from b in document.Descendants("address")
                        select b;

            //XmlNodeList Query = document.GetElementsByTagName("address");

            var Country = ""; var State = ""; var County = ""; var City = ""; var District = "";
            var Label = ""; var PostalCode = ""; var HouseNumber = "";            

            string CivicAddress = "";            

            foreach (XElement address in Query)
            {
                //if (address.Elements("HouseNumber").Count() > 0)
                if (address.Elements("houseNumber").Count() > 0)
                {
                    //Label = address.Element("HouseNumber").Value;
                    HouseNumber = address.Element("houseNumber").Value;
                    //CivicAddress += "\n" + HouseNumber;
                }
                //if (address.Elements("Label").Count() > 0)
                if (address.Elements("label").Count() > 0)
                {
                    //Label = address.Element("Label").Value;
                    Label = address.Element("label").Value;
                    //CivicAddress += "\n" + Label;
                }
                //if (address.Elements("City").Count() > 0)
                if (address.Elements("city").Count() > 0)
                {
                    //City = address.Element("City").Value;
                    City = address.Element("city").Value;
                    //CivicAddress += "\n" + City;
                }
                //if (address.Elements("County").Count() > 0)
                if (address.Elements("county").Count() > 0)
                {
                    //County = address.Element("County").Value;
                    County = address.Element("county").Value;
                    //CivicAddress += "\n" + County;
                }
                //if (address.Elements("District").Count() > 0)
                if (address.Elements("district").Count() > 0)
                {
                    //District = address.Element("District").Value;
                    District = address.Element("district").Value;
                    //CivicAddress += "\n" + District;
                }
                //if (address.Elements("PostalCode").Count() > 0)
                if (address.Elements("postalCode").Count() > 0)
                {
                    //PostalCode = address.Element("PostalCode").Value;
                    PostalCode = address.Element("postalCode").Value;
                    //CivicAddress += "\n" + PostalCode;
                }
                //if (address.Elements("State").Count() > 0)
                if (address.Elements("state").Count() > 0)
                {
                    State = address.Element("state").Value;
                    //CivicAddress += "\n" + State;
                }
                //if (address.Elements("Country").Count() > 0)
                if (address.Elements("country").Count() > 0)
                {
                    //Country = address.Element("Country").Value;
                    Country = address.Element("country").Value;
                    //CivicAddress += "\n" + Country;
                }
            }
            //                CivicAddress += "\n" + HouseNumber + "" + Street + "\n" + City + "\n" + County + "\n" + District +
            //                    "\n" + PostalCode + "\n" + State + "\n" + Country;

            if (Label != "")
                CivicAddress += Label;
            if (HouseNumber != "")
                CivicAddress += HouseNumber;
            if (County != "")
                CivicAddress += "\n" + County;
            if (City != "")
                CivicAddress += "\n" + City;
            if (District != "")
                CivicAddress += "\n" + District;
            if (PostalCode != "")
                CivicAddress += "\n" + PostalCode;
            if (State != "")
                CivicAddress += "\n" + State;
            if (Country != "")
                CivicAddress += "\n" + Country;

            return CivicAddress;
        }        
        public List<RouteShapeList> LoadRouteShapeHERERv7(string HERE_XML)
        {
            XDocument document = new XDocument();
            document = XDocument.Parse(HERE_XML);

            var Query1 = from a in document.Descendants("Route")
                         select a;
            string Content1 = Query1.First().Element("Shape").Value;

            var RouteLatLngList = Content1.Split(' ').ToList<string>();
            
            for (var i = 0; i < RouteLatLngList.Count; i++)
            {
                var location = Convert_String_To_Coordinates(RouteLatLngList[i]);
                routeShapeList.Add(new RouteShapeList() { LocLat = location.Latitude, LocLon = location.Longitude });
            }

            return routeShapeList;
        }
        public Location Convert_String_To_Coordinates(string RouteLatLngList)
        {
            var STR = RouteLatLngList.Split(',').ToList<string>();

            Location location = null;

            double lat = 0; double lon;

            if (double.TryParse(STR[0], out lat))
            {
                if (double.TryParse(STR[1], out lon))
                {
                    location = new Location(lat, lon);
                }
            }

            return location;
        }
        public List<RouteShapeData> LoadRouteDataHERERv7(string HERE_XML)
        {
            XDocument document = new XDocument();
            document = XDocument.Parse(HERE_XML);

            var Query2 = from a in document.Descendants("Summary")
                         select a;

            var Query3 = from c in document.Descendants("TopLeft")
                         select c;            

            var Query4 = from d in document.Descendants("BottomRight")
                         select d;

            routeShapeData.Add(new RouteShapeData() {
                Distance = Convert.ToDouble(Query2.First().Element("Distance").Value),
                TravelTime = Query2.First().Element("TravelTime").Value,
                Text = Query2.First().Element("Text").Value,
                LatitudeNW = Convert.ToDouble(Query3.First().Element("Latitude").Value),
                LongitudeNW = Convert.ToDouble(Query3.First().Element("Longitude").Value),
                LatitudeSE = Convert.ToDouble(Query4.First().Element("Latitude").Value),
                LongitudeSE = Convert.ToDouble(Query4.First().Element("Longitude").Value)
            });

            return routeShapeData;
        }
        public List<RouteManeuverData> LoadRouteManeuverDataHERERv7(string HERE_XML)
        {
            XDocument document = new XDocument();
            document = XDocument.Parse(HERE_XML);

            var Query5 = from a in document.Descendants("Leg").Elements("Maneuver")
                         select a;

            foreach (XElement maneuver in Query5)
            {
                string nextManeuver = string.Empty;
                double toLink = 0;
                List<Location> shapeloc = new List<Location>();

                var maneuverId = maneuver.Attribute("id").Value;

                var shape = maneuver.Element("Shape").Value.Split(' ').ToList<string>();

                for (var i = 0; i < shape.Count(); i++)
                {
                    shapeloc.Add(Convert_String_To_Coordinates(shape[i]));
                }

                var trafficTime = Convert.ToDouble(maneuver.Element("TrafficTime").Value);
                var travelTime = Convert.ToDouble(maneuver.Element("TravelTime").Value);
                var baseTime = Convert.ToDouble(maneuver.Element("BaseTime").Value);
                var instruction = maneuver.Element("Instruction").Value;
                var direction = maneuver.Element("Direction").Value;
                var action = maneuver.Element("Action").Value;
                var length = Convert.ToDouble(maneuver.Element("Length").Value);
                var startAngle = Convert.ToDouble(maneuver.Element("StartAngle").Value);
                var roadName = maneuver.Element("RoadName").Value;
                var nextRoadName = maneuver.Element("NextRoadName").Value;                
                
                if(maneuver.Elements("NextManeuver").Count() > 0)
                    nextManeuver = maneuver.Element("NextManeuver").Value;

                if (maneuver.Elements("ToLink").Count() > 0)
                    toLink = Convert.ToDouble(maneuver.Element("ToLink").Value);

                var firstPoint = Convert.ToDouble(maneuver.Element("FirstPoint").Value);
                var lastPoint = Convert.ToDouble(maneuver.Element("LastPoint").Value);

                var QueryPosition = from b in maneuver.Descendants("Position")
                              select b;
                var positionLatitude = Convert.ToDouble(QueryPosition.First().Element("Latitude").Value);
                var positionLongitude = Convert.ToDouble(QueryPosition.First().Element("Longitude").Value);

                var QueryNW = from c in maneuver.Descendants("TopLeft")
                              select c;
                var latitudeNW = Convert.ToDouble(QueryNW.First().Element("Latitude").Value);
                var longitudeNW = Convert.ToDouble(QueryNW.First().Element("Longitude").Value);

                var QuerySE = from d in maneuver.Descendants("BottomRight")
                              select d;
                var latitudeSE = Convert.ToDouble(QuerySE.First().Element("Latitude").Value);
                var longitudeSE = Convert.ToDouble(QuerySE.First().Element("Longitude").Value);

                routeManeuverData.Add(new RouteManeuverData(){
                    ManeuverId = maneuverId,
                    Shape = shapeloc,
                    TrafficTime = trafficTime,
                    TravelTime = travelTime,
                    BaseTime = baseTime,
                    Instruction = instruction,
                    Direction = direction,
                    Action = action,
                    Length = length,
                    StartAngle = startAngle,
                    RoadName = roadName,
                    NextRoadName = nextRoadName,
                    NextManeuver = nextManeuver,
                    ToLink = toLink,
                    FirstPoint = firstPoint,
                    LastPoint = lastPoint,
                    PositionLatitude = positionLatitude,
                    PositionLongitude = positionLongitude,
                    LatitudeNW = latitudeNW,
                    LongitudeNW = longitudeNW,
                    LatitudeSE = latitudeSE,
                    LongitudeSE = longitudeSE
                });
            }
            return routeManeuverData;
        }
        public List<RouteLinkData> LoadRouteLinkDataHERERv7(string HERE_XML)
        {
            XDocument document = new XDocument();
            document = XDocument.Parse(HERE_XML);

            var Query6 = from a in document.Descendants("Leg").Elements("Link")
                         select a;

            foreach (XElement link in Query6)
            {
                double nextlink = 0;
                List<Location> shapeloc = new List<Location>();

                var linkid = Convert.ToDouble(link.Element("LinkId").Value);

                var shape = link.Element("Shape").Value.Split(' ').ToList<string>();

                for (var i = 0; i < shape.Count(); i++)
                {
                    shapeloc.Add(Convert_String_To_Coordinates(shape[i]));
                }

                var firstPoint = Convert.ToDouble(link.Element("FirstPoint").Value);
                var lastPoint = Convert.ToDouble(link.Element("LastPoint").Value);
                var length = Convert.ToDouble(link.Element("Length").Value);
                var remaindistance = Convert.ToDouble(link.Element("RemainDistance").Value);
                var remaintime = Convert.ToDouble(link.Element("RemainTime").Value);

                if (link.Elements("NextLink").Count() > 0)                
                    nextlink = Convert.ToDouble(link.Element("NextLink").Value);

                var maneuverid = link.Element("Maneuver").Value;

                var QueryDynamicSpeedInfo = from b in link.Descendants("DynamicSpeedInfo")
                                    select b;
                var trafficSpeed = Convert.ToDouble(QueryDynamicSpeedInfo.First().Element("TrafficSpeed").Value);
                var trafficTime = Convert.ToDouble(QueryDynamicSpeedInfo.First().Element("TrafficTime").Value);
                var baseSpeed = Convert.ToDouble(QueryDynamicSpeedInfo.First().Element("BaseSpeed").Value);
                var baseTime = Convert.ToDouble(QueryDynamicSpeedInfo.First().Element("BaseTime").Value);
                var jamFactor = Convert.ToDouble(QueryDynamicSpeedInfo.First().Element("JamFactor").Value);
                
                var roadName = link.Element("RoadName").Value;

                routeLinkData.Add(new RouteLinkData()
                {
                    LinkId = linkid,
                    Shape = shapeloc,
                    FirstPoint = firstPoint,
                    LastPoint = lastPoint,
                    Length = length,
                    RemainDistance = remaindistance,
                    RemainTime = remaintime,
                    NextLink = nextlink,
                    ManeuverId = maneuverid,
                    TrafficSpeed = trafficSpeed,
                    TrafficTime = trafficTime,
                    BaseSpeed = baseSpeed,
                    BaseTime = baseTime,
                    JamFactor = jamFactor,
                    RoadName = roadName
                });
            }
            return routeLinkData;
        }
        private void ContentPage_Disappearing(object sender, EventArgs e)
        {
            base.OnDisappearing();
        }
        public class PoILongListData
        {
            public ImageSource PoIImg { get; set; }
            public string PoIName { get; set; }
            public string PoILayer { get; set; }
            public string PoIStreet { get; set; }
            public string PoICity { get; set; }
            public string PoIAddress { get; set; }
            public string PoILat { get; set; }
            public string PoILon { get; set; }
            public string PoIGasType { get; set; }
            public string PoITemperature { get; set; }
            public string PoIDensity { get; set; }
            public string PoIVolume { get; set; }
            public string PoIFuelQualityRating { get; set; }
            public string PoIFuelQuantityRating { get; set; }
            public string PoIConfidenceFactorOne { get; set; }
            public string PoIConfidenceFactorTwo { get; set; }
            public string PoIDatestamp { get; set; }
            public string PoITimestamp { get; set; }
        }
        public class GeocodeData
        {
            public string LocName { get; set; }            
            public string LocLat { get; set; }
            public string LocLon { get; set; }
        }
        public class RouteShapeList
        {
            public double LocLat { get; set; }
            public double LocLon { get; set; }
        }
        public class RouteShapeData
        {
            public double Distance { get; set; }
            public string TravelTime { get; set; }
            public string Text { get; set; }
            public double LatitudeNW { get; set; }
            public double LongitudeNW { get; set; }
            public double LatitudeSE { get; set; }
            public double LongitudeSE { get; set; }
        }
        public class RouteManeuverData
        {
            public string ManeuverId { get; set; }
            public List<Location> Shape { get; set; }
            public double TrafficTime { get; set; }
            public double BaseTime { get; set; }
            public double TravelTime { get; set; }
            public double Length { get; set; }
            public double StartAngle { get; set; }
            public double FirstPoint { get; set; }
            public double LastPoint { get; set; }
            public string Instruction { get; set; }
            public string Direction { get; set; }
            public string Action { get; set; }
            public string RoadName { get; set; }
            public string NextRoadName { get; set; }
            public string NextManeuver { get; set; }
            public double ToLink { get; set; }
            public double PositionLatitude { get; set; }
            public double PositionLongitude { get; set; }
            public double LatitudeNW { get; set; }
            public double LongitudeNW { get; set; }
            public double LatitudeSE { get; set; }
            public double LongitudeSE { get; set; }
        }
        public class RouteLinkData
        {
            public double LinkId { get; set; }
            public List<Location> Shape { get; set; }
            public double FirstPoint { get; set; }
            public double LastPoint { get; set; }
            public double Length { get; set; }
            public double RemainDistance { get; set; }
            public double RemainTime { get; set; }
            public double NextLink { get; set; }
            public string ManeuverId { get; set; }
            public double TrafficSpeed { get; set; }
            public double TrafficTime { get; set; }
            public double BaseSpeed { get; set; }
            public double BaseTime { get; set; }
            public double JamFactor { get; set; }
            public string RoadName { get; set; }
        }

        public ObservableCollection<PoILongListData> PoIListData = new ObservableCollection<PoILongListData>();
        public List<GeocodeData> geocodeResultList = new List<GeocodeData>();
        public List<RouteShapeData> routeShapeData = new List<RouteShapeData>();
        public List<RouteShapeList> routeShapeList = new List<RouteShapeList>();
        public List<RouteManeuverData> routeManeuverData = new List<RouteManeuverData>();
        public List<RouteLinkData> routeLinkData = new List<RouteLinkData>();
    }
}
