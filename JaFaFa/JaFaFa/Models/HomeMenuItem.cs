using System;
using System.Collections.Generic;
using System.Text;

namespace JaFaFa.Models
{
    public enum MenuItemType
    {
        Location_Services,
        Online_Access,
        Compass,
        Traffic_Legend,
        Journey_Statistics,
        Compass_Callibration,
        Terms_And_Conditions,
        Credits,
        Feedback,
        About,
        Go_Back
    }
    public class HomeMenuItem
    {
        public MenuItemType Id { get; set; }

        public string Title { get; set; }
    }
}
