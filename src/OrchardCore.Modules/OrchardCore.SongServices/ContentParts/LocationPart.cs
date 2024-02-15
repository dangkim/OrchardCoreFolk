using System;
using System.Collections.Generic;
using System.Text;
using OrchardCore.ContentManagement;

namespace OrchardCore.SongServices.ContentParts
{
    public class LocationPart : ContentPart
    {
        public string Country { get; set; }
        public string City { get; set; }
        public string Street { get; set; }
        public string Site { get; set; }
        public string Building { get; set; }
        public string Floor { get; set; }
        public string Zone { get; set; }
        public string Room { get; set; }
        public DateTime? DateTime { get; set; }
    }
}
