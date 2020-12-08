using JaFaFa.Views;
using System;
using Xamarin.Essentials;

namespace JaFaFa.Magneto
{
    /// <summary>
    /// 
    /// </summary>
    public class Magneto
    {
        public Magneto()
        {
                        
        }
        public Boolean StartCompass()
        {
            try
            {
                if (!Compass.IsMonitoring)
                {
                    Compass.Start(speed, applyLowPassFilter: true);
                    //DisplayAlert("Compass", "Compass Has Started", "OK");
                    Console.WriteLine("Compass Has Started");

                    //Set call backs for events by registering for reading changes
                    Compass.ReadingChanged += Compass_ReadingChanged;

                    return true;
                }                
            }
            catch (FeatureNotSupportedException fnsEx)
            {
                //DisplayAlert("Compass", fnsEx.Message, "OK");
                Console.WriteLine(fnsEx.Message);
                return false;
                // Feature not supported on device
            }
            catch (Exception ex)
            {
                //DisplayAlert("Compass", ex.Message, "OK
                Console.WriteLine(ex.Message);
                return false;
                // Some other exception has occurred
            }
            
            return false;
        }
        void Compass_ReadingChanged(object sender, CompassChangedEventArgs e)
        {
            Console.WriteLine("Compass is Reading");
            _data = e.Reading.HeadingMagneticNorth;
            //var data = e.Reading;
            //MainPage._headingAccuracy = e.Reading.HeadingMagneticNorth;
            //DisplayAlert("Compass Reading Changed", e.Reading.HeadingMagneticNorth.ToString(), "OK");
            //Console.WriteLine($"Reading: {data.HeadingMagneticNorth} degrees");
            //MainThread.BeginInvokeOnMainThread(() => { MainPage._headingAccuracy = data.HeadingMagneticNorth; });
            // Process Heading Magnetic North
        }
        public Boolean StopCompass()
        {
            try
            {
                if (Compass.IsMonitoring)
                {
                    Compass.Stop();

                    //Unsubscribe call backs for events
                    Compass.ReadingChanged -= Compass_ReadingChanged;

                    return true;
                }
            }
            catch (FeatureNotSupportedException fnsEx)
            {
                //DisplayAlert("Compass", fnsEx.Message, "OK");
                Console.WriteLine(fnsEx.Message);
                return false;
                // Feature not supported on device
            }
            catch (Exception ex)
            {
                //DisplayAlert("Compass", ex.Message, "OK");
                Console.WriteLine(ex.Message);
                return false;
                // Some other exception has occurred
            }            

            return false;
        }

        // Set speed delay for monitoring changes.
        SensorSpeed speed = SensorSpeed.UI;
        public static double _data;
    }
}