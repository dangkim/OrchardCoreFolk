using System;
using System.Collections.Generic;
using System.Text;
using OrchardCore.ContentManagement;

namespace OrchardCore.SongServices.ContentParts
{
    public class OfferFilteringPart : ContentPart
    {
        public decimal MinAmount { get; set; }
        public decimal MaxAmount { get; set; }
        public string OfferStatus { get; set; }
        public string Wallet { get; set; }
        public string PaymentMethod { get; set; }
        public string PreferredCurrency { get; set; }
        public decimal Percentage { get; set; }
        public string OfferType { get; set; }
        public string OfferLabel { get; set; }
        public string OfferTerms { get; set; }
        public string TradeInstructions { get; set; }
        public decimal OfferPrice { get; set; }
        public decimal OfferGet { get; set; }
        public decimal CurrentRate { get; set; }
        public decimal EscrowFee { get; set; }
        public DateTime? DateTime { get; set; }
    }
}
