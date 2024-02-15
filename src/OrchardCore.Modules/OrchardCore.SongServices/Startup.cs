using System;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using OrchardCore.Apis;
using OrchardCore.BackgroundTasks;
using OrchardCore.ContentManagement;
using OrchardCore.ContentManagement.Display.ContentDisplay;
using OrchardCore.Data.Migration;
using OrchardCore.Entities;
using OrchardCore.Modules;
using OrchardCore.Security.Permissions;
using OrchardCore.SongServices.ContentParts;
using OrchardCore.SongServices.Drivers;
using OrchardCore.SongServices.Handlers;
using OrchardCore.SongServices.Indexes;
using OrchardCore.SongServices.Indexing;
using OrchardCore.SongServices.Migrations;
using OrchardCore.SongServices.Models;
using OrchardCore.SongServices.Permissions;
using OrchardCore.SongServices.Services;
using YesSql.Indexes;

namespace OrchardCore.SongServices
{
    public class Startup : StartupBase
    {
        public override void ConfigureServices(IServiceCollection services)
        {
            services
                .AddContentPart<PersonPart>()
                .UseDisplayDriver<PersonPartDisplayDriver>()
                .AddHandler<PersonPartHandler>();
            services.AddScoped<IDataMigration, PersonMigrations>();
            services.AddSingleton<IIndexProvider, PersonPartIndexProvider>();

            // Bets //
            services
                .AddContentPart<BetsPart>()
                .UseDisplayDriver<BetsPartDisplayDriver>()
                .AddHandler<BetsPartHandler>();
            services.AddSingleton<IIndexProvider, BetsPartIndexProvider>();

            // Cards //
            services
                .AddContentPart<CardsPart>()
                .UseDisplayDriver<CardsPartDisplayDriver>()
                .AddHandler<CardsPartHandler>();
            services.AddSingleton<IIndexProvider, CardsPartIndexProvider>();

            // Dealer //
            services
                .AddContentPart<DealerPart>()
                .UseDisplayDriver<DealerPartDisplayDriver>()
                .AddHandler<DealerPartHandler>();
            services.AddSingleton<IIndexProvider, DealerPartIndexProvider>();

            // History //
            services
                .AddContentPart<HistoryPart>()
                .UseDisplayDriver<HistoryPartDisplayDriver>()
                .AddHandler<HistoryPartHandler>();
            services.AddSingleton<IIndexProvider, HistoryPartIndexProvider>();

            // JoinRoomSuccess //
            services
                .AddContentPart<JoinRoomSuccessPart>()
                .UseDisplayDriver<JoinRoomSuccessPartDisplayDriver>()
                .AddHandler<JoinRoomSuccessPartHandler>();
            services.AddSingleton<IIndexProvider, JoinRoomSuccessPartIndexProvider>();

            // Pool //
            services
                .AddContentPart<PoolPart>()
                .UseDisplayDriver<PoolPartDisplayDriver>()
                .AddHandler<PoolPartHandler>();
            services.AddSingleton<IIndexProvider, PoolPartIndexProvider>();

            // RoomBetInfo //
            services
                .AddContentPart<RoomBetInfoPart>()
                .UseDisplayDriver<RoomBetInfoPartDisplayDriver>()
                .AddHandler<RoomBetInfoPartHandler>();
            services.AddSingleton<IIndexProvider, RoomBetInfoPartIndexProvider>();

            // ShowLobby //
            services
                .AddContentPart<ShowLobbyPart>()
                .UseDisplayDriver<ShowLobbyPartDisplayDriver>()
                .AddHandler<ShowLobbyPartHandler>();
            services.AddSingleton<IIndexProvider, ShowLobbyPartIndexProvider>();

            // StartStopBetSucceed //
            services
                .AddContentPart<StartStopBetSucceedPart>()
                .UseDisplayDriver<StartStopBetSucceedPartDisplayDriver>()
                .AddHandler<StartStopBetSucceedPartHandler>();
            services.AddSingleton<IIndexProvider, StartStopBetSucceedPartIndexProvider>();

            // Ticks //
            services
                .AddContentPart<TicksPart>()
                .UseDisplayDriver<TicksPartDisplayDriver>()
                .AddHandler<TicksPartHandler>();
            services.AddSingleton<IIndexProvider, TicksPartIndexProvider>();

            // Trader //
            services
                .AddContentPart<TraderForFilteringPart>()
                .UseDisplayDriver<TraderPartDisplayDriver>()
                .AddHandler<TraderPartHandler>();
            services.AddSingleton<IIndexProvider, TraderForFilteringPartIndexProvider>();

            // Trade //
            services
                .AddContentPart<TradeFilteringPart>()
                .UseDisplayDriver<TradePartDisplayDriver>()
                .AddHandler<TradePartHandler>();
            services.AddSingleton<IIndexProvider, TradeFilteringPartIndexProvider>();

            // Trade //
            services
                .AddContentPart<OfferFilteringPart>()
                .UseDisplayDriver<OfferPartDisplayDriver>()
                .AddHandler<OfferPartHandler>();
            services.AddSingleton<IIndexProvider, OfferFilteringPartIndexProvider>();

            // User //
            services
                .AddContentPart<UserPart>()
                .UseDisplayDriver<UserPartDisplayDriver>()
                .AddHandler<UserPartHandler>();
            services.AddSingleton<IIndexProvider, UserPartIndexProvider>();

            // Location //
            services
                .AddContentPart<LocationPart>()
                .UseDisplayDriver<LocationPartDisplayDriver>()
                .AddHandler<LocationPartHandler>();
            services.AddSingleton<IIndexProvider, LocationPartIndexProvider>();

            // Specification
            services
                .AddContentPart<SpecificationPart>()
                .UseDisplayDriver<SpecificationPartDisplayDriver>()
                .AddHandler<SpecificationPartHandler>();
            services.AddSingleton<IIndexProvider, SpecificationPartIndexProvider>();

            // Staff
            services
                .AddContentPart<StaffPart>()
                .UseDisplayDriver<StaffPartDisplayDriver>()
                .AddHandler<StaffPartHandler>();
            services.AddSingleton<IIndexProvider, StaffPartIndexProvider>();


            //services.AddScoped<IPermissionProvider, AccessApiPermissions>();

            //services.AddScoped<IRabbitMQProducer, RabbitMQProducer>();
            //services.AddScoped<ITeleUpdateService, TeleUpdateService>();
            //services.AddScoped<IBotService, BotService>();
            services.TryAddSingleton<IRabbitMQProducer, RabbitMQProducer>();

            services.AddSingleton<IBackgroundTask, LobbyBackGroundTask>();

        }

        public override void Configure(IApplicationBuilder builder, IEndpointRouteBuilder routes, IServiceProvider serviceProvider)
        {
            routes.MapAreaControllerRoute(
                name: "Home",
                areaName: "OrchardCore.SongServices",
                pattern: "Home/Index",
                defaults: new { controller = "Home", action = "Index" }
            );
        }
    }
}
