using SlideOverKit;
using System;
using System.ComponentModel;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace JaFaFa.Views
{
    // Learn more about making custom code visible in the Xamarin.Forms previewer
    // by visiting https://aka.ms/xamarinforms-previewer    
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class PopupPage : SlidePopupView
    {   
        public PopupPage(String title, String message)
        {
            InitializeComponent();

            this.BackgroundColor = Color.White;
            // If you set Target attached property, you should not set TopMargin and LeftMargin
            // But you need to set Width and Height request
            this.WidthRequest = 200;
            this.HeightRequest = 300;

            DoneButton.Clicked += (object sender, EventArgs e) => {
                this.HideMySelf();
            };
            
            //this.Title = title;

            //this.IsVisible = true;

            //DisplayAlert("Compass", "Turn on the Compass in Settings", "OK");
        }        
    }
}