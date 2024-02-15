using OrchardCore.SongServices.ContentParts;
using OrchardCore.ContentManagement;
using System;
using YesSql.Indexes;

namespace OrchardCore.SongServices.Indexes;

public class StaffPartIndex : MapIndex
{
    public string ContentItemId { get; set; }
    public string Nickname { get; set; }
    public string AvatarId { get; set; }
    public string Operator { get; set; }
    public string Team { get; set; }
    public string FullName { get; set; }
    public string UserName { get; set; }
    public string BookmarkedReportContentItemIds { get; set; }// split comma
    public decimal Balance { get; set; }
    public string Currency { get; set; }
    public string CustomNickname { get; set; }
    public string StaffId { get; set; }
    public DateTime? Birthday { get; set; }
    public DateTime? DateTime { get; set; }
}

// Don't forget to register this class with the service provider (see: Startup.cs).
public class StaffPartIndexProvider : IndexProvider<ContentItem>
{
    public override void Describe(DescribeContext<ContentItem> context) =>
        context.For<StaffPartIndex>()
            .When(contentItem => contentItem.Has<StaffPart>())
            .Map(contentItem =>
            {
                var staffPart = contentItem.As<StaffPart>();

                return new StaffPartIndex
                {
                    Nickname = staffPart.Nickname,
                    AvatarId = staffPart.AvatarId,
                    Operator = staffPart.Operator,
                    Team = staffPart.Team,
                    FullName = staffPart.FullName,
                    UserName = staffPart.UserName,
                    BookmarkedReportContentItemIds = staffPart.BookmarkedReportContentItemIds,
                    Balance = staffPart.Balance,
                    Currency = staffPart.Currency,
                    CustomNickname = staffPart.CustomNickname,
                    StaffId = staffPart.StaffId,
                    Birthday = staffPart.Birthday
                };
            });
}
