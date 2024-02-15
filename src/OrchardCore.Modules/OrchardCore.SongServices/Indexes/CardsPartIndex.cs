using OrchardCore.SongServices.ContentParts;
using OrchardCore.ContentManagement;
using System;
using YesSql.Indexes;

namespace OrchardCore.SongServices.Indexes;

public class CardsPartIndex : MapIndex
{
    public string ContentItemId { get; set; }
    public string Table { get; set; }
    public string Kind { get; set; }
    public string Card { get; set; }
    public string Pos { get; set; }
    public DateTime? DateTime { get; set; }
}

// Don't forget to register this class with the service provider (see: Startup.cs).
public class CardsPartIndexProvider : IndexProvider<ContentItem>
{
    public override void Describe(DescribeContext<ContentItem> context) =>
        context.For<CardsPartIndex>()
            .When(contentItem => contentItem.Has<CardsPart>())
            .Map(contentItem =>
            {
                var CardsPart = contentItem.As<CardsPart>();

                return new CardsPartIndex
                {
                    ContentItemId = contentItem.ContentItemId,
                    Table = CardsPart.Table,
                    Kind = CardsPart.Kind,
                    Card = CardsPart.Card,
                    Pos = CardsPart.Pos,
                    DateTime = CardsPart.DateTime,
                };
            });
}