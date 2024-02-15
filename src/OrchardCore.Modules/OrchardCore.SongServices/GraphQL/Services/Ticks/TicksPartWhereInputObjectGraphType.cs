using GraphQL.Types;
using OrchardCore.SongServices.Indexes;
using OrchardCore.SongServices.ContentParts;
using OrchardCore.Apis.GraphQL.Queries;
using static OrchardCore.SongServices.GraphQL.Services.Ticks.TicksPartObjectGraphType;

namespace OrchardCore.SongServices.GraphQL.Services.Ticks;

// This class adds a content part-specific section in the "where" filter. It relies on the content part index to
// automatically generate YesSql logic for database-side filtering.
public class TicksPartWhereInputObjectGraphType : WhereInputObjectGraphType<TicksPart>
{
    public TicksPartWhereInputObjectGraphType()
    {        
        // Since filters depend on the index, we use their "nameof" as reference.
        AddScalarFilterFields<StringGraphType>(nameof(TicksPartIndex.Table), TableDescription);
        AddScalarFilterFields<StringGraphType>(nameof(TicksPartIndex.Value), ValueDescription);
        AddScalarFilterFields<StringGraphType>(nameof(TicksPartIndex.DateTime), DateTimeDescription);
    }
}
