using OrchardCore.SongServices.ContentParts;
using OrchardCore.ContentManagement;
using System;
using YesSql.Indexes;

namespace OrchardCore.SongServices.Indexes;

public class SpecificationPartIndex : MapIndex
{
    public string ContentItemId { get; set; }
    public string Description { get; set; }
    public string AssignerContentItemId { get; set; }
    public string AssigneeContentItemId { get; set; }
    public string Supplement { get; set; }
    public string RootCause { get; set; }
    public bool IsPlanned { get; set; }
    public bool IsIncident { get; set; }
    public bool IsInHouse { get; set; }
    public bool IsOutsourced { get; set; }
    public string Status { get; set; }
    public string OfferContentItemId { get; set; }
    public string Behavior { get; set; }
    public string Asset { get; set; }
    public string Event { get; set; }
    public string Others { get; set; }
    public string Sender { get; set; }
    public string Writer { get; set; }
    public string Photos { get; set; }
    public string Clips { get; set; }
    public string Audio { get; set; }
    public string Files { get; set; }
    public string LocationContentItemId { get; set; }
    public DateTime? DateTime { get; set; }
}

// Don't forget to register this class with the service provider (see: Startup.cs).
public class SpecificationPartIndexProvider : IndexProvider<ContentItem>
{
    public override void Describe(DescribeContext<ContentItem> context) =>
        context.For<SpecificationPartIndex>()
            .When(contentItem => contentItem.Has<SpecificationPart>())
            .Map(contentItem =>
            {
                var specificationPart = contentItem.As<SpecificationPart>();

                return new SpecificationPartIndex
                {
                    ContentItemId = contentItem.ContentItemId,
                    Description = specificationPart.Description,
                    AssignerContentItemId = specificationPart.AssignerContentItemId,
                    AssigneeContentItemId = specificationPart.AssigneeContentItemId,
                    Supplement = specificationPart.Supplement,
                    RootCause = specificationPart.RootCause,
                    IsPlanned = specificationPart.IsPlanned,
                    IsIncident = specificationPart.IsIncident,
                    IsInHouse = specificationPart.IsInHouse,
                    IsOutsourced = specificationPart.IsOutsourced,
                    Status = specificationPart.ReportStatus,
                    OfferContentItemId = specificationPart.OfferContentItemId,
                    Behavior = specificationPart.Behavior,
                    Asset = specificationPart.Asset,
                    Event = specificationPart.Event,
                    Others = specificationPart.Others,
                    Sender = specificationPart.Sender,
                    Writer = specificationPart.Writer,
                    Photos = specificationPart.Photos,
                    Clips = specificationPart.Clips,
                    Audio = specificationPart.Audio,
                    Files = specificationPart.Files,
                    LocationContentItemId = specificationPart.LocationContentItemId,
                    DateTime = specificationPart.DateTime
                };
            });
}
