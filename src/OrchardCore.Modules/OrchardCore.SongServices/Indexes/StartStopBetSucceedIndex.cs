using OrchardCore.SongServices.ContentParts;
using OrchardCore.ContentManagement;
using System;
using YesSql.Indexes;

namespace OrchardCore.SongServices.Indexes;

public class StartStopBetSucceedPartIndex : MapIndex
{
    public string ContentItemId { get; set; }
    public string Powerup { get; set; }
    public string Kind { get; set; }
    public string Content { get; set; }
    public string Round { get; set; }
    public string RoundId { get; set; }
    public string Shoe { get; set; }
    public string Status { get; set; }
    public DateTime? DateTime { get; set; }
}

// Don't forget to register this class with the service provider (see: Startup.cs).
public class StartStopBetSucceedPartIndexProvider : IndexProvider<ContentItem>
{
    public override void Describe(DescribeContext<ContentItem> context) =>
        context.For<StartStopBetSucceedPartIndex>()
            .When(contentItem => contentItem.Has<StartStopBetSucceedPart>())
            .Map(contentItem =>
            {
                var StartStopBetSucceedPart = contentItem.As<StartStopBetSucceedPart>();

                return new StartStopBetSucceedPartIndex
                {
                    ContentItemId = contentItem.ContentItemId,
                    Powerup = StartStopBetSucceedPart.Powerup,
                    Kind = StartStopBetSucceedPart.Kind,
                    Content = StartStopBetSucceedPart.Content,                    
                    Round = StartStopBetSucceedPart.Round,
                    RoundId = StartStopBetSucceedPart.RoundId,
                    Shoe = StartStopBetSucceedPart.Shoe,
                    Status = StartStopBetSucceedPart.Status,
                    DateTime = StartStopBetSucceedPart.DateTime,
                };
            });
}