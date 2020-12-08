using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SlideOverKit;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

using Xamarin.Essentials;
using JaFaFa.PoI;

namespace JaFaFa.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class LeftSideMasterView : SlideMenuView
    {
        public LeftSideMasterView()
        {
            InitializeComponent();

            // You must set IsFullScreen in this case, 
            // otherwise you need to set HeightRequest, 
            // just like the QuickInnerMenu sample
            this.IsFullScreen = true;
            // You must set WidthRequest in this case
            this.WidthRequest = 250;
            this.MenuOrientations = MenuOrientation.LeftToRight;

            // You must set BackgroundColor, 
            // and you cannot put another layout with background color cover the whole View
            // otherwise, it cannot be dragged on Android
            this.BackgroundColor = Color.LemonChiffon;
            //this.BackgroundColor = Xamarin.Forms.Color.FromHex("#D8DDE7");

            // This is shadow view color, you can set a transparent color
            this.BackgroundViewColor = Color.FromHex("#CE766C");            
        }
        private void Compass_Calibration_Grid_Clicked(object sender, EventArgs e)
        {
            if (Preferences.Get("Compass", 0) == 1)
            {
                Navigation.PushAsync(new CalibrationPage());
            }
            else
            {                
                App.Current.MainPage.DisplayAlert("Compass", "Turn on the Compass in Settings", "OK");
            }            
        }
        private void Terms_and_Conditions_Grid_Clicked(object sender, EventArgs e)
        {

        }
        private void Credits_Grid_Clicked(object sender, EventArgs e)
        {
            try
            {
                Navigation.PushAsync(new PoI.PoI());
            }
            catch (Exception ex)
            {
                // For debugging

                //MessageBox.Show(ex.Message, AppResources.ApplicationTitle, MessageBoxButton.OK);
                DisplayAlert("Credits_Grid_Clicked", ex.ToString(), "Ok");
            }
        }

        private void DisplayAlert(string v1, string v2, string v3)
        {
            throw new NotImplementedException();
        }

        private void Feedback_Grid_Clicked(object sender, EventArgs e)
        {
            Navigation.PushAsync(new FeedbackPage());
        }
        private void About_Grid_Clicked(object sender, EventArgs e)
        {
            Navigation.PushAsync(new AboutPage());
        }
    }
}