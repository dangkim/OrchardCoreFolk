using OrchardCore.SongServices.ContentParts;
using OrchardCore.ContentManagement;
using System;
using YesSql.Indexes;

namespace OrchardCore.SongServices.Indexes;

public class TicksPartIndex : MapIndex
{
    public string ContentItemId { get; set; }
    public string Table { get; set; }
    public string Value { get; set; }
    public DateTime? DateTime { get; set; }
}

// Don't forget to register this class with the service provider (see: Startup.cs).
public class TicksPartIndexProvider : IndexProvider<ContentItem>
{
    public override void Describe(DescribeContext<ContentItem> context) =>
        context.For<TicksPartIndex>()
            .When(contentItem => contentItem.Has<TicksPart>())
            .Map(contentItem =>
            {
                var TicksPart = contentItem.As<TicksPart>();

                return new TicksPartIndex
                {
                    ContentItemId = contentItem.ContentItemId,
                    Table = TicksPart.Table,
                    Value = TicksPart.Value,
                    DateTime = TicksPart.DateTime,
                };
            });
}