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
using static OrchardCore.SongServices.GraphQL.Services.Specification.SpecificationPartTypeBuilder;
using OrchardCore.SongServices.Indexes;
using OrchardCore.ContentManagement.Records;
using GraphQL;
using OrchardCore.SongServices.Indexing;
using OrchardCore.SongServices.ApiModels;

namespace OrchardCore.SongServices.GraphQL.Services.Specification;

public class SpecificationPartGraphQLIsPlannedFilter : IGraphQLFilter<ContentItem>
{
    public Task<IQuery<ContentItem>> PreQueryAsync(IQuery<ContentItem> query, IResolveFieldContext context)
    {
        if (context.FieldDefinition.Name != "specificationPage")
        {
            return Task.FromResult(query);
        }

        var listArgs = context.Arguments.Where(argument => argument.Value.Value != null && argument.Key.StartsWith(IsPlannedFilter, StringComparison.Ordinal));

        if (!listArgs.Any())
        {
            return Task.FromResult(query);
        }
        else
        {
            var (comparasion, valueComparasion) = listArgs.FirstOrDefault();

            if (comparasion != null && valueComparasion.Value != null)
            {
                var specificationQuery = query.With<SpecificationPartIndex>(index => index.IsPlanned == (bool)valueComparasion.Value);

                return Task.FromResult(specificationQuery.Take(10000));
            }

            return Task.FromResult(query);
        }
    }

    public Task<IEnumerable<ContentItem>> PostQueryAsync(
        IEnumerable<ContentItem> contentItems,
        IResolveFieldContext context)
    {
        return Task.FromResult(contentItems);
    }
}
