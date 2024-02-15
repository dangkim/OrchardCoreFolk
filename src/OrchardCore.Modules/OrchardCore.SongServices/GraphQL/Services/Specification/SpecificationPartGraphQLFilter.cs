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

namespace OrchardCore.SongServices.GraphQL.Services.Specification;

// IGraphQLFilters can append conditions to the YesSql query, alter its result, or do both.
public class SpecificationPartGraphQLFilter : IGraphQLFilter<ContentItem>
{
    private readonly IClock _clock;

    public SpecificationPartGraphQLFilter(IClock clock) => _clock = clock;

    // While you can use this to execute some complex YesSql query it's best to stick with the IIndexAliasProvider
    // approach for such things.
    public Task<IQuery<ContentItem>> PreQueryAsync(IQuery<ContentItem> query, IResolveFieldContext context)
    {
        var (description, valueDescription) = context.Arguments.FirstOrDefault(
            argument => argument.Key.StartsWith(DescriptionFilter, StringComparison.Ordinal));

        var (assignerContentItemId, valueAssignerContentItemId) = context.Arguments.FirstOrDefault(
            argument => argument.Key.StartsWith(AssignerContentItemIdFilter, StringComparison.Ordinal));

        var (sssigneeContentItemId, valueAssigneeContentItemId) = context.Arguments.FirstOrDefault(
            argument => argument.Key.StartsWith(AssigneeContentItemIdFilter, StringComparison.Ordinal));

        var (supplement, valueSupplement) = context.Arguments.FirstOrDefault(
            argument => argument.Key.StartsWith(SupplementFilter, StringComparison.Ordinal));

        var (rootCause, valueRootCause) = context.Arguments.FirstOrDefault(
            argument => argument.Key.StartsWith(RootCauseFilter, StringComparison.Ordinal));

        var (isPlanned, valueIsPlanned) = context.Arguments.FirstOrDefault(
            argument => argument.Key.StartsWith(IsPlannedFilter, StringComparison.Ordinal));

        var (isIncident, valueIsIncident) = context.Arguments.FirstOrDefault(
            argument => argument.Key.StartsWith(IsIncidentFilter, StringComparison.Ordinal));

        var (isInHouse, valueIsInHouse) = context.Arguments.FirstOrDefault(
            argument => argument.Key.StartsWith(IsInHouseFilter, StringComparison.Ordinal));

        var (isOutsourced, valueIsOutsourced) = context.Arguments.FirstOrDefault(
            argument => argument.Key.StartsWith(IsOutsourcedFilter, StringComparison.Ordinal));

        var (reportStatus, valueReportStatuse) = context.Arguments.FirstOrDefault(
            argument => argument.Key.StartsWith(ReportStatusFilter, StringComparison.Ordinal));

        var (offerContentItemId, valueOfferContentItemId) = context.Arguments.FirstOrDefault(
            argument => argument.Key.StartsWith(OfferContentItemIdFilter, StringComparison.Ordinal));

        var (behavior, valueBehavior) = context.Arguments.FirstOrDefault(
            argument => argument.Key.StartsWith(BehaviorFilter, StringComparison.Ordinal));

        var (asset, valueAsset) = context.Arguments.FirstOrDefault(
            argument => argument.Key.StartsWith(AssetFilter, StringComparison.Ordinal));

        var (eventV, valueEvent) = context.Arguments.FirstOrDefault(
            argument => argument.Key.StartsWith(EventFilter, StringComparison.Ordinal));

        var (others, valueOthers) = context.Arguments.FirstOrDefault(
            argument => argument.Key.StartsWith(OthersFilter, StringComparison.Ordinal));

        var (sender, valueSender) = context.Arguments.FirstOrDefault(
            argument => argument.Key.StartsWith(SenderFilter, StringComparison.Ordinal));

        var (writer, valueWriter) = context.Arguments.FirstOrDefault(
            argument => argument.Key.StartsWith(WriterFilter, StringComparison.Ordinal));

        var (photos, valuePhotost) = context.Arguments.FirstOrDefault(
            argument => argument.Key.StartsWith(PhotosFilter, StringComparison.Ordinal));

        var (clips, valueClips) = context.Arguments.FirstOrDefault(
            argument => argument.Key.StartsWith(ClipsFilter, StringComparison.Ordinal));

        var (audio, valueAudio) = context.Arguments.FirstOrDefault(
            argument => argument.Key.StartsWith(AudioFilter, StringComparison.Ordinal));

        var (files, valueFiles) = context.Arguments.FirstOrDefault(
            argument => argument.Key.StartsWith(FilesFilter, StringComparison.Ordinal));

        var (locationContentItemId, valueLocationContentItemId) = context.Arguments.FirstOrDefault(
            argument => argument.Key.StartsWith(LocationContentItemIdFilter, StringComparison.Ordinal));

        var (dateTime, valueDateTime) = context.Arguments.FirstOrDefault(
            argument => argument.Key.StartsWith(DateTimeFilter, StringComparison.Ordinal));

        if (description != null && valueDescription.Value != null)
        {
            var specificationQuery = query.With<ContentItemIndex>(c => c.Published && c.Latest).With<SpecificationPartIndex>(index => index.Description == valueDescription.Value.ToString()).Take(10000);
            return Task.FromResult(specificationQuery);
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
