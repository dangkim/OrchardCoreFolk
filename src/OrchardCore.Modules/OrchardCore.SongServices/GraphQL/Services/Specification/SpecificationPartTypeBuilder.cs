using GraphQL.Types;
using OrchardCore.ContentManagement.GraphQL.Queries.Types;
using OrchardCore.ContentManagement.Metadata.Models;
using System.Linq;

namespace OrchardCore.SongServices.GraphQL.Services.Specification;

// Services that implement IContentTypeBuilder extend the features of existing ContentItem type fields, including the
// top level fields automatically created by Orchard Core for every content type. You can use this to add new sub-fields
// or filter attributes to existing ContentItem type fields.
public class SpecificationPartTypeBuilder : IContentTypeBuilder
{
    // It's a good practice to make the argument name a const because you will reuse it in the IGraphQLFilter.
    public const string DescriptionFilter = "description";
    public const string AssignerContentItemIdFilter = "assignerContentItemId";
    public const string AssigneeContentItemIdFilter = "assigneeContentItemId";
    public const string SupplementFilter = "supplement";
    public const string RootCauseFilter = "rootCause";
    public const string IsPlannedFilter = "isPlanned";
    public const string IsIncidentFilter = "isIncident";
    public const string IsInHouseFilter = "isInHouse";
    public const string IsOutsourcedFilter = "IsOutsourced";
    public const string ReportStatusFilter = "reportStatus";
    public const string OfferContentItemIdFilter = "offerContentItemId";
    public const string BehaviorFilter = "behavior";
    public const string AssetFilter = "asset";
    public const string EventFilter = "event";
    public const string OthersFilter = "others";
    public const string SenderFilter = "sender";
    public const string WriterFilter = "writer";
    public const string PhotosFilter = "photos";
    public const string ClipsFilter = "clips";
    public const string AudioFilter = "audio";
    public const string FilesFilter = "files";
    public const string LocationContentItemIdFilter = "locationContentItemId";
    public const string DateTimeFilter = "date";

    // Here you can add arguments to every Content Type (top level) field.
    public void Build(ISchema schema,
        FieldType contentQuery,
        ContentTypeDefinition contentTypeDefinition,
        ContentItemType contentItemType)
    {
        if (contentItemType.Name != ContentTypes.SpecificationPage) return;

        contentQuery.Arguments.Add(new QueryArgument<StringGraphType>
        {
            Name = DescriptionFilter,
            ResolvedType = new StringGraphType(),
        });

        contentQuery.Arguments.Add(new QueryArgument<StringGraphType>
        {
            Name = AssignerContentItemIdFilter,
            ResolvedType = new StringGraphType(),
        });

        contentQuery.Arguments.Add(new QueryArgument<StringGraphType>
        {
            Name = AssigneeContentItemIdFilter,
            ResolvedType = new StringGraphType(),
        });

        contentQuery.Arguments.Add(new QueryArgument<StringGraphType>
        {
            Name = SupplementFilter,
            ResolvedType = new StringGraphType(),
        });

        contentQuery.Arguments.Add(new QueryArgument<StringGraphType>
        {
            Name = RootCauseFilter,
            ResolvedType = new StringGraphType(),
        });

        contentQuery.Arguments.Add(new QueryArgument<BooleanGraphType>
        {
            Name = IsPlannedFilter,
            ResolvedType = new BooleanGraphType(),
        });

        contentQuery.Arguments.Add(new QueryArgument<BooleanGraphType>
        {
            Name = IsIncidentFilter,
            ResolvedType = new BooleanGraphType(),
        });

        contentQuery.Arguments.Add(new QueryArgument<BooleanGraphType>
        {
            Name = IsInHouseFilter,
            ResolvedType = new BooleanGraphType(),
        });

        contentQuery.Arguments.Add(new QueryArgument<BooleanGraphType>
        {
            Name = IsOutsourcedFilter,
            ResolvedType = new BooleanGraphType(),
        });

        contentQuery.Arguments.Add(new QueryArgument<StringGraphType>
        {
            Name = ReportStatusFilter,
            ResolvedType = new StringGraphType(),
        });

        contentQuery.Arguments.Add(new QueryArgument<StringGraphType>
        {
            Name = OfferContentItemIdFilter,
            ResolvedType = new StringGraphType(),
        });

        contentQuery.Arguments.Add(new QueryArgument<StringGraphType>
        {
            Name = BehaviorFilter,
            ResolvedType = new StringGraphType(),
        });

        contentQuery.Arguments.Add(new QueryArgument<StringGraphType>
        {
            Name = AssetFilter,
            ResolvedType = new StringGraphType(),
        });

        contentQuery.Arguments.Add(new QueryArgument<StringGraphType>
        {
            Name = EventFilter,
            ResolvedType = new StringGraphType(),
        });

        contentQuery.Arguments.Add(new QueryArgument<StringGraphType>
        {
            Name = OthersFilter,
            ResolvedType = new StringGraphType(),
        });

        contentQuery.Arguments.Add(new QueryArgument<StringGraphType>
        {
            Name = SenderFilter,
            ResolvedType = new StringGraphType(),
        });

        contentQuery.Arguments.Add(new QueryArgument<StringGraphType>
        {
            Name = WriterFilter,
            ResolvedType = new StringGraphType(),
        });

        contentQuery.Arguments.Add(new QueryArgument<StringGraphType>
        {
            Name = PhotosFilter,
            ResolvedType = new StringGraphType(),
        });

        contentQuery.Arguments.Add(new QueryArgument<StringGraphType>
        {
            Name = ClipsFilter,
            ResolvedType = new StringGraphType(),
        });

        contentQuery.Arguments.Add(new QueryArgument<StringGraphType>
        {
            Name = AudioFilter,
            ResolvedType = new StringGraphType(),
        });

        contentQuery.Arguments.Add(new QueryArgument<StringGraphType>
        {
            Name = FilesFilter,
            ResolvedType = new StringGraphType(),
        });

        contentQuery.Arguments.Add(new QueryArgument<StringGraphType>
        {
            Name = LocationContentItemIdFilter,
            ResolvedType = new StringGraphType(),
        });

        contentQuery.Arguments.Add(new QueryArgument<StringGraphType>
        {
            Name = DateTimeFilter,
            ResolvedType = new StringGraphType(),
        });

        /*------------------------------------------------------------*/
        AddFilterDescription(contentQuery, "_ne");
        AddFilterAssignerContentItemId(contentQuery, "_ne");
        AddFilterAssigneeContentItemId(contentQuery, "_ne");
        AddFilterSupplement(contentQuery, "_ne");
        AddFilterRootCause(contentQuery, "_ne");
        AddFilterReportStatus(contentQuery, "_ne");
        AddFilterOfferContentItemId(contentQuery, "_ne");
        AddFilterBehavior(contentQuery, "_ne");
        AddFilterAsset(contentQuery, "_ne");
        AddFilterEvent(contentQuery, "_ne");
        AddFilterOthers(contentQuery, "_ne");
        AddFilterSender(contentQuery, "_ne");
        AddFilterWriter(contentQuery, "_ne");
        AddFilterPhotos(contentQuery, "_ne");
        AddFilterClips(contentQuery, "_ne");
        AddFilterAudio(contentQuery, "_ne");
        AddFilterFiles(contentQuery, "_ne");
        AddFilterLocationContentItemId(contentQuery, "_ne");
    }

    private static void AddFilterDescription(FieldType contentQuery, string suffix) =>
        contentQuery.Arguments.Add(new QueryArgument<StringGraphType>
        {
            Name = DescriptionFilter + suffix,
            ResolvedType = new StringGraphType(),
        });

    private static void AddFilterAssignerContentItemId(FieldType contentQuery, string suffix) =>
        contentQuery.Arguments.Add(new QueryArgument<StringGraphType>
        {
            Name = AssignerContentItemIdFilter + suffix,
            ResolvedType = new StringGraphType(),
        });

    private static void AddFilterAssigneeContentItemId(FieldType contentQuery, string suffix) =>
        contentQuery.Arguments.Add(new QueryArgument<StringGraphType>
        {
            Name = AssigneeContentItemIdFilter + suffix,
            ResolvedType = new StringGraphType(),
        });

    private static void AddFilterSupplement(FieldType contentQuery, string suffix) =>
        contentQuery.Arguments.Add(new QueryArgument<StringGraphType>
        {
            Name = SupplementFilter + suffix,
            ResolvedType = new StringGraphType(),
        });

    private static void AddFilterRootCause(FieldType contentQuery, string suffix) =>
        contentQuery.Arguments.Add(new QueryArgument<StringGraphType>
        {
            Name = RootCauseFilter + suffix,
            ResolvedType = new StringGraphType(),
        });

    private static void AddFilterReportStatus(FieldType contentQuery, string suffix) =>
        contentQuery.Arguments.Add(new QueryArgument<StringGraphType>
        {
            Name = ReportStatusFilter + suffix,
            ResolvedType = new StringGraphType(),
        });

    private static void AddFilterOfferContentItemId(FieldType contentQuery, string suffix) =>
        contentQuery.Arguments.Add(new QueryArgument<StringGraphType>
        {
            Name = OfferContentItemIdFilter + suffix,
            ResolvedType = new StringGraphType(),
        });

    private static void AddFilterBehavior(FieldType contentQuery, string suffix) =>
        contentQuery.Arguments.Add(new QueryArgument<StringGraphType>
        {
            Name = BehaviorFilter + suffix,
            ResolvedType = new StringGraphType(),
        });

    private static void AddFilterAsset(FieldType contentQuery, string suffix) =>
        contentQuery.Arguments.Add(new QueryArgument<StringGraphType>
        {
            Name = AssetFilter + suffix,
            ResolvedType = new StringGraphType(),
        });

    private static void AddFilterEvent(FieldType contentQuery, string suffix) =>
        contentQuery.Arguments.Add(new QueryArgument<StringGraphType>
        {
            Name = EventFilter + suffix,
            ResolvedType = new StringGraphType(),
        });

    private static void AddFilterOthers(FieldType contentQuery, string suffix) =>
        contentQuery.Arguments.Add(new QueryArgument<StringGraphType>
        {
            Name = OthersFilter + suffix,
            ResolvedType = new StringGraphType(),
        });

    private static void AddFilterSender(FieldType contentQuery, string suffix) =>
        contentQuery.Arguments.Add(new QueryArgument<StringGraphType>
        {
            Name = SenderFilter + suffix,
            ResolvedType = new StringGraphType(),
        });

    private static void AddFilterWriter(FieldType contentQuery, string suffix) =>
        contentQuery.Arguments.Add(new QueryArgument<StringGraphType>
        {
            Name = WriterFilter + suffix,
            ResolvedType = new StringGraphType(),
        });

    private static void AddFilterPhotos(FieldType contentQuery, string suffix) =>
        contentQuery.Arguments.Add(new QueryArgument<StringGraphType>
        {
            Name = PhotosFilter + suffix,
            ResolvedType = new StringGraphType(),
        });

    private static void AddFilterClips(FieldType contentQuery, string suffix) =>
        contentQuery.Arguments.Add(new QueryArgument<StringGraphType>
        {
            Name = ClipsFilter + suffix,
            ResolvedType = new StringGraphType(),
        });

    private static void AddFilterAudio(FieldType contentQuery, string suffix) =>
        contentQuery.Arguments.Add(new QueryArgument<StringGraphType>
        {
            Name = AudioFilter + suffix,
            ResolvedType = new StringGraphType(),
        });

    private static void AddFilterFiles(FieldType contentQuery, string suffix) =>
        contentQuery.Arguments.Add(new QueryArgument<StringGraphType>
        {
            Name = FilesFilter + suffix,
            ResolvedType = new StringGraphType(),
        });

    private static void AddFilterLocationContentItemId(FieldType contentQuery, string suffix) =>
        contentQuery.Arguments.Add(new QueryArgument<StringGraphType>
        {
            Name = LocationContentItemIdFilter + suffix,
            ResolvedType = new StringGraphType(),
        });
}
