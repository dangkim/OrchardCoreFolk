using GraphQL.Types;
using OrchardCore.SongServices.ContentParts;

namespace OrchardCore.SongServices.GraphQL.Services.Location;

// We add a model for the content part to the GraphQL schema. Content Types are added by Orchard Core automatically.
public class LocationPartObjectGraphType : ObjectGraphType<LocationPart>
{
    // These fields have counterparts in the index so we should include the same text in the
    // LocationPartWhereInputObjectGraphType without duplication.
    internal const string CountryDescription = "The Location's country.";
    internal const string CityDescription = "The Location's city.";
    internal const string StreetDescription = "The Location's street.";
    internal const string SiteDescription = "The Location's site.";
    internal const string BuildingDescription = "The Location's building.";
    internal const string FloorDescription = "The Location's floor.";
    internal const string ZoneDescription = "The Location's zone.";
    internal const string RoomDescription = "The Location's room.";
    internal const string DateTimeDescription = "The Location's date";

    public LocationPartObjectGraphType()
    {
        Field(part => part.Country, nullable: true).Description(CountryDescription);
        Field(part => part.City, nullable: true).Description(CityDescription);
        Field(part => part.Street, nullable: true).Description(StreetDescription);
        Field(part => part.Site, nullable: true).Description(SiteDescription);
        Field(part => part.Building, nullable: true).Description(BuildingDescription);
        Field(part => part.Floor, nullable: true).Description(FloorDescription);
        Field(part => part.Zone, nullable: true).Description(ZoneDescription);
        Field(part => part.Room, nullable: true).Description(RoomDescription);
        Field(part => part.DateTime, nullable: true).Description(DateTimeDescription);
    }
}
