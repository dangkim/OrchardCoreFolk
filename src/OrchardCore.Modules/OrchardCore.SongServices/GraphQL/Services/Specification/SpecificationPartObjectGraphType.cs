using GraphQL.Types;
using OrchardCore.SongServices.ContentParts;

namespace OrchardCore.SongServices.GraphQL.Services.Specification;

// We add a model for the content part to the GraphQL schema. Content Types are added by Orchard Core automatically.
public class SpecificationPartObjectGraphType : ObjectGraphType<SpecificationPart>
{
    // These fields have counterparts in the index so we should include the same text in the
    // SpecificationPartWhereInputObjectGraphType without duplication.
    internal const string DescriptionDescription = "The Specification's description.";
    internal const string AssignerContentItemIdDescription = "The Specification's assignerContentItemId.";
    internal const string AssigneeContentItemIdDescription = "The Specification's assigneeContentItemId.";
    internal const string SupplementDescription = "The Specification's supplement.";
    internal const string RootCauseDescription = "The Specification's rootCause.";
    internal const string IsPlannedDescription = "The Specification's isPlanned.";
    internal const string IsIncidentDescription = "The Specification's isIncident.";
    internal const string IsInHouseDescription = "The Specification's isInHouse.";
    internal const string IsOutsourcedDescription = "The Specification's isOutsourced.";
    internal const string ReportStatusDescription = "The Specification's reportStatus.";
    internal const string OfferContentItemIdDescription = "The Specification's offerContentItemId.";
    internal const string BehaviorDescription = "The Specification's behavior.";
    internal const string AssetDescription = "The Specification's asset.";
    internal const string EventDescription = "The Specification's event.";
    internal const string OthersDescription = "The Specification's others.";
    internal const string SenderDescription = "The Specification's sender.";
    internal const string WriterDescription = "The Specification's writer.";
    internal const string PhotosDescription = "The Specification's photos.";
    internal const string ClipsDescription = "The Specification's clips.";
    internal const string AudioDescription = "The Specification's audio.";
    internal const string FilesDescription = "The Specification's files.";
    internal const string LocationContentItemIdDescription = "The Specification's locationContentItemId.";
    internal const string DateTimeDescription = "The Specification's date";

    public SpecificationPartObjectGraphType()
    {
        Field(part => part.Description, nullable: true).Description(DescriptionDescription);
        Field(part => part.AssignerContentItemId, nullable: true).Description(AssignerContentItemIdDescription);
        Field(part => part.AssigneeContentItemId, nullable: true).Description(AssigneeContentItemIdDescription);
        Field(part => part.Supplement, nullable: true).Description(SupplementDescription);
        Field(part => part.RootCause, nullable: true).Description(RootCauseDescription);
        Field(part => part.IsPlanned, nullable: true).Description(IsPlannedDescription);
        Field(part => part.IsIncident, nullable: true).Description(IsIncidentDescription);
        Field(part => part.IsInHouse, nullable: true).Description(IsInHouseDescription);
        Field(part => part.IsOutsourced, nullable: true).Description(IsOutsourcedDescription);
        Field(part => part.ReportStatus, nullable: true).Description(ReportStatusDescription);
        Field(part => part.OfferContentItemId, nullable: true).Description(OfferContentItemIdDescription);
        Field(part => part.Behavior, nullable: true).Description(BehaviorDescription);
        Field(part => part.Asset, nullable: true).Description(AssetDescription);
        Field(part => part.Event, nullable: true).Description(EventDescription);
        Field(part => part.Others, nullable: true).Description(OthersDescription);
        Field(part => part.Sender, nullable: true).Description(SenderDescription);
        Field(part => part.Writer, nullable: true).Description(WriterDescription);
        Field(part => part.Photos, nullable: true).Description(PhotosDescription);
        Field(part => part.Clips, nullable: true).Description(ClipsDescription);
        Field(part => part.Audio, nullable: true).Description(AudioDescription);
        Field(part => part.Files, nullable: true).Description(FilesDescription);
        Field(part => part.DateTime, nullable: true).Description(DateTimeDescription);
    }
}
