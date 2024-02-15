/* Orchard Core's GraphQL module gives you a lot of features out-of-the-box. Content types, fields and some built-in
 * parts are automatically included. So all you have to worry about is adding your custom content parts and filter
 * arguments to the GraphQL "schema".
 * Warning: GraphQL calls its properties "fields". To minimize confusion with Orchard Core's fields, we refer to the
 * latter exclusively as "content fields" throughout this training section.
 */

using OrchardCore.SongServices.GraphQL.Services;
using OrchardCore.SongServices.ContentParts;
using Microsoft.Extensions.DependencyInjection;
using OrchardCore.Apis;
using OrchardCore.ContentManagement;
using OrchardCore.ContentManagement.GraphQL.Queries;
using OrchardCore.ContentManagement.GraphQL.Queries.Types;
using OrchardCore.Modules;
using OrchardCore.SongServices.GraphQL.Services.Bets;
using OrchardCore.SongServices.GraphQL.Services.Cards;
using OrchardCore.SongServices.GraphQL.Services.Dealer;
using OrchardCore.SongServices.GraphQL.Services.History;
using OrchardCore.SongServices.GraphQL.Services.JoinRoomSuccess;
using OrchardCore.SongServices.GraphQL.Services.Pool;
using OrchardCore.SongServices.GraphQL.Services.RoomBetInfo;
using OrchardCore.SongServices.GraphQL.Services.ShowLobby;
using OrchardCore.SongServices.GraphQL.Services.StartStopBetSucceed;
using OrchardCore.SongServices.GraphQL.Services.Ticks;
using OrchardCore.SongServices.GraphQL.Services.User;
using OrchardCore.SongServices.GraphQL.Services.Trader;
using OrchardCore.SongServices.GraphQL.Services.Offer;
using OrchardCore.SongServices.GraphQL.Services.Trade;
using OrchardCore.SongServices.GraphQL.Services.Location;
using OrchardCore.SongServices.GraphQL.Services.Specification;
using OrchardCore.SongServices.GraphQL.Services.Staff;

namespace OrchardCore.SongServices.GraphQL;

// By convention the GraphQL specific services should be placed inside the GraphQL directory of their module. Don't
// forget the RequireFeatures attribute: the schema is built at startup in the singleton scope, so it would be wasteful
// to let it run when GraphQL is disabled. When the GraphQL feature is enabled you can go to Configuration > GraphiQL to
// inspect and play around with the queries without needing an external query editor.
[RequireFeatures("OrchardCore.Apis.GraphQL")]
public class Startup : StartupBase
{
    public override void ConfigureServices(IServiceCollection services)
    {
        // The first 3 lines here add the "person" field to any ContentItem type field that has a PersonPart. Implement
        // ObjectGraphType<TPart> to display a content part-specific field. This is required.
        services.AddObjectGraphType<PersonPart, PersonPartObjectGraphType>();
        // Optionally, if you have a content part index, implement WhereInputObjectGraphType<TPart> and
        // PartIndexAliasProvider<TPartIndex>. These will give you database-side filtering via the "where" argument.
        services.AddInputObjectGraphType<PersonPart, PersonPartWhereInputObjectGraphType>();
        services.AddTransient<IIndexAliasProvider, PersonPartIndexAliasProvider>();

        // Sometimes you want more advanced filter logic or filtering that can't be expressed with YesSql queries. In
        // this case you can add custom filter attributes by implementing IContentTypeBuilder and then evaluate their
        // values in a class that implements IGraphQLFilter<ContentItem>.
        services.AddScoped<IContentTypeBuilder, ContentItemTypeBuilder>();
        services.AddTransient<IGraphQLFilter<ContentItem>, PersonAgeGraphQLFilter>();

        // Bets Part
        services.AddObjectGraphType<BetsPart, BetsPartObjectGraphType>();
        services.AddInputObjectGraphType<BetsPart, BetsPartWhereInputObjectGraphType>();
        services.AddTransient<IIndexAliasProvider, BetsPartIndexAliasProvider>();
        services.AddScoped<IContentTypeBuilder, BetsPartTypeBuilder>();
        services.AddTransient<IGraphQLFilter<ContentItem>, BetsPartGraphQLFilter>();

        //// Cards Part
        services.AddObjectGraphType<CardsPart, CardsPartObjectGraphType>();
        services.AddInputObjectGraphType<CardsPart, CardsPartWhereInputObjectGraphType>();
        services.AddTransient<IIndexAliasProvider, CardsPartIndexAliasProvider>();
        services.AddScoped<IContentTypeBuilder, CardsPartTypeBuilder>();
        services.AddTransient<IGraphQLFilter<ContentItem>, CardsPartGraphQLFilter>();

        // Dealer Part
        services.AddObjectGraphType<DealerPart, DealerPartObjectGraphType>();
        services.AddInputObjectGraphType<DealerPart, DealerPartWhereInputObjectGraphType>();
        services.AddTransient<IIndexAliasProvider, DealerPartIndexAliasProvider>();
        services.AddScoped<IContentTypeBuilder, DealerPartTypeBuilder>();
        services.AddTransient<IGraphQLFilter<ContentItem>, DealerPartGraphQLFilter>();

        // History Part
        services.AddObjectGraphType<HistoryPart, HistoryPartObjectGraphType>();
        services.AddInputObjectGraphType<HistoryPart, HistoryPartWhereInputObjectGraphType>();
        services.AddTransient<IIndexAliasProvider, HistoryPartIndexAliasProvider>();
        services.AddScoped<IContentTypeBuilder, HistoryPartTypeBuilder>();
        services.AddTransient<IGraphQLFilter<ContentItem>, HistoryPartGraphQLFilter>();

        // JoinRoomSuccess Part
        services.AddObjectGraphType<JoinRoomSuccessPart, JoinRoomSuccessPartObjectGraphType>();
        services.AddInputObjectGraphType<JoinRoomSuccessPart, JoinRoomSuccessPartWhereInputObjectGraphType>();
        services.AddTransient<IIndexAliasProvider, JoinRoomSuccessPartIndexAliasProvider>();
        services.AddScoped<IContentTypeBuilder, JoinRoomSuccessPartTypeBuilder>();
        services.AddTransient<IGraphQLFilter<ContentItem>, JoinRoomSuccessPartGraphQLFilter>();

        // Pool Part
        services.AddObjectGraphType<PoolPart, PoolPartObjectGraphType>();
        services.AddInputObjectGraphType<PoolPart, PoolPartWhereInputObjectGraphType>();
        services.AddTransient<IIndexAliasProvider, PoolPartIndexAliasProvider>();
        services.AddScoped<IContentTypeBuilder, PoolPartTypeBuilder>();
        services.AddTransient<IGraphQLFilter<ContentItem>, PoolPartGraphQLFilter>();

        // RoomBetInfo Part
        services.AddObjectGraphType<RoomBetInfoPart, RoomBetInfoPartObjectGraphType>();
        services.AddInputObjectGraphType<RoomBetInfoPart, RoomBetInfoPartWhereInputObjectGraphType>();
        services.AddTransient<IIndexAliasProvider, RoomBetInfoPartIndexAliasProvider>();
        services.AddScoped<IContentTypeBuilder, RoomBetInfoPartTypeBuilder>();
        services.AddTransient<IGraphQLFilter<ContentItem>, RoomBetInfoPartGraphQLFilter>();

        // ShowLobby Part
        services.AddObjectGraphType<ShowLobbyPart, ShowLobbyPartObjectGraphType>();
        services.AddInputObjectGraphType<ShowLobbyPart, ShowLobbyPartWhereInputObjectGraphType>();
        services.AddTransient<IIndexAliasProvider, ShowLobbyPartIndexAliasProvider>();
        services.AddScoped<IContentTypeBuilder, ShowLobbyPartTypeBuilder>();
        services.AddTransient<IGraphQLFilter<ContentItem>, ShowLobbyPartGraphQLFilter>();

        // StartStopBetSucceed Part
        services.AddObjectGraphType<StartStopBetSucceedPart, StartStopBetSucceedPartObjectGraphType>();
        services.AddInputObjectGraphType<StartStopBetSucceedPart, StartStopBetSucceedPartWhereInputObjectGraphType>();
        services.AddTransient<IIndexAliasProvider, StartStopBetSucceedPartIndexAliasProvider>();
        services.AddScoped<IContentTypeBuilder, StartStopBetSucceedPartTypeBuilder>();
        services.AddTransient<IGraphQLFilter<ContentItem>, StartStopBetSucceedPartGraphQLFilter>();

        // Ticks Part
        services.AddObjectGraphType<TicksPart, TicksPartObjectGraphType>();
        services.AddInputObjectGraphType<TicksPart, TicksPartWhereInputObjectGraphType>();
        services.AddTransient<IIndexAliasProvider, TicksPartIndexAliasProvider>();
        services.AddScoped<IContentTypeBuilder, TicksPartTypeBuilder>();
        services.AddTransient<IGraphQLFilter<ContentItem>, TicksPartGraphQLFilter>();

        // User Part
        services.AddObjectGraphType<UserPart, UserPartObjectGraphType>();
        services.AddInputObjectGraphType<UserPart, UserPartWhereInputObjectGraphType>();
        services.AddTransient<IIndexAliasProvider, UserPartIndexAliasProvider>();
        services.AddScoped<IContentTypeBuilder, UserPartTypeBuilder>();
        services.AddTransient<IGraphQLFilter<ContentItem>, UserPartGraphQLFilter>();

        // Trader Part
        services.AddObjectGraphType<TraderForFilteringPart, TraderForFilteringPartObjectGraphType>();
        services.AddInputObjectGraphType<TraderForFilteringPart, TraderForFilteringPartWhereInputObjectGraphType>();
        services.AddTransient<IIndexAliasProvider, TraderForFilteringPartIndexAliasProvider>();
        services.AddScoped<IContentTypeBuilder, TraderForFilteringPartTypeBuilder>();
        services.AddTransient<IGraphQLFilter<ContentItem>, TraderForFilteringPartGraphQLFilter>();

        // Offer Part
        services.AddObjectGraphType<OfferFilteringPart, OfferFilteringPartObjectGraphType>();
        services.AddInputObjectGraphType<OfferFilteringPart, OfferFilteringPartWhereInputObjectGraphType>();
        services.AddTransient<IIndexAliasProvider, OfferFilteringPartIndexAliasProvider>();
        services.AddScoped<IContentTypeBuilder, OfferFilteringPartTypeBuilder>();
        services.AddTransient<IGraphQLFilter<ContentItem>, OfferFilteringPartGraphQLCurrencyFilter>();
        services.AddTransient<IGraphQLFilter<ContentItem>, OfferFilteringPartGraphQLMaxFilter>();
        services.AddTransient<IGraphQLFilter<ContentItem>, OfferFilteringPartGraphQLMethodFilter>();
        services.AddTransient<IGraphQLFilter<ContentItem>, OfferFilteringPartGraphQLMinFilter>();
        services.AddTransient<IGraphQLFilter<ContentItem>, OfferFilteringPartGraphQLOfferTypeFilter>();
        services.AddTransient<IGraphQLFilter<ContentItem>, OfferFilteringPartGraphQLOfferStatusFilter>();
        services.AddTransient<IGraphQLFilter<ContentItem>, OfferFilteringPartGraphQLPercentageFilter>();
        services.AddTransient<IGraphQLFilter<ContentItem>, OfferFilteringPartGraphQLWalletFilter>();

        // Trade Part
        services.AddObjectGraphType<TradeFilteringPart, TradeFilteringPartObjectGraphType>();
        services.AddInputObjectGraphType<TradeFilteringPart, TradeFilteringPartWhereInputObjectGraphType>();
        services.AddTransient<IIndexAliasProvider, TradeFilteringPartIndexAliasProvider>();
        services.AddScoped<IContentTypeBuilder, TradeFilteringPartTypeBuilder>();
        services.AddTransient<IGraphQLFilter<ContentItem>, TradeFilteringPartGraphQLBuyerContentIdFilter>();
        services.AddTransient<IGraphQLFilter<ContentItem>, TradeFilteringPartGraphQLBuyerFilter>();
        services.AddTransient<IGraphQLFilter<ContentItem>, TradeFilteringPartGraphQLMethodFilter>();
        services.AddTransient<IGraphQLFilter<ContentItem>, TradeFilteringPartGraphQLCurrencyFilter>();
        services.AddTransient<IGraphQLFilter<ContentItem>, TradeFilteringPartGraphQLDurationFilter>();
        services.AddTransient<IGraphQLFilter<ContentItem>, TradeFilteringPartGraphQLFeeBTCAmountFilter>();
        services.AddTransient<IGraphQLFilter<ContentItem>, TradeFilteringPartGraphQLFeeETHAmountFilter>();
        services.AddTransient<IGraphQLFilter<ContentItem>, TradeFilteringPartGraphQLFeeTypeFilter>();
        services.AddTransient<IGraphQLFilter<ContentItem>, TradeFilteringPartGraphQLFeeUSDT20AmountFilter>();
        services.AddTransient<IGraphQLFilter<ContentItem>, TradeFilteringPartGraphQLFeeVNDAmountFilter>();
        services.AddTransient<IGraphQLFilter<ContentItem>, TradeFilteringPartGraphQLOfferIdFilter>();
        services.AddTransient<IGraphQLFilter<ContentItem>, TradeFilteringPartGraphQLOfferTypeFilter>();
        services.AddTransient<IGraphQLFilter<ContentItem>, TradeFilteringPartGraphQLOfferWalletFilter>();
        services.AddTransient<IGraphQLFilter<ContentItem>, TradeFilteringPartGraphQLSellerContentIdFilter>();
        services.AddTransient<IGraphQLFilter<ContentItem>, TradeFilteringPartGraphQLSellerFilter>();
        services.AddTransient<IGraphQLFilter<ContentItem>, TradeFilteringPartGraphQLTradeBTCAmountFilter>();
        services.AddTransient<IGraphQLFilter<ContentItem>, TradeFilteringPartGraphQLTradeETHAmountFilter>();
        services.AddTransient<IGraphQLFilter<ContentItem>, TradeFilteringPartGraphQLTradeUSDT20AmountFilter>();
        services.AddTransient<IGraphQLFilter<ContentItem>, TradeFilteringPartGraphQLTradeVNDAmountFilter>();
        services.AddTransient<IGraphQLFilter<ContentItem>, TradeFilteringPartGraphQLTradeStatusFilter>();
        services.AddTransient<IGraphQLFilter<ContentItem>, TradeFilteringPartGraphQLTradeTypeFilter>();

        // Location
        services.AddObjectGraphType<LocationPart, LocationPartObjectGraphType>();
        services.AddInputObjectGraphType<LocationPart, LocationPartWhereInputObjectGraphType>();
        services.AddTransient<IIndexAliasProvider, LocationPartIndexAliasProvider>();
        services.AddScoped<IContentTypeBuilder, LocationPartTypeBuilder>();
        services.AddTransient<IGraphQLFilter<ContentItem>, LocationPartGraphQLCountryFilter>();
        services.AddTransient<IGraphQLFilter<ContentItem>, LocationPartGraphQLCityFilter>();
        services.AddTransient<IGraphQLFilter<ContentItem>, LocationPartGraphQLStreetFilter>();
        services.AddTransient<IGraphQLFilter<ContentItem>, LocationPartGraphQLSiteFilter>();
        services.AddTransient<IGraphQLFilter<ContentItem>, LocationPartGraphQLBuildingFilter>();
        services.AddTransient<IGraphQLFilter<ContentItem>, LocationPartGraphQLFloorFilter>();
        services.AddTransient<IGraphQLFilter<ContentItem>, LocationPartGraphQLZoneFilter>();
        services.AddTransient<IGraphQLFilter<ContentItem>, LocationPartGraphQLRoomFilter>();

        // Specification Part
        services.AddObjectGraphType<SpecificationPart, SpecificationPartObjectGraphType>();
        services.AddInputObjectGraphType<SpecificationPart, SpecificationPartWhereInputObjectGraphType>();
        services.AddTransient<IIndexAliasProvider, SpecificationPartIndexAliasProvider>();
        services.AddScoped<IContentTypeBuilder, SpecificationPartTypeBuilder>();
        services.AddTransient<IGraphQLFilter<ContentItem>, SpecificationPartGraphQLDescriptionFilter>();
        services.AddTransient<IGraphQLFilter<ContentItem>, SpecificationPartGraphQLAssignerContentItemIdFilter>();
        services.AddTransient<IGraphQLFilter<ContentItem>, SpecificationPartGraphQLAssigneeContentItemIdFilter>();
        services.AddTransient<IGraphQLFilter<ContentItem>, SpecificationPartGraphQLSupplementFilter>();
        services.AddTransient<IGraphQLFilter<ContentItem>, SpecificationPartGraphQLRootCauseFilter>();
        services.AddTransient<IGraphQLFilter<ContentItem>, SpecificationPartGraphQLIsPlannedFilter>();
        services.AddTransient<IGraphQLFilter<ContentItem>, SpecificationPartGraphQLIsIncidentFilter>();
        services.AddTransient<IGraphQLFilter<ContentItem>, SpecificationPartGraphQLIsInHouseFilter>();
        services.AddTransient<IGraphQLFilter<ContentItem>, SpecificationPartGraphQLIsOutsourcedFilter>();
        services.AddTransient<IGraphQLFilter<ContentItem>, SpecificationPartGraphQLReportStatusFilter>();
        services.AddTransient<IGraphQLFilter<ContentItem>, SpecificationPartGraphQLOfferContentItemIdFilter>();
        services.AddTransient<IGraphQLFilter<ContentItem>, SpecificationPartGraphQLBehaviorFilter>();
        services.AddTransient<IGraphQLFilter<ContentItem>, SpecificationPartGraphQLAssetFilter>();
        services.AddTransient<IGraphQLFilter<ContentItem>, SpecificationPartGraphQLOthersFilter>();
        services.AddTransient<IGraphQLFilter<ContentItem>, SpecificationPartGraphQLSenderFilter>();
        services.AddTransient<IGraphQLFilter<ContentItem>, SpecificationPartGraphQLWriterFilter>();
        services.AddTransient<IGraphQLFilter<ContentItem>, SpecificationPartGraphQLPhotosFilter>();
        services.AddTransient<IGraphQLFilter<ContentItem>, SpecificationPartGraphQLClipsFilter>();
        services.AddTransient<IGraphQLFilter<ContentItem>, SpecificationPartGraphQLAudioFilter>();
        services.AddTransient<IGraphQLFilter<ContentItem>, SpecificationPartGraphQLFilesFilter>();
        services.AddTransient<IGraphQLFilter<ContentItem>, SpecificationPartGraphQLLocationContentItemIdFilter>();

        // Staff Part
        services.AddObjectGraphType<StaffPart, StaffPartObjectGraphType>();
        services.AddInputObjectGraphType<StaffPart, StaffPartWhereInputObjectGraphType>();
        services.AddTransient<IIndexAliasProvider, StaffPartIndexAliasProvider>();
        services.AddScoped<IContentTypeBuilder, StaffPartTypeBuilder>();
        services.AddTransient<IGraphQLFilter<ContentItem>, StaffPartGraphQLNickNameFilter>();
        services.AddTransient<IGraphQLFilter<ContentItem>, StaffPartGraphQLAvatarIdFilter>();
        services.AddTransient<IGraphQLFilter<ContentItem>, StaffPartGraphQLOperatorFilter>();
        services.AddTransient<IGraphQLFilter<ContentItem>, StaffPartGraphQLTeamFilter>();
        services.AddTransient<IGraphQLFilter<ContentItem>, StaffPartGraphQLFullNameFilter>();
        services.AddTransient<IGraphQLFilter<ContentItem>, StaffPartGraphQLUserNameFilter>();
        services.AddTransient<IGraphQLFilter<ContentItem>, StaffPartGraphQLBookmarkedReportContentItemIdsFilter>();
        services.AddTransient<IGraphQLFilter<ContentItem>, StaffPartGraphQLBalanceFilter>();
        services.AddTransient<IGraphQLFilter<ContentItem>, StaffPartGraphQLCurrencyFilter>();
        services.AddTransient<IGraphQLFilter<ContentItem>, StaffPartGraphQLCustomNicknameFilter>();
        services.AddTransient<IGraphQLFilter<ContentItem>, StaffPartGraphQLStaffIdFilter>();
        services.AddTransient<IGraphQLFilter<ContentItem>, StaffPartGraphQLBirthDayFilter>();
    }
}
