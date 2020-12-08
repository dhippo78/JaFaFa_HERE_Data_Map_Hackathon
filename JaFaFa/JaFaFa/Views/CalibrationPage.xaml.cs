using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


using Xamarin.Forms;
using Xamarin.Forms.Xaml;

using System.Timers;
using JaFaFa.Magneto;

namespace JaFaFa.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class CalibrationPage : ContentPage
    {
        public CalibrationPage()
        {
            InitializeComponent();

            if (magneto.StartCompass())
            {
                SetupTimer(); //Placed in CalibrationPage() method to ensure timer is run on same thread
            }

            this.BackgroundColor = Color.LemonChiffon;
        }
        private void ContentPage_Appearing(object sender, EventArgs e)
        {
            base.OnAppearing();
        }
        void SetupTimer()
        {
            // Initialize the timer, add event handler calibrationTimer_Tick, and start it.            
            // The application thread creates the timer, which executes the Tick event handler
            // callback method every 250 milliseconds.
            timer = new Timer(250);
            timer.Elapsed += calibrationTimer_Tick;
            timer.AutoReset = true;
            timer.Enabled = true;
            timer.Start();
        }
        
        // This method is called by the timer delegate.        
        void calibrationTimer_Tick(/*Object stateInfo*/Object source, ElapsedEventArgs e)
        {
            DisplayAlert("Compass", MainPage._headingAccuracy.ToString(), "OK");

            if ((Math.Abs(MainPage._headingAccuracy) <= 7))
            {
                calibrationStatus.Text = "The compass on your device does not need to\nbe calibrated.";
                calibrationTextBlock.TextColor = Color.Green;
                calibrationTextBlock.Text = "Complete!";
                //calibrationTextBlock.Text = Math.Abs(MainPage._headingAccuracy).ToString("0.0");
            }
            else if ((Math.Abs(MainPage._headingAccuracy) > 7))
            {
                calibrationStatus.Text = "The compass on your device needs to be calibrated.\nHold the device in front of you and sweep it\nthrough a figure 8 pattern as shown until\nthe calibration is complete.";
                calibrationTextBlock.TextColor = Color.Red;
                calibrationTextBlock.Text = Math.Abs(MainPage._headingAccuracy).ToString("0.0");
            }            
        }
        private void calibrationButton_OnClick(object sender, EventArgs e)
        {            
            //timer.Start();
        }
        private void ContentPage_Disappearing(object sender, EventArgs e)
        {
            if (magneto.StopCompass())
            {
                timer.Elapsed -= calibrationTimer_Tick;
                timer.Stop();
                timer.Dispose();
            }
            
            base.OnDisappearing();
        }

        public Magneto.Magneto magneto = new Magneto.Magneto();
        public Timer timer;
    }
}