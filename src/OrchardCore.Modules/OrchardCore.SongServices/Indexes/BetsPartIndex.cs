using OrchardCore.SongServices.ContentParts;
using OrchardCore.ContentManagement;
using System;
using YesSql.Indexes;

namespace OrchardCore.SongServices.Indexes;

public class BetsPartIndex : MapIndex
{
    public string ContentItemId { get; set; }
    public string Kind { get; set; }
    public string Code { get; set; }
    public string Name { get; set; }
    public string Chips { get; set; }
    //public new string Id { get; set; }
    public decimal Max { get; set; }
    public decimal Min { get; set; }
    public bool Allowed { get; set; }
    public string Table { get; set; }
    public DateTime? DateTime { get; set; }
}

// Don't forget to register this class with the service provider (see: Startup.cs).
public class BetsPartIndexProvider : IndexProvider<ContentItem>
{
    public override void Describe(DescribeContext<ContentItem> context) =>
        context.For<BetsPartIndex>()
            .When(contentItem => contentItem.Has<BetsPart>())
            .Map(contentItem =>
            {
                var BetsPart = contentItem.As<BetsPart>();

                return new BetsPartIndex
                {
                    ContentItemId = contentItem.ContentItemId,
                    Kind = BetsPart.Kind,
                    Code = BetsPart.Code,
                    Name = BetsPart.Name,
                    Chips = BetsPart.Chips,
                    Max = BetsPart.Max,
                    Min = BetsPart.Min,
                    Allowed = BetsPart.Allowed,
                    Table = BetsPart.Table,
                    DateTime = BetsPart.DateTime,
                };
            });
}