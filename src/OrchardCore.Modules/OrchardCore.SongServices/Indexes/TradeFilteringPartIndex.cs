using System;
using OrchardCore.ContentManagement;
using OrchardCore.SongServices.ContentParts;
using YesSql.Indexes;

namespace OrchardCore.SongServices.Indexing
{
    public class TradeFilteringPartIndex : MapIndex
    {
        public string ContentItemId { get; set; }
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
        public string Status { get; set; }
        public string OfferId { get; set; }
        public DateTime? DateTime { get; set; }
    }

    public class TradeFilteringPartIndexProvider : IndexProvider<ContentItem>
    {
        public override void Describe(DescribeContext<ContentItem> context) =>
            context.For<TradeFilteringPartIndex>().Map(contentItem =>
            {
                var tradeMinMaxPart = contentItem.As<TradeFilteringPart>();

                return tradeMinMaxPart == null
                    ? null
                    : new TradeFilteringPartIndex
                    {
                        ContentItemId = contentItem.ContentItemId,
                        Buyer = tradeMinMaxPart.Buyer,
                        BuyerContentId = tradeMinMaxPart.BuyerContentId,
                        OfferId = tradeMinMaxPart.OfferId,
                        Status = tradeMinMaxPart.TradeStatus,
                        CurrencyOfTrade = tradeMinMaxPart.CurrencyOfTrade,
                        Duration = tradeMinMaxPart.Duration,
                        FeeBTCAmount = tradeMinMaxPart.FeeBTCAmount,
                        FeeETHAmount = tradeMinMaxPart.FeeETHAmount,
                        FeeType = tradeMinMaxPart.FeeType,
                        FeeUSDT20Amount = tradeMinMaxPart.FeeUSDT20Amount,
                        FeeVNDAmount = tradeMinMaxPart.FeeVNDAmount,
                        PaymentMethod = tradeMinMaxPart.PaymentMethod,
                        OfferType = tradeMinMaxPart.OfferType,
                        OfferWallet = tradeMinMaxPart.OfferWallet,
                        Seller = tradeMinMaxPart.Seller,
                        SellerContentId = tradeMinMaxPart.SellerContentId,
                        TradeBTCAmount = tradeMinMaxPart.TradeBTCAmount,
                        TradeETHAmount = tradeMinMaxPart.TradeETHAmount,
                        TradeUSDT20Amount = tradeMinMaxPart.TradeUSDT20Amount,
                        TradeVNDAmount = tradeMinMaxPart.TradeVNDAmount,
                        TradeType = tradeMinMaxPart.TradeType,
                        DateTime = tradeMinMaxPart.DateTime,
                    };
            });
    }
}
