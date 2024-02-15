using GraphQL.Types;
using OrchardCore.SongServices.ContentParts;

namespace OrchardCore.SongServices.GraphQL.Services.User;

// We add a model for the content part to the GraphQL schema. Content Types are added by Orchard Core automatically.
public class UserPartObjectGraphType : ObjectGraphType<UserPart>
{
    // These fields have counterparts in the index so we should include the same text in the
    // UserPartWhereInputObjectGraphType without duplication.
    internal const string NameDescription = "The User's name.";
    internal const string AvatarIdDescription = "The User's avatarId.";
    internal const string BookmarkedReportContentItemIdsDescription = "The User's BookmarkedIds.";
    internal const string BalanceDescription = "The User's balance.";
    internal const string CurrencyDescription = "The User's currency.";
    internal const string CustomNicknameDescription = "The User's customNickname.";
    internal const string IdDescription = "The User's Id.";
    internal const string UserNameDescription = "The User's userName.";
    internal const string NicknameDescription = "The User's nickname.";
    internal const string OpDescription = "The User's op.";
    internal const string RiskIdDescription = "The User's riskId.";
    internal const string TestUserDescription = "The User's testUser.";
    internal const string DateTimeDescription = "The User's date";

    public UserPartObjectGraphType()
    {
        Field(part => part.Name, nullable: true).Description(NameDescription);
        Field(part => part.AvatarId, nullable: true).Description(AvatarIdDescription);
        Field(part => part.BookmarkedReportContentItemIds, nullable: true).Description(BookmarkedReportContentItemIdsDescription);
        Field(part => part.Balance, nullable: true).Description(BalanceDescription);
        Field(part => part.Currency, nullable: true).Description(CurrencyDescription);
        Field(part => part.CustomNickname, nullable: true).Description(CustomNicknameDescription);
        Field(part => part.Id, nullable: true).Description(IdDescription);
        Field(part => part.UserName, nullable: true).Description(UserNameDescription);
        Field(part => part.Nickname, nullable: true).Description(NicknameDescription);
        Field(part => part.Op, nullable: true).Description(OpDescription);
        Field(part => part.RiskId, nullable: true).Description(RiskIdDescription);
        Field(part => part.TestUser, nullable: true).Description(TestUserDescription);
        Field(part => part.DateTime, nullable: true).Description(DateTimeDescription);
    }
}
