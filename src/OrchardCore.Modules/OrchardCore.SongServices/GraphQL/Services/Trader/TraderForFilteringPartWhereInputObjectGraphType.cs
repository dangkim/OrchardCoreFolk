using GraphQL.Types;
using OrchardCore.SongServices.Indexes;
using OrchardCore.SongServices.ContentParts;
using OrchardCore.Apis.GraphQL.Queries;
using static OrchardCore.SongServices.GraphQL.Services.Trader.TraderForFilteringPartObjectGraphType;
using OrchardCore.SongServices.Indexing;

namespace OrchardCore.SongServices.GraphQL.Services.Trader;

// This class adds a content part-specific section in the "where" filter. It relies on the content part index to
// automatically generate YesSql logic for database-side filtering.
public class TraderForFilteringPartWhereInputObjectGraphType : WhereInputObjectGraphType<TraderForFilteringPart>
{
    public TraderForFilteringPartWhereInputObjectGraphType()
    {        
        // Since filters depend on the index, we use their "nameof" as reference.
        AddScalarFilterFields<StringGraphType>(nameof(TraderForFilteringPartIndex.UserId), UserIdDescription);
        AddScalarFilterFields<StringGraphType>(nameof(TraderForFilteringPartIndex.ChatIdTele), ChatIdTeleDescription);
        AddScalarFilterFields<StringGraphType>(nameof(TraderForFilteringPartIndex.Name), NameDescription);
        AddScalarFilterFields<DecimalGraphType>(nameof(TraderForFilteringPartIndex.BondVndBalance), BondVndBalanceDescription);
        AddScalarFilterFields<DecimalGraphType>(nameof(TraderForFilteringPartIndex.VndBalance), VndBalanceDescription);
        AddScalarFilterFields<DecimalGraphType>(nameof(TraderForFilteringPartIndex.BTCBalance), BTCBalanceDescription);
        AddScalarFilterFields<DecimalGraphType>(nameof(TraderForFilteringPartIndex.ETHBalance), ETHBalanceDescription);
        AddScalarFilterFields<DecimalGraphType>(nameof(TraderForFilteringPartIndex.USDT20Balance), USDT20BalanceDescription);
        AddScalarFilterFields<StringGraphType>(nameof(TraderForFilteringPartIndex.ReferenceCode), ReferenceCodeDescription);
        AddScalarFilterFields<StringGraphType>(nameof(TraderForFilteringPartIndex.IsActivatedTele), IsActivatedTeleDescription);
        AddScalarFilterFields<StringGraphType>(nameof(TraderForFilteringPartIndex.DeviceId), DeviceIdDescription);
        AddScalarFilterFields<StringGraphType>(nameof(TraderForFilteringPartIndex.DateTime), DateTimeDescription);
    }
}

// NEXT STATION: Services/TraderForFilteringPartIndexAliasProvider.cs
