using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using OrchardCore.SongServices.ContentParts;

namespace OrchardCore.SongServices.ViewModels
{
    public class OfferFilteringPartViewModel
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

        [BindNever]
        public OfferFilteringPart OfferFilteringPart { get; set; }
    }
}
