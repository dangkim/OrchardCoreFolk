using System;
using Microsoft.Extensions.DependencyInjection;
using OrchardCore.BackgroundTasks;
using System.Threading;
using System.Threading.Tasks;
using OrchardCore.ContentManagement;
using YesSql;
using OrchardCore.ContentManagement.Records;
using Microsoft.Extensions.Logging;
using OrchardCore.SimService.ApiCommonFunctions;
using Microsoft.Extensions.Caching.Memory;
using OrchardCore.Environment.Cache;
using IConfiguration = Microsoft.Extensions.Configuration.IConfiguration;
using RestSharp;
using Newtonsoft.Json;
using OrchardCore.SimService.ApiModels;

namespace OrchardCore.SimService.Services
{
    /// <summary>
    /// This background task will read email.
    /// </summary>
    [BackgroundTask(Schedule = "0 */2 * * *", Description = "update rate usd by perfect money rate")]
    public class UpdateRateUsdByPMTask : IBackgroundTask
    {
        private readonly ILogger _logger;
        private readonly IConfiguration _config;
        public UpdateRateUsdByPMTask(ILogger<UpdateRateUsdByPMTask> logger, IConfiguration config)
        {
            _logger = logger;
            _config = config;
        }

        public async Task DoWorkAsync(IServiceProvider serviceProvider, CancellationToken cancellationToken)
        {
            var session = serviceProvider.GetService<ISession>();
            var memoryCache = serviceProvider.GetService<IMemoryCache>();
            var signal = serviceProvider.GetService<ISignal>();
            var contentManager = serviceProvider.GetRequiredService<IContentManager>();

            try
            {
                // Get rate from 5sim
                var fiveSimToken = await ApiCommon.ReadCache(session, memoryCache, signal, _config);

                string url = string.Format("https://5sim.net/v1/user/payment/settings");

                var client = new RestClient(url);
                var request = new RestRequest();
                request.AddHeader("Authorization", "Bearer " + fiveSimToken);

                var response = await client.ExecuteGetAsync(request);

                var resObject = JsonConvert.DeserializeObject<RateFiveSim>(response.Content);

                var exchangeRateRUBContent = await session
                .Query<ContentItem, ContentItemIndex>(index => index.ContentType == "ExchangeRate" && index.DisplayText == "RUB" && index.Published && index.Latest)
                .FirstOrDefaultAsync();

                if (exchangeRateRUBContent != null)
                {
                    dynamic echangeRateObj = exchangeRateRUBContent.Content;

                    echangeRateObj["ExchangeRate"]["RateToUsd"]["Text"] = 1 / resObject.Perfect_money_usd_rate;

                    await contentManager.UpdateAsync(exchangeRateRUBContent);

                    var resultRateContent = await contentManager.ValidateAsync(exchangeRateRUBContent);

                    if (resultRateContent.Succeeded)
                    {
                        await contentManager.PublishAsync(exchangeRateRUBContent);
                    }
                }

            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
            }

            return;
        }
    }
}
