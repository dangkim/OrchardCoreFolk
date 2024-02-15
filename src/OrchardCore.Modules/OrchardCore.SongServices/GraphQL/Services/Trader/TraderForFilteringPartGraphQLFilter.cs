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
using static OrchardCore.SongServices.GraphQL.Services.Trader.TraderForFilteringPartTypeBuilder;
using OrchardCore.SongServices.Indexes;
using OrchardCore.ContentManagement.Records;
using GraphQL;
using OrchardCore.SongServices.Indexing;

namespace OrchardCore.SongServices.GraphQL.Services.Trader;

// IGraphQLFilters can append conditions to the YesSql query, alter its result, or do both.
public class TraderForFilteringPartGraphQLFilter : IGraphQLFilter<ContentItem>
{
    private readonly IClock _clock;

    public TraderForFilteringPartGraphQLFilter(IClock clock) => _clock = clock;

    // While you can use this to execute some complex YesSql query it's best to stick with the IIndexAliasProvider
    // approach for such things.
    public Task<IQuery<ContentItem>> PreQueryAsync(IQuery<ContentItem> query, IResolveFieldContext context)
    {
        if (context.FieldDefinition.Name != "traderPage")
        {
            return Task.FromResult(query);
        }

        var (name, valueName) = context.Arguments.FirstOrDefault(
            argument => argument.Key.StartsWith(NameFilter, StringComparison.Ordinal));        

        var (userId, valueUserId) = context.Arguments.FirstOrDefault(
            argument => argument.Key.StartsWith(UserIdFilter, StringComparison.Ordinal));

        //var (date, valueDate) = context.Arguments.FirstOrDefault(
        //    argument => argument.Key.StartsWith(DateFilter, StringComparison.Ordinal));

        if ((userId != null && valueUserId.Value != null) || (name != null && valueName.Value != null))
        {
            var traderQuery = query.With<ContentItemIndex>(c => c.Published && c.Latest).With<TraderForFilteringPartIndex>(index => index.UserId == (int)valueUserId.Value || index.Name == (string)valueName.Value).Take(10000);
            return Task.FromResult(traderQuery);
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
