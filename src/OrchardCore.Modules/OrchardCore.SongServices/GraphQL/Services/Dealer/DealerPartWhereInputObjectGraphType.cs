using GraphQL.Types;
using OrchardCore.SongServices.Indexes;
using OrchardCore.SongServices.ContentParts;
using OrchardCore.Apis.GraphQL.Queries;
using static OrchardCore.SongServices.GraphQL.Services.Dealer.DealerPartObjectGraphType;

namespace OrchardCore.SongServices.GraphQL.Services.Dealer;

// This class adds a content part-specific section in the "where" filter. It relies on the content part index to
// automatically generate YesSql logic for database-side filtering.
public class DealerPartWhereInputObjectGraphType : WhereInputObjectGraphType<DealerPart>
{
    public DealerPartWhereInputObjectGraphType()
    {        
        // Since filters depend on the index, we use their "nameof" as reference.
        AddScalarFilterFields<StringGraphType>(nameof(DealerPartIndex.Name), NameDescription);
        AddScalarFilterFields<StringGraphType>(nameof(DealerPartIndex.Table), TableDescription);
        AddScalarFilterFields<StringGraphType>(nameof(DealerPartIndex.Value), ValueDescription);
        AddScalarFilterFields<StringGraphType>(nameof(DealerPartIndex.DateTime), DateTimeDescription);
    }
}

// NEXT STATION: Services/DealerPartIndexAliasProvider.cs
