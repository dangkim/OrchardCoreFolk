using GraphQL.Types;
using OrchardCore.SongServices.Indexes;
using OrchardCore.SongServices.ContentParts;
using OrchardCore.Apis.GraphQL.Queries;
using static OrchardCore.SongServices.GraphQL.Services.Cards.CardsPartObjectGraphType;

namespace OrchardCore.SongServices.GraphQL.Services.Cards;

// This class adds a content part-specific section in the "where" filter. It relies on the content part index to
// automatically generate YesSql logic for database-side filtering.
public class CardsPartWhereInputObjectGraphType : WhereInputObjectGraphType<CardsPart>
{
    public CardsPartWhereInputObjectGraphType()
    {        
        // Since filters depend on the index, we use their "nameof" as reference.
        AddScalarFilterFields<StringGraphType>(nameof(CardsPartIndex.Kind), KindDescription);
        AddScalarFilterFields<StringGraphType>(nameof(CardsPartIndex.Card), CardDescription);
        AddScalarFilterFields<StringGraphType>(nameof(CardsPartIndex.Pos), PosDescription);
        AddScalarFilterFields<StringGraphType>(nameof(CardsPartIndex.Table), TableDescription);
        AddScalarFilterFields<StringGraphType>(nameof(CardsPartIndex.DateTime), DateTimeDescription);
    }
}

// NEXT STATION: Services/CardsPartIndexAliasProvider.cs
