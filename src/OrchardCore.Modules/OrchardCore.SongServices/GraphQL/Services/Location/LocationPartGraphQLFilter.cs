using GraphQL.Types;
using OrchardCore.SongServices.ContentParts;
using OrchardCore.ContentManagement;
using OrchardCore.ContentManagement.GraphQL.Queries;
using OrchardCore.DisplayManagement.Shapes;
using OrchardCore.Modules;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using YesSql;
using static OrchardCore.SongServices.GraphQL.Services.Location.LocationPartTypeBuilder;
using OrchardCore.SongServices.Indexes;
using OrchardCore.ContentManagement.Records;
using GraphQL;

namespace OrchardCore.SongServices.GraphQL.Services.Location;

// IGraphQLFilters can append conditions to the YesSql query, alter its result, or do both.
public class LocationPartGraphQLFilter : IGraphQLFilter<ContentItem>
{
    private readonly IClock _clock;

    public LocationPartGraphQLFilter(IClock clock) => _clock = clock;

    // While you can use this to execute some complex YesSql query it's best to stick with the IIndexAliasProvider
    // approach for such things.
    public Task<IQuery<ContentItem>> PreQueryAsync(IQuery<ContentItem> query, IResolveFieldContext context)
    {
        var (country, valueCountry) = context.Arguments.FirstOrDefault(
            argument => argument.Key.StartsWith(CountryFilter, StringComparison.Ordinal));

        var (city, valueCity) = context.Arguments.FirstOrDefault(
            argument => argument.Key.StartsWith(CityFilter, StringComparison.Ordinal));

        var (street, valueStreet) = context.Arguments.FirstOrDefault(
            argument => argument.Key.StartsWith(StreetFilter, StringComparison.Ordinal));

        var (site, valueSite) = context.Arguments.FirstOrDefault(
            argument => argument.Key.StartsWith(SiteFilter, StringComparison.Ordinal));

        var (building, valueBuilding) = context.Arguments.FirstOrDefault(
            argument => argument.Key.StartsWith(BuildingFilter, StringComparison.Ordinal));

        var (floor, valueFloor) = context.Arguments.FirstOrDefault(
            argument => argument.Key.StartsWith(FloorFilter, StringComparison.Ordinal));

        var (zone, valueZone) = context.Arguments.FirstOrDefault(
            argument => argument.Key.StartsWith(ZoneFilter, StringComparison.Ordinal));

        var (room, valueRoom) = context.Arguments.FirstOrDefault(
            argument => argument.Key.StartsWith(RoomFilter, StringComparison.Ordinal));

        var (dateTime, valueDateTime) = context.Arguments.FirstOrDefault(
            argument => argument.Key.StartsWith(DateTimeFilter, StringComparison.Ordinal));

        if (country != null && valueCountry.Value != null)
        {
            var LocationQuery = query.With<ContentItemIndex>(c => c.Published && c.Latest).With<LocationPartIndex>(index => index.Country == valueCountry.Value.ToString()).Take(10000);
            return Task.FromResult(LocationQuery);
        }

        return Task.FromResult(query);
    }

    // You can use this method to filter offline or in separate requests. This is less efficient but it's necessary if
    // the request can't be described as a single YesSql query. In this case we work off of a property that's not
    // indexed for demonstration's sake.
    public Task<IEnumerable<ContentItem>> PostQueryAsync(
        IEnumerable<ContentItem> contentItems,
        IResolveFieldContext context)
    {
        return Task.FromResult(contentItems);
    }
}
