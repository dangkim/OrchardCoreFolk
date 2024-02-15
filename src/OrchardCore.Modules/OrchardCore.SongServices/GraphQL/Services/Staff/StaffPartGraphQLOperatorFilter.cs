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

public class StaffPartGraphQLOperatorFilter : IGraphQLFilter<ContentItem>
{
    public Task<IQuery<ContentItem>> PreQueryAsync(IQuery<ContentItem> query, IResolveFieldContext context)
    {
        if (context.FieldDefinition.Name != "staffPage")
        {
            return Task.FromResult(query);
        }

        var listArgs = context.Arguments.Where(argument => argument.Value.Value != null && argument.Key.StartsWith(OperatorFilter, StringComparison.Ordinal));

        if (!listArgs.Any())
        {
            return Task.FromResult(query);
        }
        else
        {
            var locationQuery = query.With<StaffPartIndex>(index => index.Operator != String.Empty);

            foreach (var item in listArgs)
            {
                var (comparasion, valueComparasion) = item;

                if (comparasion != null && valueComparasion.Value != null)
                {
                    if (comparasion == OperatorFilter) comparasion = OperatorFilter + "_eq";
                    var comparasionType = comparasion[^2..]; // The name operator like gt, le, etc.

                    if (comparasionType == "ne")
                    {
                        locationQuery = query.With<StaffPartIndex>(index => index.Operator != (string)valueComparasion.Value);
                    }
                    else
                    {
                        locationQuery = query.With<StaffPartIndex>(index => index.Operator == (string)valueComparasion.Value);
                    }
                }
            }

            return Task.FromResult(locationQuery.Take(10000));
        }
    }

    public Task<IEnumerable<ContentItem>> PostQueryAsync(
        IEnumerable<ContentItem> contentItems,
        IResolveFieldContext context)
    {
        return Task.FromResult(contentItems);
    }
}
