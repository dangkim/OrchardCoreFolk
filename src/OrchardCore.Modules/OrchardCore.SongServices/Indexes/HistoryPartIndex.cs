using OrchardCore.SongServices.ContentParts;
using OrchardCore.ContentManagement;
using System;
using YesSql.Indexes;

namespace OrchardCore.SongServices.Indexes;

public class HistoryPartIndex : MapIndex
{
    public string ContentItemId { get; set; }
    public string Cards { get; set; }
    public string Powerup { get; set; }
    public string Status { get; set; }
    public string Kind { get; set; }
    public string Result { get; set; }
    public string Round { get; set; }
    public string RoundId { get; set; }
    public string Shoe { get; set; }
    public string Table { get; set; }
    public DateTime? DateTime { get; set; }
}

// Don't forget to register this class with the service provider (see: Startup.cs).
public class HistoryPartIndexProvider : IndexProvider<ContentItem>
{
    public override void Describe(DescribeContext<ContentItem> context) =>
        context.For<HistoryPartIndex>()
            .When(contentItem => contentItem.Has<HistoryPart>())
            .Map(contentItem =>
            {
                var HistoryPart = contentItem.As<HistoryPart>();

                return new HistoryPartIndex
                {
                    ContentItemId = contentItem.ContentItemId,
                    Cards = HistoryPart.Cards,
                    Powerup = HistoryPart.Powerup,
                    Status = HistoryPart.Status,
                    Kind = HistoryPart.Kind,
                    Result = HistoryPart.Result,
                    Round = HistoryPart.Round,
                    RoundId = HistoryPart.RoundId,
                    Shoe = HistoryPart.Shoe,
                    Table = HistoryPart.Table,
                    DateTime = HistoryPart.DateTime,
                };
            });
}