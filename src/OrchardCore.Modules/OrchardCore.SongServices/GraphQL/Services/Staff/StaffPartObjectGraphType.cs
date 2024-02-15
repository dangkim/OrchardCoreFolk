using GraphQL.Types;
using OrchardCore.SongServices.ContentParts;

namespace OrchardCore.SongServices.GraphQL.Services.Staff;

// We add a model for the content part to the GraphQL schema. Content Types are added by Orchard Core automatically.
public class StaffPartObjectGraphType : ObjectGraphType<StaffPart>
{
    // These fields have counterparts in the index so we should include the same text in the
    // StaffPartWhereInputObjectGraphType without duplication.
    internal const string NicknameDescription = "The Staff's nickname.";
    internal const string AvatarIdDescription = "The Staff's avatarId.";
    internal const string OperatorDescription = "The Staff's operator.";
    internal const string TeamDescription = "The Staff's team.";
    internal const string FullNameDescription = "The Staff's fullName.";
    internal const string UserNameDescription = "The Staff's userName.";
    internal const string BookmarkedReportContentItemIdsDescription = "The Staff's bookmarkedReportContentItemIds.";
    internal const string BalanceDescription = "The Staff's balance.";
    internal const string CurrencyDescription = "The Staff's currency.";
    internal const string CustomNicknameDescription = "The Staff's customNickname.";
    internal const string StaffIdDescription = "The Staff's staffId.";
    internal const string BirthdayDescription = "The Staff's birthday.";
    internal const string DateTimeDescription = "The Staff's date";

    public StaffPartObjectGraphType()
    {
        Field(part => part.Nickname, nullable: true).Description(NicknameDescription);
        Field(part => part.AvatarId, nullable: true).Description(AvatarIdDescription);
        Field(part => part.Operator, nullable: true).Description(OperatorDescription);
        Field(part => part.Team, nullable: true).Description(TeamDescription);
        Field(part => part.FullName, nullable: true).Description(FullNameDescription);
        Field(part => part.UserName, nullable: true).Description(UserNameDescription);
        Field(part => part.BookmarkedReportContentItemIds, nullable: true).Description(BookmarkedReportContentItemIdsDescription);
        Field(part => part.Balance, nullable: true).Description(BalanceDescription);
        Field(part => part.Currency, nullable: true).Description(CurrencyDescription);
        Field(part => part.CustomNickname, nullable: true).Description(CustomNicknameDescription);
        Field(part => part.StaffId, nullable: true).Description(StaffIdDescription);
        Field(part => part.Birthday, nullable: true).Description(BirthdayDescription);
        Field(part => part.DateTime, nullable: true).Description(DateTimeDescription);
    }
}
