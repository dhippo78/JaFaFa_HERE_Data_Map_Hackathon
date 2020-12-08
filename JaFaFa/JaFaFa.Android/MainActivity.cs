/*
 * Copyright © 2020 MODE Communications International (MCI) Limited. All rights reserved.
 * MCI and Já Fá Fá are registered trademarks of MODE Communications International (MCI) Limited. 
 * Other product and company names mentioned herein may be trademarks
 * or trade names of their respective owners. 
 * See LICENSE.TXT for license information.
 */
using System;

using Android.App;
using Android.Content.PM;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Android.OS;

using System.Collections.Generic;

using JaFaFa.Views;

using Android.Gms.Maps;
using Android.Gms.Maps.Model;

using Android.Content;
using Android.Support.V4.Widget;
using Android.Support.V4.View;
using Android.Support.Design.Widget;

using Android.Content.Res;

using System.Reflection;
using System.IO;
using System.Collections.ObjectModel;

//Ambiguities
using Xamarin.Essentials;
using System.Linq;
using System.Xml;
using System.Net.Http;
using Newtonsoft.Json;
using System.Xml.Linq;
using Android.Locations;
using Android;
using AndroidX.Core.Content;
using JaFaFa.Helpers;

using Android.Views.InputMethods;

using JaFaFa.HEREXCoding;
using com.here.flexpolyline;

using RecyclerView = Android.Support.V7.Widget.RecyclerView;

using Com.Here.Android.Mpa.Common;
using Com.Here.Android.Mpa.Mapping;
using Com.Here.Android.Mpa.Guidance;
using AndroidX.Core.App;

using System.Runtime.Serialization.Json;
using System.Text;
using Java.Lang;
using Map = Com.Here.Android.Mpa.Mapping.Map;
using Com.Here.Android.Mpa.Odml;
using Android.Graphics;
using Android.Util;
using AndroidX.AppCompat.App;
using Xamarin.Forms.Platform.Android;
using Android.Hardware;
using Java.Nio;
using AndroidX.Lifecycle;
using Java.Lang.Annotation;
using Java.Util;
using Location = Com.Here.Android.Mpa.Mapping.Location;
using System.Threading;
using Xamarin.Forms;
using Image = Com.Here.Android.Mpa.Common.Image;
using Color = Android.Graphics.Color;
using View = Android.Views.View;
using System.ComponentModel;
using Com.Here.Android.Mpa.Cluster;
using Com.Here.Android.Mpa.Service;

using GeoJSON.Net.Converters;
using GeoJSON.Net.Geometry;
using System.Runtime.CompilerServices;
using GeoJSON.Net.Feature;
using Com.Here.Android.Mpa.Routing;
using Android.Opengl;

using StickyHeader;
using Com.Here.Android.Mpa.Customlocation2;
using Com.Here.Android.Mpa.Electronic_horizon;
using Newtonsoft.Json.Linq;
using JaFaFa.Models;
using GeoJSON.Net;
using System.Text.RegularExpressions;

namespace JaFaFa.Droid
{
    [Activity(Label = "JáFáFá", Icon = "@drawable/JaFaFa_icon", Theme = "@style/MainTheme", MainLauncher = true, ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation)]
    //public class MainActivity : global::Xamarin.Forms.Platform.Android.FormsAppCompatActivity, IOnMapReadyCallback, ILocationListener
    //public class MainActivity : AndroidX.AppCompat.App.AppCompatActivity, IOnMapReadyCallback, ILocationListener
    public class MainActivity : AndroidX.AppCompat.App.AppCompatActivity, Map.IOnTransformListener
    {
        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);            

            if (savedInstanceState != null)
            {
                isRequestingLocationUpdates = savedInstanceState.KeySet().Contains(KEY_REQUESTING_LOCATION_UPDATES) &&
                                              savedInstanceState.GetBoolean(KEY_REQUESTING_LOCATION_UPDATES);
            }
            else
            {
                isRequestingLocationUpdates = false;
            }

            if (ContextCompat.CheckSelfPermission(this, Manifest.Permission.AccessFineLocation) == Permission.Granted)
            {
                //Requesting single location update                
            }
            else
            {
                RequestLocationPermission(RC_LAST_LOCATION_PERMISSION_CHECK);
            }

            // Set our view from the "main" layout resource:
            SetContentView(Resource.Layout.MapLayout);

            // get the system sensors for compass
            mSensorManager = (SensorManager)GetSystemService(Context.SensorService);
            mAccelerometer = mSensorManager.GetDefaultSensor(SensorType.Accelerometer);
            mMagnetometer = mSensorManager.GetDefaultSensor(SensorType.MagneticField);

            // Set up disk cache path for the map service for this application
            bool success = MapSettings.SetIsolatedDiskCacheRootPath(ApplicationContext.GetExternalFilesDir(null) + Java.IO.File.Separator + ".here-maps");

            if (!success)
            {
                Toast.MakeText(ApplicationContext, "Unable to set isolated disk cache path.", ToastLength.Long);
            }
            else
            {
                // Search for the map fragment to finish setup.
                m_mapFragment = (AndroidXMapFragment)this.SupportFragmentManager.FindFragmentById(Resource.Id.map);                

                if (m_mapFragment != null)
                {
                    OnEngineInitListener onEngineInitListener = new OnEngineInitListener(m_mapFragment, this);
                    
                    // Initialize the AndroidXMapFragment, results will be given via the called back.
                    m_mapFragment.Init(onEngineInitListener);                    
                }
            }

            toolbar = FindViewById<Android.Support.V7.Widget.Toolbar>(Resource.Id.toolbartop);
            toolbar.Title = this.Title;
            toolbar.InflateMenu(Resource.Menu.toolbar_top_menu);

            toolbar.MenuItemClick += OnToolBarMenuItemClick;

            lowertoolbar = FindViewById<Android.Support.V7.Widget.Toolbar>(Resource.Id.toolbarbottom);
            lowertoolbar.InflateMenu(Resource.Menu.toolbar_bottom_menu);
            lowertoolbar.MenuItemClick += OnLowerToolBarMenuItemClick;

            if (success && m_mapFragment != null)
                m_mapFragment.View.SetPadding(0, toolbar.Height, 0, lowertoolbar.Height);

            mDrawerLayout = FindViewById<DrawerLayout>(Resource.Id.drawer_layout);

            // Get our NavigationView layout:
            mDrawerViewLeft = FindViewById<NavigationView>(Resource.Id.left_drawer);
            mDrawerViewRight = FindViewById<NavigationView>(Resource.Id.right_drawer);

            Navigation_RecyclerView_Route = this.FindViewById<RecyclerView>(Resource.Id.routeRecyclerView);
            Navigation_RecyclerView_Route.Visibility = ViewStates.Invisible;

            mDrawerViewLeft.NavigationItemSelected += mDrawerViewLeft_NavigationItemSelected;
            mDrawerViewRight.NavigationItemSelected += mDrawerViewLeft_NavigationItemSelected;

            // set a custom shadow that overlays the main content when the drawer opens
            mDrawerLayout.SetDrawerShadow(Resource.Drawable.drawer_shadow, GravityCompat.Start);

            // ActionBarDrawerToggle ties together the the proper interactions
            // between the sliding drawer and the action bar app icon
            mDrawerToggle = new MyActionBarDrawerToggle(this, mDrawerLayout,
                toolbar,
                Resource.String.drawer_open,
                Resource.String.drawer_close);

            mDrawerLayout.SetDrawerListener(mDrawerToggle);

            SetupStreetButton();
            SetupSatelliteButton();
            SetupHybridButton();

            _routelines = new MapPolyline();

            Xamarin.Essentials.Platform.Init(this, savedInstanceState);
            global::Xamarin.Forms.Forms.Init(this, savedInstanceState);
            Xamarin.FormsMaps.Init(this, savedInstanceState);
        }        
        private void OnToolBarMenuItemClick(object sender, Android.Support.V7.Widget.Toolbar.MenuItemClickEventArgs e)
        {
            // The action bar home/up action should open or close the drawer.
            // ActionBarDrawerToggle will take care of this.
            // This handles top ActionBar Toolbar icons

            if (mDrawerToggle.OnOptionsItemSelected(e.Item))
            {
                return;
            }

            // Handle action buttons
            switch (e.Item.ItemId)
            {
                /*case Android.Resource.Id.Home:
                    mDrawerLayout.OpenDrawer(Android.Support.V4.View.GravityCompat.Start);
                    return true;*/
                case Resource.Id.locate_me:
                    locateMeMenuClicked = true;
                    _tracklineDisposed = false;
                    _routelineDisposed = false;
                    fromTopToolbar = true;
                    LocateMeHEREWorker();
                    break;
                case Resource.Id.track_me:
                    trackMeMenuClicked = true;
                    _tracklineDisposed = false;
                    _routelineDisposed = false;
                    TrackMeHERE();
                    break;
                case Resource.Id.navigate:
                    OnPositionChangedListener.navigateMeMenuClicked = true;
                    _tracklineDisposed = false;
                    _routelineDisposed = false;
                    NavigateMe();
                    break;
                default:
                    return;
            }
        }

        private void OnLowerToolBarMenuItemClick(object sender, Android.Support.V7.Widget.Toolbar.MenuItemClickEventArgs e)
        {
            //Preferences.Get("Compass", 0);

            switch (e.Item.ItemId)
            {
                case Resource.Id.begin_journey:
                    if (OnPositionChangedListener.isOnJourney)
                    {  
                        if (positionManager.IsActive)
                        {
                            OnPositionChangedListener.isOnJourney = false;

                            //Return view to last known position
                            var LastKnowPosition = positionManager.LastKnownPosition;
                            navigationManager.Stop();
                            positionManager.Stop();                            
                            //m_mapFragment.Map.SetCenter(LastKnowPosition.Coordinate, Map.Animation.Linear, 18, 90, 45);
                            m_mapFragment.Map.SetCenter(LastKnowPosition.Coordinate, Map.Animation.None);

                            //Make hint panel invisible
                            HintPanel.Visibility = ViewStates.Invisible;
                            m_mapFragment.View.SetPadding(0, 0, 0, 0);

                            //Make GasAlert icon invisible
                            var gasAlertButton = FindViewById<Android.Widget.ImageButton>(Resource.Id.gasAlertButton);
                            
                            if (gasAlertButton.Visibility == ViewStates.Visible)
                                gasAlertButton.Visibility = ViewStates.Invisible;

                            if (Gas_Alert_RecyclerView.Visibility == ViewStates.Visible)
                                Gas_Alert_RecyclerView.Visibility = ViewStates.Invisible;

                            //Set toolbar icon
                            //lowertoolbar = FindViewById<Android.Support.V7.Widget.Toolbar>(Resource.Id.toolbarbottom);
                            var lowertoolbarMenuItem = lowertoolbar.Menu.GetItem(0);
                            lowertoolbarMenuItem.SetIcon(Resource.Drawable.transport_play2);

                            //Make new destination menu item invisible
                            lowertoolbar.Menu.GetItem(3).SetVisible(false);
                        }                        
                    }
                    else if (!OnPositionChangedListener.isOnJourney)
                    {
                        if (ContextCompat.CheckSelfPermission(this, Manifest.Permission.AccessFineLocation) == Permission.Granted)
                        {
                            if (!positionManager.IsActive)
                            {
                                if (positionManager.Start(PositioningManager.LocationMethod.GpsNetwork))
                                {
                                    //Start Navigation manager
                                    var getData = new GetDataHERE(this);
                                    getData.StartNavigation(HEREFromMarker.Coordinate, HEREThroughMarker.Coordinate, HEREDestinationMarker.Coordinate);
                                    
                                    //Move view to starting position
                                    AddHEREMarkerToMap(new GeoPosition(HEREFromMarker.Coordinate));
                                    m_mapFragment.Map.SetCenter(HEREFromMarker.Coordinate, Map.Animation.Linear, 18, (float)270, 45);

                                    //Make PoI markers visible
                                    getData.ShowHEREPOIMarkersHERE(true);

                                    lastManeuverIndex = 0;
                                    lIndexFoundIterated = false;
                                    pastManeuverIndexList = new List<int>();
                                    instructionannounced = false;

                                    //AddStartJourneyMarker(new LatLng(currentLocation.Latitude, currentLocation.Longitude));

                                    //Set toolbar icon
                                    //lowertoolbar = FindViewById<Android.Support.V7.Widget.Toolbar>(Resource.Id.toolbarbottom);
                                    var lowertoolbarMenuItem = lowertoolbar.Menu.GetItem(0);
                                    lowertoolbarMenuItem.SetIcon(Resource.Drawable.transport_pause);

                                    //Make new destination menu item visible
                                    lowertoolbar.Menu.GetItem(3).SetVisible(true);
                                }
                            }
                        }
                        else
                        {
                            RequestLocationPermission(RC_LAST_LOCATION_PERMISSION_CHECK);
                        }
                    }
                    break;
                case Resource.Id.show_hide_route:
                    var recyclerViewRouteHeader = this.FindViewById<FrameLayout>(Resource.Id.header);

                    if (isShowingRoute)
                    {
                        m_mapFragment.View.SetPadding(0, 0, 0, 0);

                        Navigation_RecyclerView_Route.Visibility = ViewStates.Invisible;

                        isShowingRoute = false;

                        //Set toolbar icon
                        //lowertoolbar = FindViewById<Android.Support.V7.Widget.Toolbar>(Resource.Id.toolbarbottom);
                        var lowertoolbarMenuItem = lowertoolbar.Menu.GetItem(1);
                        lowertoolbarMenuItem.SetIcon(Resource.Drawable.new_plus_2);                        
                        
                        recyclerViewRouteHeader.Visibility = ViewStates.Invisible;
                    }
                    else
                    {
                        //m_mapFragment.View.SetPadding(0, 0, 0, Navigation_RecyclerView_Route.Height + recyclerViewRouteHeader.Height);
                        m_mapFragment.View.SetPadding(0, 0, 0, Navigation_RecyclerView_Route.Height);

                        Navigation_RecyclerView_Route.Visibility = ViewStates.Visible;

                        isShowingRoute = true;

                        //Set toolbar icon
                        //lowertoolbar = FindViewById<Android.Support.V7.Widget.Toolbar>(Resource.Id.toolbarbottom);
                        var lowertoolbarMenuItem = lowertoolbar.Menu.GetItem(1);
                        lowertoolbarMenuItem.SetIcon(Resource.Drawable.minus);
                        
                        recyclerViewRouteHeader.Visibility = ViewStates.Visible;
                    }
                    break;
                case Resource.Id.compass: //New destination
                    New_Destination_Dialog_Show();
                    break;
                case Resource.Id.traffic://Show Map Markers
                    System.ComponentModel.BackgroundWorker ShowMarkersHEREWorker = new System.ComponentModel.BackgroundWorker();
                    ShowMarkersHEREWorker.DoWork += ShowMarkersHEREWorker_DoWork;
                    ShowMarkersHEREWorker.RunWorkerCompleted += ShowMarkersHEREWorker_RunWorkerCompleted; ;
                    ShowMarkersHEREWorker.RunWorkerAsync();
                    break;
                case Resource.Id.journey://Show Map Polygons
                    System.ComponentModel.BackgroundWorker ShowPolygonsHEREWorker = new System.ComponentModel.BackgroundWorker();
                    ShowPolygonsHEREWorker.DoWork += ShowPolygonsHEREWorker_DoWork;
                    ShowPolygonsHEREWorker.RunWorkerCompleted += ShowPolygonsHEREWorker_RunWorkerCompleted;
                    ShowPolygonsHEREWorker.RunWorkerAsync();                    
                    break;
                case Resource.Id.cartographic:
                    var streetButton = FindViewById<Android.Widget.Button>(Resource.Id.streetButton);
                    var satelliteButton = FindViewById<Android.Widget.Button>(Resource.Id.satelliteButton);
                    var hybridButton = FindViewById<Android.Widget.Button>(Resource.Id.hybridButton);

                    if (streetButton.Visibility == ViewStates.Visible)
                    {
                        streetButton.Visibility = ViewStates.Invisible;
                        satelliteButton.Visibility = ViewStates.Invisible;
                        hybridButton.Visibility = ViewStates.Invisible;

                        //Set toolbar icon
                        //lowertoolbar = FindViewById<Android.Support.V7.Widget.Toolbar>(Resource.Id.toolbarbottom);
                        var lowertoolbarMenuItem = lowertoolbar.Menu.GetItem(6);
                        lowertoolbarMenuItem.SetIcon(Resource.Drawable.Cartography_2);
                    }
                    else
                    {
                        streetButton.Visibility = ViewStates.Visible;
                        satelliteButton.Visibility = ViewStates.Visible;
                        hybridButton.Visibility = ViewStates.Visible;

                        //Set toolbar icon
                        //lowertoolbar = FindViewById<Android.Support.V7.Widget.Toolbar>(Resource.Id.toolbarbottom);
                        var lowertoolbarMenuItem = lowertoolbar.Menu.GetItem(6);
                        lowertoolbarMenuItem.SetIcon(Resource.Drawable.Cartography_2);
                    }
                    break;
            }
        }

        private void ShowPolygonsHEREWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            //progressBar.Dispose();
            //progressDialog.Cancel();
            HideProgressBar();

            if (menuactive)
            {
                var lowertoolbarMenuItem = lowertoolbar.Menu.GetItem(5);
                lowertoolbarMenuItem.SetIcon(Resource.Drawable.HERE_Data_Layer_Building_1);
            }
            else if (!menuactive)
            {
                var lowertoolbarMenuItem = lowertoolbar.Menu.GetItem(5);
                lowertoolbarMenuItem.SetIcon(Resource.Drawable.HERE_Data_Layer_Building_1);
            }
        }

        private void ShowPolygonsHEREWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            menuactive = false;

            if (mapPolygon != null)
            {
                UnloadHEREGeoJSONBuildings();
                menuactive = true;

            }
            else if (mapPolygon == null)
            {
                ShowProgressBar("Loading HERE Data Layer Buildings...");
                LoadHEREGeoJSONBuildings();
                menuactive = false;
            }
        }

        private void ShowMarkersHEREWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            //progressBar.Dispose();
            //progressDialog.Cancel();

            HideProgressBar();

            if (menuactive)
            {
                var lowertoolbarMenuItem = lowertoolbar.Menu.GetItem(4);
                lowertoolbarMenuItem.SetIcon(Resource.Drawable.HERE_Data_Layer_6);
            }
            else if (!menuactive)
            {
                var lowertoolbarMenuItem = lowertoolbar.Menu.GetItem(4);
                lowertoolbarMenuItem.SetIcon(Resource.Drawable.HERE_Data_Layer_6);
            }
        }

        private void ShowMarkersHEREWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            menuactive = false;

            ShowProgressBar("Loading HERE Markers Cluster and HERE Places Data Layer...");

            if (HERECluster != null && PoIListData != null)
            {
                m_mapFragment.Map.RemoveClusterLayer(HERECluster);
                PoIListData.Clear();
                PoIListData = null;
                HERECluster = null;
            }
            else if (HERECluster == null)
            {                
                LoadHEREMarkers();
            }
            /*
            if (HEREClusterGas != null && PoIListDataGas != null)
            {
                m_mapFragment.Map.RemoveClusterLayer(HEREClusterGas);
                PoIListDataGas.Clear();
                PoIListDataGas = null;
                HEREClusterGas = null;
                menuactive = true;
            }                    
            else if (HEREClusterGas == null)
            {
                LoadHEREMarkersGas();
                menuactive = false;
            }
            */
            if (HEREPlacesListData != null)
            {
                UnloadHEREGeoJSONPlaces();
                menuactive = true;
            }
            else if (HEREPlacesListData == null)
            {   
                LoadHEREGeoJSONPlaces();
                menuactive = false;
            }
        }

        private void ShowProgressBar(string message)
        {
            //Activity.RunOnUiThread(() => {
            MainThread.BeginInvokeOnMainThread(() =>
            {
                Android.App.AlertDialog.Builder dialogBuilder = new Android.App.AlertDialog.Builder(this);
                var inflater = (LayoutInflater)GetSystemService(Context.LayoutInflaterService);
                var dialogView = inflater.Inflate(Resource.Layout.Progress_Dialog, null);
                dialogBuilder.SetView(dialogView);
                dialogBuilder.SetCancelable(false);
                var tvMsg = dialogView.FindViewById<TextView>(Resource.Id.tv_message);
                tvMsg.Text = message;
                progressBarDialog = dialogBuilder.Create();
                progressBarDialog.Show();

                //progressBar = FindViewById<Android.Widget.ProgressBar>(Resource.Id.ProgressBar);
                //progressBar.Visibility = ViewStates.Visible;
                //progressBar.Progress = 25;
                // or alternatively
                //progressBar.IncrementProgressBy(30);

                /*            
                progressDialog = new ProgressDialog(this);
                progressDialog.Indeterminate = true;
                progressDialog.SetCancelable(true);
                progressDialog.SetProgressStyle(ProgressDialogStyle.Spinner);
                progressDialog.Show();
                *
                */
            });
        }
        private void HideProgressBar()
        {
            //Activity.RunOnUiThread(() => {
            MainThread.BeginInvokeOnMainThread(() =>
            {
                if (progressBarDialog != null)
                {
                    progressBarDialog.Dismiss();
                }
            });
        }
        private void New_Destination_Dialog_Show()
        {
            LayoutInflater layoutInflater = LayoutInflater.From(this);

            New_Destination_Dialog_View = layoutInflater.Inflate(Resource.Layout.NewDestinationDialog, null);

            Android.Support.V7.App.AlertDialog.Builder alertbuilder = new Android.Support.V7.App.AlertDialog.Builder(this);

            alertbuilder.SetView(New_Destination_Dialog_View);

            alertbuilder.SetCancelable(false)
                .SetPositiveButton("Go", delegate
                {
                    //Route from current location to gas station
                    //Toast.MakeText(activity, "Item with position: " + position.ToString() + "clicked", ToastLength.Long).Show();
                    var currentCoord = positionManager.LastKnownPosition.Coordinate;

                    Navigation_Dialog_fromLatLng = new LatLng(currentCoord.Latitude, currentCoord.Longitude);
                    Navigation_Dialog_throughLatLng = new LatLng(currentCoord.Latitude, currentCoord.Longitude);
                    Navigation_Dialog_destinationLatLng = New_Destination_Dialog_destinationLatLng;                    

                    //Get the new route
                    var getData = new GetDataHERE(this);
                    getData.GetRouteHERE();

                    var Coord = new GeoCoordinate(New_Destination_Dialog_destinationLatLng.Latitude, New_Destination_Dialog_destinationLatLng.Longitude);

                    //Re-start the Navigation manager with the new route
                    navigationManager.Stop();
                    getData.StartNavigation(currentCoord, currentCoord, Coord);
                })
                .SetNegativeButton("Cancel", delegate
                {
                    //Do something
                    alertbuilder.Dispose();
                });

            Android.Support.V7.App.AlertDialog dialog = alertbuilder.Create();

            dialog.Show();

            InitiateNewDestinationDialog();
        }
        private void InitiateNewDestinationDialog()        
        {
            New_Destination_Dialog_destinationUserData = New_Destination_Dialog_View.FindViewById<AutoCompleteTextView>(Resource.Id.newDestination);

            New_Destination_Dialog_destinationUserData.TextChanged += New_Destination_Dialog_Userdata_TextChanged;

            New_Destination_Dialog_destinationUserData.ItemClick += New_Destination_Dialog_UserData_ItemClick;
        }
        private void New_Destination_Dialog_Userdata_TextChanged(object sender, Android.Text.TextChangedEventArgs e)
        {
            if (New_Destination_Dialog_destinationUserData.HasFocus && New_Destination_Dialog_destinationUserData.IsPopupShowing == false)
            {
                if (e.Text.Count() > 2)
                {
                    GetLocationAsync(e.Text.ToString().ToLower());
                }
            }
        }
        private void New_Destination_Dialog_UserData_ItemClick(object sender, AdapterView.ItemClickEventArgs e)
        {
            if (New_Destination_Dialog_destinationSelectedItem)
            {                

                New_Destination_Dialog_destinationLatLng = new LatLng(Convert.ToDouble(geocodeResult[e.Position].LocLat),
                    Convert.ToDouble(geocodeResult[e.Position].LocLon));

                New_Destination_Dialog_destinationSelectedItem = false;

                //Update Header for RecyclerView
                New_Destination_Dialog_destinationLabel = geocodeResult[e.Position].LocName;
                var recyclerViewRouteHeaderText = this.FindViewById<TextView>(Resource.Id.header_text);
                recyclerViewRouteHeaderText.Text = "Heading to:\n" + New_Destination_Dialog_destinationLabel;

                HideSoftKeyboard(New_Destination_Dialog_View);
            }
        }
        private void AddStartJourneyMarker(LatLng latlng)
        {
            if (journeyMarker != null)
                journeyMarker.Remove();

            var JAFAFAMarkerOptions = new MarkerOptions()
                .SetPosition(latlng)
                .SetTitle("Start position")
                .SetSnippet("Latitude: " + latlng.Latitude.ToString() + "," +
                "Longitude: " + latlng.Longitude.ToString());
                //.SetIcon(GetIcon("journey"));

            journeyMarker = googleMap.AddMarker(JAFAFAMarkerOptions);
            journeyMarker.ShowInfoWindow();

            Toast.MakeText(this, "StartJourneyMarker is added at: " + latlng.ToString(), ToastLength.Long).Show();

            previousLocation = new Android.Locations.Location(providerJAFAFA);

            previousLocation.Latitude = latlng.Latitude;
            previousLocation.Longitude = latlng.Longitude;

            MoveToLocation(latlng);
        }
        internal class MyActionBarDrawerToggle : Android.Support.V7.App.ActionBarDrawerToggle
        {
            MainActivity owner;

            public MyActionBarDrawerToggle(MainActivity activity, DrawerLayout layout, Android.Support.V7.Widget.Toolbar tbar, int openRes, int closeRes)
                : base(activity, layout, tbar, openRes, closeRes)
            {
                owner = activity;
            }
            public override void OnDrawerClosed(Android.Views.View drawerView)
            {
                if (!owner.mDrawerLayout.IsDrawerOpen(owner.mDrawerViewLeft) && !owner.mDrawerLayout.IsDrawerOpen(owner.mDrawerViewRight))
                    //owner.SupportActionBar.Title = owner.Title;
                    owner.toolbar.Title = owner.Title;
                owner.InvalidateOptionsMenu();
            }
            public override void OnDrawerOpened(Android.Views.View drawerView)
            {
                //if (owner.mDrawerViewLeft.Visibility == ViewStates.Visible)
                if (owner.mDrawerLayout.IsDrawerOpen(owner.mDrawerViewLeft))
                {
                    owner.mDrawerLayout.CloseDrawer(owner.mDrawerViewRight);
                    //owner.SupportActionBar.Title = owner.mDrawerTitleLeft;
                    owner.toolbar.Title = owner.mDrawerTitleLeft;
                }
                //else if (owner.mDrawerViewRight.Visibility == ViewStates.Visible)
                else if (owner.mDrawerLayout.IsDrawerOpen(owner.mDrawerViewRight))
                {
                    owner.mDrawerLayout.CloseDrawer(owner.mDrawerViewLeft);
                    //owner.SupportActionBar.Title = owner.mDrawerTitleRight;
                    owner.toolbar.Title = owner.mDrawerTitleRight;
                }

                owner.InvalidateOptionsMenu();
            }
        }
        public override bool OnCreateOptionsMenu(IMenu menu)
        {
            // Inflate the menu; this adds items to the action bar if it is present.            
            this.MenuInflater.Inflate(Resource.Menu.toolbar_top_menu, menu);
            return base.OnCreateOptionsMenu(menu);
            //return true;
        }
        /* Called whenever we call invalidateOptionsMenu() */
        public override bool OnPrepareOptionsMenu(IMenu menu)
        {
            // If the nav drawer is open, hide action items related to the content view
            //bool drawerOpenLeft = mDrawerLayout.IsDrawerOpen(mDrawerViewLeft);
            //bool drawerOpenRight = mDrawerLayout.IsDrawerOpen(mDrawerViewRight);
            //menu.FindItem(Resource.Id.action_websearch).SetVisible(!drawerOpenLeft);
            //menu.FindItem(Resource.Id.action_websearch).SetVisible(!drawerOpenRight);
            return base.OnPrepareOptionsMenu(menu);
        }
        void mDrawerViewLeft_NavigationItemSelected(object sender, NavigationView.NavigationItemSelectedEventArgs e)
        {
            switch (e.MenuItem.ItemId)
            {
                case Resource.Id.about:
                    Toast.MakeText(this, Resource.Id.about.ToString() + " clicked!", ToastLength.Long).Show();
                    break;
                case Resource.Id.feedback:
                    Toast.MakeText(this, Resource.Id.feedback.ToString() + " clicked!", ToastLength.Long).Show();
                    break;
                case Resource.Id.termsandcondition:
                    Toast.MakeText(this, Resource.Id.termsandcondition.ToString() + " clicked!", ToastLength.Long).Show();
                    break;
                case Resource.Id.credits:
                    Toast.MakeText(this, Resource.Id.credits.ToString() + " clicked!", ToastLength.Long).Show();
                    break;
            }

            //mDrawerLayout.CloseDrawer(mDrawerViewLeft);
            mDrawerLayout.CloseDrawers();
        }
        public override bool OnOptionsItemSelected(IMenuItem item)
        {
            // The action bar home/up action should open or close the drawer.
            // ActionBarDrawerToggle will take care of this.
            // This handles top ActionBar Toolbar icons

            if (mDrawerToggle.OnOptionsItemSelected(item))
            {
                return true;
            }

            // Handle action buttons
            switch (item.ItemId)
            {
                /*case Android.Resource.Id.Home:
                    mDrawerLayout.OpenDrawer(Android.Support.V4.View.GravityCompat.Start);
                    return true;*/
                case Resource.Id.locate_me:
                    locateMeMenuClicked = true;
                    _tracklineDisposed = false;
                    _routelineDisposed = false;
                    fromTopToolbar = true;
                    LocateMeHEREWorker();                    
                    break;
                case Resource.Id.track_me:
                    trackMeMenuClicked = true;
                    _tracklineDisposed = false;
                    _routelineDisposed = false;
                    TrackMeHERE();
                    break;
                case Resource.Id.navigate:
                    OnPositionChangedListener.navigateMeMenuClicked = true;
                    _tracklineDisposed = false;
                    _routelineDisposed = false;
                    NavigateMe();
                    break;
                default:
                    return base.OnOptionsItemSelected(item);
            }

            return true;
        }
        private static void AddHEREMarkerToMap(GeoPosition position)
        {
            if (locatemerequested || trackmerequested || OnPositionChangedListener.isOnJourney)
            {
                /*if (m_PositionMesh.IsVisible)
                {
                    //m_mapFragment.Map.RemoveMapObject(m_PositionAccuracyIndicator);
                    //m_mapFragment.Map.RemoveMapObject(m_PositionMarker);
                    m_mapFragment.Map.RemoveMapObject(m_PositionMesh);
                }*/

                //OnEngineInitListener.positionListener.setIndicator(position);

                //m_mapFragment.Map.AddMapObject(m_PositionAccuracyIndicator);
                //m_mapFragment.Map.AddMapObject(m_PositionMarker);
                //m_mapFragment.Map.AddMapObject(m_PositionMesh);

                //locateMeHEREMarkerId = locateMeMarker.Id;

                if (locatemerequested || trackmerequested)
                    //m_mapFragment.Map.SetCenter(position.Coordinate, Map.Animation.Linear, 17, (float)position.Heading, 45);
                    m_mapFragment.Map.SetCenter(position.Coordinate, Map.Animation.Linear);

                if (locatemerequested)
                    locatemerequested = false;
            }
            //else if (trackmerequested)
            //{
                //    locateMeHEREMarkerImage.SetImageResource(Resource.Drawable.appbar_track_me_light);
            //}
        }
        private static XDocument JsonToXml(string jsonString)
        {
            var stream = new MemoryStream(Encoding.ASCII.GetBytes(jsonString));
            var quotas = new XmlDictionaryReaderQuotas();
            var document = XDocument.Load(JsonReaderWriterFactory.CreateJsonReader(stream, quotas));

            return document;
        }        
        private void AddWayPointMarkerToMap(string marker, string title, string snippet, LatLng latlng, Image icon)
        {
            var coordinate = new GeoCoordinate(Convert.ToDouble(latlng.Latitude), Convert.ToDouble(latlng.Longitude));

            switch (marker)
            {
                case "from":
                    if (HEREFromMarker != null)
                        m_mapFragment.Map.RemoveMapObject(HEREFromMarker);
                    HEREFromMarker = new MapLabeledMarker(coordinate);

                    HEREFromMarker.SetTitle(title)
                                  .SetDescription(snippet)
                                  .SetIcon(icon)
                                  .SetCoordinate(coordinate);

                    HEREFromMarker.SetLabelText(m_mapFragment.Map.MapDisplayLanguage, title);
                    m_mapFragment.Map.AddMapObject(HEREFromMarker);
                    break;
                case "through":
                    if (HEREThroughMarker != null)
                        m_mapFragment.Map.RemoveMapObject(HEREThroughMarker);
                    HEREThroughMarker = new MapLabeledMarker(coordinate);

                    HEREThroughMarker.SetTitle(title)
                                     .SetDescription(snippet)
                                     .SetIcon(icon)
                                     .SetCoordinate(coordinate);

                    HEREThroughMarker.SetLabelText(m_mapFragment.Map.MapDisplayLanguage, title);
                    m_mapFragment.Map.AddMapObject(HEREThroughMarker);
                    break;
                case "destination":
                    if (HEREDestinationMarker != null)
                        m_mapFragment.Map.RemoveMapObject(HEREDestinationMarker);
                    HEREDestinationMarker = new MapLabeledMarker(coordinate);

                    HEREDestinationMarker.SetTitle(title)
                                         .SetDescription(snippet)
                                         .SetIcon(icon)
                                         .SetCoordinate(coordinate);

                    HEREDestinationMarker.SetLabelText(m_mapFragment.Map.MapDisplayLanguage, title);
                    m_mapFragment.Map.AddMapObject(HEREDestinationMarker);
                    break;
            }

            //locateMeMarkerId = locateMeMarker.Id;
        }
        //private void SetTitle (string title)
        //{
        //	this.Title = title;
        //	this.SupportActionBar.Title = title;
        //}
        protected override void OnTitleChanged(Java.Lang.ICharSequence title, Android.Graphics.Color color)
        {
            base.OnTitleChanged(title, color);
            //this.SupportActionBar.Title = title.ToString();
            this.toolbar.Title = title.ToString();
        }
        /*
        When using the ActionBarDrawerToggle, you must call it during
        onPostCreate() and onConfigurationChanged()...
        */
        protected override void OnPostCreate(Bundle savedInstanceState)
        {
            base.OnPostCreate(savedInstanceState);
            // Sync the toggle state after onRestoreInstanceState has occurred.
            mDrawerToggle.SyncState();

            //if (currentLocation != null)
                //locationManager.RemoveUpdates(this);
        }
        public override void OnConfigurationChanged(Configuration newConfig)
        {
            base.OnConfigurationChanged(newConfig);
            // Pass any configuration change to the drawer toggls
            mDrawerToggle.OnConfigurationChanged(newConfig);
        }
        private static void LoadHEREGeoJSONPlaces()
        {
            List<GeoJSONDataPlaces> list = new List<GeoJSONDataPlaces>(LoadGeoJSONPlacesData());
            
            HEREPlacesListData = new List<GeoJSONDataPlaces>();

            for (var i = 0; i < list.Count; i++)
            {
                if (list[i].Geometry.Type == GeoJSON.Net.GeoJSONObjectType.Point)
                {
                    var point = list[i].Geometry as GeoJSON.Net.Geometry.Point;                    
                    var name = list[i].Name;
                    var roadName = list[i].roadName;

                    HEREPlacesListData.Add(new GeoJSONDataPlaces()
                    {
                        Coordinate = new GeoCoordinate(point.Coordinates.Latitude, point.Coordinates.Longitude, (double)point.Coordinates.Altitude),
                        roadName = roadName,
                        Name = name
                    });
                }
            }
        }
        private static void UnloadHEREGeoJSONPlaces()
        {
            if (HEREPlacesListData.Count > 0)
            {
                HEREPlacesListData.Clear();
                HEREPlacesListData = null;
            }
        }
        private static void LoadHEREGeoJSONBuildings()
        {
            List<GeoJSONData> list = new List<GeoJSONData>(LoadGeoJSONBuildingsData());

            for (var i = 0; i < list.Count; i++)
            {
                if (list[i].Geometry.Type == GeoJSON.Net.GeoJSONObjectType.MultiPolygon)
                {
                    var polygon = list[i].Geometry as GeoJSON.Net.Geometry.MultiPolygon;

                    foreach (var polycoordinates in polygon.Coordinates)
                    {
                        foreach (var coordinates in polycoordinates.Coordinates)
                        {
                            //var coordinates = new List<Point3D>();
                            var Coord = new List<GeoCoordinate>();

                            foreach (var coordinate in coordinates.Coordinates)
                            {
                                Coord.Add(new GeoCoordinate(coordinate.Latitude, coordinate.Longitude));
                            }

                            geoPolygon = new GeoPolygon(Coord);

                            mapPolygon = new MapPolygon(geoPolygon);
                            mapPolygon.SetFillColor(0);
                            mapPolygon.SetOverlayType(MapOverlayType.RoadOverlay);

                            //var polygonMarker = new MapLabeledMarker(Coord[Coord.Count / 2]);                            
                            //polygonMarker.SetLabelText(m_mapFragment.Map.MapDisplayLanguage, list[i].Name);
                            //polygonMarker.SetOverlayType(MapOverlayType.AreaOverlay);

                            m_mapFragment.Map.AddMapObject(mapPolygon);
                            //m_mapFragment.Map.AddMapObject(polygonMarker);
                        }
                    }
                }
            }
        }
        private static void UnloadHEREGeoJSONBuildings()
        {
            if (mapPolygon != null)
                m_mapFragment.Map.RemoveMapObject(mapPolygon);
        }
        private static List<GeoJSONDataPlaces> LoadGeoJSONPlacesData()
        {
            List<GeoJSONDataPlaces> placesList = new List<GeoJSONDataPlaces>();
            List<string> nameList = new List<string>();
            List<string> roadNameList = new List<string>();

            Stream stream;
            StreamReader reader;
            
            string resPrefix = "JaFaFa.GeoJSON.";
            var assembly = IntrospectionExtensions.GetTypeInfo(typeof(MainPage)).Assembly;
            //stream = assembly.GetManifestResourceStream(resPrefix + "lagos-buildings-v89_XGR33Hed.geojson");
            stream = assembly.GetManifestResourceStream(resPrefix + "lagos-places-v90_emWo9u7W.geojson");            

            reader = new StreamReader(stream);
            var json = reader.ReadToEnd();
            reader.Close();
            //var data = JsonConvert.DeserializeObject<System.Object>(json);
            //var datastring = data.ToString();
            //var document = JsonToXml(datastring);
            var fdata = JsonConvert.DeserializeObject<FeatureCollection>(json);
            var jfdata = JObject.Parse(json);

            int count = 0;

            foreach (var feature in fdata.Features)
            {
                foreach (var property in feature.Properties)
                {   
                    property.Deconstruct(out var key, out var value);
                    
                    if (key == "names")
                    {
                        string name = value.ToString();

                        name = Regex.Replace(name, @"[^\w, ]\b(name|nameType|isPrimary|languageCode)\b", string.Empty, RegexOptions.IgnoreCase);

                        name = Regex.Replace(name, @"[^\w, ]*", string.Empty);                        

                        //name = string.Concat(Array.FindAll(name.ToCharArray(), Char.IsLetterOrDigit));

                        var namearray = name.Split(',');

                        name = namearray.First<string>();

                        name = name.Remove(0, 7);

                        nameList.Add(name);
                    }
                }
            }

            foreach (var feature in jfdata["features"])
            {
                /*
                if (feature["properties"]["names"] != null)
                {
                    foreach (var item in feature["properties"]["names"])
                    {   
                        if (item["name"] != null)
                        {
                            var check = item["name"];
                            nameList.Add(check.ToString());
                        }
                        else
                        {
                            nameList.Add(string.Empty);
                        }
                    }
                }
                else
                {
                    nameList.Add(string.Empty);
                }
                */
                if (feature["properties"]["locations"] != null)
                {
                    foreach (var item in feature["properties"]["locations"])
                    {
                        if (item["address"]["roadName"] != null)
                        {
                            var check = item["address"]["roadName"]["name"];
                            roadNameList.Add(check.ToString());
                        }
                        else
                        {
                            roadNameList.Add(string.Empty);
                        }
                    }
                }
                else
                {
                    roadNameList.Add(string.Empty);
                }
            }

            var i = 0;

            foreach (var item in fdata.Features)
            {
                placesList.Add(new GeoJSONDataPlaces()
                {
                    Geometry = item.Geometry,                    
                    roadName = roadNameList[i],
                    Name = nameList[i]
                });

                i++;
            }

            return placesList;
        }        
        private static List<GeoJSONData> LoadGeoJSONBuildingsData()
        {
            List<GeoJSONData> list = new List<GeoJSONData>();
            List<string> namelist = new List<string>();

            Stream stream;
            StreamReader reader;
            
            string resPrefix = "JaFaFa.GeoJSON.";
            var assembly = IntrospectionExtensions.GetTypeInfo(typeof(MainPage)).Assembly;
            stream = assembly.GetManifestResourceStream(resPrefix + "lagos-buildings-v89_XGR33Hed.geojson");
            //stream = assembly.GetManifestResourceStream(resPrefix + "lagos-places-v90_emWo9u7W.geojson");

            reader = new StreamReader(stream);
            var json = reader.ReadToEnd();
            reader.Close();
            //var data = JsonConvert.DeserializeObject<System.Object>(json);
            //var datastring = data.ToString();
            //var document = JsonToXml(datastring);
            var fdata = JsonConvert.DeserializeObject<FeatureCollection>(json);
            var jfdata = JObject.Parse(json);

            foreach (var feature in jfdata["features"])
            {
                if (feature["properties"]["names"] != null)
                {
                    foreach (var item in feature["properties"]["names"])
                    {
                        namelist.Add(item["name"].ToString());
                    }
                }
                else
                {
                    namelist.Add(string.Empty);
                }
            }

            var i = 0;

            foreach (var item in fdata.Features)
            {
                list.Add(new GeoJSONData()
                {
                    BB = item.BoundingBoxes,
                    CRS = item.CRS,
                    Geometry = item.Geometry,
                    Id = item.Id,
                    Properties = item.Properties,
                    Name = namelist[i]
                });
                
                i++;
            }

            return list;
        }
        private static void LoadHEREMarkers() 
        {
            string resPrefix = "JaFaFa.PoI.";
            var assembly = IntrospectionExtensions.GetTypeInfo(typeof(MainPage)).Assembly;
            Stream stream = assembly.GetManifestResourceStream(resPrefix + "PoIDB.xml");

            JaFaFa.PoI.PoI poi = new JaFaFa.PoI.PoI();

            PoIListData = poi.LoadPoIDatabaseMainPage(stream);

            AddHEREMarkersToMap(PoIListData, "poi");
        }
        private static void LoadHEREMarkersGas()
        {
            string resPrefix = "JaFaFa.PoI.";
            var assembly = IntrospectionExtensions.GetTypeInfo(typeof(MainPage)).Assembly;
            Stream stream = assembly.GetManifestResourceStream(resPrefix + "PoIDBGas.xml");

            JaFaFa.PoI.PoI poi = new JaFaFa.PoI.PoI();

            PoIListDataGas = poi.LoadPoIDatabaseGas(stream);

            AddHEREMarkersToMap(PoIListDataGas, "gas");
        }
        private static void AddHEREMarkersToMap(ObservableCollection<PoI.PoI.PoILongListData> PoIListData, string type)
        {
            if (PoIListData != null)
            {
                if (type != string.Empty)
                {
                    if (type == "gas")
                        HEREClusterGas = new ClusterLayer();
                    else
                        HERECluster = new ClusterLayer();

                    for (int i = 0; i < PoIListData.Count; i++)
                    {
                        var icon = GetIcon(PoIListData[i].PoILayer);
                        var coordinate = new GeoCoordinate(Convert.ToDouble(PoIListData[i].PoILat), Convert.ToDouble(PoIListData[i].PoILon));
                        var title = PoIListData[i].PoIName;
                        var description = PoIListData[i].PoIAddress;
                        
                        if (type == "gas")
                        {
                            var gas_type = PoIListData[i].PoIGasType;
                            var temperature = PoIListData[i].PoITemperature;
                            var density = PoIListData[i].PoIDensity;
                            var volume = PoIListData[i].PoIVolume;
                            var fuel_quality_rating = PoIListData[i].PoIFuelQualityRating;
                            var fuel_quantity_rating = PoIListData[i].PoIFuelQuantityRating;
                            var confidence_factor_one = PoIListData[i].PoIConfidenceFactorOne;
                            var confidence_factor_two = PoIListData[i].PoIConfidenceFactorTwo;
                            var datestamp = PoIListData[i].PoIDatestamp;
                            var timestamp = PoIListData[i].PoITimestamp;
                        }

                        //HEREMarker = new MapMarker();
                        HEREMarker = new MapLabeledMarker(coordinate);

                        HEREMarker.SetTitle(title)
                                  .SetDescription(description)
                                  .SetIcon(icon)
                                  .SetCoordinate(coordinate);

                        HEREMarker.SetLabelText(m_mapFragment.Map.MapDisplayLanguage, title);

                        HEREMarker.SetOverlayType(MapOverlayType.PoiOverlay);

                        if (type == "gas")
                            HEREClusterGas.AddMarker(HEREMarker);
                        else
                            HERECluster.AddMarker(HEREMarker);
                        
                        //m_mapFragment.Map.AddMapObject(HEREMarker);
                    }

                    if (type == "gas")
                        m_mapFragment.Map.AddClusterLayer(HEREClusterGas);
                    else
                        m_mapFragment.Map.AddClusterLayer(HERECluster);
                }
            }
        }
        /// <summary>
        /// Get gas station logo.
        /// </summary>
        static int GetGasStationRatingIconResourceId(string rating)
        {
            switch (rating)
            {
                case "1":
                    return Resource.Drawable.Number_1_icon_67x67;
                case "2":
                    return Resource.Drawable.Number_2_icon_67x67;
                case "3":
                    return Resource.Drawable.Number_3_icon_67x67;
                case "4":
                    return Resource.Drawable.Number_4_icon_67x67;
                case "5":
                    return Resource.Drawable.Number_5_icon_67x67;
            }

            return Resource.Drawable.Number_0_icon_67x67;
        }
        static int GetGasStationIconResourceId(string name)
        {
            switch (name)
            {
                case "Mobil":
                    return Resource.Drawable.Logo_Mobil_logo;
                case "M-R-S":
                    return Resource.Drawable.Logo_MRS_logo;
                case "Texaco":
                    return Resource.Drawable.Logo_Texaco_logo;
                case "Chevron":
                    return Resource.Drawable.Logo_Chevron_logo;
                case "OANDO":
                    return Resource.Drawable.Logo_Oando_logo;
                case "General":
                    return Resource.Drawable.Logo_General_logo;
                case "N-N-P-C":
                    return Resource.Drawable.Logo_NNPC_Logo;
                case "CONOIL":
                    return Resource.Drawable.Logo_Conoil_logo;
                case "Total":
                    return Resource.Drawable.Logo_Total_logo;
                case "Shell":
                    return Resource.Drawable.Logo_Shell_logo;
                case "Agip":
                    return Resource.Drawable.Logo_Agip_logo;
            }

            return Resource.Drawable.JaFaFa_icon; //JaFaFa Logo
        }
        /// <summary>
        /// Displaying Direction Hint.
        /// </summary>
        static int GetIconResourceId(string action, string direction)
        {
            bool roundabout = Check_For_Roundabout(action);

            var Id = 0;

            if (roundabout)
            {
                Id = Resource.Drawable.Signs_roundabout_86x86;
            }
            else if (!roundabout)
            {
                switch (direction)
                {
                    case "forward":
                        if (action == "arrive" || action == "depart")
                        {
                            Id = Resource.Drawable.Signs_arrive_depart_86x86;
                        }
                        else
                        {
                            Id = Resource.Drawable.Signs_motorway_86x86;
                        }
                        break;
                    case "bearRight":
                        Id = Resource.Drawable.Signs_bearRight_69x86;
                        break;
                    case "lightRight":
                        Id = Resource.Drawable.Signs_lightRight_86x86;
                        break;
                    case "right":
                        Id = Resource.Drawable.Signs_right_86x86;
                        break;
                    case "hardRight":
                        Id = Resource.Drawable.Signs_hardRight_86x86;
                        break;
                    case "uTurnRight":
                        Id = Resource.Drawable.Signs_uTurnRight_86x86;
                        break;
                    case "bearLeft":
                        Id = Resource.Drawable.Signs_bearLeft_69x86;
                        break;
                    case "lightLeft":
                        Id = Resource.Drawable.Signs_lightLeft_86x86;
                        break;
                    case "left":
                        Id = Resource.Drawable.Signs_left_86x86;
                        break;
                    case "hardLeft":
                        Id = Resource.Drawable.Signs_hardLeft_86x86;
                        break;
                    case "uTurnLeft":
                        Id = Resource.Drawable.Signs_uTurnLeft_86x86;
                        break;
                }
            }

            return Id;
        }
        /// <summary>
        /// Check for roundabout hints
        /// </summary>
        static bool Check_For_Roundabout(string action)
        {
            if (action == "leftRoundaboutExit1" || action == "leftRoundaboutExit2" || action == "leftRoundaboutExit3" || action == "leftRoundaboutExit4" ||
                        action == "leftRoundaboutExit5" || action == "leftRoundaboutExit6" || action == "leftRoundaboutExit7" || action == "leftRoundaboutExit8" ||
                        action == "leftRoundaboutExit9" || action == "leftRoundaboutExit10" || action == "leftRoundaboutExit11" || action == "leftRoundaboutExit12" ||
                        action == "rightRoundaboutExit1" || action == "rightRoundaboutExit2" || action == "rightRoundaboutExit3" || action == "rightRoundaboutExit4" ||
                        action == "rightRoundaboutExit5" || action == "rightRoundaboutExit6" || action == "rightRoundaboutExit7" || action == "rightRoundaboutExit8" ||
                        action == "rightRoundaboutExit9" || action == "rightRoundaboutExit10" || action == "rightRoundaboutExit11" || action == "rightRoundaboutExit12")
            {
                return true;
            }
            else return false;
        }
        private static Image GetIcon(string layer)
        {
            Image image = new Image();

            switch (layer)
            {
                case "Amusement_Center":
                    image.SetImageResource(Resource.Drawable.ferriswheel);
                    break;
                case "Bank":
                    image.SetImageResource(Resource.Drawable.bank);
                    break;
                case "ATM":
                    image.SetImageResource(Resource.Drawable.atm_2);
                    break;
                case "Airport":
                    image.SetImageResource(Resource.Drawable.airport);
                    break;
                case "Building":
                    image.SetImageResource(Resource.Drawable.office_building);
                    break;
                case "Cemetery":
                    image.SetImageResource(Resource.Drawable.cemetary);
                    break;
                case "Church":
                    image.SetImageResource(Resource.Drawable.church_2);
                    break;
                case "Mosque":
                    image.SetImageResource(Resource.Drawable.mosquee);
                    break;
                case "Place_of_Worship":
                    image.SetImageResource(Resource.Drawable.prayer);
                    break;
                case "Golf_Course":
                    image.SetImageResource(Resource.Drawable.golfing);
                    break;
                case "Hospital":
                    image.SetImageResource(Resource.Drawable.ambulance);
                    break;
                case "Landmark":
                    image.SetImageResource(Resource.Drawable.landmark);
                    break;
                case "Laundry_Services":
                    image.SetImageResource(Resource.Drawable.laundromat);
                    break;
                case "Library":
                    image.SetImageResource(Resource.Drawable.library);
                    break;
                case "Military_Installation":
                    image.SetImageResource(Resource.Drawable.military);
                    break;
                case "Post_Office":
                    image.SetImageResource(Resource.Drawable.postal);
                    break;
                case "Educational_Institution":
                    image.SetImageResource(Resource.Drawable.university);
                    break;
                case "Stadium":
                    image.SetImageResource(Resource.Drawable.stadium);
                    break;
                case "from":
                    image.SetImageResource(Resource.Drawable.Actions_flag_blue_icon);
                    break;
                case "through":
                    image.SetImageResource(Resource.Drawable.Actions_flag_green_icon);
                    break;
                case "destination":
                    image.SetImageResource(Resource.Drawable.checkered_flag_icon);
                    break;
                case "journey":
                    image.SetImageResource(Resource.Drawable.Location_Map_icon);
                    break;
                case "Gas_Station_Rated":
                    image.SetImageResource(Resource.Drawable.fillingstation);
                    break;
                default:
                    image.SetImageResource(Resource.Drawable.Map_Marker_Bubble_Pink_32_32_icon);
                    break;
            }

            return image;
        }        
        void SetupStreetButton()
        {
            var streetButton = FindViewById<Android.Widget.Button>(Resource.Id.streetButton);
            streetButton.Visibility = ViewStates.Invisible;
            //streetButton.Click += (sender, e) => { googleMap.MapType = GoogleMap.MapTypeNormal; };
            streetButton.Click += (sender, e) => { m_mapFragment.Map.SetMapScheme(Map.Scheme.CarnavTrafficDay); };
        }
        void SetupSatelliteButton()
        {
            var satelliteButton = FindViewById<Android.Widget.Button>(Resource.Id.satelliteButton);
            satelliteButton.Visibility = ViewStates.Invisible;
            //satelliteButton.Click += (sender, e) => { googleMap.MapType = GoogleMap.MapTypeSatellite; };
            satelliteButton.Click += (sender, e) => { m_mapFragment.Map.SetMapScheme(Map.Scheme.SatelliteDay); };
        }
        void SetupHybridButton()
        {
            var hybridButton = FindViewById<Android.Support.V7.Widget.AppCompatButton>(Resource.Id.hybridButton);
            hybridButton.Visibility = ViewStates.Invisible;
            //hybridButton.Click += (sender, e) => { googleMap.MapType = GoogleMap.MapTypeHybrid; };
            hybridButton.Click += (sender, e) => { m_mapFragment.Map.SetMapScheme(Map.Scheme.CarnavTrafficHybridDay); };
        }
        public override void OnRequestPermissionsResult(int requestCode, string[] permissions, [GeneratedEnum] Android.Content.PM.Permission[] grantResults)
        {
            Xamarin.Essentials.Platform.OnRequestPermissionsResult(requestCode, permissions, grantResults);

            if (requestCode == RequestLocationId)
            {
                if ((grantResults.Length == 1) && (grantResults[0] == (int)Permission.Granted))
                {
                    // Permissions granted - display a message.
                    Preferences.Set("Location", 1);
                }

                else
                {
                    // Permissions denied - display a message.
                    Preferences.Set("Location", 0);
                }
            }
            else
            {
                base.OnRequestPermissionsResult(requestCode, permissions, grantResults);
            }
        }
        protected override void OnStart()
        {
            base.OnStart();

            if ((int)Build.VERSION.SdkInt >= 23)
            {
                if (CheckSelfPermission(Android.Manifest.Permission.AccessFineLocation) != Permission.Granted)
                {
                    RequestPermissions(LocationPermissions, RequestLocationId);
                }
                else
                {
                    // Permissions already granted - display a message.
                    Toast.MakeText(this, "Permissions already granted", ToastLength.Long).Show();
                }
            }

            //locationManager = GetSystemService(LocationService) as LocationManager;
        }        
        protected override void OnPause()
        {
            base.OnPause();

            mSensorManager.UnregisterListener(OnEngineInitListener.sensorEventListener);

            if (m_mapFragment.Map != null && positionManager.IsActive)
            {
                positionManager.Stop();
            }
        }
        protected override void OnResume()
        {
            base.OnResume();

            // See: http://stackoverflow.com/questions/17337504/need-to-read-android-sensors-with-fixed-sampling-rate
            
            mSensorManager.RegisterListener(OnEngineInitListener.sensorEventListener, mAccelerometer, SensorDelay.Normal);
            mSensorManager.RegisterListener(OnEngineInitListener.sensorEventListener, mMagnetometer, SensorDelay.Normal);

            if (m_mapFragment.Map != null && !positionManager.IsActive)
            {
                positionManager.Start(PositioningManager.LocationMethod.GpsNetwork); // use gps plus cell and wifi
            }
        }
        protected override void OnDestroy()
        {
            if (m_mapFragment.Map != null)
            {
                positionManager.Stop();
                positionManager.RemoveListener(OnEngineInitListener.positionListener);
            }

            base.OnDestroy();
        }
        /// <summary>
        /// HERE Map Transformation
        /// </summary>
        public void OnMapTransformStart()
        {
            mTransforming = true;
        }
        public void OnMapTransformEnd(MapState mapState)
        {
            mTransforming = false;

            if (mPendingUpdate != null)
            {
                mPendingUpdate.Run();
                mPendingUpdate = null;
            }
        }
        // Implement the ViewHolder pattern: each ViewHolder holds references
        // to the UI components (ImageView and TextView) within the CardView 
        // that is displayed in a row of the RecyclerView:
        public class RouteItemViewHolder : RecyclerView.ViewHolder
        {
            public ImageView Icon { get; private set; }
            public TextView Instruction { get; private set; }

            // Get references to the views defined in the CardView layout.
            public RouteItemViewHolder(Android.Views.View itemView, Action<int> listener)
                : base(itemView)
            {
                // Locate and cache view references:                
                Icon = itemView.FindViewById<ImageView>(Resource.Id.routeImageView);
                Instruction = itemView.FindViewById<TextView>(Resource.Id.routeTextView);

                // Detect user clicks on the item view and report which item
                // was clicked (by layout position) to the listener:
                itemView.Click += (sender, e) => listener(base.LayoutPosition);
            }
        }
        // Adapter to connect the data set (gas station) to the RecyclerView: 
        public class GasAlertItemAdapter : RecyclerView.Adapter
        {
            AppCompatActivity activity;

            // Event handler for item clicks:
            //public event EventHandler<int> ItemClick;

            // Underlying data set (an address):
            List<GasAlertData> gasList;

            // Load the adapter with the data set (an address) at construction time:
            public GasAlertItemAdapter(List<GasAlertData> gList, AppCompatActivity m_activity)
            {
                activity = m_activity;
                gasList = gList;
            }
            // Create a new view (invoked by the layout manager): 
            public override RecyclerView.ViewHolder OnCreateViewHolder(ViewGroup parent, int viewType)
            {
                // Inflate the View for the route:
                Android.Views.View itemView = LayoutInflater.From(parent.Context).
                            Inflate(Resource.Layout.RouteLLS, parent, false);

                // Create a ViewHolder to find and hold these view references, and 
                // register OnClick with the view holder:
                RouteItemViewHolder vh = new RouteItemViewHolder(itemView, OnClick);
                return vh;
            }
            // Fill in the contents of the photo card (invoked by the layout manager):
            public override void OnBindViewHolder(RecyclerView.ViewHolder holder, int position)
            {
                RouteItemViewHolder vh = holder as RouteItemViewHolder;

                vh.ItemView.LongClick -= ItemView_LongClick;
                vh.ItemView.LongClick += ItemView_LongClick;

                //vh.Icon.SetImageResource(GetGasStationIconResourceId(gasList[position].Name));
                vh.Icon.SetImageResource(GetGasStationRatingIconResourceId(gasList[position].Rating));

                vh.Instruction.SetSingleLine(false);
                vh.Instruction.SetMinLines(1);
                vh.Instruction.SetMaxLines(3);
                vh.Instruction.SetWidth(320); // In px converted from dp
                vh.Instruction.TextSize = 14;
                vh.Instruction.Text = gasList[position].Address;
            }
            private void ItemView_LongClick(object sender, View.LongClickEventArgs e)
            {
                int position = Gas_Alert_RecyclerView.GetChildAdapterPosition((View)sender);                
                Gas_Alert_Dialog_Show(position);
            }
            public override int ItemCount
            {                
                get { return gasList.Count; }                
            }
            
            // Raise an event when the item-click takes place:
            void OnClick(int position)
            {
                //if (ItemClick != null)
                //ItemClick(this, position);
                //Toast.MakeText(activity, "Item with position: " + position.ToString() + "clicked", ToastLength.Long).Show();

                //Move camera to the selected maneuver instruction:                

                OnPositionChangedListener.XamarinEssentialsTextToSpeech(gasList[position].Address, 1f, 2f);
                
                positionManager.Stop();

                m_mapFragment.Map.SetCenter(gasList[position].Coord, Map.Animation.Linear);

                positionManager.Start(PositioningManager.LocationMethod.GpsNetwork);
            }
            private void Gas_Alert_Dialog_Show(int position)
            {
                LayoutInflater layoutInflater = LayoutInflater.From(activity);                
                
                Gas_Alert_Dialog_View = layoutInflater.Inflate(Resource.Layout.GasAlertDialog, null);
                
                Android.Support.V7.App.AlertDialog.Builder alertbuilder = new Android.Support.V7.App.AlertDialog.Builder(activity);
                
                alertbuilder.SetView(Gas_Alert_Dialog_View);
                
                alertbuilder.SetCancelable(false)
                    .SetPositiveButton("Go", delegate
                    {
                        //Route from current location to gas station
                        //Toast.MakeText(activity, "Item with position: " + position.ToString() + "clicked", ToastLength.Long).Show();
                        var currentCoord = positionManager.LastKnownPosition.Coordinate;

                        Navigation_Dialog_fromLatLng = new LatLng(currentCoord.Latitude, currentCoord.Longitude);
                        Navigation_Dialog_throughLatLng = new LatLng(currentCoord.Latitude, currentCoord.Longitude);
                        Navigation_Dialog_destinationLatLng = new LatLng(gasList[position].Coord.Latitude, gasList[position].Coord.Longitude);

                        //Update Header for RecyclerView
                        Navigation_Dialog_destinationLabel = gasList[position].Name;
                        var recyclerViewRouteHeaderText = activity.FindViewById<TextView>(Resource.Id.header_text);
                        recyclerViewRouteHeaderText.Text = "Heading to:\n" + Navigation_Dialog_destinationLabel;

                        //Get the new route
                        var getData = new GetDataHERE(activity);
                        getData.GetRouteHERE();
                        
                        //Re-start the Navigation manager with the new route
                        navigationManager.Stop();
                        getData.StartNavigation(currentCoord, currentCoord, gasList[position].Coord);
                    })
                    .SetNegativeButton("Cancel", delegate
                    {
                        //Do something                        
                        alertbuilder.Dispose();
                    });

                Android.Support.V7.App.AlertDialog dialog = alertbuilder.Create();
                
                dialog.Show();
            }
        }        

        // Adapter to connect the data set (route) to the RecyclerView: 
        public class RouteItemAdapter : RecyclerView.Adapter
        {
            // Event handler for item clicks:
            //public event EventHandler<int> ItemClick;

            // Underlying data set (a route):
            List<PoI.PoI.RouteManeuverData> mRouteManeuverData;

            AppCompatActivity activity;

            // Load the adapter with the data set (route) at construction time:
            public RouteItemAdapter(List<PoI.PoI.RouteManeuverData> routeManeuverData, AppCompatActivity m_activity)
            {
                mRouteManeuverData = routeManeuverData;
                activity = m_activity;
            }

            // Create a new view (invoked by the layout manager): 
            public override RecyclerView.ViewHolder OnCreateViewHolder(ViewGroup parent, int viewType)
            {                
                // Inflate the View for the route:
                Android.Views.View itemView = LayoutInflater.From(parent.Context).
                            Inflate(Resource.Layout.RouteLLS, parent, false);

                // Create a ViewHolder to find and hold these view references, and 
                // register OnClick with the view holder:
                RouteItemViewHolder vh = new RouteItemViewHolder(itemView, OnClick);
                
                return vh;
            }

            // Fill in the contents of the photo card (invoked by the layout manager):
            public override void OnBindViewHolder(RecyclerView.ViewHolder holder, int position)
            {
                //RouteItemViewHolder vh = holder as RouteItemViewHolder;
                var vh = (RouteItemViewHolder) holder;

                if (mRouteManeuverData[position].Direction != string.Empty)
                    vh.Icon.SetImageResource(GetIconResourceId(mRouteManeuverData[position].Action, mRouteManeuverData[position].Direction));
                else
                    vh.Icon.SetImageResource(GetIconResourceId(mRouteManeuverData[position].Action, string.Empty));
                
                vh.Instruction.SetSingleLine(false);
                vh.Instruction.SetMinLines(1);
                vh.Instruction.SetMaxLines(3);
                vh.Instruction.SetWidth(750);
                vh.Instruction.TextSize = 16;
                vh.Instruction.Text = mRouteManeuverData[position].Instruction;
            }
            public override int ItemCount
            {
                get { return mRouteManeuverData.Count; }
            }
            // Raise an event when the item-click takes place:
            void OnClick(int position)
            {
                // /Move camera to the selected maneuver instruction:

                //MoveToLocation(new LatLng(RouteManeuverData[position].PositionLatitude, RouteManeuverData[position].PositionLongitude));            
                m_mapFragment.Map.SetCenter(new GeoCoordinate(RouteManeuverData[position].PositionLatitude, RouteManeuverData[position].PositionLongitude), Map.Animation.None, 17, 90, 45);
                OnPositionChangedListener.XamarinEssentialsTextToSpeech(RouteManeuverData[position].Instruction, 1f, 2f);

                //Display info window
            }
        }        
        /// <summary>
        /// Positioning Manager
        /// </summary>
        private class OnPositionChangedListener : Java.Lang.Object, PositioningManager.IOnPositionChangedListener
        {
            //AndroidXMapFragment mapFragment;
            AppCompatActivity m_activity;
            public static bool navigateMeMenuClicked = false;
            public static bool isOnJourney = false;

            public OnPositionChangedListener(AndroidXMapFragment m_mapFragment, AppCompatActivity activity)
            {
                //mapFragment = m_mapFragment;
                m_activity = activity;
            }
            public void OnPositionFixChanged(PositioningManager.LocationMethod method, PositioningManager.LocationStatus status)
            {
                // Positioning method changed
                if (status == PositioningManager.LocationStatus.Available && method == PositioningManager.LocationMethod.GpsNetwork)
                {

                }
            }
            public void OnPositionUpdated(PositioningManager.LocationMethod method, GeoPosition position, bool isMapMatched)
            {
                try
                {
                    //MatchedGeoPosition mgp = (MatchedGeoPosition)position;
                }
                catch (System.Exception ex)
                {
                    Toast.MakeText(m_activity, ex.Message, ToastLength.Long).Show();
                }

                if (position.IsValid)
                {
                    // New position update received
                    // set the center only when the app is in the foreground
                    // to reduce CPU consumption

                    ///Note: For the position indicator to stay in the center of the map and illustrate real-time updates of the device 
                    ///position, it is necessary to update the map center whenever a new location update is received.
                    m_mapFragment.Map.SetTilt(45);
                    m_mapFragment.Map.SetZoomLevel(18);
                    m_mapFragment.Map.SetCenter(position.Coordinate, Map.Animation.None);                    

                    if (m_activity.Lifecycle.CurrentState.IsAtLeast(Lifecycle.State.Started))
                    {
                        // set custom position indicator and accuracy indicator
                        //setIndicator(position);
                        
                        if (initrequested && HEREpositionfixcount >= 6)
                        {
                            //mapFragment.Map.SetCenter(position.Coordinate, Map.Animation.Bow, mapFragment.Map.MaxZoomLevel * 0.80, 90, mapFragment.Map.MaxTilt);
                            m_mapFragment.Map.SetCenter(position.Coordinate, Map.Animation.Bow, 18, 90, 45);

                            positionManager.Stop();

                            initrequested = false;

                            //Toast.MakeText(m_activity, "Initial position fixed!", ToastLength.Long).Show();
                        }
                        if (locatemerequested && HEREpositionfixcount >= 6)
                        {
                            //mapFragment.Map.SetCenter(position.Coordinate, Map.Animation.Bow, mapFragment.Map.MaxZoomLevel * 0.80, 90, mapFragment.Map.MaxTilt);
                            m_mapFragment.Map.SetCenter(position.Coordinate, Map.Animation.Bow, 18, 90, 45);

                            HEREposition = position;

                            positionManager.Stop();

                            locatemerequested = false;

                            //Toast.MakeText(m_activity, "Located position fixed!", ToastLength.Long).Show();
                        }
                        if (trackmerequested)
                        {
                            AddHEREMarkerToMap(position);

                            //mapFragment.Map.SetCenter(position.Coordinate, Map.Animation.None, 18, 90, 45);
                        }
                        if (navigateMeMenuClicked)
                        {
                            navigateMeMenuClicked = false;
                        }
                        if (isOnJourney/* && position.LatitudeAccuracy < 50f*/)
                        {
                            try
                            {
                                Dynamic_Journey_Update_HERE_SDK(position);                                
                            }
                            catch (System.Exception ex)
                            {
                                //Xamarin.Forms.Application.Current.MainPage.DisplayAlert("JáFáFá System.Exception", ex.Message, "Ok");                                
                                Toast.MakeText(m_activity, "JáFáFá System Exception\n\n" + ex.Message, ToastLength.Long).Show();
                            }                            
                        }
                    }

                    HEREpositionfixcount++;
                }
                else return;
            }
            /// <summary>
            /// Display Direction Hint dynamically.
            /// </summary>
            private void Dynamic_Journey_Update_HERE_SDK(GeoPosition position)
            {
                var mIndex = 0; var lIndex = 0;
                bool mIndexFound = false; bool lIndexFound = false;
                var RoadName = string.Empty;

                if (positionManager.RoadElement != null)
                    RoadName = positionManager.RoadElement.RoadName;

                AddHEREMarkerToMap(position);
                m_mapFragment.Map.SetCenter(position.Coordinate, Map.Animation.None);

                //var coorbounds = new GeoBoundingBox(position.Coordinate, 0, 0);

                GeoBoundingBox currentpositionbounds;
                GeoBoundingBox maneuvershapebounds;
                GeoBoundingBox linkshapebounds;

                List<GeoCoordinate> geoCoordinatesList = new List<GeoCoordinate>();

                geoCoordinatesList.Add(position.Coordinate);

                currentpositionbounds = GeoBoundingBox.GetBoundingBoxContainingGeoCoordinates(geoCoordinatesList);

                //Find Maneuver
                for (int i = 0; i < RouteManeuverData.Count(); i++) //FLoop-1
                {
                    var Coord_TopLeft_NW = new GeoCoordinate(RouteManeuverData[i].LatitudeNW, RouteManeuverData[i].LongitudeNW);
                    var Coord_BottomRight_SE = new GeoCoordinate(RouteManeuverData[i].LatitudeSE, RouteManeuverData[i].LongitudeSE);

                    maneuvershapebounds = new GeoBoundingBox(Coord_TopLeft_NW, Coord_BottomRight_SE);
                    
                    if (maneuvershapebounds.Contains(position.Coordinate)/* && !pastManeuverIndexList.Contains(i)*/)
                    //if (maneuvershapebounds.Intersects(currentpositionbounds) && !pastManeuverIndexList.Contains(i))
                    {
                        mIndex = i; //Maneuver index has been found

                        /*Toast.MakeText(m_activity, "Maneuver Index Found!!" + "\n\n" +
                            "Maneuver Index : " + mIndex.ToString() + "\n\n" +
                            "Link Index : " + lIndex.ToString(), ToastLength.Short).Show();*/

                        if (!pastManeuverIndexList.Contains(0))
                        {
                            XamarinEssentialsTextToSpeech("start", 0f, 0f);
                        }

                        mIndexFound = true;
                        //m_mapFragment.Map.SetOrientation((float)RouteManeuverData[mIndex].StartAngle);
                        //m_mapFragment.Map.SetOrientation((float)position.Heading);
                        AddHEREMarkerToMap(position);
                        //m_mapFragment.Map.SetCenter(position.Coordinate, Map.Animation.None, 18, (float)RouteManeuverData[mIndex].StartAngle, 45);
                        //m_mapFragment.Map.SetCenter(position.Coordinate, Map.Animation.None, 18, (float)270, 45);
                        m_mapFragment.Map.SetCenter(position.Coordinate, Map.Animation.None);                        

                        //lastManeuverIndex = mIndex;

                        //Find Link
                        //if (mIndexFound && lastmIndex != mIndex)
                        if (mIndexFound)
                        {
                            for (int j = 0; j < RouteLinkData.Count(); j++) //FLoop-2
                            { //Search through all links in route for links belonging to maneuver mIndex
                                
                                if (RouteLinkData[j].ManeuverId == RouteManeuverData[mIndex].ManeuverId)
                                {
                                    geoCoordinatesList = new List<GeoCoordinate>();

                                    for (int k = 0; k < RouteLinkData[j].Shape.Count(); k++) //For each Link j found...
                                    {
                                        //...store the coordinates that make up the link's shape and then...
                                        var linkshapeboundcoord = new GeoCoordinate(RouteLinkData[j].Shape[k].Latitude, RouteLinkData[j].Shape[k].Longitude);

                                        //geoCoordinatesList contains the shape of Link j
                                        geoCoordinatesList.Add(linkshapeboundcoord);
                                    }

                                    //...create a bounding box using Link j's shape using geoCoordinatesList
                                    linkshapebounds = GeoBoundingBox.GetBoundingBoxContainingGeoCoordinates(geoCoordinatesList);

                                    //If the current position is within or intersects the link's bounding box...
                                    if (linkshapebounds.Contains(position.Coordinate))
                                    //if (linkshapebounds.Contains(currentpositionbounds))
                                    //if (linkshapebounds.Intersects(currentpositionbounds))
                                    {
                                        lIndex = j; //Link index has been found                                        
                                        /*Toast.MakeText(m_activity, "Link Index Found!!" + "\n\n" +
                                            "Maneuver Index : " + mIndex.ToString() + "\n\n" +
                                            "Link Index : " + lIndex.ToString(), ToastLength.Short).Show();*/
                                        lIndexFound = true;                                        

                                        /*if (lastLinkIndex == lIndex)
                                            lIndexFoundIterated = true;
                                        else
                                            lIndexFoundIterated = false;
                                            */

                                        //Display update
                                        if (lIndexFound) //Current link index must be > last link index
                                        {
                                            var Instruction = string.Empty; var NextRoadName = string.Empty; var Action = string.Empty;
                                            var Direction = string.Empty; var RemainDistance = 0.0; var RemainTime = 0.0;
                                            //var RoadName = string.Empty;
                                            var JamFactor = 0.0; var NextLink = 0.0;

                                            var currentlinklastpoint = RouteLinkData[lIndex].LastPoint;
                                            var currentlinkfirstpoint = RouteLinkData[lIndex].FirstPoint;
                                            var currentmaneuverlastpoint = RouteManeuverData[mIndex].LastPoint;
                                            var nextmaneuverhintindicator = currentmaneuverlastpoint - 2f;

                                            Toast.MakeText(m_activity, "Link LastPoint : " + currentlinklastpoint.ToString() + "\n\n" +
                                                "Maneuver LastPoint : " + currentmaneuverlastpoint.ToString() + "\n\n" +
                                                "Next Maneuver Indicator: " + nextmaneuverhintindicator.ToString(), ToastLength.Short).Show();

                                            if (currentlinkfirstpoint >= 0 && !pastManeuverIndexList.Contains(mIndex)) //Start of Route Leg Maneuver
                                            {
                                                //RoadName = RouteManeuverData[mIndex].RoadName;
                                                //RoadName = positionManager.RoadElement.RoadName;
                                                Instruction = RouteManeuverData[mIndex].Instruction;
                                                Action = RouteManeuverData[mIndex].Action;
                                                Direction = RouteManeuverData[mIndex].Direction;

                                                Toast.MakeText(m_activity, "NOT Almost at next maneuver", ToastLength.Short).Show();

                                                /// <summary>
                                                /// The number between 0.0 and 10.0 indicating the expected quality of travel,
                                                /// where 0 is high quality and 10.0 is poor quality or high level of traffic jam.
                                                /// -1.0 indicates that the service could not calculate Jam Factor.
                                                /// </summary>
                                                //NextRoadName = RouteManeuverData[mIndex].NextRoadName;
                                                //RemainDistance = RouteLinkData[lIndex].RemainDistance;
                                                //RemainTime = RouteLinkData[lIndex].RemainTime;
                                                //JamFactor = RouteLinkData[lIndex].JamFactor;
                                                //NextLink = RouteLinkData[lIndex].NextLink;

                                                //Display update
                                                DisplayUpdate(RoadName, Instruction, Action, Direction);

                                                //Announce Update
                                                if (!instructionannounced)
                                                {
                                                    XamarinEssentialsTextToSpeech(Instruction, 1f, 2f);
                                                    AnnounceGasStations(maneuvershapebounds);
                                                    instructionannounced = true;
                                                }                                                
                                            }                                            
                                            else if (currentlinklastpoint >= nextmaneuverhintindicator && !pastManeuverIndexList.Contains(mIndex+1))//End of maneuver. Hint next maneuver
                                            {
                                                if (mIndex < RouteManeuverData.Count)
                                                {
                                                    //RoadName = RouteManeuverData[mIndex+1].RoadName;
                                                    //RoadName = positionManager.RoadElement.RoadName;
                                                    Instruction = RouteManeuverData[mIndex + 1].Instruction;
                                                    Action = RouteManeuverData[mIndex + 1].Action;
                                                    Direction = RouteManeuverData[mIndex + 1].Direction;

                                                    Toast.MakeText(m_activity, "Almost at next maneuver", ToastLength.Short).Show();

                                                    //Display update
                                                    DisplayUpdate(RoadName, Instruction, Action, Direction);

                                                    //Announce Update
                                                    if (instructionannounced)
                                                    {
                                                        XamarinEssentialsTextToSpeech(Instruction, 1f, 2f);
                                                        //AnnounceGasStations(maneuvershapebounds);
                                                        instructionannounced = false;
                                                    }
                                                }
                                            }

                                            //...add the maneuver's current mIndex to the list of intersected maneuver indices
                                            if (!pastManeuverIndexList.Contains(mIndex))
                                                pastManeuverIndexList.Add(mIndex);

                                            break; //Index has been found. Break from FLoop-2
                                        } //END: if (lIndexFound)                                        
                                    } //END: if (linkshapebounds.Intersects(currentpositionbounds))
                                }//END: if (RouteLinkData[j].ManeuverId == RouteManeuverData[mIndex].ManeuverId)
                            } //END: FLoop-2
                            
                            break; //Maneuver has been found. Break from FLoop-1
                        }//END: if (mIndexFound)
                    }//END: if (maneuvershapebounds.Intersects(currentpositionbounds))
                }//END: FLoop-1
            }
            private void AnnounceGasStations(GeoBoundingBox maneuvershapebounds)
            {
                maneuverGasList = new List<GasAlertData>();

                //Implement popup
                for (int a = 0; a < PoIListDataGas.Count(); a++)
                {
                    var poilistdatagascoordinate = new GeoCoordinate(Convert.ToDouble(PoIListDataGas[a].PoILat), Convert.ToDouble(PoIListDataGas[a].PoILon));
                    List<GeoCoordinate> poilistdatagascoordinateList = new List<GeoCoordinate>();
                    poilistdatagascoordinateList.Add(poilistdatagascoordinate);
                    var poilistdatagascoordinatebounds = GeoBoundingBox.GetBoundingBoxContainingGeoCoordinates(poilistdatagascoordinateList);

                    if (maneuvershapebounds.Intersects(poilistdatagascoordinatebounds))
                    {
                        //Gas PoI is nearby...
                        //Add to list
                        maneuverGasList.Add(new GasAlertData()
                        {
                            Name = PoIListDataGas[a].PoIName,
                            Address = PoIListDataGas[a].PoIAddress,
                            Rating = PoIListDataGas[a].PoIFuelQualityRating,
                            Coord = poilistdatagascoordinate
                        });
                    }
                }

                if (maneuverGasList.Count() > 0)
                {
                    //Gas station alert!
                    XamarinEssentialsTextToSpeech("Gas station alert!", 1f, 2f);
                    SetupGasStationAlertList();
                    SetupGasStationAlertButton(maneuverGasList.Count());
                }
                else
                {
                    var gasAlertButton = m_activity.FindViewById<Android.Widget.ImageButton>(Resource.Id.gasAlertButton);
                    gasAlertButton.Visibility = ViewStates.Invisible;
                }
            }
            private void SetupGasStationAlertButton(int count)
            {
                var gasAlertButton = m_activity.FindViewById<Android.Widget.ImageButton>(Resource.Id.gasAlertButton);
                gasAlertButton.SetImageResource(GetImageResourceGas(count));
                gasAlertButton.Visibility = ViewStates.Visible;
                gasAlertButton.Click += (sender, e) => { SoroSoke(); };
            }
            private void SetupGasStationAlertList()
            {
                // Layout Manager Setup:
                // Use the built-in linear layout manager:
                Gas_Alert_RecyclerView_LayoutManager = new Android.Support.V7.Widget.LinearLayoutManager(m_activity);

                // Plug the layout manager into the RecyclerView:
                Gas_Alert_RecyclerView.SetLayoutManager(Gas_Alert_RecyclerView_LayoutManager);

                // Adapter Setup:
                // Create an adapter for the RecyclerView, and pass it the
                // data set (the gas stations) to manage:
                Gas_Alert_RecyclerView_Adapter = new GasAlertItemAdapter(maneuverGasList, m_activity);

                // Register the item click handler (below) with the adapter:
                //Gas_Alert_RecyclerView_Adapter.ItemClick += OnGasItemClick;

                // Plug the adapter into the RecyclerView:
                Gas_Alert_RecyclerView.SetAdapter(Gas_Alert_RecyclerView_Adapter);
            }
            private int GetImageResourceGas(int count)
            {
                switch (count)
                {
                    case 1:
                        return Resource.Drawable.number_1_red_100x100;
                    case 2:
                        return Resource.Drawable.number_2_red_100x100;
                    case 3:
                        return Resource.Drawable.number_3_red_100x100;
                    case 4:
                        return Resource.Drawable.number_4_red_100x100;
                    case 5:
                        return Resource.Drawable.number_5_red_100x100;
                    case 6:
                        return Resource.Drawable.number_6_red_100x100;
                    case 7:
                        return Resource.Drawable.number_7_red_100x100;
                    case 8:
                        return Resource.Drawable.number_8_red_100x100;
                    case 9:
                        return Resource.Drawable.number_9_red_100x100;
                    case 10:
                        return Resource.Drawable.number_10_red_100x100;
                    default:
                        return Resource.Drawable.number_0_red_100x100;
                }
            }
            private void SoroSoke()
            {
                if (Gas_Alert_RecyclerView.Visibility == ViewStates.Invisible)
                {
                    Gas_Alert_RecyclerView.Visibility = ViewStates.Visible;

                    for (int i = 0; i < maneuverGasList.Count(); i++)
                    {
                        //Announce the Gas PoI
                        XamarinEssentialsTextToSpeech(maneuverGasList[i].Address, 1f, 2f);
                    }
                }
                else if (Gas_Alert_RecyclerView.Visibility == ViewStates.Visible)
                {
                    Gas_Alert_RecyclerView.Visibility = ViewStates.Invisible;
                }
            }
            private void DisplayUpdate(string RoadName, string Instruction, string Action, string Direction)
            {
                HintPanelRoadName.Text = RoadName;
                HintPanelInstruction.Text = Instruction;
                HintPanelIcon.SetImageResource(GetIconResourceId(Action, Direction));

                HintPanelRoadName.TextAlignment = Android.Views.TextAlignment.TextStart;
                HintPanelInstruction.TextAlignment = Android.Views.TextAlignment.TextStart;

                HintPanelRoadName.SetSingleLine(true);
                HintPanelRoadName.SetMinLines(1);
                HintPanelRoadName.SetMaxLines(1);
                //HintPanelRoadName.SetWidth(400); // In px converted from dp
                HintPanelRoadName.TextSize = 18;

                HintPanelInstruction.SetSingleLine(false);
                HintPanelInstruction.SetMinLines(1);
                HintPanelInstruction.SetMaxLines(3);
                //HintPanelInstruction.SetWidth(225); // In px converted from dp
                HintPanelInstruction.TextSize = 16;
                
                m_mapFragment.View.SetPadding(0, HintPanel.Height, 0, 0);
                
                HintPanel.Visibility = ViewStates.Visible;
            }

            /// <summary>
            /// This method implements a priority based selection from the collection of installed voices.
            /// The first priority (prio equals 1) is given to a voice in the current UI language.
            /// Second priority is given to a British voice if available and third priority is an American English voice.
            /// </summary>
            public static async void XamarinEssentialsTextToSpeech(string text, float volume, float pitch)
            {                
                var locales = await TextToSpeech.GetLocalesAsync();                

                var ntext = NormalizeText(text);

                CancellationTokenSource cancellationToken = new CancellationTokenSource();

                // Grab the first locale of locale of choice
                //var currentlocale = locales.FirstOrDefault();                
                var currentlocale = locales.FirstOrDefault(y => string.Equals(y.Name, "English (Nigeria)"));

                /*Priority: Nigeria, UK, US English
                foreach(var locale in locales)
                {
                    if (locale.Name == "English (Nigeria)")
                        currentlocale = locale;
                    else if (locale.Name == "English (UK)")
                        currentlocale = locale;
                    else if (locale.Name == "English (US)")
                        currentlocale = locale;
                }
                */

                var settings = new SpeechOptions()
                {
                    Volume = volume,
                    Pitch = pitch,
                    Locale = currentlocale
                };
                
                await TextToSpeech.SpeakAsync(ntext, settings, cancelToken: cancellationToken.Token);

                // This method will block until utterance finishes.

                if (cancellationToken?.IsCancellationRequested ?? false)
                    cancellationToken.Cancel();
            }
            private static string NormalizeText(string text)
            {
                if (text.Contains(" m."))
                {
                    text = text.Replace(" m.", " Meters.");
                }
                if (text.Contains(" km."))
                {
                    text = text.Replace(" km.", " Kilometers.");
                }
                if (text.Contains("Ave.") || text.Contains("Ave/") || text.Contains(" Ave "))
                {
                    text = text.Replace("Ave.", "Avenue.");
                    text = text.Replace("Ave/", "Avenue/");
                    text = text.Replace(" Ave ", " Avenue ");
                }
                if (text.Contains(" Rd ") || text.Contains("Rd.") || text.Contains("Rd/"))
                {
                    text = text.Replace("Rd.", "Road.");
                    text = text.Replace(" Rd ", " Road ");
                    text = text.Replace("Rd/", "Road/");
                }
                if (text.Contains(" Ln ") || text.Contains("Ln.") || text.Contains("Ln/"))
                {
                    text = text.Replace(" Ln ", " Lane ");
                    text = text.Replace("Ln.", "Lane.");
                    text = text.Replace("Ln/", "Lane/");
                }
                if (text.Contains(" St ") || text.Contains("St.") || text.Contains("St/"))
                {
                    text = text.Replace(" St ", " Street ");
                    text = text.Replace("St.", "Street.");
                    text = text.Replace("St/", "Street/");
                }
                if (text.Contains(" Cres ") || text.Contains("Cres.") || text.Contains("Cres/"))
                {
                    text = text.Replace(" Cres ", " Crescent ");
                    text = text.Replace("Cres.", "Crescent.");
                    text = text.Replace("Cres/", "Crescent/");
                }
                if (text.Contains(" Brg ") || text.Contains("Brg.") || text.Contains("Brg/"))
                {
                    text = text.Replace(" Brg ", " Bridge ");
                    text = text.Replace("Brg.", "Bridge.");
                    text = text.Replace("Brg/", "Bridge/");
                }
                if (text.Contains("Expy"))
                {
                    text = text.Replace("Expy", "Express Way");
                }
                if (text.Contains(" Exp "))
                {
                    text = text.Replace(" Exp ", " Express ");
                }
                if (text.Contains("Int'l"))
                {
                    text = text.Replace("Int'l", "International");
                }
                if (text.Contains("1st exit"))
                {
                    text = text.Replace("1st exit", "First exit");
                }
                if (text.Contains("2nd exit"))
                {
                    text = text.Replace("2nd exit", "Second exit");
                }
                if (text.Contains("3rd exit"))
                {
                    text = text.Replace("3rd exit", "Third exit");
                }
                if (text.Contains("4th exit"))
                {
                    text = text.Replace("4th exit", "Fourth exit");
                }
                if (text.Contains("5th exit"))
                {
                    text = text.Replace("5th exit", "Fifth exit");
                }
                
                return text;
            }

            public static void RemovePolyLines(MapPolyline routelines)
            {
                m_mapFragment.Map.RemoveMapObject(routelines);
            }

            public void setIndicator(GeoPosition position)
            {
                //m_PositionMarker.SetCenter(position.Coordinate);
                //m_PositionMesh.SetAnchor(position.Coordinate);
                //m_PositionAccuracyIndicator.SetCenter(position.Coordinate);
                //m_PositionAccuracyIndicator.SetRadius(position.LatitudeAccuracy);
            }            
        }
        /// <summary>
        /// HERE Map Engine Initialization
        /// </summary>
        private class OnEngineInitListener : Java.Lang.Object, IOnEngineInitListener
        {
            AndroidXMapFragment mapFragment;
            AppCompatActivity activity;
            public static OnPositionChangedListener positionListener;
            public static OnRealisticViewListener realisticViewListener;
            public static OnSensorEventListener sensorEventListener;

            public OnEngineInitListener(AndroidXMapFragment m_mapFragment, AppCompatActivity m_appActivity)
            {
                mapFragment = m_mapFragment;
                activity = m_appActivity;
            }
            public void OnEngineInitializationCompleted(OnEngineInitListenerError error)
            {
                if (error == OnEngineInitListenerError.None)
                {
                    GeoCoordinate HEREANYWASHCoordinate = new GeoCoordinate(6.5138126, 3.3964648, 0.0);

                    // retrieve a reference of the map from the map fragment
                    mapFragment.Map.SetProjectionMode(Map.Projection.Globe);
                    mapFragment.Map.SetLandmarksVisible(true);
                    mapFragment.Map.SetExtrudedBuildingsVisible(true);
                    mapFragment.Map.SetCartoMarkersVisible(true);
                    mapFragment.Map.SetTrafficInfoVisible(true);
                    mapFragment.Map.SetMapScheme(Map.Scheme.CarnavDay);

                    //var Traffic = TrafficUpdater.Instance;
                    //var traffic = mapFragment.Map.MapTrafficLayer;

                    //Traffic.EnableUpdate(true);
                    //Traffic.SetRefreshInterval(3);
                    //traffic.SetDisplayFilter(TrafficEvent.Severity.VeryHigh);

                    /*By default traffic events are automatically loaded inside the viewport when traffic is
                     * enabled. You can also explicitly fetch traffic around a given set of geocoordinates by using
                     * TrafficUpdater.request(GeoCoordinate, int, Listener).
                     * To completely customize the traffic-updating implementation in your app, first turn off automatic traffic
                     * updates via the TrafficUpdater.disableAutoUpdate() method, then use the above mentioned
                     * method to fetch traffic only when it is required.*/

                    // Set the map center to the ANYWASH Nigeria UNILAG region
                    //mapFragment.Map.SetCenter(HEREANYWASHCoordinate, Map.Animation.None, mapFragment.Map.MaxZoomLevel * 0.80, 90, mapFragment.Map.MaxTilt);
                    mapFragment.Map.SetCenter(HEREANYWASHCoordinate, Map.Animation.None, 16, 90, 45);

                    //Load PoIs
                    LoadHEREMarkersGas();

                    // create a 3D mesh as position marker, since we can then use yaw to rotate
                    //CreatePositionMarkerMesh();

                    // create a custom position indicator dot
                    //m_PositionMarker = new MapCircle();
                    //m_PositionMarker.SetFillColor(Color.Argb(200, 0, 200, 0));
                    //m_PositionMarker.SetLineWidth(3);
                    //m_PositionMarker.SetLineColor(Color.Black);
                    //m_PositionMarker.SetRadius(3);

                    // create a custom accuracy indicator circle
                    //m_PositionAccuracyIndicator = new MapCircle();
                    //m_PositionAccuracyIndicator.SetFillColor(Color.Argb(70, 0, 200, 0)); // translucent
                    //m_PositionAccuracyIndicator.SetLineWidth(3);
                    //m_PositionAccuracyIndicator.SetLineColor(Color.Black);
                    //m_PositionAccuracyIndicator.SetRadius(20);
                    //m_PositionAccuracyIndicator.SetOverlayType(MapOverlayType.RoadOverlay); // put accuracy indicator behind buildings in render stack

                    // add this to the map. at this moment we still didn't define the position, we'll do this later on position updates
                    //mapFragment.Map.AddMapObject(m_PositionAccuracyIndicator);
                    //mapFragment.Map.AddMapObject(m_PositionMarker);

                    // Register positioning listener
                    positionListener = new OnPositionChangedListener(mapFragment, activity);
                    Java.Lang.Ref.WeakReference weakReference = new Java.Lang.Ref.WeakReference(positionListener);

                    /*
                    //METHOD  1
                    PositioningManager.Instance.AddListener(weakReference);

                    positionManager = PositioningManager.Instance;

                    if (positionManager.Start(PositioningManager.LocationMethod.GpsNetwork))
                    {
                        // Position updates started successfully.

                        // Display position indicator
                        mapFragment.PositionIndicator.SetVisible(true);
                        mapFragment.PositionIndicator.SetAccuracyIndicatorVisible(true);

                        var LastKnownPosition = positionManager.LastKnownPosition;

                        // set custom position indicator
                        //m_PositionMarker.SetCenter(LastKnownPosition.Coordinate);
                        //m_PositionAccuracyIndicator.SetCenter(LastKnownPosition.Coordinate);
                        
                        // ignoring altitude, since it would also set the imnage on this height
                        m_PositionMesh.SetAnchor(new GeoCoordinate(LastKnownPosition.Coordinate.Latitude, LastKnownPosition.Coordinate.Longitude));

                        initrequested = true;
                    }
                    */

                    //METHOD  2
                    var m_hereDataSource = LocationDataSourceHERE.Instance;

                    if (m_hereDataSource != null)
                    {
                        positionManager = PositioningManager.Instance;
                        positionManager.SetDataSource(m_hereDataSource);
                        positionManager.AddListener(weakReference);
                        positionManager.MapMatchingEnabled = true;

                        if (positionManager.Start(PositioningManager.LocationMethod.GpsNetwork))
                        {
                            // Position updates started successfully.
                            // Display position indicator
                            mapFragment.PositionIndicator.SetVisible(true);
                            mapFragment.PositionIndicator.SetAccuracyIndicatorVisible(true);

                            //var LastKnownPosition = positionManager.LastKnownPosition;

                            // ignoring altitude, since it would also set the imnage on this height
                            //m_PositionMesh.SetAnchor(new GeoCoordinate(LastKnownPosition.Coordinate.Latitude, LastKnownPosition.Coordinate.Longitude));
                            //m_PositionMesh.SetAnchor(new GeoCoordinate(positionManager.Position.Coordinate.Latitude, positionManager.Position.Coordinate.Longitude));

                            initrequested = true;

                            HEREpositionfixcount = 0;
                        }
                    }                   

                    //For Navigation, you need to assign the map instance to navigation manager
                    navigationManager = NavigationManager.Instance;
                    
                    //Set the map where the navigation will be performed
                    navigationManager.SetMap(mapFragment.Map);
                    
                    //Realistic view is disabled by default. To enable it...
                    navigationManager.SetRealisticViewMode(NavigationManager.RealisticViewMode.Day); //set the view mode
                    navigationManager.AddRealisticViewAspectRatio(NavigationManager.AspectRatio.AR4x3); //register the desired image aspect ratios

                    //Set guidance view to position with road ahead, tilt and zoomlevel was setup before manually
                    //choose other update modes for different position and zoom behavior                    
                    navigationManager.SetMapUpdateMode(NavigationManager.MapUpdateMode.RoadviewNozoom);

                    //Register realisticview listener
                    realisticViewListener = new OnRealisticViewListener(activity);
                    Java.Lang.Ref.WeakReference weakReference2 = new Java.Lang.Ref.WeakReference(realisticViewListener);
                    navigationManager.AddRealisticViewListener(weakReference2);

                    // Get RecyclerView
                    Gas_Alert_RecyclerView = activity.FindViewById<RecyclerView>(Resource.Id.gasAlertRecyclerView);

                    // Register Map Loader listener
                    OnMapLoaderListener mapLoaderListener = new OnMapLoaderListener();
                    MapLoader mapLoader = MapLoader.Instance;
                    mapLoader.AddListener(mapLoaderListener);

                    // Register Gesture listener
                    OnGestureListener mapGestureListener = new OnGestureListener(activity);
                    mapFragment.MapGesture.AddOnGestureListener(mapGestureListener, 100, true);

                    // Register Sensor listener
                    sensorEventListener = new OnSensorEventListener();
                }
                else
                {
                    // print error
                    //Log.Error("MapFragmentView", "ERROR: Cannot initialize Map Fragment: " + error.Details);

                    Toast.MakeText(activity, "Error : " + error.Name() + "\n\n" + error.Details, ToastLength.Long).Show();

                    var alertDialogBuilder = new Android.App.AlertDialog.Builder(activity);
                    alertDialogBuilder
                        .SetMessage("JáFáFá" + "\n\n" + "Error : " + error.Name() + "\n\n" + error.Details)
                        .SetTitle("JáFáFá")
                        .SetNegativeButton("Ok", delegate
                        {
                            alertDialogBuilder.Dispose();
                        });

                    var alertDialog = alertDialogBuilder.Create();
                    alertDialog.Show();
                }
            }

            // create a very simple triangle to render on the map. normally you would exported a 3D model and use a 3D loader for this
            private void CreatePositionMarkerMesh()
            {
                FloatBuffer buff = FloatBuffer.Allocate(9);

                buff.Put(0);
                buff.Put(0.5f);
                buff.Put(0);
                buff.Put(0.2f);
                buff.Put(-0.3f);
                buff.Put(0);
                buff.Put(-0.2f);
                buff.Put(-0.3f);
                buff.Put(0);

                IntBuffer vertIndicieBuffer = IntBuffer.Allocate(3);
                vertIndicieBuffer.Put(2);
                vertIndicieBuffer.Put(1);
                vertIndicieBuffer.Put(0);

                LocalMesh myMesh = new LocalMesh();
                myMesh.SetVertices(buff);
                myMesh.SetVertexIndices(vertIndicieBuffer);

                m_PositionMesh = new MapLocalModel();
                m_PositionMesh.SetMesh(myMesh);
                m_PositionMesh.SetScale(7.0f);
                m_PositionMesh.SetDynamicScalingEnabled(true); // keep size when zooming

                mapFragment.Map.AddMapObject(m_PositionMesh); // add mesh to map. we set position later when we have the first reliable information
            }
        }
        /// <summary>
        /// Map Loader
        /// </summary>
        private class OnMapLoaderListener : Java.Lang.Object, MapLoader.IListener
        {
            public void OnUninstallMapPackagesComplete(MapPackage rootMapPackage, MapLoader.ResultCode mapLoaderResultCode)
            {
            }
            public void OnProgress(int progressPercentage)
            {
            }
            public void OnPerformMapDataUpdateComplete(MapPackage rootMapPackage, MapLoader.ResultCode mapLoaderResultCode)
            {
            }
            public void OnInstallationSize(long diskSize, long networkSize)
            {
            }
            public void OnInstallMapPackagesComplete(MapPackage rootMapPackage, MapLoader.ResultCode mapLoaderResultCode)
            {
            }
            public void OnGetMapPackagesComplete(MapPackage rootMapPackage, MapLoader.ResultCode mapLoaderResultCode)
            {
            }
            public void OnCheckForUpdateComplete(bool updateAvailable, string currentMapVersion, string newestMapVersion, MapLoader.ResultCode mapLoaderResultCode)
            {
            }
        }

        /// <summary>
        /// Listen for Realistic View Events
        /// </summary>
        private class OnRealisticViewListener : NavigationManager.RealisticViewListener
        {
            AppCompatActivity activity;
            public OnRealisticViewListener(AppCompatActivity m_activity)
            {
                activity = m_activity;
            }
            public void OnRelisticViewHide()
            {
                Toast.MakeText(activity, "OnRelisticViewHide", ToastLength.Long).Show();
            }
            public override void OnRealisticViewNextManeuver(NavigationManager.AspectRatio ratio, Image junctionImage, Image signImageIn2D)
            {
                Toast.MakeText(activity, "OnRealisticViewNextManeuver Aspect Ratio:" + ratio.ToString(), ToastLength.Long).Show();
            }
            public override void OnRealisticViewShow(NavigationManager.AspectRatio ratio, Image junctionImage, Image signImageIn2D)
            {
                if (junctionImage.GetType() == Image.Type.Svg)
                {
                    // full size is too big (will cover most of the screen), so cut the size in half
                    Bitmap bmpImage = junctionImage.GetBitmap((int)(junctionImage.Width * 0.5), (int)(junctionImage.Height * 0.5));
                    
                    if (bmpImage != null)
                    {
                        //
                        // show bmpImage on-screen
                        Toast.MakeText(activity, "OnRealisticViewShow Aspect Ratio:", ToastLength.Long).Show();
                        //
                    }
                }
                else if (signImageIn2D.GetType() == Image.Type.Svg)
                {
                    // full size is too big (will cover most of the screen), so cut the size in half
                    Bitmap bmpImage = signImageIn2D.GetBitmap((int)(signImageIn2D.Width * 0.5), (int)(signImageIn2D.Height * 0.5));

                    if (bmpImage != null)
                    {
                        //
                        // show bmpImage on-screen
                        Toast.MakeText(activity, "OnRealisticViewShow Aspect Ratio:", ToastLength.Long).Show();
                        //
                    }
                }
            }
        }

        /// <summary>
        /// Route listener
        /// </summary>
        private class OnRouterListener : Java.Lang.Object, IRouterListener
        {
            public static MapRoute mapRoute;
            AppCompatActivity activity;

            public OnRouterListener(AppCompatActivity m_activity)
            {
                activity = m_activity;
            }
            public void OnCalculateRouteFinished(Java.Lang.Object results, Java.Lang.Object routingError)
            {
                /* Calculation is done. Let's handle the result */
                if (routingError == RoutingError.None)
                {
                    try
                    {
                        //Java.Util.ArrayList routeResults = (Java.Util.ArrayList)results;
                        JavaList routeResults = (JavaList)results;
                        RouteResult routeResult = routeResults.Get(0) as RouteResult;                        

                        if (routeResult != null)
                        {
                            /* Create a MapRoute so that it can be placed on the map */
                            mapRoute = new MapRoute(routeResult.Route);

                            /* Show the maneuver number on top of the route */
                            mapRoute.SetManeuverNumberVisible(true);

                            mapRoute.SetVisible(false);

                            /* Add the MapRoute to the map */
                            //parent.m_map.AddMapObject(parent.m_mapRoute);
                            m_mapFragment.Map.AddMapObject(mapRoute);

                            //Note: MapRoute.SetRenderType(RenderType) can only be called after the MapRoute is added to a Map.
                            mapRoute.SetRenderType(MapRoute.RenderType.Primary);

                            /*
                             * We may also want to make sure the map view is orientated properly
                             * so the entire route can be easily seen.
                             */
                            //GeoBoundingBox gbb = route.Route.BoundingBox;
                            //parent.m_map.ZoomTo(gbb, Map.Animation.None, Map.MovePreserveOrientation);
                            if (navigationManager.StartNavigation(mapRoute.Route) == NavigationManager.Error.None)
                            {
                                Toast.MakeText(activity, "NavigationManager started...", ToastLength.Long).Show();
                            }
                        }
                        else
                        {
                            //Toast.MakeText(parent.m_activity,
                            //        "Error:route results returned is not valid",
                            //        ToastLength.Long).Show();
                        }
                    }
                    catch (System.InvalidCastException e)
                    {
                        Toast.MakeText(activity, e.Message, ToastLength.Long).Show();
                    }
                    catch (System.Exception e)
                    {
                        Toast.MakeText(activity, e.Message, ToastLength.Long).Show();
                    }
                }
                else
                {
                    //Toast.MakeText(parent.m_activity,
                    //        "Error:route calculation returned error code: " + routingError,
                    //        ToastLength.Long).Show();
                }
            }

            public void OnProgress(int p0)
            {
                /* The calculation progress can be retrieved in this callback. */
            }
        }


        /// <summary>
        /// Gesture
        /// </summary>
        // Create a gesture listener and add it to the AndroidXMapFragment
        private class OnGestureListener : Java.Lang.Object, IMapGestureOnGestureListener
        {
            AppCompatActivity activity;
            IList<ViewObject> Objects;
            MapOverlay MapOverlay;
            public OnGestureListener(AppCompatActivity m_appActivity)
            {
                activity = m_appActivity;
            }

            public bool OnMapObjectsSelected(IList<ViewObject> objects)
            {
                Objects = objects;

                foreach (ViewObject viewObj in objects)
                {
                    if (viewObj.BaseType == ViewObject.Type.UserObject)
                    {
                        if (((MapObject)viewObj).GetType() == MapObject.Type.Marker)
                        {
                            // At this point we have the originally added
                            // map marker, so we can do something with it
                            // (like change the visibility, or more
                            // marker-specific actions)

                            ((MapMarker)viewObj).SetTitle("JáFáFá");
                            ((MapMarker)viewObj).SetDescription(CivicAddress);
                            ((MapMarker)viewObj).SetVisible(true);
                            //((MapMarker)viewObj).SetCoordinate(positionManager.Position.Coordinate);
                            //Image locateMeHEREMapMarkerImage = new Image();
                            //locateMeHEREMapMarkerImage.SetImageResource(Resource.Drawable.Map_Marker_Bubble_Pink_icon);
                            //((MapMarker)viewObj).SetIcon(locateMeHEREMapMarkerImage);

                            var mapFragment = (AndroidXMapFragment)activity.SupportFragmentManager.FindFragmentById(Resource.Id.map);

                            if (mapFragment != null)
                            {
                                if (MapOverlay != null)
                                {
                                    mapFragment.Map.RemoveMapOverlay(MapOverlay);
                                    MapOverlay = null;
                                }

                                var mapMarker = (MapMarker)viewObj;

                                View view = LayoutInflater.From(activity).Inflate(Resource.Layout.MarkerInfoWindow, null);

                                var infoWindowText = view.FindViewById<TextView>(Resource.Id.infoMessage);

                                infoWindowText.SetText(mapMarker.Description, TextView.BufferType.Normal);
                                infoWindowText.SetMaxLines(1);

                                var mapOverlay = new MapOverlay(view, mapMarker.Coordinate);

                                //dp to pixel
                                var paddingX = view.MeasuredWidth + (int)(35 * Resources.System.DisplayMetrics.Density);
                                var paddingY = view.MeasuredHeight + (int)(50 * Resources.System.DisplayMetrics.Density);

                                PointF pointF = new PointF(mapMarker.Icon.Width / 2f, 0.9f * mapMarker.Icon.Height);
                                //PointF pointF = new PointF(mapMarker.AnchorPoint.X + paddingX, mapMarker.AnchorPoint.Y + paddingY);
                                mapOverlay.SetAnchorPoint(pointF);

                                //mapOverlay.SetAnchorPoint(new PointF(80.0f, 150.0f));                                

                                MapOverlay = mapOverlay;

                                mapFragment.Map.AddMapOverlay(MapOverlay);
                            }
                        }
                    }
                }
                // return false to allow the map to handle this callback also
                return false;
            }
            public bool OnDoubleTapEvent(PointF pointF)
            {
                // return false to allow the map to handle this callback also
                return false;
            }
            public bool OnLongPressEvent(PointF pointF)
            {
                // return false to allow the map to handle this callback also
                return false;
            }
            public void OnLongPressRelease()
            {
                // return false to allow the map to handle this callback also
                return;
            }
            public void OnMultiFingerManipulationStart()
            {
                // return false to allow the map to handle this callback also
                return;
            }
            public void OnMultiFingerManipulationEnd()
            {
                // return false to allow the map to handle this callback also
                return;
            }
            public void OnPanStart()
            {
                // return false to allow the map to handle this callback also
                return;
            }
            public void OnPanEnd()
            {
                // return false to allow the map to handle this callback also
                return;
            }
            public void OnPinchLocked()
            {
                // return false to allow the map to handle this callback also
                return;
            }
            public bool OnPinchZoomEvent(float float1, PointF pointF)
            {
                // return false to allow the map to handle this callback also
                return false;
            }
            public bool OnRotateEvent(float float1)
            {
                // return false to allow the map to handle this callback also
                return false;
            }
            public void OnRotateLocked()
            {
                // return false to allow the map to handle this callback also
                return;
            }
            public bool OnTapEvent(PointF pointF)
            {
                if (Objects != null)
                {
                    foreach (ViewObject viewObj in Objects)
                    {
                        if (viewObj.BaseType == ViewObject.Type.UserObject)
                        {
                            if (((MapObject)viewObj).GetType() == MapObject.Type.Marker)
                            {
                                // At this point we have the originally added
                                // map marker, so we can do something with it
                                // (like change the visibility, or more
                                // marker-specific actions)

                                //MapMarker locateMeHEREMapMarker = (MapMarker)viewObj;

                                var mapFragment = (AndroidXMapFragment)activity.SupportFragmentManager.FindFragmentById(Resource.Id.map);

                                if (mapFragment != null && MapOverlay != null)
                                {
                                    mapFragment.Map.RemoveMapOverlay(MapOverlay);
                                    MapOverlay = null;
                                }
                            }
                        }
                    }
                }

                // return false to allow the map to handle this callback also
                //return super.OnTapEvent(pointF);                
                return false;
            }
            public bool OnTiltEvent(float float1)
            {
                // return false to allow the map to handle this callback also
                return false;
            }
            public bool OnTwoFingerTapEvent(PointF pointF)
            {
                // return false to allow the map to handle this callback also
                return false;
            }
        }
        /// <summary>
        /// Sensors
        /// </summary>
        private class OnSensorEventListener : Java.Lang.Object, ISensorEventListener
        {
            public void OnSensorChanged(SensorEvent sensorEvent)
            {
                float alpha = (float)0.8;

                if (sensorEvent.Sensor.StringType == Sensor.StringTypeAccelerometer &&
                    sensorEvent.Sensor.StringType == Sensor.StringTypeMagneticField)
                {
                    // Isolate the force of gravity with the low-pass filter. See Android documentation for details:
                    // http://developer.android.com/guide/topics/sensors/sensors_motion.html#sensors-motion-accel

                    mGravity[0] = alpha * mGravity[0] + (1 - alpha) * sensorEvent.Values[0];
                    mGravity[1] = alpha * mGravity[1] + (1 - alpha) * sensorEvent.Values[1];
                    mGravity[2] = alpha * mGravity[2] + (1 - alpha) * sensorEvent.Values[2];

                    mGeomagnetic = (float[])sensorEvent.Values.ToArray().Clone();
                }                

                if (mGravity != null && mGeomagnetic != null)
                {
                    float[] R = new float[9];
                    float[] I = new float[9];

                    if (SensorManager.GetRotationMatrix(R, I, mGravity, mGeomagnetic))
                    {
                        float[] mOrientation = new float[3];

                        // mOrientation contains: azimuth, pitch and roll
                        SensorManager.GetOrientation(R, mOrientation);

                        mAzimuth = (float)Java.Lang.Math.ToDegrees(mOrientation[0]);
                        //float mPitch = (float)Java.Lang.Math.ToDegrees(mOrientation[1]);
                        //float mRoll = (float)Java.Lang.Math.ToDegrees(mOrientation[2]);
                        //float mInclination = (float)Java.Lang.Math.ToDegrees(SensorManager.Inclination(I));

                        if (mAzimuth < 0.0f)
                        {
                            mAzimuth += 360.0f;
                        }

                        Log.Verbose("JáFáFá", "Rotate to " + mAzimuth);

                        // set yaw of our 3D position indicator to indicate compass direction
                        if (m_PositionMesh != null)
                            m_PositionMesh.SetYaw(-mAzimuth);  // Think about animation and less updates here in production environments
                    }
                }
            }
            public void OnAccuracyChanged(Sensor sensor, SensorStatus status)
            {
                Log.Debug("JáFáFá", "Accuracy changed for " + sensor.Name + " to " + status);
            }
        }
        private void TrackMeHERE()
        {
            var trackMenuItem = this.toolbar.Menu.GetItem(1);

            if (!positionManager.IsActive && trackmerequested == false)
            {
                if (positionManager.Start(PositioningManager.LocationMethod.GpsNetwork))
                {
                    // Position updates started successfully.

                    trackmerequested = true;

                    //AddHEREMarkerToMap(positionManager.Position.Coordinate, this);
                    //AddHEREMarkerToMap(positionManager.LastKnownPosition);
                    AddHEREMarkerToMap(positionManager.Position);

                    //Set toolbar icon                
                    trackMenuItem.SetIcon(Resource.Drawable.appbar_track_me_light);
                }
            }
            else if (positionManager.IsActive)
            {
                if (trackmerequested == true)
                {
                    // Position updates started successfully.
                    //Stop requesting location updates
                    positionManager.Stop();

                    trackmerequested = false;

                    trackMenuItem.SetIcon(Resource.Drawable.appbar_track_me);
                }
                else if (trackmerequested == false)
                {
                    //Stop requesting location updates
                    positionManager.Stop();

                    //Start requesting location updates
                    positionManager.Start(PositioningManager.LocationMethod.GpsNetwork);                    

                    trackmerequested = true;

                    //AddHEREMarkerToMap(positionManager.Position.Coordinate, this);
                    //AddHEREMarkerToMap(positionManager.LastKnownPosition);
                    AddHEREMarkerToMap(positionManager.Position);

                    //Set toolbar icon                
                    trackMenuItem.SetIcon(Resource.Drawable.appbar_track_me_light);
                }
            }
        }        
        private void NavigateMe()
        {
            LayoutInflater layoutInflater = LayoutInflater.From(this);

            NavigationDialogView = layoutInflater.Inflate(Resource.Layout.NavigationDialog, null);

            Android.Support.V7.App.AlertDialog.Builder alertbuilder = new Android.Support.V7.App.AlertDialog.Builder(this);

            alertbuilder.SetView(NavigationDialogView);

            lowertoolbar.Menu.GetItem(0).SetVisible(false);
            lowertoolbar.Menu.GetItem(1).SetVisible(false);

            ClearUpRouteGarbage();

            alertbuilder.SetCancelable(false)
            .SetPositiveButton("Get Route", delegate
            {
                if (Navigation_Dialog_fromLatLng == null)
                    Toast.MakeText(this, "From cannot be empty", ToastLength.Short).Show();
                else if (Navigation_Dialog_destinationLatLng == null)
                    Toast.MakeText(this, "Destination cannot be empty", ToastLength.Short).Show();
                else
                {
                    HideSoftKeyboard(NavigationDialogView);
                    var getData = new GetDataHERE(this);
                    getData.GetRouteHERE();
                }
            })
            .SetNegativeButton("Cancel", delegate
            {
                //Do something
                OnPositionChangedListener.navigateMeMenuClicked = false;
                alertbuilder.Dispose();
            });

            Android.Support.V7.App.AlertDialog dialog = alertbuilder.Create();
            dialog.Show();

            InitializeNavigateMe();

            //Toast.MakeText(this, Resource.Id.about.ToString() + " clicked!", ToastLength.Long).Show();
        }
        private void ClearUpRouteGarbage()
        {
            /*
            if (_routelineDisposed == false && _routelines.Count > 0)
            {
                //polOptions = new PolylineOptions();

                for (var i = 0; i < _routelines.Count; i++)
                {
                    _routelines[i].Remove();
                }
            }

            _routelineDisposed = true;

            if (fromMarker != null)
                fromMarker.Remove();
            if (throughMarker != null)
                throughMarker.Remove();
            if (destinationMarker != null)
                destinationMarker.Remove();
                */
        }
        private void InitializeNavigateMe()
        {
            Navigation_Dialog_fromUserData = NavigationDialogView.FindViewById<AutoCompleteTextView>(Resource.Id.fromText);
            Navigation_Dialog_throughUserData = NavigationDialogView.FindViewById<AutoCompleteTextView>(Resource.Id.throughText);
            Navigation_Dialog_destinationUserData = NavigationDialogView.FindViewById<AutoCompleteTextView>(Resource.Id.destinationText);

            Navigation_Dialog_fromUserData.TextChanged += Navigation_Dialog_Userdata_TextChanged;
            Navigation_Dialog_throughUserData.TextChanged += Navigation_Dialog_Userdata_TextChanged;
            Navigation_Dialog_destinationUserData.TextChanged += Navigation_Dialog_Userdata_TextChanged;

            Navigation_Dialog_fromUserData.ItemClick += Navigation_Dialog_UserData_ItemClick;
            Navigation_Dialog_throughUserData.ItemClick += Navigation_Dialog_UserData_ItemClick;
            Navigation_Dialog_destinationUserData.ItemClick += Navigation_Dialog_UserData_ItemClick;

            Navigation_Dialog_fromLabel = null; Navigation_Dialog_fromLatLng = null;
            Navigation_Dialog_throughLabel = null; Navigation_Dialog_throughLatLng = null;
            Navigation_Dialog_destinationLabel = null; Navigation_Dialog_destinationLatLng = null;

            HintPanel = FindViewById<GridLayout>(Resource.Id.routeTitlePanel);
            HintPanelRoadName = FindViewById<TextView>(Resource.Id.routeRoadNameText);
            HintPanelIcon = FindViewById<ImageView>(Resource.Id.routeTitlePanelImageView);
            HintPanelInstruction = FindViewById<TextView>(Resource.Id.routeTitlePanelText);

            fromCurrentLocationIcon = NavigationDialogView.FindViewById<ImageView>(Resource.Id.fromCurrentLocationImageView);
            throughCurrentLocationIcon = NavigationDialogView.FindViewById<ImageView>(Resource.Id.throughCurrentLocationImageView);
            destinationCurrentLocationIcon = NavigationDialogView.FindViewById<ImageView>(Resource.Id.destinationCurrentLocationImageView);
            
            fromCurrentLocationIcon.Click += FromCurrentLocationIcon_Click;
            throughCurrentLocationIcon.Click += ThroughCurrentLocationIcon_Click;
            destinationCurrentLocationIcon.Click += DestinationCurrentLocationIcon_Click;
        }
        private void FromCurrentLocationIcon_Click(object sender, EventArgs e)
        {
            boxclicked = 1;
            LocateMeHEREWorker();
        }
        private void ThroughCurrentLocationIcon_Click(object sender, EventArgs e)
        {
            boxclicked = 2;
            LocateMeHEREWorker();
        }
        private void DestinationCurrentLocationIcon_Click(object sender, EventArgs e)
        {
            boxclicked = 3;
            LocateMeHEREWorker();
        }
        private void LocateMeHEREWorker()
        {
            if (positionManager.Start(PositioningManager.LocationMethod.GpsNetwork))
            {
                locatemerequested = true;

                HEREpositionfixcount = 0;

                System.ComponentModel.BackgroundWorker LocateMeHEREWorker = new System.ComponentModel.BackgroundWorker();
                LocateMeHEREWorker.DoWork += LocateMeHEREWorker_DoWork;
                LocateMeHEREWorker.RunWorkerCompleted += LocateMeHEREWorker_RunWorkerCompleted;
                LocateMeHEREWorker.RunWorkerAsync();
            }
        }
        private void LocateMeHEREWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            while (locatemerequested)
            {
                //Until position is fixed and updated
            }
        }
        private void LocateMeHEREWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            GetAddress(HEREposition);
        }
        public void GetAddress(GeoPosition position)
        {
            var coordinate = position.Coordinate;

            string URL_HERE_REVERSE_GEOCODE_v1 = HERE_Reverse_Geocode_API_Base_URL_v1 + HERE_Reverse_Geocode_API_Path_v1 +
                HERE_Reverse_Geocode_API_Resource_v1 + "?" + "at=" +
                coordinate.Latitude.ToString() + "%2C" + coordinate.Longitude.ToString() + "&" + "lang=en-US" + "&" +
                HERE_API_apiKey;

            Uri HERE_URI_REVERSE_GEOCODE = new Uri(URL_HERE_REVERSE_GEOCODE_v1, UriKind.Absolute);

            returntype = "revgeocode";

            //Download the XML string to HERE_XML and Create a Parseable stream to HERE_READER
            var getData = new GetDataHERE(this);
            getData.DownloadDataHERE(HERE_URI_REVERSE_GEOCODE, returntype, position);
            //HEREDownloadData(HERE_URI_REVERSE_GEOCODE, returntype, position);
        }
        private void Navigation_Dialog_Userdata_TextChanged(object sender, Android.Text.TextChangedEventArgs e)
        {
            if (e.Text.Count() > 2)
            {
                if ((Navigation_Dialog_fromUserData.HasFocus && Navigation_Dialog_fromUserData.IsPopupShowing == false) ||
                    (Navigation_Dialog_throughUserData.HasFocus && Navigation_Dialog_throughUserData.IsPopupShowing == false) ||
                    (Navigation_Dialog_destinationUserData.HasFocus && Navigation_Dialog_destinationUserData.IsPopupShowing == false))
                {
                    GetLocationAsync(e.Text.ToString().ToLower());
                }
            }
        }
        private void GetLocationAsync(string freestylesearchterm)
        {
            var getData = new GetDataHERE(this);

            if (SearchPoIDB(freestylesearchterm, PoIListData) && !menuactive) //check PoI database
            {
                //in PoI database                
                getData.NavigationDialogListHERE(geocodeResult);
            }
            else if (SearchHEREPlacesDataLayer(freestylesearchterm, HEREPlacesListData) && !menuactive) //check HERE Places Data Layer
            {
                //in HERE Places Data Layer
                getData.NavigationDialogListHERE(geocodeResult);
            }
            else //check online
            {
                // Android 4.1 or higher, Xamarin.Android 6.1 or higher
                HttpClient HEREGeocodeQuery = new HttpClient(new Xamarin.Android.Net.AndroidClientHandler());
                //HttpClient HEREGeocodeQuery = new HttpClient(); // start loading XML-data

                // Limit the max buffer size for the response so we don't get overwhelmed
                //HEREGeocodeQuery.MaxResponseContentBufferSize = 256000;
                //HEREGeocodeQuery.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0 (compatible; MSIE 10.0; Windows NT 6.2; WOW64; Trident/6.0)");
                //HEREGeocodeQuery.DefaultRequestHeaders.Add("User-Agent", "Anything");
                //HEREGeocodeQuery.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                //HEREGeocodeQuery.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/xml"));

                string URL_HERE_GEOCODE_v1 = HERE_Geocode_API_Base_URL_v1 + HERE_Geocode_API_Path_v1 +
                    HERE_Geocode_API_Resource_v1 + "?" + "q=" + freestylesearchterm + "&" +
                    HERE_API_apiKey;

                //await Xamarin.Forms.Application.Current.MainPage.DisplayAlert("GET Request", URL_HERE_GEOCODE_v1, "Ok");

                Uri HERE_URI_GEOCODE = new Uri(URL_HERE_GEOCODE_v1, UriKind.Absolute);

                returntype = "geocode";

                //Download the XML string to HERE_XML and Create a Parseable stream to HERE_READER
                getData.DownloadDataHERE(HERE_URI_GEOCODE, returntype, null);
                //HEREDownloadData(HERE_URI_GEOCODE, returntype, null);
            }            
        }

        private bool SearchPoIDB(string freestylesearchterm, ObservableCollection<PoI.PoI.PoILongListData> record)
        {
            geocodeResult = new List<PoI.PoI.GeocodeData>();
            Nextsearchterm = false;

            for (var i = 0; i < record.Count; i++)
            {
                if (record[i].PoIName.ToLower().Contains(freestylesearchterm))
                {
                    if (geocodeResult.Count == 0) //Empty list
                    {
                        geocodeResult.Add(new PoI.PoI.GeocodeData()
                        {
                            LocName = record[i].PoIName,
                            LocLat = record[i].PoILat,
                            LocLon = record[i].PoILon
                        });
                    }
                    else if (geocodeResult.Count > 0) //Non-empty list
                    {
                        for (var j = 0; j < geocodeResult.Count; j++) //check to see if search result already in list
                        {
                            if (geocodeResult[j].LocName.Contains(record[i].PoIName) == false) //not already in list
                            {
                                geocodeResult.Add(new PoI.PoI.GeocodeData()
                                {
                                    LocName = record[i].PoIName,
                                    LocLat = record[i].PoILat,
                                    LocLon = record[i].PoILon
                                });
                                break; //exit first outer for-loop
                            }
                        }
                    }
                }
            }

            if (geocodeResult.Count > 0)
                return true;
            else
                return false;
        }
        private bool SearchHEREPlacesDataLayer(string freestylesearchterm, List<GeoJSONDataPlaces> record)
        {
            geocodeResult = new List<PoI.PoI.GeocodeData>();            
            Nextsearchterm = false;

            for (var i = 0; i < record.Count; i++)
            {
                if (record[i].Name.ToLower().Contains(freestylesearchterm))
                {
                    if (geocodeResult.Count == 0) //Empty list
                    {
                        geocodeResult.Add(new PoI.PoI.GeocodeData()
                        {
                            LocName = record[i].Name + "," + record[i].roadName,
                            LocLat = record[i].Coordinate.Latitude.ToString(),
                            LocLon = record[i].Coordinate.Longitude.ToString()
                        });
                    }
                    else if (geocodeResult.Count > 0) //Non-empty list
                    {
                        for (var j = 0; j < geocodeResult.Count; j++) //check to see if search result already in list
                        {
                            if (geocodeResult[j].LocName.Contains(record[i].Name + "," + record[i].roadName) == false) //not already in list
                            {
                                geocodeResult.Add(new PoI.PoI.GeocodeData()
                                {
                                    LocName = record[i].Name + "," + record[i].roadName,
                                    LocLat = record[i].Coordinate.Latitude.ToString(),
                                    LocLon = record[i].Coordinate.Longitude.ToString()
                                });
                                break; //exit first outer for-loop
                            }
                        }
                    }
                }
            }

            if (geocodeResult.Count > 0)
                return true;
            else
                return false;
        }

        private void Navigation_Dialog_UserData_ItemClick(object sender, AdapterView.ItemClickEventArgs e)
        {
            if (Navigation_Dialog_fromSelectedItem)
            {
                Navigation_Dialog_fromLabel = geocodeResult[e.Position].LocName;

                Navigation_Dialog_fromLatLng = new LatLng(Convert.ToDouble(geocodeResult[e.Position].LocLat),
                    Convert.ToDouble(geocodeResult[e.Position].LocLon));

                Navigation_Dialog_fromSelectedItem = false;

                //Navigation_Dialog_fromUserData.ClearFocus();

                //Navigation_Dialog_throughUserData.RequestFocus();

                //Navigation_Dialog_throughUserData.DismissDropDown();

                //Navigation_Dialog_fromUserData.DismissDropDown();

                HideSoftKeyboard(NavigationDialogView);

                //Xamarin.Forms.Application.Current.MainPage.DisplayAlert("Position: " + e.Position.ToString(),
                //    "Label/Selected: " + Navigation_Dialog_fromLabel + "/" + Navigation_Dialog_fromSelectedItem.ToString(),
                //    "Geolocation: " + Navigation_Dialog_fromLatLng.ToString());
            }
            else if (Navigation_Dialog_throughSelectedItem)
            {
                Navigation_Dialog_throughLabel = geocodeResult[e.Position].LocName;

                Navigation_Dialog_throughLatLng = new LatLng(Convert.ToDouble(geocodeResult[e.Position].LocLat),
                    Convert.ToDouble(geocodeResult[e.Position].LocLon));

                Navigation_Dialog_throughSelectedItem = false;

                //Navigation_Dialog_throughUserData.ClearFocus();

                //Navigation_Dialog_destinationUserData.RequestFocus();

                //Navigation_Dialog_throughUserData.DismissDropDown();

                HideSoftKeyboard(NavigationDialogView);
            }
            else if (Navigation_Dialog_destinationSelectedItem)
            {
                Navigation_Dialog_destinationLatLng = new LatLng(Convert.ToDouble(geocodeResult[e.Position].LocLat),
                    Convert.ToDouble(geocodeResult[e.Position].LocLon));

                Navigation_Dialog_destinationSelectedItem = false;

                //Update Header for RecyclerView
                Navigation_Dialog_destinationLabel = geocodeResult[e.Position].LocName;
                var recyclerViewRouteHeaderText = this.FindViewById<TextView>(Resource.Id.header_text);
                recyclerViewRouteHeaderText.Text = "Heading to:\n" + Navigation_Dialog_destinationLabel;

                //Xamarin.Forms.Application.Current.MainPage.DisplayAlert("Destination coordinates", Navigation_Dialog_destinationLatLng.ToString(), "Ok");

                //Navigation_Dialog_destinationUserData.ClearFocus();

                //Navigation_Dialog_fromUserData.RequestFocus();

                //Navigation_Dialog_destinationUserData.DismissDropDown();

                HideSoftKeyboard(NavigationDialogView);
            }
        }
        public class GetDataHERE
        {
            AppCompatActivity activity;
            public GetDataHERE(AppCompatActivity m_activity)
            {
                activity = m_activity;                
            }
            public void GetRouteHERE()
            {
                OnPositionChangedListener.navigateMeMenuClicked = true;

                string URL_HERE_ROUTING_v7 = HERE_Routing_API_Base_URL_v7 + HERE_Routing_API_Path_v7 + HERE_Routing_API_Resource_v7 +
                HERE_Routing_API_Format_v7 + "?" + HERE_API_apiKey + "&" + "waypoint0=geo!" + Navigation_Dialog_fromLatLng.Latitude.ToString() +
                "," + Navigation_Dialog_fromLatLng.Longitude.ToString() + "&" + "waypoint1=geo!" + "passThrough!" + Navigation_Dialog_throughLatLng.Latitude.ToString() +
                "," + Navigation_Dialog_throughLatLng.Longitude.ToString() + "&" + "waypoint2=geo!" + Navigation_Dialog_destinationLatLng.Latitude.ToString() +
                "," + Navigation_Dialog_destinationLatLng.Longitude.ToString() + "&" + "representation=turnByTurn" + "&" + "routeAttributes=wp,sm,sh,bb,lg" + "&" +
                "legAttributes=mn" + "&" + "maneuverAttributes=po,sh,tt,le,ti,rn,nr,ru,nu,sp,ac,di,nm,bt,tm,ix,bb,sa" + "&" + "linkAttributes=sh,le,sl,ds,rn,ro,nl,rt,rd,ma,ix" + "&"
                + "instructionformat=text" + "&" + "mode=fastest;car;traffic:enabled;motorway:-2";

                Uri HERE_URI_ROUTING = new Uri(URL_HERE_ROUTING_v7, UriKind.Absolute);

                returntype = "routing";

                //Download the XML string to HERE_XML and Create a Parseable stream to HERE_READER
                DownloadDataHERE(HERE_URI_ROUTING, returntype, null);
            }
            public async void DownloadDataHERE(Uri uri, string returntype, GeoPosition position)
            {
                // Android 4.1 or higher, Xamarin.Android 6.1 or higher
                HttpClient downloader = new HttpClient(new Xamarin.Android.Net.AndroidClientHandler());
                downloader.DefaultRequestHeaders.ExpectContinue = false;

                // Call asynchronous network methods in a try/catch block to handle exceptions 
                try
                {
                    var HERE_XML = await downloader.GetStringAsync(uri);

                    JaFaFa.PoI.PoI poi = new JaFaFa.PoI.PoI();

                    if (returntype == "geocode") //Text to Address and Coordinates
                    {
                        //var document = JsonConvert.DeserializeXNode(HERE_XML, "Root");
                        var document = JsonToXml(HERE_XML);
                        //geocodeResult = poi.LoadStreamPosition(document);

                        NavigationDialogListHERE(poi.LoadStreamPosition(document));

                        //await Xamarin.Forms.Application.Current.MainPage.DisplayAlert("Geocode Results", geocodeResultString.ToString(), "Ok");
                    }
                    else if (returntype == "revgeocode") //Coordinate to Address
                    {
                        //var document = JsonConvert.DeserializeXNode(HERE_XML, "Root");
                        var document = JsonToXml(HERE_XML);

                        CivicAddress = poi.LoadStreamMainPage(document);

                        // Display results
                        if (fromTopToolbar)
                        {
                            ShowReverseGeoCodeResultDialogHERE(CivicAddress, position);

                            fromTopToolbar = false;
                        }

                        var reader = new StringReader(CivicAddress);

                        if (boxclicked == 1)
                        {
                            Navigation_Dialog_fromUserData.RequestFocus();
                            Navigation_Dialog_fromUserData.SetText(reader.ReadLine(), false);
                        }
                        else if (boxclicked == 2)
                        {
                            Navigation_Dialog_throughUserData.RequestFocus();
                            Navigation_Dialog_throughUserData.SetText(reader.ReadLine(), false);
                        }
                        else if (boxclicked == 3)
                        {
                            Navigation_Dialog_destinationUserData.RequestFocus();
                            Navigation_Dialog_destinationUserData.SetText(reader.ReadLine(), false);
                        }

                        boxclicked = 0;

                        // Create a custom marker image
                        AddHEREMarkerToMap(position);
                    }
                    else if (returntype == "routing")
                    {
                        RouteShapeLatLng = new List<PoI.PoI.RouteShapeList>();
                        RouteShapeData = new List<PoI.PoI.RouteShapeData>();
                        RouteManeuverData = new List<PoI.PoI.RouteManeuverData>();
                        RouteLinkData = new List<PoI.PoI.RouteLinkData>();

                        RouteShapeLatLng = poi.LoadRouteShapeHERERv7(HERE_XML);
                        RouteShapeData = poi.LoadRouteDataHERERv7(HERE_XML);
                        RouteManeuverData = poi.LoadRouteManeuverDataHERERv7(HERE_XML);
                        RouteLinkData = poi.LoadRouteLinkDataHERERv7(HERE_XML);

                        if (RouteShapeLatLng.Count == 0)
                            //await Xamarin.Forms.Application.Current.MainPage.DisplayAlert("JáFáFá Alert", "No route found!", "Ok");
                            Toast.MakeText(activity, "No route found!", ToastLength.Long);
                        else
                        {
                            if (_routelines.IsVisible)
                                OnPositionChangedListener.RemovePolyLines(_routelines);

                            DisplayRouteHERE(RouteShapeLatLng);

                            // Layout Manager Setup:
                            // Use the built-in linear layout manager:
                            Navigation_RecyclerView_Route_LayoutManager = new Android.Support.V7.Widget.LinearLayoutManager(activity);

                            // Plug the layout manager into the RecyclerView:
                            Navigation_RecyclerView_Route.SetLayoutManager(Navigation_RecyclerView_Route_LayoutManager);

                            // Adapter Setup:
                            // Create an adapter for the RecyclerView, and pass it the
                            // data set (the route) to manage:
                            Navigation_RecyclerView_Route_Adapter = new RouteItemAdapter(RouteManeuverData, activity);

                            // Register the item click handler (below) with the adapter:
                            //Navigation_RecyclerView_Route_Adapter.ItemClick += OnItemClick;

                            //Stick a header to the view
                            //StickHeaderToRecyclerView();

                            // Plug the adapter into the RecyclerView:
                            Navigation_RecyclerView_Route.SetAdapter(Navigation_RecyclerView_Route_Adapter);                            

                            lowertoolbar.Menu.GetItem(0).SetVisible(true);
                            lowertoolbar.Menu.GetItem(1).SetVisible(true);
                        }

                        if (HintPanel.Visibility == ViewStates.Visible)
                        {
                            HintPanel.Visibility = ViewStates.Invisible;
                            m_mapFragment.View.SetPadding(0, 0, 0, 0);
                        }
                    }
                }
                catch (HttpRequestException e)
                {
                    //await Xamarin.Forms.Application.Current.MainPage.DisplayAlert("JáFáFá HttpRequestException", e.Message, "Ok");
                    Toast.MakeText(activity, "JáFáFá HttpRequest Exception\n\n" + e.Message, ToastLength.Long).Show();
                }
                catch (System.Exception ex)
                {
                    //await Xamarin.Forms.Application.Current.MainPage.DisplayAlert("JáFáFá System.Exception", ex.Message, "Ok");
                    Toast.MakeText(activity, "JáFáFá System Exception\n\n" + ex.Message, ToastLength.Long).Show();
                }

                // Need to call dispose on the HttpClient object
                // when done using it, so the app doesn't leak resources
                downloader.Dispose();
            }

            private void StickHeaderToRecyclerView()
            {
                View view = LayoutInflater.From(activity).Inflate(Resource.Layout.RecyclerViewHeader, null);

                var header = view.FindViewById<TextView>(Resource.Id.routeHeader_Text);

                try
                {
                    StickyHeaderBuilder
                        .StickTo(Navigation_RecyclerView_Route)
                        //.SetHeader(recyclerViewRouteHeader)
                        .SetHeader(header)
                        .SetMinHeightDimension(Resource.Dimension.min_height_header)
                        .PreventTouchBehindHeader()
                        .Apply();
                }
                catch (System.Exception ex)
                {
                    Toast.MakeText(activity, ex.Message, ToastLength.Long).Show();
                }
            }

            public void NavigationDialogListHERE(List<PoI.PoI.GeocodeData> result)
            {
                geocodeResult = result;

                var list = ReadListResultHERE(result);

                if (New_Destination_Dialog_destinationUserData != null && New_Destination_Dialog_destinationUserData.HasFocus && New_Destination_Dialog_View.Visibility == ViewStates.Visible)
                {
                    //Navigation_Dialog_fromUserData.DismissDropDown();
                    //Navigation_Dialog_throughUserData.DismissDropDown();                    

                    New_Destination_Dialog_destinationUserData.Adapter = new ArrayAdapter<string>(activity, Resource.Layout.SearchLLS, list);

                    //Navigation_Dialog_destinationUserData.ClearFocus();
                    New_Destination_Dialog_destinationUserData.ShowDropDown();                    

                    New_Destination_Dialog_destinationSelectedItem = true;

                    //if (geocodeResult.Count() > 0)
                    //HideSoftKeyboard(New_Destination_Dialog_View);
                }
                else if (Navigation_Dialog_fromUserData != null && Navigation_Dialog_fromUserData.HasFocus && NavigationDialogView.Visibility == ViewStates.Visible)
                {
                    //Navigation_Dialog_throughUserData.DismissDropDown();
                    //Navigation_Dialog_destinationUserData.DismissDropDown();

                    Navigation_Dialog_fromUserData.Adapter = new ArrayAdapter<string>(activity, Resource.Layout.SearchLLS, list);

                    //Navigation_Dialog_fromUserData.ClearFocus();
                    Navigation_Dialog_fromUserData.ShowDropDown();
                    Navigation_Dialog_fromSelectedItem = true;

                    //if (geocodeResult.Count() > 0)
                    //HideSoftKeyboard(NavigationDialogView);
                }
                else if (Navigation_Dialog_throughUserData != null && Navigation_Dialog_throughUserData.HasFocus && NavigationDialogView.Visibility == ViewStates.Visible)
                {
                    //Navigation_Dialog_fromUserData.DismissDropDown();
                    //Navigation_Dialog_destinationUserData.DismissDropDown();

                    Navigation_Dialog_throughUserData.Adapter = new ArrayAdapter<string>(activity, Resource.Layout.SearchLLS, list);

                    //Navigation_Dialog_throughUserData.ClearFocus();
                    Navigation_Dialog_throughUserData.ShowDropDown();
                    Navigation_Dialog_throughSelectedItem = true;

                    //if (geocodeResult.Count() > 0)
                    //HideSoftKeyboard(NavigationDialogView);
                }
                else if (Navigation_Dialog_destinationUserData != null && Navigation_Dialog_destinationUserData.HasFocus && NavigationDialogView.Visibility == ViewStates.Visible)
                {
                    //Navigation_Dialog_fromUserData.DismissDropDown();
                    //Navigation_Dialog_throughUserData.DismissDropDown();

                    Navigation_Dialog_destinationUserData.Adapter = new ArrayAdapter<string>(activity, Resource.Layout.SearchLLS, list);

                    //Navigation_Dialog_destinationUserData.ClearFocus();
                    Navigation_Dialog_destinationUserData.ShowDropDown();
                    Navigation_Dialog_destinationSelectedItem = true;

                    //if (geocodeResult.Count() > 0)
                    //HideSoftKeyboard(NavigationDialogView);
                }                
            }
            public void ShowReverseGeoCodeResultDialogHERE(string CivicAdress, GeoPosition position)
            {
                var HEREReverseGeocodeAlertDialogBuilder = new Android.App.AlertDialog.Builder(activity);
                HEREReverseGeocodeAlertDialogBuilder
                    .SetMessage("Civic Address : " + "\n\n" + CivicAddress + "\n\n" + position.Coordinate.ToString())
                    .SetTitle("JáFáFá")
                    .SetNegativeButton("Ok", delegate
                    {
                        HEREReverseGeocodeAlertDialogBuilder.Dispose();
                    });

                var HEREReverseGeocodeAlertDialog = HEREReverseGeocodeAlertDialogBuilder.Create();
                HEREReverseGeocodeAlertDialog.Show();
            }
            public void DisplayRouteHERE(List<PoI.PoI.RouteShapeList> LatLngList)
            {
                // Instantiate a new Polyline object and adds points to define a route
                List<GeoCoordinate> MapPolylinePoints = new List<GeoCoordinate>();

                foreach (var point in LatLngList)
                {
                    MapPolylinePoints.Add(new GeoCoordinate(point.LocLat, point.LocLon));
                }

                GeoPolyline MapGeoPolyline = new GeoPolyline(MapPolylinePoints);

                _routelines = new MapPolyline(MapGeoPolyline);

                _routelines.SetLineColor(Color.Red);
                _routelines.SetLineWidth(5);
                _routelines.SetVisible(true);

                if (_routelines != null && m_mapFragment.Map.AllMapObjects.Count > 0)
                {
                    //Hide PoI markers
                    ShowHEREPOIMarkersHERE(false);

                    m_mapFragment.Map.AddMapObject(_routelines);

                    //Add waypoint markers
                    var From = "Start Here";
                    var Through = "Through Here";
                    var Destination = "End Here";

                    AddWayPointMarkerToMapHERE("from", Navigation_Dialog_fromLabel, From, Navigation_Dialog_fromLatLng, GetIcon("from"));
                    AddWayPointMarkerToMapHERE("through", Navigation_Dialog_throughLabel, Through, Navigation_Dialog_throughLatLng, GetIcon("through"));
                    AddWayPointMarkerToMapHERE("destination", Navigation_Dialog_destinationLabel, Destination, Navigation_Dialog_destinationLatLng, GetIcon("destination"));

                    // Create a Geo Bounding Box (rectangular area in the Map's geographic coordinate system)
                    // with specified top-left and bottom-right coordinates of the entire journey.

                    var NorthWest = new GeoCoordinate(RouteShapeData[0].LatitudeNW, RouteShapeData[0].LongitudeNW);
                    var SouthEast = new GeoCoordinate(RouteShapeData[0].LatitudeSE, RouteShapeData[0].LongitudeSE);

                    GeoBoundingBox bounds = new GeoBoundingBox(NorthWest, SouthEast);

                    m_mapFragment.Map.ZoomTo(bounds, Map.Animation.Linear, 90);
                }
            }
            public void ShowHEREPOIMarkersHERE(bool show)
            {
                IList<MapObject> objects = m_mapFragment.Map.AllMapObjects;

                foreach (MapObject mapObj in objects)
                {
                    if (((MapObject)mapObj).GetType() == MapObject.Type.LabeledMarker)
                    {
                        // At this point we have the originally added
                        // map marker, so we can do something with it
                        // (like change the visibility, or more
                        // marker-specific actions)

                        ((MapLabeledMarker)mapObj).SetVisible(show);
                    }
                    else if (((MapObject)mapObj).GetType() == MapObject.Type.LocalModel)
                    {
                        ((MapLocalModel)mapObj).SetVisible(show);
                    }
                    else if (((MapObject)mapObj).GetType() == MapObject.Type.Circle)
                    {
                        ((MapCircle)mapObj).SetVisible(show);
                    }
                    else if (((MapObject)mapObj).GetType() == MapObject.Type.Marker)
                    {
                        ((MapMarker)mapObj).SetVisible(show);
                    }
                }
            }
            public void AddWayPointMarkerToMapHERE(string marker, string title, string snippet, LatLng latlng, Image icon)
            {
                var coordinate = new GeoCoordinate(Convert.ToDouble(latlng.Latitude), Convert.ToDouble(latlng.Longitude));

                switch (marker)
                {
                    case "from":
                        if (HEREFromMarker != null)
                            m_mapFragment.Map.RemoveMapObject(HEREFromMarker);
                        HEREFromMarker = new MapLabeledMarker(coordinate);

                        HEREFromMarker.SetTitle(title)
                                      .SetDescription(snippet)
                                      .SetIcon(icon)
                                      .SetCoordinate(coordinate);

                        HEREFromMarker.SetLabelText(m_mapFragment.Map.MapDisplayLanguage, title);
                        m_mapFragment.Map.AddMapObject(HEREFromMarker);
                        break;
                    case "through":
                        if (HEREThroughMarker != null)
                            m_mapFragment.Map.RemoveMapObject(HEREThroughMarker);
                        HEREThroughMarker = new MapLabeledMarker(coordinate);

                        HEREThroughMarker.SetTitle(title)
                                         .SetDescription(snippet)
                                         .SetIcon(icon)
                                         .SetCoordinate(coordinate);

                        HEREThroughMarker.SetLabelText(m_mapFragment.Map.MapDisplayLanguage, title);
                        m_mapFragment.Map.AddMapObject(HEREThroughMarker);
                        break;
                    case "destination":
                        if (HEREDestinationMarker != null)
                            m_mapFragment.Map.RemoveMapObject(HEREDestinationMarker);
                        HEREDestinationMarker = new MapLabeledMarker(coordinate);

                        HEREDestinationMarker.SetTitle(title)
                                             .SetDescription(snippet)
                                             .SetIcon(icon)
                                             .SetCoordinate(coordinate);

                        HEREDestinationMarker.SetLabelText(m_mapFragment.Map.MapDisplayLanguage, title);
                        m_mapFragment.Map.AddMapObject(HEREDestinationMarker);
                        break;
                }

                //locateMeMarkerId = locateMeMarker.Id;
            }
            public string[] ReadListResultHERE(List<PoI.PoI.GeocodeData> geocodeListData)
            {
                var listOfStrings = new List<string>();

                for (int i = 0; i < geocodeListData.Count; i++)
                {
                    listOfStrings.Add(geocodeListData[i].LocName);
                }

                string[] stringResultArray = listOfStrings.ToArray();

                return stringResultArray;
            }
            public void StartNavigation(GeoCoordinate from, GeoCoordinate through, GeoCoordinate destination)
            {
                OnPositionChangedListener.isOnJourney = true;

                /*
                 * Initialize a RouteOption. HERE Mobile SDK allow users to define their own parameters for the
                 * route calculation,including transport modes,route types and route restrictions etc.Please
                 * refer to API doc for full list of APIs
                */
                RouteOptions ro = new RouteOptions();
                ro.SetTransportMode(RouteOptions.TransportMode.Car);
                ro.SetRouteType(RouteOptions.Type.Fastest);
                ro.SetHighwaysAllowed(true);
                //Calculate 1 route
                ro.SetRouteCount(1);

                //Initialize a RoutePlan and set its route options
                RoutePlan rp = new RoutePlan();
                rp.SetRouteOptions(ro);

                //Get the waypoints
                var wpFrom = new RouteWaypoint(from);
                var wpThrough = new RouteWaypoint(through, RouteWaypoint.Type.ViaWaypoint);
                var wpDestination = new RouteWaypoint(destination, RouteWaypoint.Type.StopWaypoint);

                //Add waypoints to the route plan
                rp.AddWaypoint(wpFrom);
                rp.AddWaypoint(wpThrough);
                rp.AddWaypoint(wpDestination);

                /* Initialize a CoreRouter */
                //RouteManager rm = new RouteManager();
                CoreRouter router = new CoreRouter();

                //Trigger the route calculation, results will be called back via the listener
                var onRouterListener = new OnRouterListener(activity);
                router.CalculateRoute(rp, onRouterListener);
            }
        }        
        /// <summary>
        /// Check whether two given bounds intersect.
        /// return true if the given bounds intersect, false otherwise
        /// </summary>
        public bool Check_For_LatLngBounds_Intersection(LatLngBounds bounds, LatLngBounds coordbounds)
        {
            bool latIntersects = (bounds.Northeast.Latitude >= coordbounds.Southwest.Latitude) && (bounds.Southwest.Latitude <= coordbounds.Northeast.Latitude);
            bool lngIntersects = (bounds.Northeast.Longitude >= coordbounds.Southwest.Longitude) && (bounds.Southwest.Longitude <= coordbounds.Northeast.Longitude);
            
            return latIntersects && lngIntersects;
        }
        void MoveToLocation(LatLng location)
        {
         //   if (previousLocation != null)
         //       _currentBearing = _bearing.CalculateBearing(new Helpers.Position(currentLocation.Latitude, currentLocation.Longitude),
         //       new Helpers.Position(previousLocation.Latitude, previousLocation.Longitude));
         //   else
         //       _currentBearing = _bearing.CalculateBearing(new Helpers.Position(currentLocation.Latitude, currentLocation.Longitude),
         //       new Helpers.Position(currentLocation.Latitude, currentLocation.Longitude));

            // Move the camera to the latlng position.
            var builder = CameraPosition.InvokeBuilder();
            builder.Target(location);
            //builder.Zoom(16);
            builder.Zoom(20);
            builder.Bearing(Convert.ToSingle(_currentBearing));
            //builder.Bearing(0);
            builder.Tilt(65);
            var cameraPosition = builder.Build();

            // We create an instance of CameraUpdate, and move the map to it.
            googleMap.MoveCamera(CameraUpdateFactory.NewCameraPosition(cameraPosition));
        }
        public void OnProviderDisabled(string provider)
        {
            isRequestingLocationUpdates = false;
        }
        public void OnProviderEnabled(string provider)
        {
            Toast.MakeText(this, "The provider " + provider + " is enabled.", ToastLength.Short).Show();
        }
        public void OnStatusChanged(string provider, [GeneratedEnum] Availability status, Bundle extras)
        {
            if (status == Availability.OutOfService)
            {
                //Stop requesting location updates
                isRequestingLocationUpdates = false;
                //if (currentLocation != null)
                    //locationManager.RemoveUpdates(this);

                //Set toolbar icon
                var trackMenuItem = this.toolbar.Menu.GetItem(1);
                trackMenuItem.SetIcon(Resource.Drawable.appbar_track_me);
            }
        }
        private void RequestLocationPermission(int requestCode)
        {
            isRequestingLocationUpdates = false;
            if (ActivityCompat.ShouldShowRequestPermissionRationale(this, Manifest.Permission.AccessFineLocation))
            {
                Snackbar.Make(mDrawerLayout, Resource.String.permission_location_rationale, Snackbar.LengthIndefinite)
                        .SetAction(Resource.String.ok,
                                   delegate
                                   {
                                       ActivityCompat.RequestPermissions(this, new[] { Manifest.Permission.AccessFineLocation }, requestCode);
                                   })
                        .Show();
            }
            else
            {
                ActivityCompat.RequestPermissions(this, new[] { Manifest.Permission.AccessFineLocation }, requestCode);
            }
        }
        //Hides the soft keyboard 
        public void HideSoftKeyboard(Android.Views.View view)
        {
            var currentFocus = view.FindFocus();
            if (currentFocus != null)
            {
                InputMethodManager inputMethodManager = (InputMethodManager)GetSystemService(Context.InputMethodService);
                inputMethodManager.HideSoftInputFromWindow(currentFocus.WindowToken, 0);
            }
        }
        //Shows the soft keyboard
        public void ShowSoftKeyboard(Android.Views.View view)
        {
            InputMethodManager inputMethodManager = (InputMethodManager)GetSystemService(Context.InputMethodService);
            view.RequestFocus();
            inputMethodManager.ShowSoftInput(view, 0);
        }

        static readonly LatLng PasschendaeleLatLng = new LatLng(50.897778, 3.013333);
        static readonly LatLng VimyRidgeLatLng = new LatLng(50.379444, 2.773611);
        static readonly LatLng HEREANYWASHCoordinate = new LatLng(6.5138126, 3.3964648);

        //Android.Widget.Button animateToLocationButton;
        //MapFragment mapFragment;
        GoogleMap googleMap;

        private static AndroidXMapFragment m_mapFragment;
        private MapFragmentView mapFragmentView;
        private static PositioningManager positionManager;
        private static NavigationManager navigationManager;

        // flag that indicates whether maps is being transformed
        private static bool mTransforming = false;
        // callback that is called when transforming ends
        private static Runnable mPendingUpdate;
        private static bool initrequested = false;
        private static bool locatemerequested = false;
        private static bool trackmerequested = false;
        private static string CivicAddress;
        private static MapMarker locateMeHEREMarker;
        private static MapLabeledMarker HEREMarker;

        private static ClusterLayer HERECluster;
        private static ClusterLayer HEREClusterGas;

        private static GeoPosition HEREposition;
        private static int HEREpositionfixcount;
        private static int boxclicked = 0;

        private static bool fromTopToolbar = false;

        // custom position marker
        private static MapCircle m_PositionMarker;
        private static MapCircle m_PositionAccuracyIndicator;
        private static MapLocalModel m_PositionMesh;

        // compass sensors
        private static SensorManager mSensorManager;
        private static Sensor mAccelerometer;
        private static Sensor mMagnetometer;

        // compass data
        private static float mAzimuth;
        private static float[] mGravity = new float[3];
        private static float[] mGeomagnetic = new float[3];

        private DrawerLayout mDrawerLayout;
        private NavigationView mDrawerViewLeft;
        private NavigationView mDrawerViewRight;
        private Android.Support.V7.App.ActionBarDrawerToggle mDrawerToggle;
        private Android.Support.V7.Widget.Toolbar toolbar = null;
        //private AndroidX.AppCompat.Widget.Toolbar toolbar = null;
        private static Android.Support.V7.Widget.Toolbar lowertoolbar = null;
        //private AndroidX.AppCompat.Widget.Toolbar lowertoolbar = null;

        private string mDrawerTitleLeft = "Help & Support";
        private string mDrawerTitleRight = "Settings";
        
        private bool isShowingRoute = false;

        private string geocodeAddress;
        private Marker locateMeMarker;
        private string locateMeMarkerId;
        private const int zoomLevel = 20;

        private Marker fromMarker;
        private Marker throughMarker;
        private Marker destinationMarker;

        private static MapLabeledMarker HEREFromMarker;
        private static MapLabeledMarker HEREThroughMarker;
        private static MapLabeledMarker HEREDestinationMarker;

        private Marker journeyMarker;

        private LocationManager locationManager;
        private string providerJAFAFA;

        private List<Android.Gms.Maps.Model.Polyline> _tracklines = new List<Android.Gms.Maps.Model.Polyline>();
        //private List<Android.Gms.Maps.Model.Polyline> _routelines = new List<Android.Gms.Maps.Model.Polyline>();
        
        private static MapPolyline _routelines;
        
        private static double lastManeuverIndex;
        private static bool lIndexFoundIterated;
        private static List<int> pastManeuverIndexList;

        private static bool instructionannounced;

        private PolylineOptions polOptions = new PolylineOptions();
        private Android.Locations.Location currentLocation = null;
        private Android.Locations.Location previousLocation = null;
        private PositionHandler _bearing = new PositionHandler();
        private System.Double _currentBearing;

        private bool locateMeMenuClicked = false;
        private bool trackMeMenuClicked = false;        
        private bool _tracklineDisposed = false;
        private bool _routelineDisposed = false;

        private static string HERE_API_apiKey = "apiKey=5GjlNrq8s1QR8mP8Fi4aVu6ObRdcxVdDwMAkNy1L9wY";

        private string HERE_Reverse_Geocode_API_Base_URL = "https://reverse.geocoder.ls.hereapi.com";
        private string HERE_Reverse_Geocode_API_Base_URL_v1 = "https://revgeocode.search.hereapi.com";
        private string HERE_Reverse_Geocode_API_Path = "/6.2";
        private string HERE_Reverse_Geocode_API_Path_v1 = "/v1";
        private string HERE_Reverse_Geocode_API_Resource = "/reversegeocode";
        private string HERE_Reverse_Geocode_API_Resource_v1 = "/revgeocode";
        private string HERE_Reverse_Geocode_API_Format = ".xml";

        private string HERE_Geocode_API_Base_URL_v1 = "https://geocode.search.hereapi.com";
        private string HERE_Geocode_API_Path_v1 = "/v1";
        private string HERE_Geocode_API_Resource_v1 = "/geocode";

        private string HERE_Routing_API_Base_URL_v8 = "https://router.hereapi.com";
        private string HERE_Routing_API_Path_v8 = "/v8";
        private string HERE_Routing_API_Resource_v8 = "/routes";

        private static string HERE_Routing_API_Base_URL_v7 = "https://route.ls.hereapi.com";
        private static string HERE_Routing_API_Path_v7 = "/routing/7.2";
        private static string HERE_Routing_API_Resource_v7 = "/calculateroute";
        private static string HERE_Routing_API_Format_v7 = ".xml";

        private static string returntype = "";
        private bool Nextsearchterm;

        private static List<JaFaFa.PoI.PoI.GeocodeData> geocodeResult;

        private List<string> lines = new List<string>();
        private static Android.Views.View NavigationDialogView;
        private static AutoCompleteTextView Navigation_Dialog_fromUserData;
        private static AutoCompleteTextView Navigation_Dialog_throughUserData;
        private static AutoCompleteTextView Navigation_Dialog_destinationUserData;
        private ImageView fromCurrentLocationIcon;
        private ImageView throughCurrentLocationIcon;
        private ImageView destinationCurrentLocationIcon;

        private static AutoCompleteTextView New_Destination_Dialog_destinationUserData;
        private static Android.Views.View New_Destination_Dialog_View;

        // RecyclerView instance that displays the view:
        private static RecyclerView Navigation_RecyclerView_Route;

        // Layout manager that lays out each card in the RecyclerView:
        private static RecyclerView.LayoutManager Navigation_RecyclerView_Route_LayoutManager;

        // Adapter that accesses the data set (a route entry):
        private static RouteItemAdapter Navigation_RecyclerView_Route_Adapter;
        public class GeoJSONData
        {
            public double[] BB { get; set; }
            public GeoJSON.Net.CoordinateReferenceSystem.ICRSObject CRS { get; set; }
            public IGeometryObject Geometry { get; set; }
            public string Id { get; set; }
            public IDictionary<string, object> Properties { get; set; }
            public string Name { get; set; }
        }
        public class GeoJSONDataPlaces
        {            
            public IGeometryObject Geometry { get; set; }
            public string Name { get; set; }
            public string roadName { get; set; }
            public GeoCoordinate Coordinate { get; set; }
        }
        public class GeoJSONName
        {
            public string name { get; set; }
            public string nameType { get; set; }
            public bool isPrimary { get; set; }
            public string languageCode { get; set; }
        }
        public class GasAlertData
        {
            public string Name { get; set; }
            public string Address { get; set; }
            public string Rating { get; set; }
            public GeoCoordinate Coord { get; set; }
        }

        // RecyclerView instance that displays the view:
        private static RecyclerView Gas_Alert_RecyclerView;

        // Layout manager that lays out each card in the RecyclerView:
        private static RecyclerView.LayoutManager Gas_Alert_RecyclerView_LayoutManager;

        // Adapter that accesses the data set (a route entry):
        private static GasAlertItemAdapter Gas_Alert_RecyclerView_Adapter;

        // Popup dialog box
        private static Android.Views.View Gas_Alert_Dialog_View;

        // Top panel that displays journey hints
        private static GridLayout HintPanel;
        private static TextView HintPanelRoadName;
        private static ImageView HintPanelIcon;
        private static TextView HintPanelInstruction;        

        private static List<PoI.PoI.RouteShapeList> RouteShapeLatLng;
        private static List<PoI.PoI.RouteShapeData> RouteShapeData;
        private static List<PoI.PoI.RouteManeuverData> RouteManeuverData;
        private static List<PoI.PoI.RouteLinkData> RouteLinkData;

        private static ObservableCollection<PoI.PoI.PoILongListData> PoIListData;
        private static ObservableCollection<PoI.PoI.PoILongListData> PoIListDataGas;
        private static List<GasAlertData> maneuverGasList;

        private static List<GeoJSONDataPlaces> HEREPlacesListData;
        private static MapPolygon mapPolygon;
        private static GeoPolygon geoPolygon;

        private static bool menuactive;
        private static Android.Widget.ProgressBar progressBar;
        private static ProgressDialog progressDialog;
        private static Android.App.AlertDialog progressBarDialog;

        private static string Navigation_Dialog_fromLabel;
        private static LatLng Navigation_Dialog_fromLatLng;
        private static string Navigation_Dialog_throughLabel;
        private static LatLng Navigation_Dialog_throughLatLng;
        private static string Navigation_Dialog_destinationLabel;
        private static LatLng Navigation_Dialog_destinationLatLng;

        private static string New_Destination_Dialog_destinationLabel;
        private static LatLng New_Destination_Dialog_destinationLatLng;

        private static bool Navigation_Dialog_fromSelectedItem;
        private static bool Navigation_Dialog_throughSelectedItem;
        private static bool Navigation_Dialog_destinationSelectedItem;

        private static bool New_Destination_Dialog_destinationSelectedItem;

        const int RequestLocationId = 0;

        const long ONE_SECOND = 1000;
        const long ONE_MINUTE = 60 * ONE_SECOND;
        const long FIVE_SECONDS = 5 * ONE_SECOND;
        
        static readonly string KEY_REQUESTING_LOCATION_UPDATES = "requesting_location_updates";

        static readonly int RC_LAST_LOCATION_PERMISSION_CHECK = 1000;

        bool isRequestingLocationUpdates;

        readonly string[] LocationPermissions =
        {            
            Android.Manifest.Permission.AccessCoarseLocation,
            Android.Manifest.Permission.AccessFineLocation,
            Android.Manifest.Permission_group.Sensors
        };
    }
}