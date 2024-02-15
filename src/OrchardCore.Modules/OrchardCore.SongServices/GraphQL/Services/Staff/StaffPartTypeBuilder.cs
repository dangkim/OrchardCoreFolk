using GraphQL.Types;
using OrchardCore.ContentManagement.GraphQL.Queries.Types;
using OrchardCore.ContentManagement.Metadata.Models;
using System.Linq;

namespace OrchardCore.SongServices.GraphQL.Services.Staff;

// Services that implement IContentTypeBuilder extend the features of existing ContentItem type fields, including the
// top level fields automatically created by Orchard Core for every content type. You can use this to add new sub-fields
// or filter attributes to existing ContentItem type fields.
public class StaffPartTypeBuilder : IContentTypeBuilder
{
    // It's a good practice to make the argument name a const because you will reuse it in the IGraphQLFilter.
    public const string NickNameFilter = "nickname";
    public const string AvatarIdFilter = "avatarId";
    public const string OperatorFilter = "operator";
    public const string TeamFilter = "team";
    public const string FullNameFilter = "fullName";
    public const string UserNameFilter = "userName";
    public const string BookmarkedReportContentItemIdsFilter = "bookmarkedReportContentItemIds";
    public const string BalanceFilter = "balance";
    public const string CurrencyFilter = "currency";
    public const string CustomNicknameFilter = "customNickname";
    public const string StaffIdFilter = "staffId";
    public const string BirthdayFilter = "birthday";
    public const string DateTimeFilter = "date";

    // Here you can add arguments to every Content Type (top level) field.
    public void Build(
        FieldType contentQuery,
        ContentTypeDefinition contentTypeDefinition,
        ContentItemType contentItemType)
    {
        // You can check to see if the field has any specific sub-field, if you want to rely on its features. For
        // example if you only want to apply to ContentItem fields that have the "person" sub-field (i.e. those that
        // have a PersonPart). This is useful if you want to expand your content part field in another module.

        if (contentItemType.Name != ContentTypes.StaffPage) return;

        contentQuery.Arguments.Add(new QueryArgument<StringGraphType>
        {
            Name = NickNameFilter,
            ResolvedType = new StringGraphType(),
        });

        contentQuery.Arguments.Add(new QueryArgument<StringGraphType>
        {
            Name = AvatarIdFilter,
            ResolvedType = new StringGraphType(),
        });

        contentQuery.Arguments.Add(new QueryArgument<StringGraphType>
        {
            Name = OperatorFilter,
            ResolvedType = new StringGraphType(),
        });

        contentQuery.Arguments.Add(new QueryArgument<StringGraphType>
        {
            Name = TeamFilter,
            ResolvedType = new StringGraphType(),
        });

        contentQuery.Arguments.Add(new QueryArgument<StringGraphType>
        {
            Name = FullNameFilter,
            ResolvedType = new StringGraphType(),
        });

        contentQuery.Arguments.Add(new QueryArgument<StringGraphType>
        {
            Name = UserNameFilter,
            ResolvedType = new StringGraphType(),
        });

        contentQuery.Arguments.Add(new QueryArgument<StringGraphType>
        {
            Name = BookmarkedReportContentItemIdsFilter,
            ResolvedType = new StringGraphType(),
        });

        contentQuery.Arguments.Add(new QueryArgument<DecimalGraphType>
        {
            Name = BalanceFilter,
            ResolvedType = new DecimalGraphType(),
        });

        contentQuery.Arguments.Add(new QueryArgument<StringGraphType>
        {
            Name = CurrencyFilter,
            ResolvedType = new StringGraphType(),
        });

        contentQuery.Arguments.Add(new QueryArgument<StringGraphType>
        {
            Name = CustomNicknameFilter,
            ResolvedType = new StringGraphType(),
        });

        contentQuery.Arguments.Add(new QueryArgument<StringGraphType>
        {
            Name = StaffIdFilter,
            ResolvedType = new StringGraphType(),
        });

        contentQuery.Arguments.Add(new QueryArgument<StringGraphType>
        {
            Name = BirthdayFilter,
            ResolvedType = new StringGraphType(),
        });

        contentQuery.Arguments.Add(new QueryArgument<StringGraphType>
        {
            Name = DateTimeFilter,
            ResolvedType = new StringGraphType(),
        });

        /*------------------------------------------------------------*/
        AddFilterNickname(contentQuery, "_ne");
        AddFilterAvatarId(contentQuery, "_ne");
        AddFilterOperator(contentQuery, "_ne");
        AddFilterTeam(contentQuery, "_ne");
        AddFilterFullName(contentQuery, "_ne");
        AddFilterUserName(contentQuery, "_ne");
        AddFilterBookmarkedReportContentItemIds(contentQuery, "_ne");
        AddFilterCurrency(contentQuery, "_ne");
        AddFilterCustomNickname(contentQuery, "_ne");
        AddFilterStaffId(contentQuery, "_ne");
        AddFilterBirthday(contentQuery, "_ne");

        /*------------------------------------------------------------*/
        AddFilterBalance(contentQuery, "_lt");
        AddFilterBalance(contentQuery, "_le");
        AddFilterBalance(contentQuery, "_ge");
        AddFilterBalance(contentQuery, "_gt");
        AddFilterBalance(contentQuery, "_ne");
    }

    private static void AddFilterNickname(FieldType contentQuery, string suffix) =>
        contentQuery.Arguments.Add(new QueryArgument<StringGraphType>
        {
            Name = NickNameFilter + suffix,
            ResolvedType = new StringGraphType(),
        });

    private static void AddFilterAvatarId(FieldType contentQuery, string suffix) =>
        contentQuery.Arguments.Add(new QueryArgument<StringGraphType>
        {
            Name = AvatarIdFilter + suffix,
            ResolvedType = new StringGraphType(),
        });

    private static void AddFilterOperator(FieldType contentQuery, string suffix) =>
        contentQuery.Arguments.Add(new QueryArgument<StringGraphType>
        {
            Name = OperatorFilter + suffix,
            ResolvedType = new StringGraphType(),
        });

    private static void AddFilterTeam(FieldType contentQuery, string suffix) =>
        contentQuery.Arguments.Add(new QueryArgument<StringGraphType>
        {
            Name = TeamFilter + suffix,
            ResolvedType = new StringGraphType(),
        });

    private static void AddFilterFullName(FieldType contentQuery, string suffix) =>
        contentQuery.Arguments.Add(new QueryArgument<StringGraphType>
        {
            Name = FullNameFilter + suffix,
            ResolvedType = new StringGraphType(),
        });

    private static void AddFilterUserName(FieldType contentQuery, string suffix) =>
        contentQuery.Arguments.Add(new QueryArgument<StringGraphType>
        {
            Name = UserNameFilter + suffix,
            ResolvedType = new StringGraphType(),
        });

    private static void AddFilterBookmarkedReportContentItemIds(FieldType contentQuery, string suffix) =>
        contentQuery.Arguments.Add(new QueryArgument<StringGraphType>
        {
            Name = BookmarkedReportContentItemIdsFilter + suffix,
            ResolvedType = new StringGraphType(),
        });

    private static void AddFilterBalance(FieldType contentQuery, string suffix) =>
        contentQuery.Arguments.Add(new QueryArgument<DecimalGraphType>
        {
            Name = BalanceFilter + suffix,
            ResolvedType = new DecimalGraphType(),
        });
    private static void AddFilterCurrency(FieldType contentQuery, string suffix) =>
        contentQuery.Arguments.Add(new QueryArgument<StringGraphType>
        {
            Name = CurrencyFilter + suffix,
            ResolvedType = new StringGraphType(),
        });

    private static void AddFilterCustomNickname(FieldType contentQuery, string suffix) =>
        contentQuery.Arguments.Add(new QueryArgument<StringGraphType>
        {
            Name = CustomNicknameFilter + suffix,
            ResolvedType = new StringGraphType(),
        });

    private static void AddFilterStaffId(FieldType contentQuery, string suffix) =>
        contentQuery.Arguments.Add(new QueryArgument<StringGraphType>
        {
            Name = StaffIdFilter + suffix,
            ResolvedType = new StringGraphType(),
        });

    private static void AddFilterBirthday(FieldType contentQuery, string suffix) =>
        contentQuery.Arguments.Add(new QueryArgument<DateTimeGraphType>
        {
            Name = BirthdayFilter + suffix,
            ResolvedType = new DateTimeGraphType(),
        });

}
