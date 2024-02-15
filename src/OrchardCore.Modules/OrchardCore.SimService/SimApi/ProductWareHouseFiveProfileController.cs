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

namespace OrchardCore.SimService.SimApi
{
    [Route("api/content/[action]/{country}")]
    [ApiController]
    [Authorize(AuthenticationSchemes = "Api"), IgnoreAntiforgeryToken, AllowAnonymous]
    [OpenApiTag("Products and prices", Description = "Get information of products of Ware House Five.")]
    public class ProductWareHouseFiveProfileController : Controller
    {
        private readonly ISession _session;
        private readonly UserManager<IUser> _userManager;
        private readonly IContentManager _contentManager;
        private readonly IAuthorizationService _authorizationService;
        private readonly IConfiguration _config;
        private readonly IMemoryCache _memoryCache;
        private readonly ISignal _signal;

        public ProductWareHouseFiveProfileController(
            ISession session,
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

        #region API Product Ware House Five

        #region All products
        /// <summary>
        /// Products Ware House Five request
        /// </summary>
        /// <remarks>
        /// 
        /// </remarks>
        [HttpGet]
        [ActionName("productsWareHouseFive")]
        [ProducesResponseType(typeof(ProductsWareHouseFiveRequestDto), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(ProblemDetails), (int)HttpStatusCode.Unauthorized)]
        [ProducesResponseType(typeof(ProblemDetails), (int)HttpStatusCode.BadRequest)]
        [ProducesResponseType(typeof(ProblemDetails), (int)HttpStatusCode.InternalServerError)]
        [ReDocCodeSample("c#", "var client = new RestClient(\"https://openapi.simforrent.com/api/user/productsWareHouseFive\");" +
            "\nvar request = new RestRequest();" +
            "\nrequest.AddHeader(\"Accept\", \"application/json\");" +
            "\nvar response = await client.ExecuteGetAsync(request);")]
        [AllowAnonymous]
        public async Task<IActionResult> ProductsWareHouseFiveRequestAsync(string country)
        {
            var vSimToken = await ApiCommon.ReadCache(_session, _memoryCache, _signal, _config, "VSimToken");
            var percentStringValue = await ApiCommon.ReadCache(_session, _memoryCache, _signal, _config, "VSimPercentage");
            var percent = string.IsNullOrEmpty(percentStringValue) ? 50 : int.Parse(percentStringValue);

            var exchangeRateRUBContent = await _session
                    .Query<ContentItem, ContentItemIndex>(index => index.ContentType == "ExchangeRate" && index.DisplayText == "RUB" && index.Published && index.Latest)
                    .FirstOrDefaultAsync();

            string rubRateString = exchangeRateRUBContent.Content["ExchangeRate"]["RateToUsd"]["Text"];
            decimal rubRateDouble = Decimal.Parse(rubRateString);

            var url = string.Format("https://api.viotp.com/service/getv2?token={0}&country={1}", vSimToken, country);

            var client = new RestClient(url);
            var request = new RestRequest();

            var response = await client.ExecuteGetAsync(request);
            var resObject = JsonConvert.DeserializeObject<ProductsWareHouseFiveRequestDto>(response.Content);

            if (resObject.status_code != 200)
            {
                return BadRequest();
            }

            var productObjects = resObject.data;

            foreach (var item in productObjects)
            {
                item.price = Math.Round((((decimal)item.price + ((decimal)item.price * percent / 100)) / 24000) / rubRateDouble, 2);
            }

            return Ok(productObjects);
        }
        #endregion
        #endregion
    }
}
