using System;
using OrchardCore.ContentManagement;
using OrchardCore.SongServices.ContentParts;
using YesSql.Indexes;

namespace OrchardCore.SongServices.Indexing
{
    public class TraderForFilteringPartIndex : MapIndex
    {
        public string ContentItemId { get; set; }
        public string Name { get; set; }
        public bool IsActivatedTele { get; set; }
        public decimal BondVndBalance { get; set; }
        public decimal VndBalance { get; set; }
        public decimal BTCBalance { get; set; }
        public decimal ETHBalance { get; set; }
        public decimal USDT20Balance { get; set; }
        public string WithdrawVNDStatus { get; set; }
        public int ReferenceCode { get; set; }
        public string DateSend { get; set; }
        public long UserId { get; set; }
        public string Email { get; set; }
        public string PhoneNumber { get; set; }
        public string BankAccounts { get; set; }
        public string ChatIdTele { get; set; }
        public string DeviceId { get; set; }
        public DateTime? DateTime { get; set; }
    }

    public class TraderForFilteringPartIndexProvider : IndexProvider<ContentItem>
    {
        public override void Describe(DescribeContext<ContentItem> context) =>
            context.For<TraderForFilteringPartIndex>().Map(contentItem =>
            {
                var traderFilteringPart = contentItem.As<TraderForFilteringPart>();

                return traderFilteringPart == null
                    ? null
                    : new TraderForFilteringPartIndex
                    {
                        ContentItemId = contentItem.ContentItemId,
                        Name = traderFilteringPart.Name,
                        IsActivatedTele = traderFilteringPart.IsActivatedTele,
                        BondVndBalance = traderFilteringPart.BondVndBalance,
                        VndBalance = traderFilteringPart.VndBalance,
                        BTCBalance = traderFilteringPart.BTCBalance,
                        ETHBalance = traderFilteringPart.ETHBalance,
                        USDT20Balance = traderFilteringPart.USDT20Balance,
                        WithdrawVNDStatus = traderFilteringPart.WithdrawVNDStatus,
                        ReferenceCode = traderFilteringPart.ReferenceCode,
                        UserId = traderFilteringPart.UserId,
                        Email = traderFilteringPart.Email,
                        PhoneNumber= traderFilteringPart.PhoneNumber,
                        BankAccounts= traderFilteringPart.BankAccounts,
                        ChatIdTele= traderFilteringPart.ChatIdTele,
                        DeviceId = traderFilteringPart.DeviceId,
                        DateTime = traderFilteringPart.DateTime
                    };
            });
    }
}
