using GraphQL.Types;
using OrchardCore.SongServices.Indexes;
using OrchardCore.SongServices.ContentParts;
using OrchardCore.Apis.GraphQL.Queries;
using static OrchardCore.SongServices.GraphQL.Services.Trade.TradeFilteringPartObjectGraphType;
using OrchardCore.SongServices.Indexing;

namespace OrchardCore.SongServices.GraphQL.Services.Offer;

// This class adds a content part-specific section in the "where" filter. It relies on the content part index to
// automatically generate YesSql logic for database-side filtering.
public class TradeFilteringPartWhereInputObjectGraphType : WhereInputObjectGraphType<TradeFilteringPart>
{
    public TradeFilteringPartWhereInputObjectGraphType()
    {        
        // Since filters depend on the index, we use their "nameof" as reference.
        AddScalarFilterFields<StringGraphType>(nameof(TradeFilteringPartIndex.TradeType), TradeTypeDescription);
        AddScalarFilterFields<StringGraphType>(nameof(TradeFilteringPartIndex.PaymentMethod), PaymentMethodDescription);
        AddScalarFilterFields<IntGraphType>(nameof(TradeFilteringPartIndex.FeeType), FeeTypeDescription);
        AddScalarFilterFields<StringGraphType>(nameof(TradeFilteringPartIndex.OfferType), OfferTypeDescription);
        AddScalarFilterFields<StringGraphType>(nameof(TradeFilteringPartIndex.OfferWallet), OfferWalletDescription);
        AddScalarFilterFields<IntGraphType>(nameof(TradeFilteringPartIndex.Duration), DurationDescription);
        AddScalarFilterFields<StringGraphType>(nameof(TradeFilteringPartIndex.SellerContentId), SellerContentIdDescription);
        AddScalarFilterFields<StringGraphType>(nameof(TradeFilteringPartIndex.BuyerContentId), BuyerContentIdDescription);
        AddScalarFilterFields<StringGraphType>(nameof(TradeFilteringPartIndex.CurrencyOfTrade), CurrencyOfTradeDescription);
        AddScalarFilterFields<DecimalGraphType>(nameof(TradeFilteringPartIndex.FeeVNDAmount), FeeVNDAmountDescription);
        AddScalarFilterFields<DecimalGraphType>(nameof(TradeFilteringPartIndex.FeeBTCAmount), FeeBTCAmountDescription);
        AddScalarFilterFields<DecimalGraphType>(nameof(TradeFilteringPartIndex.FeeETHAmount), FeeETHAmountDescription);
        AddScalarFilterFields<DecimalGraphType>(nameof(TradeFilteringPartIndex.FeeUSDT20Amount), FeeUSDT20AmountDescription);
        AddScalarFilterFields<DecimalGraphType>(nameof(TradeFilteringPartIndex.TradeVNDAmount), TradeVNDAmountDescription);
        AddScalarFilterFields<DecimalGraphType>(nameof(TradeFilteringPartIndex.TradeBTCAmount), TradeBTCAmountDescription);
        AddScalarFilterFields<DecimalGraphType>(nameof(TradeFilteringPartIndex.TradeUSDT20Amount), TradeUSDT20AmountDescription);
        AddScalarFilterFields<DecimalGraphType>(nameof(TradeFilteringPartIndex.TradeETHAmount), TradeETHAmountDescription);
        AddScalarFilterFields<StringGraphType>(nameof(TradeFilteringPartIndex.Seller), SellerDescription);
        AddScalarFilterFields<StringGraphType>(nameof(TradeFilteringPartIndex.Buyer), BuyerDescription);
        AddScalarFilterFields<StringGraphType>(nameof(TradeFilteringPartIndex.Status), TradeStatusDescription);
        AddScalarFilterFields<StringGraphType>(nameof(TradeFilteringPartIndex.OfferId), OfferIdDescription);
    }
}

// NEXT STATION: Services/TradeFilteringPartIndexAliasProvider.cs
