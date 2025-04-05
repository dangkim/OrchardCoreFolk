using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using OrchardCore.ContentManagement;
using OrchardCore.ContentManagement.Handlers;
using OrchardCore.Environment.Cache;
using OrchardCore.SimService.ApiModels;
using OrchardCore.SimService.ContentParts;

namespace OrchardCore.SimService.Handlers;
public class SimServiceContentsHandler : ContentHandlerBase
{
    private readonly ITagCache _tagCache;
    private readonly IMemoryCache _memoryCache;
    private readonly ISignal _signal;
    private readonly IConfiguration _config;
    public SimServiceContentsHandler(ITagCache tagCache,
            IMemoryCache memoryCache,
            ISignal signal,
            IConfiguration config)
    {
        _tagCache = tagCache;
        _memoryCache = memoryCache;
        _signal = signal;
        _config = config;
    }

    public override async Task<Task> UpdatedAsync(UpdateContentContext context)
    {
        if (context.ContentItem.ContentType == "ExchangeRate")
        {
            await _signal.SignalTokenAsync(_config["ExchangeRateSignalUSDKey"]);
            await _signal.SignalTokenAsync(_config["ExchangeRateSignalVNDKey"]);
            await _signal.SignalTokenAsync(_config["ExchangeRateSignalCNYKey"]);
        }
        else if (context.ContentItem.ContentType == "TronAddress")
        {
            await _signal.SignalTokenAsync(_config["TronAddressesKeySignalKey"]);
        }

        return _tagCache.RemoveTagAsync($"contentitemid:{context.ContentItem.ContentItemId}");
    }

    public override async Task<Task> PublishedAsync(PublishContentContext context)
    {
        if (context.ContentItem.ContentType == "FiveSimToken")
        {
            await _signal.SignalTokenAsync(_config["FiveSimCacheSignalKey"]);
        }

        if (context.ContentItem.ContentType == "TwoLineSimToken")
        {
            await _signal.SignalTokenAsync(_config["TwoLineSimCacheSignalKey"]);
        }

        if (context.ContentItem.ContentType == "SmsHubCountries")
        {
            await _signal.SignalTokenAsync(_config["SmsHubCountryCacheSignalKey"]);
        }

        if (context.ContentItem.ContentType == "SmsHubProducts")
        {
            await _signal.SignalTokenAsync(_config["SmsHubProductCacheKey"]);
        }

        if (context.ContentItem.ContentType == "USimToken")
        {
            await _signal.SignalTokenAsync(_config["USimCacheSignalKey"]);
        }

        if (context.ContentItem.ContentType == "VSimToken")
        {
            await _signal.SignalTokenAsync(_config["VSimCacheSignalKey"]);
        }

        if (context.ContentItem.ContentType == "BtcpayToken")
        {
            await _signal.SignalTokenAsync(_config["BtcpaySignalCacheKey"]);
        }

        if (context.ContentItem.ContentType == "Percentage")
        {
            await _signal.SignalTokenAsync(_config["PercentSignalKey"]);
        }

        if (context.ContentItem.ContentType == "LSimPercentage")
        {
            await _signal.SignalTokenAsync(_config["LSimPercentSignalKey"]);
        }

        if (context.ContentItem.ContentType == "USimPercentage")
        {
            await _signal.SignalTokenAsync(_config["USimPercentSignalKey"]);
        }

        if (context.ContentItem.ContentType == "VSimPercentage")
        {
            await _signal.SignalTokenAsync(_config["VSimPercentSignalKey"]);
        }

        if (context.ContentItem.ContentType == "BtcpayStoreKey")
        {
            await _signal.SignalTokenAsync(_config["BtcpayStoreIdSignalKey"]);
        }

        if (context.ContentItem.ContentType == "TronAddress")
        {
            await _signal.SignalTokenAsync(_config["TronAddressesKeySignalKey"]);
        }

        if (context.ContentItem.ContentType == "SmsHubToken")
        {
            await _signal.SignalTokenAsync(_config["SmsHubCacheSignalKey"]);
        }

        return _tagCache.RemoveTagAsync($"contentitemid:{context.ContentItem.ContentItemId}");
    }

    public override async Task<Task> RemovedAsync(RemoveContentContext context)
    {
        if (context.ContentItem.ContentType == "BlockedUsers")
        {
            var blockedUser = context.ContentItem.DisplayText;

            if (blockedUser != null)
            {
                await _signal.SignalTokenAsync(blockedUser);
            }
        }

        return _tagCache.RemoveTagAsync($"contentitemid:{context.ContentItem.ContentItemId}");
    }

    public override async Task<Task> UnpublishedAsync(PublishContentContext context)
    {
        if (context.ContentItem.ContentType == "BlockedUsers")
        {
            var blockedUser = context.ContentItem.DisplayText;

            if (blockedUser != null)
            {
                await _signal.SignalTokenAsync(blockedUser);
            }
        }

        return _tagCache.RemoveTagAsync($"contentitemid:{context.ContentItem.ContentItemId}");
    }
}
