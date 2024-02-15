using OrchardCore.SongServices.ContentParts;
using OrchardCore.ContentManagement;
using System;
using YesSql.Indexes;

namespace OrchardCore.SongServices.Indexes;

public class LocationPartIndex : MapIndex
{
    public string ContentItemId { get; set; }
    public string Country { get; set; }
    public string City { get; set; }
    public string Street { get; set; }
    public string Site { get; set; }
    public string Building { get; set; }
    public string Floor { get; set; }
    public string Zone { get; set; }
    public string Room { get; set; }
    public DateTime? DateTime { get; set; }
}

// Don't forget to register this class with the service provider (see: Startup.cs).
public class LocationPartIndexProvider : IndexProvider<ContentItem>
{
    public override void Describe(DescribeContext<ContentItem> context) =>
        context.For<LocationPartIndex>()
            .When(contentItem => contentItem.Has<LocationPart>())
            .Map(contentItem =>
            {
                var locationPart = contentItem.As<LocationPart>();

                return new LocationPartIndex
                {
                    ContentItemId = contentItem.ContentItemId,
                    Country = locationPart.Country,
                    City = locationPart.City,
                    Street = locationPart.Street,
                    Site = locationPart.Site,
                    Building = locationPart.Building,
                    Floor = locationPart.Floor,
                    Zone = locationPart.Zone,
                    Room = locationPart.Room,
                    DateTime = locationPart.DateTime
                };
            });
}
