using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace OrchardCore.SimService.Services;
public class ApiKeyMiddleware
{
    private readonly RequestDelegate _next;
    private const string ApiKeyHeaderName = "Authorization";
    private readonly ILogger<ApiKeyMiddleware> _logger;
    private readonly IConfiguration _configuration;

    public ApiKeyMiddleware(RequestDelegate next, ILogger<ApiKeyMiddleware> logger, IConfiguration configuration)
    {
        _next = next;
        _logger = logger;
        _configuration = configuration;
    }

    public async Task Invoke(HttpContext context)
    {
        //if (!context.Request.Headers.TryGetValue(ApiKeyHeaderName, out var extractedApiKey))
        //{
        //    LogFailure(context, "Missing API Key");
        //    context.Response.StatusCode = 401;
        //    await context.Response.WriteAsync("API Key is missing");
        //    return;
        //}

        //var configuredKey = _configuration["thuesimao_wallet_key"];
        //if (!string.Equals(extractedApiKey.ToString(), configuredKey))
        //{
        //    LogFailure(context, "Invalid API Key");
        //    context.Response.StatusCode = 401;
        //    await context.Response.WriteAsync("Unauthorized");
        //    return;
        //}

        //await _next(context);
        // Only check API key for paths starting with /wallet-api or whatever path you want
        if (context.Request.Path.StartsWithSegments("/api/content/MigrateToPostgres", StringComparison.OrdinalIgnoreCase))
        {
            if (!context.Request.Headers.TryGetValue(ApiKeyHeaderName, out var extractedApiKey))
            {
                LogFailure(context, "Missing API Key");
                context.Response.StatusCode = 401;
                await context.Response.WriteAsync("API Key is missing");
                return;
            }

            var configuredKey = _configuration["thuesimao_wallet_key"];
            if (!string.Equals(extractedApiKey.ToString(), configuredKey))
            {
                LogFailure(context, "Invalid API Key");
                context.Response.StatusCode = 401;
                await context.Response.WriteAsync("Unauthorized");
                return;
            }
        }
    }

    private void LogFailure(HttpContext context, string reason)
    {
        var ip = context.Connection.RemoteIpAddress?.ToString();
        _logger.LogWarning("API Auth Failure: {Reason} | IP: {IP}", reason, ip);
    }
}

