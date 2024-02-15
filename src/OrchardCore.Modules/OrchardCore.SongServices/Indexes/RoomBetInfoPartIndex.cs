using OrchardCore.SongServices.ContentParts;
using OrchardCore.ContentManagement;
using System;
using YesSql.Indexes;

namespace OrchardCore.SongServices.Indexes;

public class RoomBetInfoPartIndex : MapIndex
{
    public string ContentItemId { get; set; }
    public string Amounts { get; set; }
    public string Avatars { get; set; }
    public string Cats { get; set; }
    public string Ids { get; set; }
    public string Nicknames { get; set; }
    public string Room { get; set; }
    public string Seats { get; set; }
    public string Table { get; set; }
    public DateTime? DateTime { get; set; }
}

// Don't forget to register this class with the service provider (see: Startup.cs).
public class RoomBetInfoPartIndexProvider : IndexProvider<ContentItem>
{
    public override void Describe(DescribeContext<ContentItem> context) =>
        context.For<RoomBetInfoPartIndex>()
            .When(contentItem => contentItem.Has<RoomBetInfoPart>())
            .Map(contentItem =>
            {
                var RoomBetInfoPart = contentItem.As<RoomBetInfoPart>();

                return new RoomBetInfoPartIndex
                {
                    ContentItemId = contentItem.ContentItemId,
                    Amounts = RoomBetInfoPart.Amounts,
                    Avatars = RoomBetInfoPart.Avatars,
                    Cats = RoomBetInfoPart.Cats,
                    Ids = RoomBetInfoPart.Ids,
                    Nicknames = RoomBetInfoPart.Nicknames,
                    Room = RoomBetInfoPart.Room,
                    Seats = RoomBetInfoPart.Seats,
                    Table = RoomBetInfoPart.Table,
                    DateTime = RoomBetInfoPart.DateTime,
                };
            });
}