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
using static OrchardCore.SongServices.GraphQL.Services.Staff.StaffPartTypeBuilder;
using OrchardCore.SongServices.Indexes;
using OrchardCore.ContentManagement.Records;
using GraphQL;
using OrchardCore.SongServices.Indexing;
using OrchardCore.SongServices.ApiModels;

namespace OrchardCore.SongServices.GraphQL.Services.Staff;

public class StaffPartGraphQLBookmarkedReportContentItemIdsFilter : IGraphQLFilter<ContentItem>
{
    public Task<IQuery<ContentItem>> PreQueryAsync(IQuery<ContentItem> query, IResolveFieldContext context)
    {
        if (context.FieldDefinition.Name != "staffPage")
        {
            return Task.FromResult(query);
        }

        var listArgs = context.Arguments.Where(argument => argument.Value.Value != null && argument.Key.StartsWith(BookmarkedReportContentItemIdsFilter, StringComparison.Ordinal));

        if (!listArgs.Any())
        {
            return Task.FromResult(query);
        }
        else
        {
            var staffQuery = query.With<StaffPartIndex>(index => index.BookmarkedReportContentItemIds != String.Empty);

            foreach (var item in listArgs)
            {
                var (comparasion, valueComparasion) = item;

                if (comparasion != null && valueComparasion.Value != null)
                {
                    if (comparasion == BookmarkedReportContentItemIdsFilter) comparasion = BookmarkedReportContentItemIdsFilter + "_eq";
                    var comparasionType = comparasion[^2..]; // The name operator like gt, le, etc.

                    if (comparasionType == "ne")
                    {
                        staffQuery = query.With<StaffPartIndex>(index => index.BookmarkedReportContentItemIds != (string)valueComparasion.Value);
                    }
                    else
                    {
                        staffQuery = query.With<StaffPartIndex>(index => index.BookmarkedReportContentItemIds == (string)valueComparasion.Value);
                    }
                }
            }

            return Task.FromResult(staffQuery.Take(10000));
        }
    }

    public Task<IEnumerable<ContentItem>> PostQueryAsync(
        IEnumerable<ContentItem> contentItems,
        IResolveFieldContext context)
    {
        return Task.FromResult(contentItems);
    }
}
