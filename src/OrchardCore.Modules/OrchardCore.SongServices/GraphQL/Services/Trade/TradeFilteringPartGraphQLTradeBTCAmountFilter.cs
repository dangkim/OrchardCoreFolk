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
using static OrchardCore.SongServices.GraphQL.Services.Trade.TradeFilteringPartTypeBuilder;
using OrchardCore.SongServices.Indexes;
using OrchardCore.ContentManagement.Records;
using GraphQL;
using OrchardCore.SongServices.Indexing;
using OrchardCore.SongServices.ApiModels;

namespace OrchardCore.SongServices.GraphQL.Services.Trade;

// IGraphQLFilters can append conditions to the YesSql query, alter its result, or do both.
public class TradeFilteringPartGraphQLTradeBTCAmountFilter : IGraphQLFilter<ContentItem>
{
    public Task<IQuery<ContentItem>> PreQueryAsync(IQuery<ContentItem> query, IResolveFieldContext context)
    {
        if (context.FieldDefinition.Name != "tradePage")
        {
            return Task.FromResult(query);
        }

        var listArgs = context.Arguments.Where(argument => argument.Value.Value != null && argument.Key.StartsWith(TradeBTCAmountFilter, StringComparison.Ordinal));

        if (!listArgs.Any())
        {
            return Task.FromResult(query);
        }
        else
        {
            var tradeQuery = query.With<TradeFilteringPartIndex>(index => index.TradeBTCAmount > 0);

            foreach (var item in listArgs)
            {
                var (comparasion, valueComparasion) = item;

                if (comparasion != null && valueComparasion.Value != null)
                {
                    if (comparasion == TradeBTCAmountFilter) comparasion = TradeBTCAmountFilter + "_eq";
                    var comparasionType = comparasion[^2..]; // The name operator like gt, le, etc.                                      

                    if (comparasionType == "ge")
                    {
                        tradeQuery = query.With<TradeFilteringPartIndex>(index => index.TradeBTCAmount >= (decimal)valueComparasion.Value);
                    }

                    if (comparasionType == "gt")
                    {
                        tradeQuery = query.With<TradeFilteringPartIndex>(index => index.TradeBTCAmount > (decimal)valueComparasion.Value);
                    }

                    if (comparasionType == "le")
                    {
                        tradeQuery = query.With<TradeFilteringPartIndex>(index => index.TradeBTCAmount <= (decimal)valueComparasion.Value);
                    }

                    if (comparasionType == "lt")
                    {
                        tradeQuery = query.With<TradeFilteringPartIndex>(index => index.TradeBTCAmount < (decimal)valueComparasion.Value);
                    }

                    if (comparasionType == "ne")
                    {
                        tradeQuery = query.With<TradeFilteringPartIndex>(index => index.TradeBTCAmount != (decimal)valueComparasion.Value);
                    }
                    else
                    {
                        tradeQuery = query.With<TradeFilteringPartIndex>(index => index.TradeBTCAmount == (decimal)valueComparasion.Value);
                    }
                }
            }

            return Task.FromResult(tradeQuery.Take(10000));
        }
    }

    public Task<IEnumerable<ContentItem>> PostQueryAsync(
        IEnumerable<ContentItem> contentItems,
        IResolveFieldContext context)
    {
        return Task.FromResult(contentItems);
    }
}
