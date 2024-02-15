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
using static OrchardCore.SongServices.GraphQL.Services.Bets.BetsPartTypeBuilder;
using OrchardCore.SongServices.Indexes;
using OrchardCore.ContentManagement.Records;
using GraphQL;

namespace OrchardCore.SongServices.GraphQL.Services.Bets;

// IGraphQLFilters can append conditions to the YesSql query, alter its result, or do both.
public class BetsPartGraphQLFilter : IGraphQLFilter<ContentItem>
{
    private readonly IClock _clock;

    public BetsPartGraphQLFilter(IClock clock) => _clock = clock;

    // While you can use this to execute some complex YesSql query it's best to stick with the IIndexAliasProvider
    // approach for such things.
    public Task<IQuery<ContentItem>> PreQueryAsync(IQuery<ContentItem> query, IResolveFieldContext context)
    {
        var (name, valueName) = context.Arguments.FirstOrDefault(
            argument => argument.Key.StartsWith(NameFilter, StringComparison.Ordinal));

        var (kind, valueKind) = context.Arguments.FirstOrDefault(
            argument => argument.Key.StartsWith(KindFilter, StringComparison.Ordinal));

        var (code, valueCode) = context.Arguments.FirstOrDefault(
            argument => argument.Key.StartsWith(CodeFilter, StringComparison.Ordinal));

        var (id, valueId) = context.Arguments.FirstOrDefault(
            argument => argument.Key.StartsWith(IdFilter, StringComparison.Ordinal));

        var (allowed, valueAllowed) = context.Arguments.FirstOrDefault(
            argument => argument.Key.StartsWith(AllowedFilter, StringComparison.Ordinal));

        var (table, valueTable) = context.Arguments.FirstOrDefault(
            argument => argument.Key.StartsWith(TableFilter, StringComparison.Ordinal));

        if (name != null && valueName.Value != null)
        {
            var BetsQuery = query.With<ContentItemIndex>(c => c.Published && c.Latest).With<BetsPartIndex>(index => index.Name == valueName.Value.ToString()).Take(10000);
            return Task.FromResult(BetsQuery);
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
