using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using OrchardCore.ContentManagement.Handlers;
using OrchardCore.Environment.Cache;

namespace OrchardCore.SimService.Handlers;
public class ContentsHandler : ContentHandlerBase
{
    private readonly ITagCache _tagCache;
    private readonly IMemoryCache _memoryCache;
    private readonly ISignal _signal;
    private readonly IConfiguration _config;
    public ContentsHandler(ITagCache tagCache,
            IMemoryCache memoryCache,
            ISignal signal,
            IConfiguration config)
    {
        _tagCache = tagCache;
        _memoryCache = memoryCache;
        _signal = signal;
        _config = config;
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

        if (context.ContentItem.ContentType == "CSimToken")
        {
            await _signal.SignalTokenAsync(_config["CSimCacheSignalKey"]);
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

        if (context.ContentItem.ContentType == "CSimPercentage")
        {
            await _signal.SignalTokenAsync(_config["CSimPercentSignalKey"]);
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

        return _tagCache.RemoveTagAsync($"contentitemid:{context.ContentItem.ContentItemId}");
    }
}
