using System;
using System.Collections.Generic;
using System.Text;

namespace OrchardCore.SimService.ApiModels
{
    public class OfferId
    {
        public string Text { get; set; }

    }
    public class FeeType
    {
        public int Text { get; set; }

    }
    public class Duration
    {
        public int Text { get; set; }

    }
    public class Status
    {
        public string Text { get; set; }

    }
    public class Seller
    {
        public string Text { get; set; }

    }
    public class SellerContentId
    {
        public string Text { get; set; }

    }
    public class Buyer
    {
        public string Text { get; set; }

    }
    public class BuyerContentId
    {
        public string Text { get; set; }

    }
    public class FeeETHAmount
    {
        public string Text { get; set; }

    }
    public class TradeETHAmount
    {
        public string Text { get; set; }

    }
    public class AmountCurrencyFromETH
    {
        public string Text { get; set; }

    }
    public class FeeBTCAmount
    {
        public string Text { get; set; }

    }
    public class TradeBTCAmount
    {
        public string Text { get; set; }

    }
    public class AmountCurrencyFromBTC
    {
        public string Text { get; set; }

    }
    public class FeeUSDT20Amount
    {
        public string Text { get; set; }

    }
    public class TradeUSDT20Amount
    {
        public string Text { get; set; }

    }
    public class AmountCurrencyFromUSDT20
    {
        public string Text { get; set; }

    }
    public class FeeVNDAmount
    {
        public string Text { get; set; }

    }
    public class TradeVNDAmount
    {
        public string Text { get; set; }

    }
    public class AmountCurrencyFromVND
    {
        public string Text { get; set; }

    }
    public class CurrencyOfTrade
    {
        public string Text { get; set; }

    }
    public class OfferType
    {
        public string Text { get; set; }

    }
    public class CreateDate
    {
        public string Text { get; set; }

    }
    public class TradeContent
    {
        public OfferId OfferId { get; set; }
        public FeeType FeeType { get; set; }
        public Duration Duration { get; set; }
        public Status Status { get; set; }
        public Seller Seller { get; set; }
        public SellerContentId SellerContentId { get; set; }
        public Buyer Buyer { get; set; }
        public BuyerContentId BuyerContentId { get; set; }
        public FeeETHAmount FeeETHAmount { get; set; }
        public TradeETHAmount TradeETHAmount { get; set; }
        public AmountCurrencyFromETH AmountCurrencyFromETH { get; set; }
        public FeeBTCAmount FeeBTCAmount { get; set; }
        public TradeBTCAmount TradeBTCAmount { get; set; }
        public AmountCurrencyFromBTC AmountCurrencyFromBTC { get; set; }
        public FeeUSDT20Amount FeeUSDT20Amount { get; set; }
        public TradeUSDT20Amount TradeUSDT20Amount { get; set; }
        public AmountCurrencyFromUSDT20 AmountCurrencyFromUSDT20 { get; set; }
        public FeeVNDAmount FeeVNDAmount { get; set; }
        public TradeVNDAmount TradeVNDAmount { get; set; }
        public AmountCurrencyFromVND AmountCurrencyFromVND { get; set; }
        public CurrencyOfTrade CurrencyOfTrade { get; set; }
        public OfferType OfferType { get; set; }
        public CreateDate CreateDate { get; set; }

    }
    public class TitlePart
    {

    }
    public class TradeModel
    {
        public string ContentItemId { get; set; }
        public string ContentItemVersionId { get; set; }
        public string ContentType { get; set; }
        public string DisplayText { get; set; }
        public bool Latest { get; set; }
        public bool Published { get; set; }
        public DateTime ModifiedUtc { get; set; }
        public DateTime PublishedUtc { get; set; }
        public DateTime CreatedUtc { get; set; }
        public string Owner { get; set; }
        public string Author { get; set; }
        public TradeContent Trade { get; set; }
        public TitlePart TitlePart { get; set; }

    }
}
