using System;
using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using Newtonsoft.Json;
using NSwag.Annotations;
using OrchardCore.ContentManagement;
using OrchardCore.ContentManagement.Records;
using OrchardCore.SimService.ApiCommonFunctions;
using OrchardCore.SimService.ApiModels;
using OrchardCore.SimService.RedocAttributeProcessors;
using OrchardCore.Environment.Cache;
using OrchardCore.Users;
using RestSharp;
using YesSql;
using IConfiguration = Microsoft.Extensions.Configuration.IConfiguration;
using OrchardCore.Data;

namespace OrchardCore.SimService.SimApi
{
    [Route("api/content/[action]")]
    [ApiController]
    [Authorize(AuthenticationSchemes = "Api"), IgnoreAntiforgeryToken, AllowAnonymous]
    [OpenApiTag("Products and prices", Description = "Get information of products of Ware House Four.")]
    public class ProductWareHouseFourProfileController : Controller
    {
        private readonly IReadOnlySession _session;
        private readonly UserManager<IUser> _userManager;
        private readonly IContentManager _contentManager;
        private readonly IAuthorizationService _authorizationService;
        private readonly IConfiguration _config;
        private readonly IMemoryCache _memoryCache;
        private readonly ISignal _signal;

        public ProductWareHouseFourProfileController(
            IReadOnlySession session,
            IMemoryCache memoryCache,
            ISignal signal,
            IContentManager contentManager,
            UserManager<IUser> userManager,
            IAuthorizationService authorizationService,
            Microsoft.Extensions.Configuration.IConfiguration config)
        {
            _session = session;
            _memoryCache = memoryCache;
            _signal = signal;
            _contentManager = contentManager;
            _userManager = userManager;
            _authorizationService = authorizationService;
            _config = config;
        }

        #region API Product Ware House Four

        #region All products
        /// <summary>
        /// Products Ware House Four request
        /// </summary>
        /// <remarks>
        /// 
        /// </remarks>
        [HttpGet]
        [ActionName("productsWareHouseFour")]
        [ProducesResponseType(typeof(ProductsWareHouseFourRequestDto), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(ProblemDetails), (int)HttpStatusCode.Unauthorized)]
        [ProducesResponseType(typeof(ProblemDetails), (int)HttpStatusCode.BadRequest)]
        [ProducesResponseType(typeof(ProblemDetails), (int)HttpStatusCode.InternalServerError)]
        [ReDocCodeSample("c#", "var client = new RestClient(\"https://openapi.simforrent.com/api/user/productsWareHouseFour\");" +
            "\nvar request = new RestRequest();" +
            "\nrequest.AddHeader(\"Accept\", \"application/json\");" +
            "\nvar response = await client.ExecuteGetAsync(request);")]
        [AllowAnonymous]
        public async Task<IActionResult> ProductsWareHouseFourRequestAsync()
        {
            var uSimToken = await ApiCommon.ReadCache(_session, _memoryCache, _signal, _config, "USimToken");
            var percentStringValue = await ApiCommon.ReadCache(_session, _memoryCache, _signal, _config, "USimPercentage");
            var percent = string.IsNullOrEmpty(percentStringValue) ? 50 : int.Parse(percentStringValue);

            var exchangeRateRUBContent = await _session
                    .Query<ContentItem, ContentItemIndex>(index => index.ContentType == "ExchangeRate" && index.DisplayText == "RUB" && index.Published && index.Latest)
                    .FirstOrDefaultAsync();

            string rubRateString = exchangeRateRUBContent.Content["ExchangeRate"]["RateToUsd"]["Text"];
            decimal rubRateDouble = Decimal.Parse(rubRateString);

            var productWareHouseFourContent = await _session
                .Query<ContentItem, ContentItemIndex>(index => index.ContentType == "WareHouseUSAProducts" && index.DisplayText == "Product1" && index.Published && index.Latest)
                .FirstOrDefaultAsync();

            string productString = productWareHouseFourContent.Content["WareHouseUSAProducts"]["Products"]["Text"];

            var resObject = JsonConvert.DeserializeObject<ProductsWareHouseFourRequestDto>(productString);

            if (!resObject.status.Equals("ok", StringComparison.Ordinal))
            {
                return BadRequest();
            }

            var productObjects = resObject.message;

            foreach (var item in productObjects)
            {
                decimal price = decimal.Parse(item.price);
                item.price = ((decimal)price + ((decimal)price * percent / 100)).ToString();

                decimal landline_price = decimal.Parse(item.landline_price);
                item.landline_price = ((decimal)landline_price + ((decimal)landline_price * percent / 100)).ToString();

                decimal ltr_price = decimal.Parse(item.ltr_price);
                item.ltr_price = ((decimal)ltr_price + ((decimal)ltr_price * percent / 100)).ToString();
            }

            return Ok(productObjects);
        }
        #endregion

        #region Products with service
        /// <summary>
        /// Products Ware House Four with service request
        /// </summary>
        /// <remarks>
        /// 
        /// </remarks>
        [HttpGet]
        [ActionName("productsWareHouseFourWithService")]
        [ProducesResponseType(typeof(ProductsWareHouseFourRequestDto), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(ProblemDetails), (int)HttpStatusCode.Unauthorized)]
        [ProducesResponseType(typeof(ProblemDetails), (int)HttpStatusCode.BadRequest)]
        [ProducesResponseType(typeof(ProblemDetails), (int)HttpStatusCode.InternalServerError)]
        [ReDocCodeSample("c#", "var client = new RestClient(\"https://openapi.simforrent.com/api/user/productsWareHouseFourWithService\");" +
            "\nvar request = new RestRequest();" +
            "\nrequest.AddHeader(\"Accept\", \"application/json\");" +
            "\nvar response = await client.ExecuteGetAsync(request);")]
        [AllowAnonymous]
        public async Task<IActionResult> ProductsWareHouseFourWithServiceRequestAsync(string service)
        {
            var uSimToken = await ApiCommon.ReadCache(_session, _memoryCache, _signal, _config, "USimToken");
            var percentStringValue = await ApiCommon.ReadCache(_session, _memoryCache, _signal, _config, "USimPercentage");
            var percent = string.IsNullOrEmpty(percentStringValue) ? 20 : int.Parse(percentStringValue);

            var exchangeRateRUBContent = await _session
                    .Query<ContentItem, ContentItemIndex>(index => index.ContentType == "ExchangeRate" && index.DisplayText == "RUB" && index.Published && index.Latest)
                    .FirstOrDefaultAsync();

            string rubRateString = exchangeRateRUBContent.Content["ExchangeRate"]["RateToUsd"]["Text"];
            decimal rubRateDouble = Decimal.Parse(rubRateString);

            var url = string.Format("https://www.unitedsms.net/api_command.php?cmd=list_services&{0}&service={1}", uSimToken, service);

            var client = new RestClient(url);
            var request = new RestRequest();

            var response = await client.ExecuteGetAsync(request);
            var resObject = JsonConvert.DeserializeObject<ProductsWareHouseFourRequestDto>(response.Content);

            if (!resObject.status.Equals("ok", StringComparison.Ordinal))
            {
                return BadRequest();
            }

            var productObjects = resObject.message;

            foreach (var item in productObjects)
            {
                decimal price = decimal.Parse(item.price);
                item.price = ((decimal)price + ((decimal)price * percent / 100)).ToString();

                decimal landline_price = decimal.Parse(item.landline_price);
                item.landline_price = ((decimal)landline_price + ((decimal)landline_price * percent / 100)).ToString();

                decimal ltr_price = decimal.Parse(item.ltr_price);
                item.ltr_price = ((decimal)ltr_price + ((decimal)ltr_price * percent / 100)).ToString();
            }

            return Ok(productObjects);
        }
        #endregion

        #endregion
    }
}
