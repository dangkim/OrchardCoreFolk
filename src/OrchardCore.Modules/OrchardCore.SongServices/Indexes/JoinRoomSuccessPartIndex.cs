using OrchardCore.SongServices.ContentParts;
using OrchardCore.ContentManagement;
using System;
using YesSql.Indexes;

namespace OrchardCore.SongServices.Indexes;

public class JoinRoomSuccessPartIndex : MapIndex
{
    public string ContentItemId { get; set; }
    public string Room { get; set; }
    public string Seat { get; set; }
    public string Table { get; set; }
    public DateTime? DateTime { get; set; }
}

// Don't forget to register this class with the service provider (see: Startup.cs).
public class JoinRoomSuccessPartIndexProvider : IndexProvider<ContentItem>
{
    public override void Describe(DescribeContext<ContentItem> context) =>
        context.For<JoinRoomSuccessPartIndex>()
            .When(contentItem => contentItem.Has<JoinRoomSuccessPart>())
            .Map(contentItem =>
            {
                var JoinRoomSuccessPart = contentItem.As<JoinRoomSuccessPart>();

                return new JoinRoomSuccessPartIndex
                {
                    ContentItemId = contentItem.ContentItemId,
                    Room = JoinRoomSuccessPart.Room,
                    Seat = JoinRoomSuccessPart.Seat,
                    Table = JoinRoomSuccessPart.Table,
                    DateTime = JoinRoomSuccessPart.DateTime,
                };
            });
}