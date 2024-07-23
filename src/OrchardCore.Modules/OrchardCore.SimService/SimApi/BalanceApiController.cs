using System;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using Newtonsoft.Json.Linq;
using OrchardCore.ContentManagement;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using OrchardCore.Email;
using OrchardCore.Users;
using OrchardCore.Users.Models;
using Microsoft.AspNetCore.Cors;
using Newtonsoft.Json;
using YesSql;
using OrchardCore.Users.Services;
using System.Collections.Generic;
using Microsoft.Net.Http.Headers;
using OrchardCore.SimService.ApiModels;
using OrchardCore.SimService.ApiCommonFunctions;
using OrchardCore.SimService.Controllers;
using OrchardCore.ContentManagement.Metadata;
using Microsoft.Extensions.Caching.Memory;
using OrchardCore.Environment.Cache;
using RestSharp;
using OrchardCore.Data;

namespace OrchardCore.SimService.SimApi
{
    [Route("api/content/[action]")]
    [ApiController]
    [Authorize(AuthenticationSchemes = "Api"), IgnoreAntiforgeryToken]
    public class BalanceApiController : Controller
    {
        private static readonly JsonMergeSettings UpdateJsonMergeSettings = new JsonMergeSettings { MergeArrayHandling = MergeArrayHandling.Replace };
        private readonly Microsoft.Extensions.Configuration.IConfiguration _config;
        private readonly IContentManager _contentManager;
        private readonly IAuthorizationService _authorizationService;
        private readonly IStringLocalizer S;
        private readonly UserManager<IUser> _userManager;
        private readonly IEmailAddressValidator _emailAddressValidator;
        private readonly ILogger _logger;
        private readonly ISession _session;
        private readonly IReadOnlySession _sessionReadOnly;
        private readonly IUserService _userService;
        private readonly IContentDefinitionManager _contentDefinitionManager;
        private readonly IMemoryCache _memoryCache;
        private readonly ISignal _signal;

        public string btcPayliteApiUrl;
        //public string btcPayliteToken;


        public BalanceApiController(
            IContentDefinitionManager contentDefinitionManager,
            IUserService userService,
            Microsoft.Extensions.Configuration.IConfiguration config,
            ISession session,
            IReadOnlySession sessionReadOnly,
            IContentManager contentManager,
            IAuthorizationService authorizationService,
            UserManager<IUser> userManager,
            IEmailAddressValidator emailAddressValidator,
            ILogger<ApiController> logger,
            IMemoryCache memoryCache,
            ISignal signal,
            IStringLocalizer<ApiController> stringLocalizer)
        {
            _contentDefinitionManager = contentDefinitionManager;
            _userService = userService;
            _config = config;
            _session = session;
            _sessionReadOnly = sessionReadOnly;
            _contentManager = contentManager;
            _authorizationService = authorizationService;
            _userManager = userManager;
            _emailAddressValidator = emailAddressValidator ?? throw new ArgumentNullException(nameof(emailAddressValidator));
            _logger = logger;
            _memoryCache = memoryCache;
            _signal = signal;
            S = stringLocalizer;
            btcPayliteApiUrl = _config["BTCPayLiteUrl"];
        }

        [HttpPost]
        [EnableCors("MyPolicy")]
        [ActionName("UpdateSixSimBalanceByCoin")]
        public async Task<IActionResult> UpdateSixSimBalanceByCoin(UpdateBalanceModel updateBalanceModel)
        {
            var user = await _userManager.FindByNameAsync(User.Identity.Name) as Users.Models.User;
            if (user == null)
            {
                return Problem(
                        title: S["One or more validation errors occurred."],
                        detail: "User not found",
                        statusCode: (int)HttpStatusCode.BadRequest);
            }

            try
            {
                var result = await ApiCommon.UpdateSixSimBalanceByCoin(_contentManager, _sessionReadOnly, updateBalanceModel, user, _logger);

                return Ok(true);
            }
            catch (Exception ex)
            {
                return Problem(
                                title: S["One or more validation errors occurred."],
                                detail: ex.Message,
                                statusCode: (int)HttpStatusCode.BadRequest);
            }
        }

        [HttpPost]
        [EnableCors("MyPolicy")]
        [ActionName("UpdateSixSimBalanceByBank")]
        public async Task<IActionResult> UpdateSixSimBalanceByBank(UpdateBalanceModel updateBalanceModel)
        {
            var user = await _userManager.FindByNameAsync(User.Identity.Name) as Users.Models.User;
            if (user == null)
            {
                return Problem(
                        title: S["One or more validation errors occurred."],
                        detail: "User not found",
                        statusCode: (int)HttpStatusCode.BadRequest);
            }

            try
            {
                var result = await ApiCommon.UpdateSixSimBalanceByBank(_contentManager, _sessionReadOnly, updateBalanceModel, user, _logger);

                return Ok(true);
            }
            catch (Exception ex)
            {
                return Problem(
                                title: S["One or more validation errors occurred."],
                                detail: ex.Message,
                                statusCode: (int)HttpStatusCode.BadRequest);
            }
        }

        [HttpGet]
        [EnableCors("MyPolicy")]
        [ActionName("GenerateInvoiceEth")]
        public async Task<IActionResult> GenerateInvoiceEth(string traderContentId)
        {
            var user = await _userManager.FindByNameAsync(User.Identity.Name) as User;

            if (user == null)
            {
                return Problem(
                        title: S["One or more validation errors occurred."],
                        detail: "User not found",
                        statusCode: (int)HttpStatusCode.BadRequest);
            }

            var btcPayliteToken = await ApiCommon.ReadCache(_sessionReadOnly, _memoryCache, _signal, _config, "BtcpayToken");
            var storeId = await ApiCommon.ReadCache(_sessionReadOnly, _memoryCache, _signal, _config, "BtcpayStoreKey");

            var updateBalanceUrl = _config["PaxHubUrl"] + "/UpdateSixSimBalanceByCoin";
            var ethApiGetNewAddressUrl = $"{btcPayliteApiUrl}/api/v1/stores/{storeId}/invoices";

            var client = new RestClient(ethApiGetNewAddressUrl);

            var bearerToken = Request.Headers[HeaderNames.Authorization].ToString().Replace("Bearer ", "");

            try
            {
                var data = new
                {
                    Metadata = new { OfferWallet = "ethereum", BuyerEmail = user.Email, UserId = user.Id, TraderContentId = traderContentId, PaxHubToken = bearerToken, PaxHubUrl = updateBalanceUrl, CurrencyOfTrade = "RUB" },
                    Checkout = new
                    {
                        SpeedPolicy = "HighSpeed",
                        PaymentMethods = new List<string>() { "ETH_EthereumLike" },
                        ExpirationMinutes = 45,
                        MonitoringMinutes = 45,
                        PaymentTolerance = 0,
                    },
                    Amount = 1,
                    Currency = "RUB"
                };

                var dataString = JsonConvert.SerializeObject(data);

                var request = new RestRequest();
                request.AddHeader("ContentType", "application/json");
                request.AddHeader("Authorization", btcPayliteToken);
                request.AddJsonBody(dataString);
                var ethInvoiceResult = await client.PostAsync(request);
                var ethInvoiceObj = JsonConvert.DeserializeObject<dynamic>(ethInvoiceResult.Content);

                return Ok(ethInvoiceObj);
            }
            catch
            {
                return Problem(
                    title: S["One or more validation errors occurred."],
                    detail: "storeName is not found.",
                    statusCode: (int)HttpStatusCode.BadRequest);
            }

        }

        [HttpGet]
        [EnableCors("MyPolicy")]
        [ActionName("GenerateInvoiceBtc")]
        public async Task<IActionResult> GenerateInvoiceBtc(string traderContentId)
        {
            var user = await _userManager.FindByNameAsync(User.Identity.Name) as User;

            if (user == null)
            {
                return Problem(
                        title: S["One or more validation errors occurred."],
                        detail: "User not found",
                        statusCode: (int)HttpStatusCode.BadRequest);
            }

            var btcPayliteToken = await ApiCommon.ReadCache(_sessionReadOnly, _memoryCache, _signal, _config, "BtcpayToken");
            var storeId = await ApiCommon.ReadCache(_sessionReadOnly, _memoryCache, _signal, _config, "BtcpayStoreKey");

            var updateBalanceUrl = _config["PaxHubUrl"] + "/UpdateSixSimBalanceByCoin";
            var btcApiGetNewAddressUrl = $"{btcPayliteApiUrl}/api/v1/stores/{storeId}/invoices";

            var client = new RestClient(btcApiGetNewAddressUrl);

            var bearerToken = Request.Headers[HeaderNames.Authorization].ToString().Replace("Bearer ", "");

            try
            {
                var data = new
                {
                    Metadata = new { OfferWallet = "bitcoin", BuyerEmail = user.Email, UserId = user.Id, TraderContentId = traderContentId, PaxHubToken = bearerToken, PaxHubUrl = updateBalanceUrl, CurrencyOfTrade = "RUB" },
                    Checkout = new
                    {
                        SpeedPolicy = "HighSpeed",
                        PaymentMethods = new List<string>() { "BTC" },
                        ExpirationMinutes = 45,
                        MonitoringMinutes = 45,
                        PaymentTolerance = 0,
                    },
                    Amount = 30,
                    Currency = "RUB"
                };

                var dataString = JsonConvert.SerializeObject(data);

                var request = new RestRequest();
                request.AddHeader("ContentType", "application/json");
                request.AddHeader("Authorization", btcPayliteToken);
                request.AddJsonBody(dataString);
                var btcInvoiceResult = await client.PostAsync(request);
                var btcInvoiceObj = JsonConvert.DeserializeObject<dynamic>(btcInvoiceResult.Content);

                return Ok(btcInvoiceObj);
            }
            catch
            {
                return Problem(
                    title: S["One or more validation errors occurred."],
                    detail: "storeName is not found.",
                    statusCode: (int)HttpStatusCode.BadRequest);
            }

        }

        [HttpGet]
        [EnableCors("MyPolicy")]
        [ActionName("GenerateInvoiceUsdt20")]
        public async Task<IActionResult> GenerateInvoiceUsdt20(string traderContentId)
        {
            var user = await _userManager.FindByNameAsync(User.Identity.Name) as User;

            if (user == null)
            {
                return Problem(
                        title: S["One or more validation errors occurred."],
                        detail: "User not found",
                        statusCode: (int)HttpStatusCode.BadRequest);
            }

            var btcPayliteToken = await ApiCommon.ReadCache(_sessionReadOnly, _memoryCache, _signal, _config, "BtcpayToken");
            var storeId = await ApiCommon.ReadCache(_sessionReadOnly, _memoryCache, _signal, _config, "BtcpayStoreKey");

            var updateBalanceUrl = _config["PaxHubUrl"] + "/UpdateSixSimBalanceByCoin";
            var usdt20ApiGetNewAddressUrl = $"{btcPayliteApiUrl}/api/v1/stores/{storeId}/invoices";

            var client = new RestClient(usdt20ApiGetNewAddressUrl);

            var bearerToken = Request.Headers[HeaderNames.Authorization].ToString().Replace("Bearer ", "");

            try
            {
                var data = new
                {
                    Metadata = new { OfferWallet = "usdt20", BuyerEmail = user.Email, UserId = user.Id, TraderContentId = traderContentId, PaxHubToken = bearerToken, PaxHubUrl = updateBalanceUrl, CurrencyOfTrade = "RUB" },
                    Checkout = new
                    {
                        SpeedPolicy = "HighSpeed",
                        //PaymentMethods = new List<string>() { "FAU_EthereumLike" },
                        PaymentMethods = new List<string>() { "USDT20_EthereumLike" },
                        ExpirationMinutes = 45,
                        MonitoringMinutes = 45,
                        PaymentTolerance = 0,
                    },
                    Amount = 1,
                    Currency = "RUB"
                };

                var dataString = JsonConvert.SerializeObject(data);

                var request = new RestRequest();
                request.AddHeader("ContentType", "application/json");
                request.AddHeader("Authorization", btcPayliteToken);
                request.AddJsonBody(dataString);
                var usdt20InvoiceResult = await client.PostAsync(request);
                var usdt20InvoiceObj = JsonConvert.DeserializeObject<dynamic>(usdt20InvoiceResult.Content);

                return Ok(usdt20InvoiceObj);
            }
            catch
            {
                return Problem(
                    title: S["One or more validation errors occurred."],
                    detail: "storeName is not found.",
                    statusCode: (int)HttpStatusCode.BadRequest);
            }

        }

        [HttpGet]
        [EnableCors("MyPolicy")]
        [ActionName("GenerateInvoiceTrc20")]
        public async Task<IActionResult> GenerateInvoiceTrc20(string traderContentId)
        {
            var user = await _userManager.FindByNameAsync(User.Identity.Name) as User;

            if (user == null)
            {
                return Problem(
                        title: S["One or more validation errors occurred."],
                        detail: "User not found",
                        statusCode: (int)HttpStatusCode.BadRequest);
            }

            var btcPayliteToken = await ApiCommon.ReadCache(_sessionReadOnly, _memoryCache, _signal, _config, "BtcpayToken");
            var storeId = await ApiCommon.ReadCache(_sessionReadOnly, _memoryCache, _signal, _config, "BtcpayStoreKey");

            var updateBalanceUrl = _config["PaxHubUrl"] + "/UpdateSixSimBalanceByCoin";
            var trcApiGetNewAddressUrl = $"{btcPayliteApiUrl}/api/v1/stores/{storeId}/invoices";

            var client = new RestClient(trcApiGetNewAddressUrl);

            var bearerToken = Request.Headers[HeaderNames.Authorization].ToString().Replace("Bearer ", "");

            try
            {
                var data = new
                {
                    Metadata = new { OfferWallet = "TRC", BuyerEmail = user.Email, UserId = user.Id, TraderContentId = traderContentId, PaxHubToken = bearerToken, PaxHubUrl = updateBalanceUrl, CurrencyOfTrade = "RUB" },
                    Checkout = new
                    {
                        SpeedPolicy = "HighSpeed",
                        PaymentMethods = new List<string>() { "trc_tronlike" },
                        ExpirationMinutes = 45,
                        MonitoringMinutes = 45,
                        PaymentTolerance = 0,
                    },
                    Amount = 1,
                    Currency = "RUB"
                };

                var dataString = JsonConvert.SerializeObject(data);

                var request = new RestRequest();
                request.AddHeader("ContentType", "application/json");
                request.AddHeader("Authorization", btcPayliteToken);
                request.AddJsonBody(dataString);
                var trc20InvoiceResult = await client.PostAsync(request);
                var trc20InvoiceObj = JsonConvert.DeserializeObject<dynamic>(trc20InvoiceResult.Content);

                return Ok(trc20InvoiceObj);
            }
            catch (Exception ex)
            {
                return Problem(
                    title: S["One or more validation errors occurred."],
                    detail: ex.Message,
                    statusCode: (int)HttpStatusCode.BadRequest);
            }

        }
    }
}
