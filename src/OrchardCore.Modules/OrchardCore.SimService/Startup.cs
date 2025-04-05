using System;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using OrchardCore.BackgroundTasks;
using OrchardCore.ContentManagement;
using OrchardCore.ContentManagement.Display.ContentDisplay;
using OrchardCore.ContentManagement.Handlers;
using OrchardCore.Data.Migration;
using OrchardCore.Modules;
using OrchardCore.SimService.Drivers;
using OrchardCore.SimService.Handlers;
using OrchardCore.SimService.Indexes;
using OrchardCore.SimService.Migrations;
using OrchardCore.SimService.ContentParts;
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

            services.AddScoped<IContentHandler, SimServiceContentsHandler>();

            services.AddHttpClient("fsim", client =>
            {
                client.BaseAddress = new Uri("https://5sim.net/v1/");
            });

            services.AddHttpClient("smshub", client =>
            {
                client.BaseAddress = new Uri("https://smshub.org/stubs/");
            });
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

