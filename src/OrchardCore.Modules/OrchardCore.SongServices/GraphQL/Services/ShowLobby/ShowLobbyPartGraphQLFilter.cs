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
using static OrchardCore.SongServices.GraphQL.Services.ShowLobby.ShowLobbyPartTypeBuilder;
using OrchardCore.SongServices.Indexes;
using OrchardCore.ContentManagement.Records;
using GraphQL;

namespace OrchardCore.SongServices.GraphQL.Services.ShowLobby;

// IGraphQLFilters can append conditions to the YesSql query, alter its result, or do both.
public class ShowLobbyPartGraphQLFilter : IGraphQLFilter<ContentItem>
{
    private readonly IClock _clock;

    public ShowLobbyPartGraphQLFilter(IClock clock) => _clock = clock;

    // While you can use this to execute some complex YesSql query it's best to stick with the IIndexAliasProvider
    // approach for such things.
    public Task<IQuery<ContentItem>> PreQueryAsync(IQuery<ContentItem> query, IResolveFieldContext context)
    {
        var (name, valueKind) = context.Arguments.FirstOrDefault(
            argument => argument.Key.StartsWith(KindFilter, StringComparison.Ordinal));       

        var (table, valueDealerName) = context.Arguments.FirstOrDefault(
            argument => argument.Key.StartsWith(DealerNameFilter, StringComparison.Ordinal));
        
        var (value, valueStatus) = context.Arguments.FirstOrDefault(
            argument => argument.Key.StartsWith(StatusFilter, StringComparison.Ordinal));

        if (name != null && valueKind.Value != null)
        {
            var ShowLobbyQuery = query.With<ContentItemIndex>(c => c.Published && c.Latest).With<ShowLobbyPartIndex>(index => index.Kind == valueKind.Value.ToString()).Take(10000);
            return Task.FromResult(ShowLobbyQuery);
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
