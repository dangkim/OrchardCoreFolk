using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using NSwag.Annotations;
using OrchardCore.ContentManagement;
using OrchardCore.SimService.ApiModels;
using OrchardCore.SimService.RedocAttributeProcessors;
using RestSharp;
using Newtonsoft.Json;
using System.Collections;
using System.Linq;
using System.Dynamic;
using Newtonsoft.Json.Converters;
using Microsoft.Extensions.Caching.Memory;
using OrchardCore.Environment.Cache;
using YesSql;
using IConfiguration = Microsoft.Extensions.Configuration.IConfiguration;
using OrchardCore.SimService.ApiCommonFunctions;
using Microsoft.AspNetCore.Identity;
using OrchardCore.Users;
using OrchardCore.ContentManagement.Records;
using OrchardCore.Data;

namespace OrchardCore.SimService.SimApi
{
    [Route("api/content/[action]/{countryId}/{operatorId}")]
    [ApiController]
    [Authorize(AuthenticationSchemes = "Api"), IgnoreAntiforgeryToken, AllowAnonymous]
    [OpenApiTag("Products and prices", Description = "Get information of products.")]
    public class ProductWareHouseTwoProfileController : Controller
    {
        private readonly IReadOnlySession _session;
        private readonly UserManager<IUser> _userManager;
        private readonly IContentManager _contentManager;
        private readonly IAuthorizationService _authorizationService;
        private readonly IConfiguration _config;
        private readonly IMemoryCache _memoryCache;
        private readonly ISignal _signal;

        public ProductWareHouseTwoProfileController(
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

        #region API Product Ware House Two

        #region Products Ware House Two request
        /// <summary>
        /// Products Ware House Two request
        /// </summary>
        /// <remarks>
        /// 
        /// </remarks>
        [HttpGet]
        [ActionName("productsWareHouseTwo")]
        [ProducesResponseType(typeof(ProductsWareHouseTwoRequestDto), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(ProblemDetails), (int)HttpStatusCode.Unauthorized)]
        [ProducesResponseType(typeof(ProblemDetails), (int)HttpStatusCode.BadRequest)]
        [ProducesResponseType(typeof(ProblemDetails), (int)HttpStatusCode.InternalServerError)]
        [ReDocCodeSample("c#", "var client = new RestClient(\"https://openapi.simforrent.com/api/user/productsWareHouseTwo\");" +
            "\nvar request = new RestRequest();" +
            "\nrequest.AddHeader(\"Accept\", \"application/json\");" +
            "\nvar response = await client.ExecuteGetAsync(request);")]
        [AllowAnonymous]
        public async Task<IActionResult> ProductsWareHouseTwoRequestAsync(int countryId, string operatorId)
        {
            var percentStringValue = await ApiCommon.ReadCache(_session, _memoryCache, _signal, _config, "LSimPercentage");
            var percent = string.IsNullOrEmpty(percentStringValue) ? 20 : int.Parse(percentStringValue);

            var exchangeRateRUBContent = await _session
                    .Query<ContentItem, ContentItemIndex>(index => index.ContentType == "ExchangeRate" && index.DisplayText == "RUB" && index.Published && index.Latest)
                    .FirstOrDefaultAsync();

            string rubRateString = exchangeRateRUBContent.Content["ExchangeRate"]["RateToUsd"]["Text"];
            decimal rubRateDouble = Decimal.Parse(rubRateString);

            string url = string.Format("https://2ndline.io/apiv1/availableservice?countryId={0}&operatorId={1}", countryId, operatorId);

            var client = new RestClient(url);
            var request = new RestRequest();

            var response = await client.ExecuteGetAsync(request);

            var resObject = JsonConvert.DeserializeObject<List<ProductsWareHouseTwoRequestDto>>(response.Content, new ExpandoObjectConverter());
            resObject = resObject.OrderBy(x => x.name).ToList();
            foreach (var item in resObject)
            {
                var price = Math.Round(((decimal)item.price + ((decimal)item.price * percent / 100)) / rubRateDouble, 2);
                item.price = price;
            }

            return Ok(resObject);
        }

        #endregion

        #region Products Viet Nam Ware House Two request
        /// <summary>
        /// Products Viet Nam Ware House Two request
        /// </summary>
        /// <remarks>
        /// 
        /// </remarks>
        [HttpGet]
        [ActionName("productsVietNamWareHouseTwo")]
        [ProducesResponseType(typeof(ProductsVietNamWareHouseTwoRequestDto), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(ProblemDetails), (int)HttpStatusCode.Unauthorized)]
        [ProducesResponseType(typeof(ProblemDetails), (int)HttpStatusCode.BadRequest)]
        [ProducesResponseType(typeof(ProblemDetails), (int)HttpStatusCode.InternalServerError)]
        [ReDocCodeSample("c#", "var client = new RestClient(\"https://openapi.simforrent.com/api/user/productsVietNamWareHouseTwo\");" +
            "\nvar request = new RestRequest();" +
            "\nrequest.AddHeader(\"Accept\", \"application/json\");" +
            "\nvar response = await client.ExecuteGetAsync(request);")]
        [AllowAnonymous]
        public async Task<IActionResult> ProductsVietNamWareHouseTwoRequestAsync(int countryId, string operatorId)
        {
            var percentStringValue = await ApiCommon.ReadCache(_session, _memoryCache, _signal, _config, "LSimPercentage");
            var percent = string.IsNullOrEmpty(percentStringValue) ? 20 : int.Parse(percentStringValue);

            var exchangeRateRUBContent = await _session
                    .Query<ContentItem, ContentItemIndex>(index => index.ContentType == "ExchangeRate" && index.DisplayText == "RUB" && index.Published && index.Latest)
                    .FirstOrDefaultAsync();

            string rubRateString = exchangeRateRUBContent.Content["ExchangeRate"]["RateToUsd"]["Text"];
            decimal rubRateDouble = Decimal.Parse(rubRateString);

            string url = string.Format("https://2ndline.io/apiv1/availableservice?country={0}&id={1}", countryId, operatorId);

            var client = new RestClient(url);
            var request = new RestRequest();

            var response = await client.ExecuteGetAsync(request);

            var resObject = JsonConvert.DeserializeObject<List<ProductsVietNamWareHouseTwoRequestDto>>(response.Content, new ExpandoObjectConverter());
            resObject = resObject.OrderBy(x => x.name).ToList();
            foreach (var item in resObject)
            {
                //var priceRub = Math.Round(((decimal)item.price ) / rubRateDouble, 2);
                //var price = Math.Round(((decimal)priceRub + ((decimal)priceRub * percent / 100)),2);

                //item.price = price;

                var price = Math.Round(((decimal)item.price + ((decimal)item.price * percent / 100)) / rubRateDouble, 2);
                item.price = price;
            }

            return Ok(resObject);

        }

        #endregion

        #endregion
    }
}

