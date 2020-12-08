using System;
using System.Collections.Generic;

using System.Threading.Tasks;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

using JaFaFa.Models;
using SlideOverKit;
using System.ComponentModel;
using Xamarin.Forms.Maps;
using System.Diagnostics;
using Xamarin.Forms.Markup;
using JaFaFa.Magneto;
using JaFaFa.JaFaFaMAP;
using Xamarin.Essentials;
using JaFaFa.PoI;

using System.Reflection;
using System.IO;
using System.Collections.ObjectModel;

namespace JaFaFa.Views
{
    // Learn more about making custom code visible in the Xamarin.Forms previewer
    // by visiting https://aka.ms/xamarinforms-previewer
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class MainPage : ContentPage
    {
        public MainPage()
        {
            InitializeComponent();

            LoadjMAPPins();

            DayNightColorMode();            
        }
        protected override void OnAppearing()
        {            
            base.OnAppearing();
        }
        private void ToolBar_MainPage_Clicked(object sender, EventArgs e)
        {
            string resPrefix = "JaFaFa.PoI.";
            var assembly = IntrospectionExtensions.GetTypeInfo(typeof(MainPage)).Assembly;
            Stream stream = assembly.GetManifestResourceStream(resPrefix + "PoIDB.xml");

            JaFaFa.PoI.PoI poi = new JaFaFa.PoI.PoI();

            //ObservableCollection<PoI.PoI.PoILongListData> longlist = new ObservableCollection<PoI.PoI.PoILongListData>();

            //Task one = Task.Run(() =>
            //{
                //longlist = poi.LoadPoIDatabaseMainPage(stream);
            //});

            //one.Wait();

            LoadjMAPPinsAsync(poi.LoadPoIDatabaseMainPage(stream), null, null, null);
        }
        private void OnSwiped(object sender, SwipedEventArgs e)
        {
            if ((RightSideMasterView_Name.IsVisible == false)
                && (LeftSideMasterView_Name.IsVisible == false))
            {
                switch (e.Direction)
                {
                    case SwipeDirection.Left:
                        // Handle the swipe
                        //DisplayAlert("Right View", "Left Swipe", "OK");
                        RightSideMasterView_Name.IsVisible = true;
                        //AbsoluteLayout_Map.Children.Add(RightSideMasterView_Name);
                        AbsoluteLayout_Map.Children.Add(RightSideMasterView_Name);
                        break;
                    case SwipeDirection.Right:
                        // Handle the swipe
                        //DisplayAlert("Left View", "Right Swipe", "OK");
                        LeftSideMasterView_Name.IsVisible = true;
                        AbsoluteLayout_Map.Children.Add(LeftSideMasterView_Name);                        
                        break;
                }
            }
            else
            {
                if (RightSideMasterView_Name.IsVisible)
                {
                    RightSideMasterView_Name.IsVisible = false;
                    AbsoluteLayout_Map.Children.Remove(RightSideMasterView_Name);
                }
                else if (LeftSideMasterView_Name.IsVisible)
                {
                    LeftSideMasterView_Name.IsVisible = false;
                    AbsoluteLayout_Map.Children.Remove(LeftSideMasterView_Name);
                }
            }
        }
        private void DayNightColorMode()
        {
            DateTime currentTime = DateTime.Now;

            DateTime DarkTimeLowerAM = new DateTime(currentTime.Year, currentTime.Month, currentTime.Day, 0, 0, 0);
            DateTime DarkTimeUpperAM = new DateTime(currentTime.Year, currentTime.Month, currentTime.Day, 6, 30, 0);

            DateTime DarkTimeLowerPM = new DateTime(currentTime.Year, currentTime.Month, currentTime.Day, 18, 30, 0);
            DateTime DarkTimeUpperPM = new DateTime(currentTime.Year, currentTime.Month, currentTime.Day, 23, 59, 59);

            if ((currentTime >= DarkTimeLowerAM && currentTime <= DarkTimeUpperAM) || (currentTime >= DarkTimeLowerPM && currentTime <= DarkTimeUpperPM))
            {
                //DisplayAlert("Mode", "Night Mode!", "OK");
                //BackgroundColor = Color.Black;
                //JaFaFaMap.Opacity = 0.8;
                JaFaFaMap.Opacity = 1;
            }
            else
            {
                //DisplayAlert("Mode", "Day Mode!", "OK");
                AbsoluteLayout_Map.BackgroundColor = Color.Black;
                JaFaFaMap.Opacity = 1;                
            }
        }
        async private void LoadjMAPPinsAsync(ObservableCollection<PoI.PoI.PoILongListData> PoIListData,
            List<Location> coordinatelist, List<String> labellist, List<String> addresslist)
        {
            if (PoIListData != null && (coordinatelist == null && labellist == null && addresslist == null))
            {
                await Task.Run(() =>
                {
                    for (int i = 0; i < PoIListData.Count; i++)
                    {
                        jMAPPin pin = new jMAPPin();
                        pin.Type = PinType.Place;
                        pin.Label = PoIListData[i].PoIName;
                        pin.Name = PoIListData[i].PoIName;                        
                        pin.Url = "https://anywash.business.site/";
                        pin.Icon = "checkered_flag_icon.png";
                        pin.Address = PoIListData[i].PoIAddress;
                        pin.Position = new Position(Convert.ToDouble(PoIListData[i].PoILat), Convert.ToDouble(PoIListData[i].PoILon));
                        JaFaFaMap.jMAPPins = new List<jMAPPin> { pin };
                        JaFaFaMap.Pins.Add(pin);
                        //pin.MarkerClicked += OnClickedAsync_jMAPPinMarker;
                        pin.InfoWindowClicked += OnClickedAsync_jMAPPinInfoWindow;
                    }
                });
            }
            else
            {
                for (int i = 0; i < coordinatelist.Count; i++)
                {
                    jMAPPin pin = new jMAPPin();
                    pin.Type = PinType.Place;
                    pin.Label = labellist[i];
                    pin.Name = labellist[i];
                    pin.Url = "https://anywash.business.site/";
                    pin.Icon = "checkered_flag_icon.png";
                    pin.Address = addresslist[i];
                    pin.Position = new Position(coordinatelist[i].Latitude, coordinatelist[i].Longitude);
                    JaFaFaMap.jMAPPins = new List<jMAPPin> { pin };
                    JaFaFaMap.Pins.Add(pin);
                    //pin.MarkerClicked += OnClickedAsync_jMAPPinMarker;
                    pin.InfoWindowClicked += OnClickedAsync_jMAPPinInfoWindow;
                }
            }

            if (JaFaFaMap.VisibleRegion == null)
            {
                JaFaFaMap.MoveToRegion(MapSpan.FromCenterAndRadius(new Position(HEREANYWASHCoordinate.Latitude, HEREANYWASHCoordinate.Longitude), Distance.FromKilometers(0.444)));
            }
        }

        private void LoadjMAPPins()
        {
            List<Location> markercoordinatelist = new List<Location>();

            /*markercoordinatelist.Add(HERELindenCoordinate);
            markercoordinatelist.Add(HEREBergwerkswaldCoordinate);
            markercoordinatelist.Add(HEREZeppelinheimCoordinate);
            markercoordinatelist.Add(HEREButzbachCoordinate);
            markercoordinatelist.Add(HEREDarmstadtCoordinate);
            markercoordinatelist.Add(HEREFloralParkCoordinate);
            markercoordinatelist.Add(HEREBrooklynHeightsOhioCoordinate);
            markercoordinatelist.Add(HEREBrooklynHeightsCoordinate);
            markercoordinatelist.Add(HEREQueensCoordinate);
            markercoordinatelist.Add(HEREUnionCityCoordinate);
            markercoordinatelist.Add(HERETimesSquareCoordinate);
            markercoordinatelist.Add(HEREHamiltonParkCoordinate);
            markercoordinatelist.Add(HEREManhattanCoordinate);
            markercoordinatelist.Add(HEREBrooklynCoordinate);
            markercoordinatelist.Add(HEREBayRidgeCoordinate);
            markercoordinatelist.Add(HEREFlushingMeadowsParkCoordinate);
            markercoordinatelist.Add(HEREANYWASHCoordinate);
            markercoordinatelist.Add(HEREUNILAGCoordinate);
            markercoordinatelist.Add(HERENetcomAdeolaOdekuCoordinate);
            markercoordinatelist.Add(HEREMMAIkejaCoordinate);
            markercoordinatelist.Add(HEREPalmsCoordinate);
            markercoordinatelist.Add(HERESouthernSunCoordinate);
            markercoordinatelist.Add(HEREMusonCentreCoordinate);*/
            markercoordinatelist.Add(HEREGTBHerbertMacaulayCoordinate);
            markercoordinatelist.Add(HEREWesternAvenueCoordinate);
            markercoordinatelist.Add(HEREThirdMainlandCoordinate);
            markercoordinatelist.Add(HEREIkoroduRoadCoordinate);            

            List<String> markerlabellist = new List<String>();

            /*markerlabellist.Add(HEREANYWASHLabel);
            markerlabellist.Add(HEREUNILAGLabel);
            markerlabellist.Add(HERENetcomAdeolaOdekuLabel);
            markerlabellist.Add(HEREMMAIkejaLabel);
            markerlabellist.Add(HEREPalmsLabel);
            markerlabellist.Add(HERESouthernSunLabel);
            markerlabellist.Add(HEREMusonCentreLabel);*/
            markerlabellist.Add(HEREGTBHerbertMacaulayLabel);
            markerlabellist.Add(HEREWesternAvenueLabel);
            markerlabellist.Add(HEREThirdMainlandLabel);
            markerlabellist.Add(HEREIkoroduRoadLabel);
            
            List<String> markeraddresslist = new List<String>();

            /*markeraddresslist.Add(HEREANYWASHAddress);
            markeraddresslist.Add(HEREUNILAGAddress);
            markeraddresslist.Add(HERENetcomAdeolaOdekuAddress);
            markeraddresslist.Add(HEREMMAIkejaAddress);
            markeraddresslist.Add(HEREPalmsAddress);
            markeraddresslist.Add(HERESouthernSunAddress);
            markeraddresslist.Add(HEREMusonCentreAddress);*/
            markeraddresslist.Add(HEREGTBHerbertMacaulayAddress);
            markeraddresslist.Add(HEREWesternAvenueAddress);
            markeraddresslist.Add(HEREThirdMainlandAddress);
            markeraddresslist.Add(HEREIkoroduRoadAddress);

            LoadjMAPPinsAsync(null, markercoordinatelist, markerlabellist, markeraddresslist);
        }

        async void OnClickedAsync_jMAPPinMarker(object sender, PinClickedEventArgs e)
        {
            e.HideInfoWindow = true;
            string pinName = ((jMAPPin)sender).Label;
            await DisplayAlert("Pin Clicked", $"{pinName} was clicked.", "Ok");
            //DisplayAlert("Mode", "Day Mode!", "OK");
        }
        async void OnClickedAsync_jMAPPinInfoWindow(object sender, PinClickedEventArgs e)
        {
            string pinName = ((jMAPPin)sender).Label;
            string pinUrl = ((jMAPPin)sender).Url;
            //await DisplayAlert("Info Window Clicked", $"The info window was clicked for {pinName}.", "Ok");
            await Browser.OpenAsync(pinUrl);
        }

        public static double _headingAccuracy = Magneto.Magneto._data;

        private Location HEREANYWASHCoordinate = new Location(6.5140107, 3.3955142);
        private String HEREANYWASHLabel = "ANYWASH Nigeria Limited";
        private String HEREANYWASHAddress = "Old UNILAG Laundry, Behind UNILAG Medical Centre, Akinpelu Adesola Road, University Of Lagos, 100213, Lagos";

        private Location HERELindenCoordinate = new Location(50.528931, 8.66205); //A485, Linden, Hessen, Germany Coordinates
        private Location HEREBergwerkswaldCoordinate = new Location(50.562391, 8.677153); //A485, Bergwerkswald, Hessen, Germany Coordinates        

        private Location HEREZeppelinheimCoordinate = new Location(50.0333, 8.62); //A5, Zeppelinheim, Hessen, Germany Coordinates
        private Location HEREButzbachCoordinate = new Location(50.436667, 8.662222); //A5, Butzbach, Hessen, Germany Coordinates
        private Location HEREDarmstadtCoordinate = new Location(49.877751, 8.650306); //A5, Darmstadt, Hessen, Germany Coordinates

        private Location HEREFloralParkCoordinate = new Location(40.723889, -73.705833); //Floral Park, New York Coordinates        
        private Location HEREBrooklynHeightsOhioCoordinate = new Location(37.169026, -94.386404); //Brooklyn Heights Village, Missouri, Jasper Coordinates
        private Location HEREBrooklynHeightsCoordinate = new Location(40.624722, -73.952222); //Brooklyn Heights, Brooklyn, New York Coordinates
        private Location HEREQueensCoordinate = new Location(40.75, -73.866667); //Queens, Queens, New York Coordinates
        private Location HEREUnionCityCoordinate = new Location(40.76746, -74.032295); //Union City, New Jersey, Hudson Coordinates
        private Location HERETimesSquareCoordinate = new Location(40.75773, -73.985708); //Times Square, Manhatten, New York Coordinates
        private Location HEREHamiltonParkCoordinate = new Location(40.7275, -74.045); //Hamilton Park, Jersey City, New Jersey Coordinates
        private Location HEREManhattanCoordinate = new Location(40.790278, -73.959722); //Manhattan, New York City, New York Coordinates
        private Location HEREBrooklynCoordinate = new Location(40.692778, -73.990278); //Brooklyn Heights, Brooklyn, New York Coordinates


        private Location HEREBayRidgeCoordinate = new Location(40.626944, -74.031111); //Bay Ridge, Brooklyn Coordinates
        private Location HEREFlushingMeadowsParkCoordinate = new Location(40.745833, -73.844722); //Flushing Meadows, Queens, New York Coordinates

        private Location HEREUNILAGCoordinate = new Location(6.516667, 3.386111);
        private String HEREUNILAGLabel = "University of Lagos";
        private String HEREUNILAGAddress = "University of Lagos, Akoka-Yaba, Lagos Coordinates";
        
        private Location HERENetcomAdeolaOdekuCoordinate = new Location(6.4302778, 3.41);
        private String HERENetcomAdeolaOdekuLabel = "Netcom Africa";
        private String HERENetcomAdeolaOdekuAddress = "Netcom Africa, Adeola Odeki Street, VI, Lagos Coordinates";
        
        private Location HEREMMAIkejaCoordinate = new Location(6.577222, 3.321111);
        private String HEREMMAIkejaLabel = "Murtala Muhammed International Airport";
        private String HEREMMAIkejaAddress = "Murtala Muhammed International Airport, Ikeja, Lagos";

        private Location HEREPalmsCoordinate = new Location(6.435833, 3.451111);
        private String HEREPalmsLabel = "Palms Shopping Mall";
        private String HEREPalmsAddress = "Palms Shopping Mall, Lekki Peninsula, Lagos";

        private Location HERESouthernSunCoordinate = new Location(6.4595417, 3.4121472);
        private String HERESouthernSunLabel = "Southern Sun";
        private String HERESouthernSunAddress = "Southern Sun, Ikoyi, Lagos";

        private Location HEREMusonCentreCoordinate = new Location(6.4433333, 3.4013889);
        private String HEREMusonCentreLabel = "Muson Centre";
        private String HEREMusonCentreAddress = "Muson Centre, Onikan, Lagos";

        private Location HEREGTBHerbertMacaulayCoordinate = new Location(6.49455, 3.37948);
        private String HEREGTBHerbertMacaulayLabel = "GTB";
        private String HEREGTBHerbertMacaulayAddress = "GTB Herbert Macaulay Way, Alagomeji, Yaba, Lagos";

        private Location HEREWesternAvenueCoordinate = new Location(6.5020945, 3.3630346);
        private String HEREWesternAvenueLabel = "Western Avenue Way";
        private String HEREWesternAvenueAddress = "Western Avenue Way, Surulere, Lagos";

        private Location HEREThirdMainlandCoordinate = new Location(6.503772, 3.4026718);
        private String HEREThirdMainlandLabel = "Third Mainland Bridge";
        private String HEREThirdMainlandAddress = "Third Mainland Bridge, Lagos";

        private Location HEREIkoroduRoadCoordinate = new Location(6.584575, 3.377308);
        private String HEREIkoroduRoadLabel = "Ikorodu Road";
        private String HEREIkoroduRoadAddress = "Ikorodu Road, Lagos";

        private void OnButtonClicked(object sender, EventArgs e)
        {
            Button button = sender as Button;
            switch (button.Text)
            {
                case "Street":
                    JaFaFaMap.MapType = MapType.Street;
                    break;
                case "Satellite":
                    JaFaFaMap.MapType = MapType.Satellite;
                    break;
                case "Hybrid":
                    JaFaFaMap.MapType = MapType.Hybrid;
                    break;
            }
        }
    }
}