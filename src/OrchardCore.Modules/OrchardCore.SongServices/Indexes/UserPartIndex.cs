using OrchardCore.SongServices.ContentParts;
using OrchardCore.ContentManagement;
using System;
using YesSql.Indexes;

namespace OrchardCore.SongServices.Indexes;

public class UserPartIndex : MapIndex
{
    public string ContentItemId { get; set; }
    public string Name { get; set; }
    public string AvatarId { get; set; }
    public string Currency { get; set; }
    public string CustomNickname { get; set; }
    public string UserName { get; set; }
    public string Nickname { get; set; }
    public string Op { get; set; }
    public string RiskId { get; set; }
    public string TestUser { get; set; }
    public DateTime? DateTime { get; set; }
}

// Don't forget to register this class with the service provider (see: Startup.cs).
public class UserPartIndexProvider : IndexProvider<ContentItem>
{
    public override void Describe(DescribeContext<ContentItem> context) =>
        context.For<UserPartIndex>()
            .When(contentItem => contentItem.Has<UserPart>())
            .Map(contentItem =>
            {
                var UserPart = contentItem.As<UserPart>();

                return new UserPartIndex
                {
                    ContentItemId = contentItem.ContentItemId,
                    Name = UserPart.Name,
                    AvatarId = UserPart.AvatarId,
                    Currency = UserPart.Currency,
                    CustomNickname = UserPart.CustomNickname,
                    UserName = UserPart.UserName,
                    Nickname = UserPart.Nickname,
                    Op = UserPart.Op,
                    RiskId = UserPart.RiskId,
                    TestUser = UserPart.TestUser,
                    DateTime = UserPart.DateTime,
                };
            });
}