using GraphQL.Types;
using OrchardCore.SongServices.ContentParts;

namespace OrchardCore.SongServices.GraphQL.Services.Trade;

// We add a model for the content part to the GraphQL schema. Content Types are added by Orchard Core automatically.
public class TradeFilteringPartObjectGraphType : ObjectGraphType<TradeFilteringPart>
{
    // These fields have counterparts in the index so we should include the same text in the
    // TradeFilteringPartWhereInputObjectGraphType without duplication.
    internal const string TradeTypeDescription = "Trade's TradeType.";
    internal const string PaymentMethodDescription = "Trade's PaymentMethod.";
    internal const string FeeTypeDescription = "Trade's FeeType.";
    internal const string OfferTypeDescription = "Trade's OfferType.";
    internal const string OfferWalletDescription = "Trade's OfferWallet.";
    internal const string DurationDescription = "Trade's Duration.";
    internal const string SellerContentIdDescription = "Trade's SellerContentId.";
    internal const string BuyerContentIdDescription = "Trade's BuyerContentId.";
    internal const string CurrencyOfTradeDescription = "Trade's CurrencyOfTrade.";
    internal const string FeeVNDAmountDescription = "Trade's FeeVNDAmount.";
    internal const string FeeBTCAmountDescription = "Trade's FeeBTCAmount.";
    internal const string FeeETHAmountDescription = "Trade's FeeETHAmount.";
    internal const string FeeUSDT20AmountDescription = "Trade's FeeUSDT20Amount.";
    internal const string TradeVNDAmountDescription = "Trade's TradeVNDAmount.";
    internal const string TradeBTCAmountDescription = "Trade's TradeBTCAmount.";
    internal const string TradeUSDT20AmountDescription = "Trade's TradeUSDT20Amount.";
    internal const string TradeETHAmountDescription = "Trade's TradeETHAmount.";
    internal const string SellerDescription = "Trade's Seller.";
    internal const string BuyerDescription = "Trade's Buyer.";
    internal const string TradeStatusDescription = "Trade's TradeStatus.";
    internal const string OfferIdDescription = "Trade's OfferId.";
    //internal const string DateTimeDescription = "Trade's Date";

    public TradeFilteringPartObjectGraphType()
    {
        Field(part => part.TradeType, nullable: true).Description(TradeTypeDescription);
        Field(part => part.PaymentMethod, nullable: true).Description(PaymentMethodDescription);
        Field(part => part.FeeType, nullable: true).Description(FeeTypeDescription);
        Field(part => part.OfferType, nullable: true).Description(OfferTypeDescription);
        Field(part => part.OfferWallet, nullable: true).Description(OfferWalletDescription);
        Field(part => part.Duration, nullable: true).Description(DurationDescription);
        Field(part => part.SellerContentId, nullable: true).Description(SellerContentIdDescription);
        Field(part => part.BuyerContentId, nullable: true).Description(BuyerContentIdDescription);
        Field(part => part.CurrencyOfTrade, nullable: true).Description(CurrencyOfTradeDescription);
        Field(part => part.FeeVNDAmount, nullable: true).Description(FeeVNDAmountDescription);
        Field(part => part.FeeBTCAmount, nullable: true).Description(FeeBTCAmountDescription);
        Field(part => part.FeeETHAmount, nullable: true).Description(FeeETHAmountDescription);
        Field(part => part.TradeVNDAmount, nullable: true).Description(TradeVNDAmountDescription);
        Field(part => part.TradeBTCAmount, nullable: true).Description(TradeBTCAmountDescription);
        Field(part => part.TradeUSDT20Amount, nullable: true).Description(TradeUSDT20AmountDescription);
        Field(part => part.TradeETHAmount, nullable: true).Description(TradeETHAmountDescription);
        Field(part => part.Seller, nullable: true).Description(SellerDescription);
        Field(part => part.Buyer, nullable: true).Description(BuyerDescription);
        Field(part => part.TradeStatus, nullable: true).Description(TradeStatusDescription);
        Field(part => part.OfferId, nullable: true).Description(OfferIdDescription);
        //Field(part => part.DateTime, nullable: true).Description(DateTimeDescription);
    }
}
