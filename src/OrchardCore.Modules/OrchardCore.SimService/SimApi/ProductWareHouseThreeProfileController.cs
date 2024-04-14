using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
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
    [Route("api/content/[action]")]
    [ApiController]
    [Authorize(AuthenticationSchemes = "Api"), IgnoreAntiforgeryToken, AllowAnonymous]
    [OpenApiTag("Products and prices", Description = "Get information of products of Ware House Three.")]
    public class ProductWareHouseThreeProfileController : Controller
    {
        private readonly ISession _session;
        private readonly UserManager<IUser> _userManager;
        private readonly IContentManager _contentManager;
        private readonly IAuthorizationService _authorizationService;
        private readonly IConfiguration _config;
        private readonly IMemoryCache _memoryCache;
        private readonly ISignal _signal;

        public ProductWareHouseThreeProfileController(
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

        #region API Product Ware House Three

        /// <summary>
        /// Products Ware House Three request
        /// </summary>
        [HttpGet]
        [ActionName("productsWareHouseThree")]
        [ProducesResponseType(typeof(ProductsWareHouseThreeRequestDto), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(ProblemDetails), (int)HttpStatusCode.Unauthorized)]
        [ProducesResponseType(typeof(ProblemDetails), (int)HttpStatusCode.BadRequest)]
        [ProducesResponseType(typeof(ProblemDetails), (int)HttpStatusCode.InternalServerError)]
        [ReDocCodeSample("c#", "var client = new RestClient(\"https://openapi.simforrent.com/api/user/productsWareHouseThree\");" +
            "\nvar request = new RestRequest();" +
            "\nrequest.AddHeader(\"Accept\", \"application/json\");" +
            "\nvar response = await client.ExecuteGetAsync(request);")]
        [AllowAnonymous]
        public async Task<IActionResult> ProductsWareHouseThreeRequestAsync()
        {
            var cSimToken = await ApiCommon.ReadCache(_session, _memoryCache, _signal, _config, "CSimToken");
            var percentStringValue = await ApiCommon.ReadCache(_session, _memoryCache, _signal, _config, "CSimPercentage");
            var percent = string.IsNullOrEmpty(percentStringValue) ? 50 : int.Parse(percentStringValue);

            var exchangeRateRUBContent = await _session
                    .Query<ContentItem, ContentItemIndex>(index => index.ContentType == "ExchangeRate" && index.DisplayText == "RUB" && index.Published && index.Latest)
                    .FirstOrDefaultAsync();

            string rubRateString = exchangeRateRUBContent.Content["ExchangeRate"]["RateToUsd"]["Text"];
            decimal rubRateDouble = Decimal.Parse(rubRateString);

            var url = string.Format("https://chothuesimcode.com/api?act=app&apik={0}", cSimToken);

            var client = new RestClient(url);
            var request = new RestRequest();

            var response = await client.ExecuteGetAsync(request);
            var resObject = JsonConvert.DeserializeObject<ProductsWareHouseThreeRequestDto>(response.Content);

            if (resObject.ResponseCode != 0)
            {
                return BadRequest();
            }

            var productObjects = resObject.Result;

            foreach (var item in productObjects)
            {
                //item.Cost = (decimal)item.Cost + ((decimal)item.Cost * 50 / 100);
                item.Cost = Math.Round((((decimal)item.Cost + ((decimal)item.Cost * percent / 100)) / 24) / rubRateDouble, 2);
            }

            return Ok(productObjects);
        }

        #endregion

        private static void AddProperty(ExpandoObject expando, string propertyName, object propertyValue)
        {
            // ExpandoObject supports IDictionary so we can extend it like this
            var expandoDict = expando as IDictionary<string, object>;
            if (expandoDict.ContainsKey(propertyName))
                expandoDict[propertyName] = propertyValue;
            else
                expandoDict.Add(propertyName, propertyValue);
        }
    }

}
