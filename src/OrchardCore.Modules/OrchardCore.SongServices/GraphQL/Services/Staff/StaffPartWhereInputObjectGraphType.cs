using GraphQL.Types;
using OrchardCore.SongServices.Indexes;
using OrchardCore.SongServices.ContentParts;
using OrchardCore.Apis.GraphQL.Queries;
using static OrchardCore.SongServices.GraphQL.Services.Staff.StaffPartObjectGraphType;

namespace OrchardCore.SongServices.GraphQL.Services.Staff;

// This class adds a content part-specific section in the "where" filter. It relies on the content part index to
// automatically generate YesSql logic for database-side filtering.
public class StaffPartWhereInputObjectGraphType : WhereInputObjectGraphType<StaffPart>
{
    public StaffPartWhereInputObjectGraphType()
    {        
        // Since filters depend on the index, we use their "nameof" as reference.
        AddScalarFilterFields<StringGraphType>(nameof(StaffPartIndex.Nickname), NicknameDescription);
        AddScalarFilterFields<StringGraphType>(nameof(StaffPartIndex.AvatarId), AvatarIdDescription);
        AddScalarFilterFields<StringGraphType>(nameof(StaffPartIndex.Operator), OperatorDescription);
        AddScalarFilterFields<StringGraphType>(nameof(StaffPartIndex.Team), TeamDescription);
        AddScalarFilterFields<StringGraphType>(nameof(StaffPartIndex.FullName), FullNameDescription);
        AddScalarFilterFields<StringGraphType>(nameof(StaffPartIndex.UserName), UserNameDescription);
        AddScalarFilterFields<StringGraphType>(nameof(StaffPartIndex.BookmarkedReportContentItemIds), BookmarkedReportContentItemIdsDescription);
        AddScalarFilterFields<DecimalGraphType>(nameof(StaffPartIndex.Balance), BalanceDescription);
        AddScalarFilterFields<StringGraphType>(nameof(StaffPartIndex.Currency), CurrencyDescription);
        AddScalarFilterFields<StringGraphType>(nameof(StaffPartIndex.CustomNickname), CustomNicknameDescription);
        AddScalarFilterFields<StringGraphType>(nameof(StaffPartIndex.StaffId), StaffIdDescription);
        AddScalarFilterFields<DateGraphType>(nameof(StaffPartIndex.Birthday), BirthdayDescription);
        AddScalarFilterFields<StringGraphType>(nameof(StaffPartIndex.DateTime), DateTimeDescription);
    }
}

// NEXT STATION: Services/StaffPartIndexAliasProvider.cs
