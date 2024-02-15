using System;
using System.Collections.Generic;
using System.Text;
using OrchardCore.ContentFields.Fields;
using OrchardCore.ContentManagement;
using OrchardCore.SimService.Models;
using YesSql.Indexes;

namespace OrchardCore.SimService.Indexes
{
    public class PaymentDetailPartIndex : MapIndex
    {
        public string ContentItemId { get; set; }
        public long PaymentId { get; set; }
        public string TypeName { get; set; }
        public string ProviderName { get; set; }
        public decimal Amount { get; set; }
        public decimal Balance { get; set; }
        public DateTime CreatedAt { get; set; }
        public string Email { get; set; }
        public long UserId { get; set; }
        public string UserName { get; set; }
        public long OrderId { get; set; }
    }

    public class PaymentDetailPartIndexProvider : IndexProvider<ContentItem>
    {
        public override void Describe(DescribeContext<ContentItem> context) =>
            context.For<PaymentDetailPartIndex>().Map(contentItem =>
            {
                var paymentDetailPart = contentItem.As<PaymentDetailPart>();

                return paymentDetailPart == null
                    ? null
                    : new PaymentDetailPartIndex
                    {
                        ContentItemId = contentItem.ContentItemId,
                        PaymentId = paymentDetailPart.PaymentId,
                        TypeName = paymentDetailPart.TypeName,
                        ProviderName = paymentDetailPart.ProviderName,
                        Amount = paymentDetailPart.Amount,
                        Balance = paymentDetailPart.Balance,
                        CreatedAt = paymentDetailPart.CreatedAt,
                        UserId = paymentDetailPart.UserId,
                        UserName = paymentDetailPart.UserName,
                        Email = paymentDetailPart.Email,
                        OrderId = paymentDetailPart.OrderId
                    };
            });
    }
}
