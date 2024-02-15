using GraphQL.Types;
using OrchardCore.SongServices.ContentParts;
using OrchardCore.ContentManagement;
using OrchardCore.ContentManagement.GraphQL.Queries;
using OrchardCore.DisplayManagement.Shapes;
using OrchardCore.Modules;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using YesSql;
using static OrchardCore.SongServices.GraphQL.Services.Staff.StaffPartTypeBuilder;
using OrchardCore.SongServices.Indexes;
using OrchardCore.ContentManagement.Records;
using GraphQL;

namespace OrchardCore.SongServices.GraphQL.Services.Staff;

// IGraphQLFilters can append conditions to the YesSql query, alter its result, or do both.
public class StaffPartGraphQLFilter : IGraphQLFilter<ContentItem>
{
    private readonly IClock _clock;

    public StaffPartGraphQLFilter(IClock clock) => _clock = clock;

    // While you can use this to execute some complex YesSql query it's best to stick with the IIndexAliasProvider
    // approach for such things.
    public Task<IQuery<ContentItem>> PreQueryAsync(IQuery<ContentItem> query, IResolveFieldContext context)
    {
        var (nickname, valueNickname) = context.Arguments.FirstOrDefault(
            argument => argument.Key.StartsWith(NickNameFilter, StringComparison.Ordinal));

        var (avatarId, valueAvatarId) = context.Arguments.FirstOrDefault(
            argument => argument.Key.StartsWith(AvatarIdFilter, StringComparison.Ordinal));

        var (operatorV, valueOperator) = context.Arguments.FirstOrDefault(
            argument => argument.Key.StartsWith(OperatorFilter, StringComparison.Ordinal));

        var (team, valueTeam) = context.Arguments.FirstOrDefault(
            argument => argument.Key.StartsWith(TeamFilter, StringComparison.Ordinal));

        var (fullName, valueFullName) = context.Arguments.FirstOrDefault(
            argument => argument.Key.StartsWith(FullNameFilter, StringComparison.Ordinal));

        var (userName, valueUserName) = context.Arguments.FirstOrDefault(
            argument => argument.Key.StartsWith(UserNameFilter, StringComparison.Ordinal));

        var (bookmarkedReportContentItemIds, valueBookmarkedReportContentItemIds) = context.Arguments.FirstOrDefault(
            argument => argument.Key.StartsWith(BookmarkedReportContentItemIdsFilter, StringComparison.Ordinal));

        var (balance, valueBalance) = context.Arguments.FirstOrDefault(
            argument => argument.Key.StartsWith(BalanceFilter, StringComparison.Ordinal));

        var (currency, valueCurrency) = context.Arguments.FirstOrDefault(
            argument => argument.Key.StartsWith(CurrencyFilter, StringComparison.Ordinal));

        var (customNickname, valueCustomNickname) = context.Arguments.FirstOrDefault(
            argument => argument.Key.StartsWith(CustomNicknameFilter, StringComparison.Ordinal));

        var (staffId, valueStaffId) = context.Arguments.FirstOrDefault(
            argument => argument.Key.StartsWith(StaffIdFilter, StringComparison.Ordinal));

        var (birthday, valueBirthday) = context.Arguments.FirstOrDefault(
            argument => argument.Key.StartsWith(BirthdayFilter, StringComparison.Ordinal));

        var (dateTime, valueDateTime) = context.Arguments.FirstOrDefault(
            argument => argument.Key.StartsWith(DateTimeFilter, StringComparison.Ordinal));

        if (nickname != null && valueNickname.Value != null)
        {
            var staffQuery = query.With<ContentItemIndex>(c => c.Published && c.Latest).With<StaffPartIndex>(index => index.Nickname == valueNickname.Value.ToString()).Take(10000);
            return Task.FromResult(staffQuery);
        }

        return Task.FromResult(query);
    }

    // You can use this method to filter offline or in separate requests. This is less efficient but it's necessary if
    // the request can't be described as a single YesSql query. In this case we work off of a property that's not
    // indexed for demonstration's sake.
    public Task<IEnumerable<ContentItem>> PostQueryAsync(
        IEnumerable<ContentItem> contentItems,
        IResolveFieldContext context)
    {
        return Task.FromResult(contentItems);
    }
}
