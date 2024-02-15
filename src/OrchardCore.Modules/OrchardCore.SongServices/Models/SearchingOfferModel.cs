using System;
using System.Collections.Generic;
using System.Text;

namespace OrchardCore.SongServices.Models
{
    public class SearchingOfferModel
    {
        public string Wallet { get; set; }
        public string UserName { get; set; }
        public string Currency { get; set; }
        public string Method { get; set; }
        public string OfferType { get; set; }
        public decimal MinAmount { get; set; }
        public decimal MaxAmount { get; set; }
        public int Take { get; set; }
    }
}
