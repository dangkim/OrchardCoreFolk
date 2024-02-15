using OrchardCore.ContentManagement;
using System;
using System.Collections.Generic;
using System.Text;

namespace OrchardCore.SongServices.Models
{
    public class RegisterOfferModel
    {
        public ContentItem Offer { get; set; }

        public string TraderContentId { get; set; }
    }
}
