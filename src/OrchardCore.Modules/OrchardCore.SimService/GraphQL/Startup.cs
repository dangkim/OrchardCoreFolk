using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using System;
using Microsoft.Extensions.DependencyInjection;
using OrchardCore.ContentManagement.GraphQL;
using OrchardCore.Modules;

namespace OrchardCore.SimService.GraphQL
{
    [RequireFeatures("OrchardCore.Apis.GraphQL")]
    public class Startup : StartupBase
    {
        public override void Configure(IApplicationBuilder builder, IEndpointRouteBuilder routes, IServiceProvider serviceProvider)
        {
            builder.UseCors("MyPolicy");
        }
    }
}
