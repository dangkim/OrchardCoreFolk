using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using OrchardCore.BackgroundTasks;
using OrchardCore.ContentManagement;
using OrchardCore.ContentManagement.Records;
using OrchardCore.SimService.ApiCommonFunctions;
using OrchardCore.SimService.ApiModels;
using OrchardCore.Environment.Cache;
using RestSharp;
using YesSql;
using IConfiguration = Microsoft.Extensions.Configuration.IConfiguration;

namespace OrchardCore.SimService.Services
{
    /// <summary>
    /// This background task will update product Ware House Four.
    /// </summary>
    [BackgroundTask(Schedule = "* 0 */3 * * *", Description = "Update product Ware House Four")]
    public class UpdateProductWareHouseFourTask : IBackgroundTask
    {
        private readonly ILogger _logger;
        private readonly IConfiguration _config;
        public UpdateProductWareHouseFourTask(ILogger<UpdateProductWareHouseFourTask> logger, IConfiguration config)
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
                // Get list from Usim

                var uSimToken = await ApiCommon.ReadCache(session, memoryCache, signal, _config, "USimToken");
                string url = string.Format("https://www.unitedsms.net/api_command.php?cmd=list_services&{0}", uSimToken);

                var client = new RestClient(url);
                var request = new RestRequest();

                var response = await client.ExecuteGetAsync(request);
                var resObject = JsonConvert.DeserializeObject<ProductsWareHouseFourRequestDto>(response.Content);

                var productWareHouseFourContent = await session
                .Query<ContentItem, ContentItemIndex>(index => index.ContentType == "WareHouseUSAProducts" && index.DisplayText == "Product1" && index.Published && index.Latest)
                .FirstOrDefaultAsync();

                if (productWareHouseFourContent != null && resObject.status.Equals("ok"))
                {
                    dynamic echangeProductObj = productWareHouseFourContent.Content;

                    echangeProductObj["WareHouseUSAProducts"]["Products"]["Text"] = response.Content;

                    await contentManager.UpdateAsync(productWareHouseFourContent);

                    var resultProductContent = await contentManager.ValidateAsync(productWareHouseFourContent);

                    if (resultProductContent.Succeeded)
                    {
                        await contentManager.PublishAsync(productWareHouseFourContent);
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
