using System;
using System.Collections.Generic;
using System.Text;
using OrchardCore.ContentFields.Fields;
using OrchardCore.ContentManagement;
using OrchardCore.SimService.ContentParts;
using YesSql.Indexes;

namespace OrchardCore.SimService.Indexes
{
    public class UserProfilePartIndex : MapIndex
    {
        public string ContentItemId { get; set; }
        public int ProfileId { get; set; }
        public string Email { get; set; }
        public long UserId { get; set; }
        public string UserName { get; set; }
        public string Vendor { get; set; }
        public string DefaultForwardingNumber { get; set; }
        public decimal Balance { get; set; }
        public decimal OriginalAmount { get; set; }
        public decimal Amount { get; set; }
        public decimal RateInUsd { get; set; }
        public string GmailMsgId { get; set; }
        public int Rating { get; set; }
        public string DefaultCountryName { get; set; }
        public string DefaultIso { get; set; }
        public string DefaultPrefix { get; set; }
        public string DefaultOperatorName { get; set; }
        public decimal FrozenBalance { get; set; }
        public string TokenApi { get; set; }
    }

    public class UserProfilePartIndexProvider : IndexProvider<ContentItem>
    {
        public override void Describe(DescribeContext<ContentItem> context) =>
            context.For<UserProfilePartIndex>().Map(contentItem =>
            {
                var userProfilePart = contentItem.As<UserProfilePart>();

                return userProfilePart == null
                    ? null
                    : new UserProfilePartIndex
                    {
                        ContentItemId = contentItem.ContentItemId,
                        ProfileId = userProfilePart.ProfileId,
                        Email = userProfilePart.Email,
                        UserId = userProfilePart.UserId,
                        UserName = userProfilePart.UserName,
                        Vendor = userProfilePart.Vendor,
                        DefaultForwardingNumber = userProfilePart.DefaultForwardingNumber,
                        Balance = userProfilePart.Balance,
                        OriginalAmount = userProfilePart.OriginalAmount,
                        Amount = userProfilePart.Amount,
                        RateInUsd = userProfilePart.RateFromRub,
                        GmailMsgId = userProfilePart.GmailMsgId,
                        Rating = userProfilePart.Rating,
                        DefaultCountryName = userProfilePart.DefaultCountryName,
                        DefaultIso = userProfilePart.DefaultIso,
                        DefaultPrefix = userProfilePart.DefaultPrefix,
                        DefaultOperatorName = userProfilePart.DefaultOperatorName,
                        FrozenBalance = userProfilePart.FrozenBalance,
                        TokenApi = userProfilePart.TokenApi
                    };
            });
    }
}
