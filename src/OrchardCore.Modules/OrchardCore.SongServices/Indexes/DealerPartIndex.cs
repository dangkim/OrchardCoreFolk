using OrchardCore.SongServices.ContentParts;
using OrchardCore.ContentManagement;
using System;
using YesSql.Indexes;

namespace OrchardCore.SongServices.Indexes;

public class DealerPartIndex : MapIndex
{
    public string ContentItemId { get; set; }
    public string Name { get; set; }
    public string Table { get; set; }
    public string Value { get; set; }
    public DateTime? DateTime { get; set; }
}

// Don't forget to register this class with the service provider (see: Startup.cs).
public class DealerPartIndexProvider : IndexProvider<ContentItem>
{
    public override void Describe(DescribeContext<ContentItem> context) =>
        context.For<DealerPartIndex>()
            .When(contentItem => contentItem.Has<DealerPart>())
            .Map(contentItem =>
            {
                var DealerPart = contentItem.As<DealerPart>();

                return new DealerPartIndex
                {
                    ContentItemId = contentItem.ContentItemId,
                    Name = DealerPart.Name,
                    Table = DealerPart.Table,
                    Value = DealerPart.Value,
                    DateTime = DealerPart.DateTime,
                };
            });
}