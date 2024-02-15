using System;
using System.Collections.Generic;
using System.Text;
using OrchardCore.ContentFields.Fields;
using OrchardCore.ContentManagement;
using OrchardCore.SongServices.ContentParts;
using YesSql.Indexes;

namespace OrchardCore.SongServices.Indexing
{
    public class OfferFilteringPartIndex : MapIndex
    {
        public string ContentItemId { get; set; }
        public decimal MinAmount { get; set; }
        public decimal MaxAmount { get; set; }
        public string Status { get; set; }
        public string Wallet { get; set; }
        public string PaymentMethod { get; set; }
        public string PreferredCurrency { get; set; }
        public decimal Percentage { get; set; }
        public string OfferType { get; set; }
        public DateTime? DateTime { get; set; }
    }

    public class OfferFilteringPartIndexProvider : IndexProvider<ContentItem>
    {
        public override void Describe(DescribeContext<ContentItem> context) =>
            context.For<OfferFilteringPartIndex>().Map(contentItem =>
            {
                var offerFilteringPart = contentItem.As<OfferFilteringPart>();

                return offerFilteringPart == null
                    ? null
                    : new OfferFilteringPartIndex
                    {
                        ContentItemId = contentItem.ContentItemId,
                        MinAmount = offerFilteringPart.MinAmount,
                        MaxAmount = offerFilteringPart.MaxAmount,
                        Status = offerFilteringPart.OfferStatus,
                        Wallet = offerFilteringPart.Wallet,
                        PaymentMethod = offerFilteringPart.PaymentMethod,
                        Percentage = offerFilteringPart.Percentage,
                        PreferredCurrency = offerFilteringPart.PreferredCurrency,
                        OfferType = offerFilteringPart.OfferType,
                        DateTime = offerFilteringPart.DateTime
                    };
            });
    }
}
