using System;
using System.Collections.Generic;
using SlideOverKit;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

using Xamarin.Essentials;

namespace JaFaFa.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class RightSideMasterView : SlideMenuView
    {
        public RightSideMasterView ()
        {
            InitializeComponent ();

            // You must set IsFullScreen in this case, 
            // otherwise you need to set HeightRequest, 
            // just like the QuickInnerMenu sample
            this.IsFullScreen = true;
            // You must set WidthRequest in this case
            this.WidthRequest = 300;
            this.MenuOrientations = MenuOrientation.RightToLeft;

            // You must set BackgroundColor, 
            // and you cannot put another layout with background color cover the whole View
            // otherwise, it cannot be dragged on Android
            //this.BackgroundColor = Color.FromHex ("#2196F3");
            this.BackgroundColor = Color.LemonChiffon;

            // This is shadow view color, you can set a transparent color
            this.BackgroundViewColor = Color.FromHex ("#CE766C");

            Load_Settings_Preferences();
            
        }
        private void Location_Grid_Clicked(object sender, EventArgs e)
        {
            if (Location_Grid.BackgroundColor == Color.White)
            {
                Location_Grid.BackgroundColor = Color.LightGray;
                Location_Grid_Label.Text = "Location:ON";
                Preferences.Set("Location", 1);
            }
            else
            {
                Location_Grid.BackgroundColor = Color.White;
                Location_Grid_Label.Text = "Location:OFF";
                Preferences.Set("Location", 0);
            }
        }
        private void Online_Grid_Clicked(object sender, EventArgs e)
        {
            if (Online_Grid.BackgroundColor == Color.White)
            {
                Online_Grid.BackgroundColor = Color.LightGray;
                Online_Grid_Label.Text = "Online";
                Preferences.Set("Online", 1);
            }
            else
            {
                Online_Grid.BackgroundColor = Color.White;
                Online_Grid_Label.Text = "Offline";
                Preferences.Set("Online", 0);
            }
        }
        private void Compass_Grid_Clicked(object sender, EventArgs e)
        {
            if (Compass_Grid.BackgroundColor == Color.White)
            {
                Compass_Grid.BackgroundColor = Color.LightGray;
                Compass_Grid_Label.Text = "Compass:ON";
                Preferences.Set("Compass", 1);
            }
            else
            {
                Compass_Grid.BackgroundColor = Color.White;
                Compass_Grid_Label.Text = "Compass:OFF";
                Preferences.Set("Compass", 0);
            }
        }
        private void Traffic_Legend_Grid_Clicked(object sender, EventArgs e)
        {
            if (Traffic_Legend_Grid.BackgroundColor == Color.White)
            {
                Traffic_Legend_Grid.BackgroundColor = Color.LightGray;
                Traffic_Legend_Grid_Label.Text = "Traffic Legend:ON";
                Preferences.Set("Traffic_Legend", 1);
            }
            else
            {
                Traffic_Legend_Grid.BackgroundColor = Color.White;
                Traffic_Legend_Grid_Label.Text = "Traffic Legend:OFF";
                Preferences.Set("Traffic_Legend", 0);
            }

        }
        private void Cartographic_Mode_Grid_Clicked(object sender, EventArgs e)
        {
            if (Cartographic_Mode_Grid.BackgroundColor == Color.White)
            {
                Cartographic_Mode_Grid.BackgroundColor = Color.LightGray;
                Cartographic_Mode_Grid_Label.Text = "Cartographic Mode:ON";
                Preferences.Set("Cartographic_Mode", 1);
            }
            else
            {
                Cartographic_Mode_Grid.BackgroundColor = Color.White;
                Cartographic_Mode_Grid_Label.Text = "Cartographic Mode:OFF";
                Preferences.Set("Cartographic_Mode", 0);
            }
        }
        private void Journey_Stats_Grid_Clicked(object sender, EventArgs e)
        {
            if (Journey_Stats_Grid.BackgroundColor == Color.White)
            {
                Journey_Stats_Grid.BackgroundColor = Color.LightGray;
                Journey_Stats_Grid_Label.Text = "Journey Stats:ON";
                Preferences.Set("Journey_Stats", 1);
            }
            else
            {
                Journey_Stats_Grid.BackgroundColor = Color.White;
                Journey_Stats_Grid_Label.Text = "Journey Stats:OFF";
                Preferences.Set("Journey_Stats", 0);
            }
        }
        private void Load_Settings_Preferences()
        {
            switch (Preferences.Get("Location", 0))
            {
                case 1:
                    Location_Grid.BackgroundColor = Color.LightGray;
                    Location_Grid_Label.Text = "Location:ON";
                    break;
                case 0:
                    Location_Grid.BackgroundColor = Color.White;
                    Location_Grid_Label.Text = "Location:OFF";
                    break;
            }
            switch (Preferences.Get("Online", 0))
            {
                case 1:
                    Online_Grid.BackgroundColor = Color.LightGray;
                    Online_Grid_Label.Text = "Online";
                    break;
                case 0:
                    Online_Grid.BackgroundColor = Color.White;
                    Online_Grid_Label.Text = "Offline";
                    break;
            }
            switch (Preferences.Get("Compass", 0))
            {
                case 1:
                    Compass_Grid.BackgroundColor = Color.LightGray;
                    Compass_Grid_Label.Text = "Compass:ON";
                    break;
                case 0:
                    Compass_Grid.BackgroundColor = Color.White;
                    Compass_Grid_Label.Text = "Compass:OFF";
                    break;
            }
            switch (Preferences.Get("Traffic_Legend", 0))
            {
                case 1:
                    Traffic_Legend_Grid.BackgroundColor = Color.LightGray;
                    Traffic_Legend_Grid_Label.Text = "Traffic Legend:ON";
                    break;
                case 0:
                    Traffic_Legend_Grid.BackgroundColor = Color.White;
                    Traffic_Legend_Grid_Label.Text = "Traffic Legend:OFF";
                    break;
            }
            switch (Preferences.Get("Cartographic_Mode", 0))
            {
                case 1:
                    Cartographic_Mode_Grid.BackgroundColor = Color.LightGray;
                    Cartographic_Mode_Grid_Label.Text = "Cartographic Mode:ON";
                    break;
                case 0:
                    Cartographic_Mode_Grid.BackgroundColor = Color.White;
                    Cartographic_Mode_Grid_Label.Text = "Cartographic Mode:OFF";
                    break;
            }

            switch (Preferences.Get("Journey_Stats", 0))
            {
                case 1:
                    Journey_Stats_Grid.BackgroundColor = Color.LightGray;
                    Journey_Stats_Grid_Label.Text = "Journey Stats:ON";
                    break;
                case 0:
                    Journey_Stats_Grid.BackgroundColor = Color.White;
                    Journey_Stats_Grid_Label.Text = "Journey Stats:OFF";
                    break;
            }            
        }
    }
}

