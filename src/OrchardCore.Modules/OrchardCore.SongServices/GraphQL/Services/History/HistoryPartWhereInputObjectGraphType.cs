using GraphQL.Types;
using OrchardCore.SongServices.Indexes;
using OrchardCore.SongServices.ContentParts;
using OrchardCore.Apis.GraphQL.Queries;
using static OrchardCore.SongServices.GraphQL.Services.History.HistoryPartObjectGraphType;

namespace OrchardCore.SongServices.GraphQL.Services.History;

// This class adds a content part-specific section in the "where" filter. It relies on the content part index to
// automatically generate YesSql logic for database-side filtering.
public class HistoryPartWhereInputObjectGraphType : WhereInputObjectGraphType<HistoryPart>
{
    public HistoryPartWhereInputObjectGraphType()
    {        
        // Since filters depend on the index, we use their "nameof" as reference.
        AddScalarFilterFields<StringGraphType>(nameof(HistoryPartIndex.Cards), CardsDescription);
        AddScalarFilterFields<StringGraphType>(nameof(HistoryPartIndex.Powerup), PowerupDescription);
        AddScalarFilterFields<StringGraphType>(nameof(HistoryPartIndex.Status), StatusDescription);
        AddScalarFilterFields<StringGraphType>(nameof(HistoryPartIndex.Kind), KindDescription);
        AddScalarFilterFields<StringGraphType>(nameof(HistoryPartIndex.Result), ResultDescription);
        AddScalarFilterFields<StringGraphType>(nameof(HistoryPartIndex.Round), RoundDescription);
        AddScalarFilterFields<StringGraphType>(nameof(HistoryPartIndex.RoundId), RoundIdDescription);
        AddScalarFilterFields<StringGraphType>(nameof(HistoryPartIndex.Shoe), ShoeDescription);
        AddScalarFilterFields<StringGraphType>(nameof(HistoryPartIndex.Table), TableDescription);
        AddScalarFilterFields<StringGraphType>(nameof(HistoryPartIndex.DateTime), DateTimeDescription);
    }
}

// NEXT STATION: Services/HistoryPartIndexAliasProvider.cs
