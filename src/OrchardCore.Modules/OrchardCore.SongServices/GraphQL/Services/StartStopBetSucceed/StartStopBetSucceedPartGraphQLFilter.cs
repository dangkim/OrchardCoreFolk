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
using static OrchardCore.SongServices.GraphQL.Services.StartStopBetSucceed.StartStopBetSucceedPartTypeBuilder;
using OrchardCore.SongServices.Indexes;
using OrchardCore.ContentManagement.Records;
using GraphQL;

namespace OrchardCore.SongServices.GraphQL.Services.StartStopBetSucceed;

// IGraphQLFilters can append conditions to the YesSql query, alter its result, or do both.
public class StartStopBetSucceedPartGraphQLFilter : IGraphQLFilter<ContentItem>
{
    private readonly IClock _clock;

    public StartStopBetSucceedPartGraphQLFilter(IClock clock) => _clock = clock;

    // While you can use this to execute some complex YesSql query it's best to stick with the IIndexAliasProvider
    // approach for such things.
    public Task<IQuery<ContentItem>> PreQueryAsync(IQuery<ContentItem> query, IResolveFieldContext context)
    {
        var (powerupFilter, valuePowerup) = context.Arguments.FirstOrDefault(
            argument => argument.Key.StartsWith(PowerupFilter, StringComparison.Ordinal));

        var (status, valueStatus) = context.Arguments.FirstOrDefault(
            argument => argument.Key.StartsWith(StatusFilter, StringComparison.Ordinal));

        var (kind, valueKind) = context.Arguments.FirstOrDefault(
            argument => argument.Key.StartsWith(KindFilter, StringComparison.Ordinal));

        var (result, valueContent) = context.Arguments.FirstOrDefault(
            argument => argument.Key.StartsWith(ContentFilter, StringComparison.Ordinal));

        var (round, valueRound) = context.Arguments.FirstOrDefault(
            argument => argument.Key.StartsWith(RoundFilter, StringComparison.Ordinal));
        
        var (roundId, valueRoundId) = context.Arguments.FirstOrDefault(
            argument => argument.Key.StartsWith(RoundIdFilter, StringComparison.Ordinal));
        
        var (shoe, valueShoe) = context.Arguments.FirstOrDefault(
            argument => argument.Key.StartsWith(ShoeFilter, StringComparison.Ordinal));

        var (table, valueTable) = context.Arguments.FirstOrDefault(
            argument => argument.Key.StartsWith(TableFilter, StringComparison.Ordinal));

        if (status != null && valueStatus.Value != null)
        {
            var startStopBetSucceedQuery = query.With<ContentItemIndex>(c => c.Published && c.Latest).With<StartStopBetSucceedPartIndex>(index => index.Status == valueStatus.Value.ToString()).Take(10000);
            return Task.FromResult(startStopBetSucceedQuery);
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
