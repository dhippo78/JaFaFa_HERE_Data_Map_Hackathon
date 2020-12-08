using System;
using System.Collections.Generic;
using System.ComponentModel;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

using JaFaFa.Models;
using System.Linq;

namespace JaFaFa.Views
{
    // Learn more about making custom code visible in the Xamarin.Forms previewer
    // by visiting https://aka.ms/xamarinforms-previewer
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class FeedbackPage : ContentPage
    {
        public Item Item { get; set; }

        public FeedbackPage()
        {
            InitializeComponent();
        }
        async void Send_Clicked(object sender, EventArgs e)
        {
            MessagingCenter.Send(this, "AddItem", Item);
            await Navigation.PopAsync();
        }
        async void Cancel_Clicked(object sender, EventArgs e)
        {
            await Navigation.PopAsync();
        }        
        async void OnPreviousPageButtonClicked(object sender, EventArgs e)
        {
            await Navigation.PopAsync();
        }
    }
}