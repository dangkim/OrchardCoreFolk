using System;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using OrchardCore.BackgroundTasks;
using OrchardCore.ContentManagement;
using OrchardCore.ContentManagement.Display.ContentDisplay;
using OrchardCore.Data.Migration;
using OrchardCore.Modules;
using OrchardCore.SimService.Drivers;
using OrchardCore.SimService.Handlers;
using OrchardCore.SimService.Indexes;
using OrchardCore.SimService.Migrations;
using OrchardCore.SimService.Models;
using OrchardCore.SimService.Services;
using YesSql.Indexes;

namespace OrchardCore.SimService
{
    public class Startup : StartupBase
    {
        public override void ConfigureServices(IServiceCollection services)
        {
            //services.AddCors(options =>
            //{
            //    options.AddDefaultPolicy(builder =>
            //        builder.WithOrigins("http://localhost:3000", "https://localhost:3000")
            //               .AllowAnyMethod()
            //               .AllowAnyHeader());
            //});

            services.AddScoped<IDataMigration, SimMigration>();

            // UserProfilePart
            services
            .AddContentPart<UserProfilePart>()
                    .UseDisplayDriver<UserProfilePartDisplayDriver>()
                    .AddHandler<UserProfilePartHandler>();

            services.AddSingleton<IIndexProvider, UserProfilePartIndexProvider>();

            // PaymentDetailPart
            services
                .AddContentPart<PaymentDetailPart>()
                .UseDisplayDriver<PaymentDetailPartDisplayDriver>()
                .AddHandler<PaymentDetailPartHandler>();

            services.AddSingleton<IIndexProvider, PaymentDetailPartIndexProvider>();

            // OrderDetailPart
            services
                .AddContentPart<OrderDetailPart>()
                .UseDisplayDriver<OrderDetailPartDisplayDriver>()
                .AddHandler<OrderDetailPartHandler>();

            services.AddSingleton<IIndexProvider, OrderDetailPartIndexProvider>();

            // SmsPart
            services
                .AddContentPart<SmsPart>()
                .UseDisplayDriver<SmsPartDisplayDriver>()
                .AddHandler<SmsPartHandler>();

            services.AddSingleton<IIndexProvider, SmsPartIndexProvider>();

            services.AddSingleton<IBackgroundTask, EmailReadingTask>();

            services.AddSingleton<IBackgroundTask, UpdateRateUsdByPMTask>();

            services.AddSingleton<IBackgroundTask, UpdateProductWareHouseFourTask>();
        }

        public override void Configure(IApplicationBuilder builder, IEndpointRouteBuilder routes, IServiceProvider serviceProvider)
        {
            builder.UseCors("MyPolicy");

            routes.MapAreaControllerRoute(
                name: "Home",
                areaName: "OrchardCore.SimService",
                pattern: "Home/Index",
                defaults: new { controller = "Home", action = "Index" }
            );
        }
    }
}

