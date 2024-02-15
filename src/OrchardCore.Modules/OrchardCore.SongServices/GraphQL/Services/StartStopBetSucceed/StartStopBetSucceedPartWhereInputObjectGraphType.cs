using GraphQL.Types;
using OrchardCore.SongServices.Indexes;
using OrchardCore.SongServices.ContentParts;
using OrchardCore.Apis.GraphQL.Queries;
using static OrchardCore.SongServices.GraphQL.Services.StartStopBetSucceed.StartStopBetSucceedPartObjectGraphType;

namespace OrchardCore.SongServices.GraphQL.Services.StartStopBetSucceed;

// This class adds a content part-specific section in the "where" filter. It relies on the content part index to
// automatically generate YesSql logic for database-side filtering.
public class StartStopBetSucceedPartWhereInputObjectGraphType : WhereInputObjectGraphType<StartStopBetSucceedPart>
{
    public StartStopBetSucceedPartWhereInputObjectGraphType()
    {        
        // Since filters depend on the index, we use their "nameof" as reference.
        AddScalarFilterFields<StringGraphType>(nameof(StartStopBetSucceedPartIndex.Powerup), PowerupDescription);
        AddScalarFilterFields<StringGraphType>(nameof(StartStopBetSucceedPartIndex.Kind), KindDescription);
        AddScalarFilterFields<StringGraphType>(nameof(StartStopBetSucceedPartIndex.Content), ContentDescription);
        AddScalarFilterFields<StringGraphType>(nameof(StartStopBetSucceedPartIndex.Round), RoundDescription);
        AddScalarFilterFields<StringGraphType>(nameof(StartStopBetSucceedPartIndex.RoundId), RoundIdDescription);
        AddScalarFilterFields<StringGraphType>(nameof(StartStopBetSucceedPartIndex.Shoe), ShoeDescription);
        AddScalarFilterFields<StringGraphType>(nameof(StartStopBetSucceedPartIndex.Status), StatusDescription);
        AddScalarFilterFields<StringGraphType>(nameof(StartStopBetSucceedPartIndex.DateTime), DateTimeDescription);
    }
}

// NEXT STATION: Services/StartStopBetSucceedPartIndexAliasProvider.cs
