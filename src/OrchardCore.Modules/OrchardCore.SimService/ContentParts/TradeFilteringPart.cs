using System;
using System.Collections.Generic;
using System.Text;
using OrchardCore.ContentManagement;

namespace OrchardCore.SongServices.ContentParts
{
    public class TradeFilteringPart : ContentPart
    {
        public string TradeType { get; set; }
        public string PaymentMethod { get; set; }
        public int FeeType { get; set; }
        public string OfferType { get; set; }
        public string OfferWallet { get; set; }
        public int Duration { get; set; }
        public string SellerContentId { get; set; }
        public string BuyerContentId { get; set; }
        public string CurrencyOfTrade { get; set; }
        public decimal FeeVNDAmount { get; set; }
        public decimal FeeBTCAmount { get; set; }
        public decimal FeeETHAmount { get; set; }
        public decimal FeeUSDT20Amount { get; set; }
        public decimal TradeVNDAmount { get; set; }
        public decimal TradeBTCAmount { get; set; }
        public decimal TradeUSDT20Amount { get; set; }
        public decimal TradeETHAmount { get; set; }
        public string Seller { get; set; }
        public string Buyer { get; set; }
        public string TradeStatus { get; set; }
        public string OfferId { get; set; }
        public string BlockExplorerLink { get; set; }
        public DateTime? DateTime { get; set; }
    }
}
