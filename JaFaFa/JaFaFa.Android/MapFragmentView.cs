using Android.Graphics;
using Android.OS;
using Android.Util;
using Android.Views;
using Android.App;
using Android.Widget;
using AndroidX.AppCompat.App;
using Com.Here.Android.Mpa.Common;
using Com.Here.Android.Mpa.Mapping;
using Com.Here.Android.Mpa.Odml;
using Com.Here.Android.Mpa.Routing;
using Java.Lang;
using System.Collections.Generic;
using static Com.Here.Android.Mpa.Mapping.Customization.CustomizableScheme;

namespace JaFaFa.Droid
{
    public class MapFragmentView
    {
        private AndroidXMapFragment m_mapFragment;
        private Button m_createRouteButton;
        private AppCompatActivity m_activity;
        private Map m_map;
        private MapRoute m_mapRoute;
        public static PositioningManager positionManager { get; set; }
        public static System.String CivicAddress { get; set; }
        public static bool locatemerequested = false;        
        public static bool trackmerequested = false;

        public MapFragmentView(AppCompatActivity activity)
        {            
            m_activity = activity;
            initMapFragment();
            /*
             * We use a button in this example to control the route calculation
             */
            initCreateRouteButton();
        }

        public AndroidXMapFragment getMapFragment()
        {
            return (AndroidXMapFragment)m_activity.SupportFragmentManager.FindFragmentById(Resource.Id.map);
        }

        private void initMapFragment()
        {
            /* Locate the mapFragment UI element */
            m_mapFragment = getMapFragment();

            if (m_mapFragment != null)
            {
                /* Initialize the AndroidXMapFragment, results will be given via the called back. */
                m_mapFragment.Init(new OnEngineInitListener(this));                
            }
        }
        private class OnEngineInitListener : Java.Lang.Object, IOnEngineInitListener
        {
            private MapFragmentView parent;

            public OnEngineInitListener(MapFragmentView parent)
            {
                this.parent = parent;                
            }

            public void OnEngineInitializationCompleted(OnEngineInitListenerError error)
            {
                if (error == OnEngineInitListenerError.None)
                {
                    //LoadMarkers();

                    GeoCoordinate HEREANYWASHCoordinate = new GeoCoordinate(6.5138126, 3.3964648, 0.0);

                    // retrieve a reference of the map from the map fragment
                    parent.m_map = parent.m_mapFragment.Map;
                    
                    parent.m_map.SetLandmarksVisible(true);
                    parent.m_map.SetExtrudedBuildingsVisible(true);
                    parent.m_map.SetCartoMarkersVisible(true);                    
                    parent.m_map.SetTrafficInfoVisible(true);

                    // Set the map center to the ANYWASH Nigeria UNILAG region
                    parent.m_map.SetCenter(HEREANYWASHCoordinate, Map.Animation.None, parent.m_map.MaxZoomLevel * 0.80, 90, parent.m_map.MaxTilt);

                    // Register Map Loader listener
                    OnMapLoaderListener mapLoaderListener = new OnMapLoaderListener();
                    MapLoader mapLoader = MapLoader.Instance;
                    mapLoader.AddListener(mapLoaderListener);

                    // Register Gesture listener
                    OnGestureListener mapGestureListener = new OnGestureListener();
                    parent.m_mapFragment.MapGesture.AddOnGestureListener(mapGestureListener, 100, true);

                    // Register positioning listener
                    OnPositionChangedListener positionListener = new OnPositionChangedListener(parent.m_mapFragment);
                    Java.Lang.Ref.WeakReference weakReference = new Java.Lang.Ref.WeakReference(positionListener);
                    
                    //METHOD  1
                    PositioningManager.Instance.AddListener(weakReference);

                    positionManager = PositioningManager.Instance;

                    if (positionManager.Start(PositioningManager.LocationMethod.GpsNetworkIndoor))
                    {
                        // Position updates started successfully.

                        //MainActivity.mainPositionManager = positionManager;

                        // Display position indicator
                        //parent.m_mapFragment.PositionIndicator.SetVisible(true);

                        locatemerequested = true;
                    }
                    
                    //positionManager.GetLocationStatus(PositioningManager.LocationMethod.GpsNetwork);
                    
                    /*
                    //METHOD  2
                    var m_hereDataSource = LocationDataSourceHERE.Instance;
                    
                    if (m_hereDataSource != null)
                    {
                        positionManager = PositioningManager.Instance;
                        positionManager.SetDataSource(m_hereDataSource);
                        positionManager.AddListener(weakReference);
                        if (positionManager.Start(PositioningManager.LocationMethod.GpsNetworkIndoor))
                        {
                            // Position updates started successfully.
                            // Display position indicator
                            parent.m_mapFragment.PositionIndicator.SetVisible(true);
                        }
                    }
                    */
                }
                else
                {
                    // print error
                    Log.Error("MapFragmentView", "ERROR: Cannot initialize Map Fragment: " + error.Details);
                    
                    var alertDialogBuilder = new Android.App.AlertDialog.Builder(parent.m_activity);
                    alertDialogBuilder
                        .SetMessage("Error : " + error.Name() + "\n\n" + error.Details)
                        .SetTitle("JáFáFá")
                        .SetNegativeButton("Ok", delegate
                        {
                            alertDialogBuilder.Dispose();
                        });

                    var alertDialog = alertDialogBuilder.Create();
                    alertDialog.Show();
                }
            }
        }
        // Create a gesture listener and add it to the AndroidXMapFragment
        private class OnGestureListener : Java.Lang.Object, IMapGestureOnGestureListener
        {
            public bool OnMapObjectsSelected(IList<ViewObject> objects)
            {
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
                            //((MapObject)viewObj).SetVisible(false);

                            //MapMarker locateMeHEREMapMarker = (MapMarker)viewObj;
                            //Image locateMeHEREMapMarkerImage = new Image();
                            //locateMeHEREMapMarkerImage.SetImageResource(Resource.Drawable.Map_Marker_Bubble_Pink_icon);

                            ((MapMarker)viewObj).SetTitle("JáFáFá");
                            ((MapMarker)viewObj).SetDescription(CivicAddress);
                            ((MapMarker)viewObj).SetVisible(true);
                            ((MapMarker)viewObj).SetCoordinate(positionManager.Position.Coordinate);
                            //((MapMarker)viewObj).SetIcon(locateMeHEREMapMarkerImage);

                            //MainActivity.HEREReverseGeocodeAlertDialog.Show();                            
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
                // return false to allow the map to handle this callback also
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
        // Define positioning listener
        //private class OnPositionChangedListener : Java.Lang.Object, PositioningManager.IOnPositionChangedListener
        private class OnPositionChangedListener : Java.Lang.Object, PositioningManager.IOnPositionChangedListener
        {
            AndroidXMapFragment mfragment;

            public OnPositionChangedListener(AndroidXMapFragment mfragment)
            {
                this.mfragment = mfragment;
            }
            public void OnPositionUpdated(PositioningManager.LocationMethod method, GeoPosition position, bool isMapMatched)
            {
                // New position update received
                // set the center only when the app is in the foreground
                // to reduce CPU consumption
                //if (!paused)
                //{
                //mfragment.Map.SetCenter(position.Coordinate, Map.Animation.None);
                if (locatemerequested)
                {
                    mfragment.Map.SetCenter(position.Coordinate, Map.Animation.Bow, mfragment.Map.MaxZoomLevel * 0.80, 90, mfragment.Map.MaxTilt);

                    // Display a marker

                    locatemerequested = false;
                }

                if (trackmerequested)
                {
                    mfragment.Map.SetCenter(position.Coordinate, Map.Animation.None, mfragment.Map.MaxZoomLevel * 0.80, 90, mfragment.Map.MaxTilt);
                }
                //}

                //MainActivity.mainPositionManager = positionManager;
            }
            public void OnPositionFixChanged(PositioningManager.LocationMethod method, PositioningManager.LocationStatus status)
            {
                // Positioning method changed
            }
        }        
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

        private class OnClickListener : Java.Lang.Object, View.IOnClickListener
        {
            private MapFragmentView parent;

            public OnClickListener(MapFragmentView parent)
            {
                this.parent = parent;
            }

            public void OnClick(View v)
            {
                /*
                 * Clear map if previous results are still on map,otherwise proceed to creating
                 * route
                 */
                if (parent.m_map != null && parent.m_mapRoute != null)
                {
                    parent.m_map.RemoveMapObject(parent.m_mapRoute);
                    parent.m_mapRoute = null;
                }
                else
                {
                    /*
                     * The route calculation requires local map data.Unless there is pre-downloaded
                     * map data on device by utilizing MapLoader APIs, it's not recommended to
                     * trigger the route calculation immediately after the MapEngine is
                     * initialized.The INSUFFICIENT_MAP_DATA error code may be returned by
                     * CoreRouter in this case.
                     *
                     */
                    parent.CreateRoute();
                }
            }
        }

        private void initCreateRouteButton()
        {
            //m_createRouteButton = (Button)m_activity.FindViewById(Resource.Id.button);

            //m_createRouteButton.SetOnClickListener(new OnClickListener(this));            
        }

        private class RouterListener : Java.Lang.Object, IRouterListener
        {
            private MapFragmentView parent;

            public RouterListener(MapFragmentView parent)
            {
                this.parent = parent;
            }

            public void OnCalculateRouteFinished(Object results, Object routingError)
            {
                /* Calculation is done. Let's handle the result */
                if (routingError == RoutingError.None)
                {
                    Java.Util.AbstractList routeResults = (Java.Util.AbstractList)results;
                    RouteResult route = routeResults.Get(0) as RouteResult;
                    if (route != null)
                    {
                        /* Create a MapRoute so that it can be placed on the map */
                        parent.m_mapRoute = new MapRoute(route.Route);

                        /* Show the maneuver number on top of the route */
                        parent.m_mapRoute.SetManeuverNumberVisible(true);

                        /* Add the MapRoute to the map */
                        parent.m_map.AddMapObject(parent.m_mapRoute);

                        /*
                         * We may also want to make sure the map view is orientated properly
                         * so the entire route can be easily seen.
                         */
                        GeoBoundingBox gbb = route.Route.BoundingBox;
                        parent.m_map.ZoomTo(gbb, Map.Animation.None, Map.MovePreserveOrientation);
                    }
                    else
                    {
                        Toast.MakeText(parent.m_activity,
                                "Error:route results returned is not valid",
                                ToastLength.Long).Show();
                    }
                }
                else
                {
                    Toast.MakeText(parent.m_activity,
                            "Error:route calculation returned error code: " + routingError,
                            ToastLength.Long).Show();
                }
            }

            public void OnProgress(int p0)
            {
                /* The calculation progress can be retrieved in this callback. */
            }
        }

        /* Creates a route from 4350 Still Creek Dr to Langley BC with highways disallowed */
        private void CreateRoute()
        {
            /* Initialize a CoreRouter */
            CoreRouter coreRouter = new CoreRouter();

            /* Initialize a RoutePlan */
            RoutePlan routePlan = new RoutePlan();

            /*
             * Initialize a RouteOption. HERE Mobile SDK allow users to define their own parameters for the
             * route calculation,including transport modes,route types and route restrictions etc.Please
             * refer to API doc for full list of APIs
             */
            RouteOptions routeOptions = new RouteOptions();
            /* Other transport modes are also available e.g Pedestrian */
            routeOptions.SetTransportMode(RouteOptions.TransportMode.Car);
            /* Disable highway in this route. */
            routeOptions.SetHighwaysAllowed(false);
            /* Calculate the shortest route available. */
            routeOptions.SetRouteType(RouteOptions.Type.Shortest);
            /* Calculate 1 route. */
            routeOptions.SetRouteCount(1);
            /* Finally set the route option */
            routePlan.SetRouteOptions(routeOptions);

            /* Define waypoints for the route */
            // START: Daimler Fleetboard
            RouteWaypoint startPoint = new RouteWaypoint(new GeoCoordinate(48.7244153, 9.1161991));
            // END: Biergarten
            RouteWaypoint destination = new RouteWaypoint(new GeoCoordinate(48.701356, 9.1393803));

            /* Add both waypoints to the route plan */
            routePlan.AddWaypoint(startPoint);
            routePlan.AddWaypoint(destination);

            /* Trigger the route calculation,results will be called back via the listener */
            coreRouter.CalculateRoute(routePlan, new RouterListener(this));
        }
    }
}