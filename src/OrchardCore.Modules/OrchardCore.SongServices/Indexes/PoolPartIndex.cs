using OrchardCore.SongServices.ContentParts;
using OrchardCore.ContentManagement;
using System;
using YesSql.Indexes;

namespace OrchardCore.SongServices.Indexes;

public class PoolPartIndex : MapIndex
{
    public string ContentItemId { get; set; }
    public string Amount { get; set; }
    public string Cat { get; set; }
    public string Table { get; set; }
    public DateTime? DateTime { get; set; }
}

// Don't forget to register this class with the service provider (see: Startup.cs).
public class PoolPartIndexProvider : IndexProvider<ContentItem>
{
    public override void Describe(DescribeContext<ContentItem> context) =>
        context.For<PoolPartIndex>()
            .When(contentItem => contentItem.Has<PoolPart>())
            .Map(contentItem =>
            {
                var PoolPart = contentItem.As<PoolPart>();

                return new PoolPartIndex
                {
                    ContentItemId = contentItem.ContentItemId,
                    Table = PoolPart.Table,
                    DateTime = PoolPart.DateTime,
                };
            });
}