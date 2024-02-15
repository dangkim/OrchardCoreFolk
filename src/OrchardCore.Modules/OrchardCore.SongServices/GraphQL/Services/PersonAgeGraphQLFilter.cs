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
using static OrchardCore.SongServices.GraphQL.Services.ContentItemTypeBuilder;
using OrchardCore.SongServices.Indexes;
using OrchardCore.ContentManagement.Records;
using GraphQL;
using Azure.Core;

namespace OrchardCore.SongServices.GraphQL.Services;

// IGraphQLFilters can append conditions to the YesSql query, alter its result, or do both.
public class PersonAgeGraphQLFilter : IGraphQLFilter<ContentItem>
{
    private readonly IClock _clock;

    public PersonAgeGraphQLFilter(IClock clock) => _clock = clock;

    // While you can use this to execute some complex YesSql query it's best to stick with the IIndexAliasProvider
    // approach for such things.
    public Task<IQuery<ContentItem>> PreQueryAsync(IQuery<ContentItem> query, IResolveFieldContext context)
    {
        var (name, value) = context.Arguments.FirstOrDefault(
            argument => argument.Key.StartsWith(NameFilter, StringComparison.Ordinal));

        //if (name != null && value.Value != null)
        //{
        //    var personQuery = query.With<ContentItemIndex>(c => c.Published && c.Latest).With<PersonPartIndex>(index => index.Name == value.Value.ToString()).Take(10000);
        //    return Task.FromResult(personQuery);
        //}

        //var personQuery = query.With<ContentItemIndex>(c => c.Published && c.Latest && c.ContentType == "PersonPage").With<PersonPartIndex>().Take(10000);
        return Task.FromResult(query);

        //return Task.FromResult(query);
    }

    // You can use this method to filter offline or in separate requests. This is less efficient but it's necessary if
    // the request can't be described as a single YesSql query. In this case we work off of a property that's not
    // indexed for demonstration's sake.
    public Task<IEnumerable<ContentItem>> PostQueryAsync(
        IEnumerable<ContentItem> contentItems,
        IResolveFieldContext context)
    {
        var (name, age) = context.Arguments.FirstOrDefault(
            argument => argument.Key.StartsWith(AgeFilterName, StringComparison.Ordinal));

        if (name != null && age.Value != null)
        {
            var now = _clock.UtcNow;
            if (name == "age") name = "age_eq";
            var filterType = name[^2..]; // The name operator like gt, le, etc.

            contentItems = contentItems.Where(item =>
                item.As<PersonPart>()?.BirthDateUtc is { } birthDateUtc &&
                Filter((now - birthDateUtc).TotalYears(), (int)age.Value, filterType));
        }

        return Task.FromResult(contentItems);
    }

    private static bool Filter(int totalYears, int age, string filterType) =>
        filterType switch
        {
            "ge" => totalYears >= age,
            "gt" => totalYears > age,
            "le" => totalYears <= age,
            "lt" => totalYears < age,
            "ne" => totalYears != age,
            _ => totalYears == age,
        };
}

// END OF TRAINING SECTION: GraphQL

// This is the end of the training. It is always hard to say goodbye so... don't do it. Let us know your thoughts about
// this module or Orchard Core itself on GitHub (https://github.com/Lombiq/Orchard-Training-Demo-Module) or send us an
// email to crew@lombiq.com instead. If you feel like you need some more training on developing Orchard Core web
// applications, don't hesitate to contact us!
