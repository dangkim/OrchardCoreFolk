using GraphQL.Types;
using OrchardCore.SongServices.Indexes;
using OrchardCore.SongServices.ContentParts;
using OrchardCore.Apis.GraphQL.Queries;
using static OrchardCore.SongServices.GraphQL.Services.Pool.PoolPartObjectGraphType;

namespace OrchardCore.SongServices.GraphQL.Services.Pool;

// This class adds a content part-specific section in the "where" filter. It relies on the content part index to
// automatically generate YesSql logic for database-side filtering.
public class PoolPartWhereInputObjectGraphType : WhereInputObjectGraphType<PoolPart>
{
    public PoolPartWhereInputObjectGraphType()
    {        
        // Since filters depend on the index, we use their "nameof" as reference.
        AddScalarFilterFields<StringGraphType>(nameof(PoolPartIndex.Amount), AmountDescription);
        AddScalarFilterFields<StringGraphType>(nameof(PoolPartIndex.Cat), CatDescription);
        AddScalarFilterFields<StringGraphType>(nameof(PoolPartIndex.Table), TableDescription);
        AddScalarFilterFields<StringGraphType>(nameof(PoolPartIndex.DateTime), DateTimeDescription);
    }
}

// NEXT STATION: Services/PoolPartIndexAliasProvider.cs
