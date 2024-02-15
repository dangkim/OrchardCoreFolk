using GraphQL.Types;
using OrchardCore.SongServices.Indexes;
using OrchardCore.SongServices.ContentParts;
using OrchardCore.Apis.GraphQL.Queries;
using static OrchardCore.SongServices.GraphQL.Services.Location.LocationPartObjectGraphType;

namespace OrchardCore.SongServices.GraphQL.Services.Location;

// This class adds a content part-specific section in the "where" filter. It relies on the content part index to
// automatically generate YesSql logic for database-side filtering.
public class LocationPartWhereInputObjectGraphType : WhereInputObjectGraphType<LocationPart>
{
    public LocationPartWhereInputObjectGraphType()
    {        
        // Since filters depend on the index, we use their "nameof" as reference.
        AddScalarFilterFields<StringGraphType>(nameof(LocationPartIndex.Country), CountryDescription);
        AddScalarFilterFields<StringGraphType>(nameof(LocationPartIndex.City), CityDescription);
        AddScalarFilterFields<StringGraphType>(nameof(LocationPartIndex.Street), StreetDescription);
        AddScalarFilterFields<StringGraphType>(nameof(LocationPartIndex.Site), SiteDescription);
        AddScalarFilterFields<StringGraphType>(nameof(LocationPartIndex.Building), BuildingDescription);
        AddScalarFilterFields<StringGraphType>(nameof(LocationPartIndex.Floor), FloorDescription);
        AddScalarFilterFields<StringGraphType>(nameof(LocationPartIndex.Zone), ZoneDescription);
        AddScalarFilterFields<StringGraphType>(nameof(LocationPartIndex.Room), RoomDescription);
        AddScalarFilterFields<StringGraphType>(nameof(LocationPartIndex.DateTime), DateTimeDescription);
    }
}

// NEXT STATION: Services/LocationPartIndexAliasProvider.cs
