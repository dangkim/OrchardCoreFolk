using GraphQL.Types;
using OrchardCore.SongServices.Indexes;
using OrchardCore.SongServices.ContentParts;
using OrchardCore.Apis.GraphQL.Queries;
using static OrchardCore.SongServices.GraphQL.Services.Offer.OfferFilteringPartObjectGraphType;
using OrchardCore.SongServices.Indexing;

namespace OrchardCore.SongServices.GraphQL.Services.Offer;

// This class adds a content part-specific section in the "where" filter. It relies on the content part index to
// automatically generate YesSql logic for database-side filtering.
public class OfferFilteringPartWhereInputObjectGraphType : WhereInputObjectGraphType<OfferFilteringPart>
{
    public OfferFilteringPartWhereInputObjectGraphType()
    {        
        // Since filters depend on the index, we use their "nameof" as reference.
        AddScalarFilterFields<DecimalGraphType>(nameof(OfferFilteringPartIndex.MinAmount), MinAmountDescription);
        AddScalarFilterFields<DecimalGraphType>(nameof(OfferFilteringPartIndex.MaxAmount), MaxAmountDescription);
        AddScalarFilterFields<StringGraphType>(nameof(OfferFilteringPartIndex.Status), OfferStatusDescription);
        AddScalarFilterFields<StringGraphType>(nameof(OfferFilteringPartIndex.Wallet), WalletDescription);
        AddScalarFilterFields<StringGraphType>(nameof(OfferFilteringPartIndex.PaymentMethod), PaymentMethodDescription);
        AddScalarFilterFields<StringGraphType>(nameof(OfferFilteringPartIndex.PreferredCurrency), PreferredCurrencyDescription);
        AddScalarFilterFields<DecimalGraphType>(nameof(OfferFilteringPartIndex.Percentage), PercentageDescription);
        AddScalarFilterFields<StringGraphType>(nameof(OfferFilteringPartIndex.OfferType), OfferTypeDescription);
    }
}

// NEXT STATION: Services/OfferFilteringPartIndexAliasProvider.cs
