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

public class StaffPartGraphQLUserNameFilter : IGraphQLFilter<ContentItem>
{
    public Task<IQuery<ContentItem>> PreQueryAsync(IQuery<ContentItem> query, IResolveFieldContext context)
    {
        if (context.FieldDefinition.Name != "StaffPage")
        {
            return Task.FromResult(query);
        }

        var listArgs = context.Arguments.Where(argument => argument.Value.Value != null && argument.Key.StartsWith(UserNameFilter, StringComparison.Ordinal));

        if (!listArgs.Any())
        {
            return Task.FromResult(query);
        }
        else
        {
            var StaffQuery = query.With<StaffPartIndex>(index => index.UserName != String.Empty);

            foreach (var item in listArgs)
            {
                var (comparasion, valueComparasion) = item;

                if (comparasion != null && valueComparasion.Value != null)
                {
                    if (comparasion == UserNameFilter) comparasion = UserNameFilter + "_eq";
                    var comparasionType = comparasion[^2..]; // The name operator like gt, le, etc.

                    if (comparasionType == "ne")
                    {
                        StaffQuery = query.With<StaffPartIndex>(index => index.UserName != (string)valueComparasion.Value);
                    }
                    else
                    {
                        StaffQuery = query.With<StaffPartIndex>(index => index.UserName == (string)valueComparasion.Value);
                    }
                }
            }

            return Task.FromResult(StaffQuery.Take(10000));
        }
    }

    public Task<IEnumerable<ContentItem>> PostQueryAsync(
        IEnumerable<ContentItem> contentItems,
        IResolveFieldContext context)
    {
        return Task.FromResult(contentItems);
    }
}
