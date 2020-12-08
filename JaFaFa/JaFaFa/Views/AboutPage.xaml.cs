using System;
using System.ComponentModel;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace JaFaFa.Views
{
    // Learn more about JáFáFá by visiting https://jafafa.modecomint.com
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class AboutPage : ContentPage
    {
        public AboutPage()
        {
            InitializeComponent();

            this.BackgroundColor = Color.LemonChiffon;
        }
    }
}