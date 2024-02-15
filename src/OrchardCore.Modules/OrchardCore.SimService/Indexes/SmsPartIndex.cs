using System;
using System.Collections.Generic;
using System.Text;
using OrchardCore.ContentFields.Fields;
using OrchardCore.ContentManagement;
using OrchardCore.SimService.Models;
using YesSql.Indexes;

namespace OrchardCore.SimService.Indexes
{
    public class SmsPartIndex : MapIndex
    {
        public string ContentItemId { get; set; }
        public DateTime Created_at { get; set; }
        public string Sender { get; set; }
        public string Text { get; set; }
        public string Code { get; set; }
        public string Email { get; set; }
        public long UserId { get; set; }
        public string UserName { get; set; }
        public DateTime Date { get; set; }
        public long OrderId { get; set; }
    }

    public class SmsPartIndexProvider : IndexProvider<ContentItem>
    {
        public override void Describe(DescribeContext<ContentItem> context) =>
            context.For<SmsPartIndex>().Map(contentItem =>
            {
                var smsPart = contentItem.As<SmsPart>();

                return smsPart == null
                    ? null
                    : new SmsPartIndex
                    {
                        ContentItemId = contentItem.ContentItemId,
                        Created_at = smsPart.Created_at,
                        Sender = smsPart.Sender,
                        Text = smsPart.Text,
                        Code = smsPart.Code,
                        Email = smsPart.Email,
                        UserId = smsPart.UserId,
                        UserName = smsPart.UserName,
                        Date = smsPart.Date,
                        OrderId = smsPart.OrderId
                    };
            });
    }
}
