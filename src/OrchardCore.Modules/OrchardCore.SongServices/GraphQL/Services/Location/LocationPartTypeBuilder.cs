using GraphQL.Types;
using OrchardCore.ContentManagement.GraphQL.Queries.Types;
using OrchardCore.ContentManagement.Metadata.Models;
using System.Linq;

namespace OrchardCore.SongServices.GraphQL.Services.Location;

// Services that implement IContentTypeBuilder extend the features of existing ContentItem type fields, including the
// top level fields automatically created by Orchard Core for every content type. You can use this to add new sub-fields
// or filter attributes to existing ContentItem type fields.
public class LocationPartTypeBuilder : IContentTypeBuilder
{
    // It's a good practice to make the argument name a const because you will reuse it in the IGraphQLFilter.
    public const string CountryFilter = "country";
    public const string CityFilter = "city";
    public const string StreetFilter = "street";
    public const string SiteFilter = "site";
    public const string BuildingFilter = "building";
    public const string FloorFilter = "floor";
    public const string ZoneFilter = "zone";
    public const string RoomFilter = "room";
    public const string DateTimeFilter = "date";

    // Here you can add arguments to every Content Type (top level) field.
    public void Build(
        FieldType contentQuery,
        ContentTypeDefinition contentTypeDefinition,
        ContentItemType contentItemType)
    {
        // You can check to see if the field has any specific sub-field, if you want to rely on its features. For
        // example if you only want to apply to ContentItem fields that have the "person" sub-field (i.e. those that
        // have a PersonPart). This is useful if you want to expand your content part field in another module.

        if (contentItemType.Name != ContentTypes.LocationPage) return;

        contentQuery.Arguments.Add(new QueryArgument<StringGraphType>
        {
            Name = CountryFilter,
            ResolvedType = new StringGraphType(),
        });

        contentQuery.Arguments.Add(new QueryArgument<StringGraphType>
        {
            Name = CityFilter,
            ResolvedType = new StringGraphType(),
        });

        contentQuery.Arguments.Add(new QueryArgument<StringGraphType>
        {
            Name = StreetFilter,
            ResolvedType = new StringGraphType(),
        });

        contentQuery.Arguments.Add(new QueryArgument<StringGraphType>
        {
            Name = SiteFilter,
            ResolvedType = new StringGraphType(),
        });

        contentQuery.Arguments.Add(new QueryArgument<StringGraphType>
        {
            Name = BuildingFilter,
            ResolvedType = new StringGraphType(),
        });

        contentQuery.Arguments.Add(new QueryArgument<StringGraphType>
        {
            Name = FloorFilter,
            ResolvedType = new StringGraphType(),
        });

        contentQuery.Arguments.Add(new QueryArgument<StringGraphType>
        {
            Name = ZoneFilter,
            ResolvedType = new StringGraphType(),
        });

        contentQuery.Arguments.Add(new QueryArgument<StringGraphType>
        {
            Name = RoomFilter,
            ResolvedType = new StringGraphType(),
        });

        contentQuery.Arguments.Add(new QueryArgument<StringGraphType>
        {
            Name = DateTimeFilter,
            ResolvedType = new StringGraphType(),
        });

        /*------------------------------------------------------------*/
        AddFilterCountry(contentQuery, "_ne");
        AddFilterCity(contentQuery, "_ne");
        AddFilterStreet(contentQuery, "_ne");
        AddFilterSite(contentQuery, "_ne");
        AddFilterBuilding(contentQuery, "_ne");
        AddFilterFloor(contentQuery, "_ne");
        AddFilterZone(contentQuery, "_ne");
        AddFilterRoom(contentQuery, "_ne");
    }

    private static void AddFilterCountry(FieldType contentQuery, string suffix) =>
        contentQuery.Arguments.Add(new QueryArgument<StringGraphType>
        {
            Name = CountryFilter + suffix,
            ResolvedType = new StringGraphType(),
        });

    private static void AddFilterCity(FieldType contentQuery, string suffix) =>
        contentQuery.Arguments.Add(new QueryArgument<StringGraphType>
        {
            Name = CityFilter + suffix,
            ResolvedType = new StringGraphType(),
        });

    private static void AddFilterStreet(FieldType contentQuery, string suffix) =>
        contentQuery.Arguments.Add(new QueryArgument<StringGraphType>
        {
            Name = StreetFilter + suffix,
            ResolvedType = new StringGraphType(),
        });

    private static void AddFilterSite(FieldType contentQuery, string suffix) =>
        contentQuery.Arguments.Add(new QueryArgument<StringGraphType>
        {
            Name = SiteFilter + suffix,
            ResolvedType = new StringGraphType(),
        });

    private static void AddFilterBuilding(FieldType contentQuery, string suffix) =>
        contentQuery.Arguments.Add(new QueryArgument<StringGraphType>
        {
            Name = BuildingFilter + suffix,
            ResolvedType = new StringGraphType(),
        });

    private static void AddFilterFloor(FieldType contentQuery, string suffix) =>
        contentQuery.Arguments.Add(new QueryArgument<StringGraphType>
        {
            Name = FloorFilter + suffix,
            ResolvedType = new StringGraphType(),
        });

    private static void AddFilterZone(FieldType contentQuery, string suffix) =>
        contentQuery.Arguments.Add(new QueryArgument<StringGraphType>
        {
            Name = ZoneFilter + suffix,
            ResolvedType = new StringGraphType(),
        });

    private static void AddFilterRoom(FieldType contentQuery, string suffix) =>
        contentQuery.Arguments.Add(new QueryArgument<StringGraphType>
        {
            Name = RoomFilter + suffix,
            ResolvedType = new StringGraphType(),
        });
}
