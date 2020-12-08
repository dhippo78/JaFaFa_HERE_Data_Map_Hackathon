using System;
using System.Collections.Generic;
using System.Text;
using Xamarin.Forms;
using Xamarin.Forms.Maps;

namespace JaFaFa.JaFaFaMAP
{
    public class jMAPPin : Pin
    {
        public string Name { get; set; }
        public string Url { get; set; }
        public ImageSource Icon { get; set; }
    }
}
