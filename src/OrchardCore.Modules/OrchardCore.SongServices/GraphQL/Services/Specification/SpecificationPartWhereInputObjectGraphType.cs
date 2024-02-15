using GraphQL.Types;
using OrchardCore.SongServices.Indexes;
using OrchardCore.SongServices.ContentParts;
using OrchardCore.Apis.GraphQL.Queries;
using static OrchardCore.SongServices.GraphQL.Services.Specification.SpecificationPartObjectGraphType;

namespace OrchardCore.SongServices.GraphQL.Services.Specification;

// This class adds a content part-specific section in the "where" filter. It relies on the content part index to
// automatically generate YesSql logic for database-side filtering.
public class SpecificationPartWhereInputObjectGraphType : WhereInputObjectGraphType<SpecificationPart>
{
    public SpecificationPartWhereInputObjectGraphType()
    {        
        // Since filters depend on the index, we use their "nameof" as reference.
        AddScalarFilterFields<StringGraphType>(nameof(SpecificationPartIndex.Description), DescriptionDescription);
        AddScalarFilterFields<StringGraphType>(nameof(SpecificationPartIndex.AssignerContentItemId), AssignerContentItemIdDescription);
        AddScalarFilterFields<StringGraphType>(nameof(SpecificationPartIndex.AssigneeContentItemId), AssigneeContentItemIdDescription);
        AddScalarFilterFields<StringGraphType>(nameof(SpecificationPartIndex.Supplement), SupplementDescription);
        AddScalarFilterFields<StringGraphType>(nameof(SpecificationPartIndex.RootCause), RootCauseDescription);
        AddScalarFilterFields<BooleanGraphType>(nameof(SpecificationPartIndex.IsPlanned), IsPlannedDescription);
        AddScalarFilterFields<BooleanGraphType>(nameof(SpecificationPartIndex.IsIncident), IsIncidentDescription);
        AddScalarFilterFields<BooleanGraphType>(nameof(SpecificationPartIndex.IsInHouse), IsInHouseDescription);
        AddScalarFilterFields<BooleanGraphType>(nameof(SpecificationPartIndex.IsOutsourced), IsOutsourcedDescription);
        AddScalarFilterFields<StringGraphType>(nameof(SpecificationPartIndex.Status), ReportStatusDescription);
        AddScalarFilterFields<StringGraphType>(nameof(SpecificationPartIndex.OfferContentItemId), OfferContentItemIdDescription);
        AddScalarFilterFields<StringGraphType>(nameof(SpecificationPartIndex.Behavior), BehaviorDescription);
        AddScalarFilterFields<StringGraphType>(nameof(SpecificationPartIndex.Asset), AssetDescription);
        AddScalarFilterFields<StringGraphType>(nameof(SpecificationPartIndex.Event), EventDescription);
        AddScalarFilterFields<StringGraphType>(nameof(SpecificationPartIndex.Others), OthersDescription);
        AddScalarFilterFields<StringGraphType>(nameof(SpecificationPartIndex.Sender), SenderDescription);
        AddScalarFilterFields<StringGraphType>(nameof(SpecificationPartIndex.Writer), WriterDescription);
        AddScalarFilterFields<StringGraphType>(nameof(SpecificationPartIndex.Photos), PhotosDescription);
        AddScalarFilterFields<StringGraphType>(nameof(SpecificationPartIndex.Clips), ClipsDescription);
        AddScalarFilterFields<StringGraphType>(nameof(SpecificationPartIndex.Audio), AudioDescription);
        AddScalarFilterFields<StringGraphType>(nameof(SpecificationPartIndex.Files), FilesDescription);
        AddScalarFilterFields<StringGraphType>(nameof(SpecificationPartIndex.LocationContentItemId), LocationContentItemIdDescription);
        AddScalarFilterFields<StringGraphType>(nameof(SpecificationPartIndex.DateTime), DateTimeDescription);
    }
}
