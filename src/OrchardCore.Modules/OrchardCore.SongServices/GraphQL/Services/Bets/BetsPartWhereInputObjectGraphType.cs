using GraphQL.Types;
using OrchardCore.SongServices.Indexes;
using OrchardCore.SongServices.ContentParts;
using OrchardCore.Apis.GraphQL.Queries;
using static OrchardCore.SongServices.GraphQL.Services.Bets.BetsPartObjectGraphType;

namespace OrchardCore.SongServices.GraphQL.Services.Bets;

// This class adds a content part-specific section in the "where" filter. It relies on the content part index to
// automatically generate YesSql logic for database-side filtering.
public class BetsPartWhereInputObjectGraphType : WhereInputObjectGraphType<BetsPart>
{
    public BetsPartWhereInputObjectGraphType()
    {        
        // Since filters depend on the index, we use their "nameof" as reference.
        AddScalarFilterFields<StringGraphType>(nameof(BetsPartIndex.Kind), KindDescription);
        AddScalarFilterFields<StringGraphType>(nameof(BetsPartIndex.Code), CodeDescription);
        AddScalarFilterFields<StringGraphType>(nameof(BetsPartIndex.Name), NameDescription);
        AddScalarFilterFields<StringGraphType>(nameof(BetsPartIndex.Chips), ChipsDescription);
        AddScalarFilterFields<StringGraphType>(nameof(BetsPartIndex.Id), IdDescription);
        AddScalarFilterFields<DecimalGraphType>(nameof(BetsPartIndex.Max), MaxDescription);
        AddScalarFilterFields<DecimalGraphType>(nameof(BetsPartIndex.Min), MinDescription);
        AddScalarFilterFields<BooleanGraphType>(nameof(BetsPartIndex.Allowed), AllowedDescription);
        AddScalarFilterFields<StringGraphType>(nameof(BetsPartIndex.Table), TableDescription);
        AddScalarFilterFields<StringGraphType>(nameof(BetsPartIndex.DateTime), DateTimeDescription);
    }
}

// NEXT STATION: Services/BetsPartIndexAliasProvider.cs
