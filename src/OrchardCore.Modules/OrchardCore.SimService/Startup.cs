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
using OrchardCore.SimService.Services;
using AspNetCoreRateLimit;
using Microsoft.Extensions.Configuration;

namespace OrchardCore.SimService
{
    public class Startup : StartupBase
    {
        private readonly IConfiguration _configuration;

        public Startup(IConfiguration configuration)
        {
            _configuration = configuration;
        }

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

            services.AddMemoryCache();
            services.Configure<IpRateLimitOptions>(_configuration.GetSection("IpRateLimiting"));
            services.AddInMemoryRateLimiting();
            services.AddSingleton<IRateLimitConfiguration, RateLimitConfiguration>();

        }

        public override void Configure(IApplicationBuilder builder, IEndpointRouteBuilder routes, IServiceProvider serviceProvider)
        {
            builder.UseCors("MyPolicy");
            builder.UseMiddleware<ApiKeyMiddleware>();

            routes.MapAreaControllerRoute(
                name: "Home",
                areaName: "OrchardCore.SimService",
                pattern: "Home/Index",
                defaults: new { controller = "Home", action = "Index" }
            );
        }
    }
}

