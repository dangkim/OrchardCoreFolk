using System;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using Newtonsoft.Json.Linq;
using OrchardCore.ContentManagement;
using OrchardCore.ContentManagement.Models;
using OrchardCore.Mvc.Utilities;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using OrchardCore.Email;
using OrchardCore.Users;
using OrchardCore.Users.Models;
using Permissions = OrchardCore.Contents.CommonPermissions;
using Microsoft.AspNetCore.Cors;
using Newtonsoft.Json;
using YesSql;
using OrchardCore.ContentManagement.Records;
using OrchardCore.Users.Services;
using System.Collections.Generic;
using Microsoft.Net.Http.Headers;
using OrchardCore.SongServices.Models;
using OrchardCore.SongServices.Controllers;
using OrchardCore.SongServices.ApiModels;
using OrchardCore.SongServices.ApiCommonFunctions;
using RestSharp;
using OrchardCore.SongServices.ContentParts;

namespace OrchardCore.Content.Controllers
{
    [Route("api/content/[action]")]
    [ApiController]
    [Authorize(AuthenticationSchemes = "Api"), IgnoreAntiforgeryToken, AllowAnonymous]
    public class SendReleaseApiController : Controller
    {
        private static readonly JsonMergeSettings UpdateJsonMergeSettings = new JsonMergeSettings { MergeArrayHandling = MergeArrayHandling.Replace };
        private readonly Microsoft.Extensions.Configuration.IConfiguration _config;
        private readonly IContentManager _contentManager;
        private readonly IAuthorizationService _authorizationService;
        private readonly IStringLocalizer S;
        private readonly UserManager<IUser> _userManager;
        private readonly IEmailAddressValidator _emailAddressValidator;
        private readonly ILogger _logger;
        private readonly IRabbitMQProducer _rabbitMQProducer;
        private readonly YesSql.ISession _session;
        private readonly IUserService _userService;

        public string btcPayliteApiUrl;
        public string btcPayliteToken;
        public SendReleaseApiController(
            IUserService userService,
            Microsoft.Extensions.Configuration.IConfiguration config,
            YesSql.ISession session,
            IContentManager contentManager,
            IAuthorizationService authorizationService,
            UserManager<IUser> userManager,
            IEmailAddressValidator emailAddressValidator,
            ILogger<ApiController> logger,
            IRabbitMQProducer rabbitMQProducer,
            IStringLocalizer<ApiController> stringLocalizer)
        {
            _userService = userService;
            _config = config;
            _contentManager = contentManager;
            _authorizationService = authorizationService;
            _userManager = userManager;
            _emailAddressValidator = emailAddressValidator ?? throw new ArgumentNullException(nameof(emailAddressValidator));
            _logger = logger;
            S = stringLocalizer;
            _rabbitMQProducer = rabbitMQProducer;
            _session = session;


#if DEBUG
            //btcPayliteApiUrl = "https://localhost:14142";
            //btcPayliteToken = "token 3d3bbe0f8ddba99ce72402fae8114d0a7240bdd3";
            btcPayliteApiUrl = _config["BTCPayLiteUrl"];
            btcPayliteToken = _config["BTCPayLiteToken"];
#else
            btcPayliteApiUrl = _config["BTCPayLiteUrl"];
            btcPayliteToken = _config["BTCPayLiteToken"];
#endif
        }


        [HttpGet]
        [ActionName("ReleaseBitcoin")]
        public async Task<IActionResult> ReleaseBitcoin(string tradeId, string twoFaCode)
        {
            // Make trade completed
            var tradeContent = await _contentManager.GetAsync(tradeId);
            if (tradeContent != null)
            {
                dynamic jsonObj = tradeContent.Content;
                var tradeObj = jsonObj["TradeFilteringPart"];
                string sellerContentId = tradeObj.SellerContentId;
                var trader = await _contentManager.GetAsync(sellerContentId);

                string sellerEmail = trader.Content["TraderForFilteringPart"].Email;

                var btcApiCheck2FACodeUrl = $"{btcPayliteApiUrl}/api/v1/users/check2facode/{twoFaCode}/{sellerEmail}";

                try
                {
                    // Check seller 2FA code                                                          
                    using var client = new RestClient(btcApiCheck2FACodeUrl);
                    var request = new RestRequest();

                    request.AddHeader("Authorization", btcPayliteToken);

                    var twoFACodeString = await client.ExecuteGetAsync(request);
                    var twoFACodeStatus = Convert.ToBoolean(twoFACodeString.Content);

                    if (twoFACodeStatus == true)
                    {
                        tradeObj.Status = "completed";

                        var tradePart = new TradeFilteringPart();

                        tradePart.Buyer = tradeObj.Buyer;
                        tradePart.BuyerContentId = tradeObj.BuyerContentId;
                        tradePart.OfferId = tradeObj.OfferId;
                        tradePart.TradeStatus = tradeObj.Status;
                        tradePart.CurrencyOfTrade = tradeObj.CurrencyOfTrade;
                        tradePart.Duration = tradeObj.Duration;
                        tradePart.FeeBTCAmount = tradeObj.FeeBTCAmount;
                        tradePart.FeeETHAmount = tradeObj.FeeETHAmount;
                        tradePart.FeeType = tradeObj.FeeType;
                        tradePart.FeeUSDT20Amount = tradeObj.FeeUSDT20Amount;
                        tradePart.FeeVNDAmount = tradeObj.FeeVNDAmount;
                        tradePart.PaymentMethod = tradeObj.PaymentMethod;
                        tradePart.OfferType = tradeObj.OfferType;
                        tradePart.OfferWallet = tradeObj.OfferWallet;
                        tradePart.Seller = tradeObj.Seller;
                        tradePart.SellerContentId = tradeObj.SellerContentId;
                        tradePart.TradeBTCAmount = tradeObj.TradeBTCAmount;
                        tradePart.TradeETHAmount = tradeObj.TradeETHAmount;
                        tradePart.TradeUSDT20Amount = tradeObj.TradeUSDT20Amount;
                        tradePart.TradeVNDAmount = tradeObj.TradeVNDAmount;
                        tradePart.TradeType = tradeObj.TradeType;
                        //tradePart.BlockExplorerLink = tradeObj.BlockExplorerLink;
                        tradePart.DateTime= tradeObj.DateTime;

                        tradeContent.Apply(tradePart);
                        await _contentManager.UpdateAsync(tradeContent);

                        var resultUpdateTrade = await _contentManager.ValidateAsync(tradeContent);

                        if (resultUpdateTrade.Succeeded)
                        {
                            await _contentManager.PublishAsync(tradeContent);

                            // Update Balance of Buyer
                            decimal updatingAmount = tradeObj.TradeBTCAmount + tradeObj.FeeBTCAmount;
                            string buyerContentId = tradeObj.BuyerContentId;
                            //var traderSeller = await _session.Query<ContentItem, ContentItemIndex>(x => x.Published && x.Latest && x.ContentItemId == tradeContent.SellerContentId.Text && x.ContentType == "TraderPage").FirstOrDefaultAsync();
                            var traderBuyer = await _session.Query<ContentItem, ContentItemIndex>(x => x.Published && x.Latest && x.ContentItemId == buyerContentId && x.ContentType == "TraderPage").FirstOrDefaultAsync();

                            dynamic traderJsonObj = traderBuyer.Content;
                            var traderObj = traderJsonObj["TraderForFilteringPart"];

                            decimal currentBuyerBTCBalance = traderObj.BTCBalance;
                            var newBuyerBTCBalance = currentBuyerBTCBalance + updatingAmount;

                            traderObj.BTCBalance = newBuyerBTCBalance;

                            traderObj.Latest = true;
                            await _contentManager.UpdateAsync(traderBuyer);

                            var resultUpdateTrader = await _contentManager.ValidateAsync(traderBuyer);

                            if (resultUpdateTrader.Succeeded)
                            {
                                await _contentManager.PublishAsync(traderBuyer);

                                return Ok(true);
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    ex.ToString();
                    return Problem(
                        title: S["One or more validation errors occurred."],
                        detail: "storeName is not found.",
                        statusCode: (int)HttpStatusCode.BadRequest);
                }
            }
            return Ok(false);
        }

        [HttpGet]
        [ActionName("ReleaseETH")]
        public async Task<IActionResult> ReleaseETH(string tradeId, string twoFaCode)
        {
            // Make trade completed
            var tradeContent = await _contentManager.GetAsync(tradeId);
            if (tradeContent != null)
            {
                dynamic jsonObj = tradeContent.Content;
                var tradeObj = jsonObj["TradeFilteringPart"];
                string sellerContentId = tradeObj.SellerContentId;
                var trader = await _contentManager.GetAsync(sellerContentId);

                string sellerEmail = trader.Content["TraderForFilteringPart"].Email;

                var btcApiCheck2FACodeUrl = $"{btcPayliteApiUrl}/api/v1/users/check2facode/{twoFaCode}/{sellerEmail}";

                try
                {
                    // Check seller 2FA code                                                          
                    using var client = new RestClient(btcApiCheck2FACodeUrl);
                    var request = new RestRequest();

                    request.AddHeader("Authorization", btcPayliteToken);

                    var twoFACodeString = await client.ExecuteGetAsync(request);
                    var twoFACodeStatus = Convert.ToBoolean(twoFACodeString.Content);

                    if (twoFACodeStatus == true)
                    {
                        tradeObj.Status = "completed";

                        var tradePart = new TradeFilteringPart();

                        tradePart.Buyer = tradeObj.Buyer;
                        tradePart.BuyerContentId = tradeObj.BuyerContentId;
                        tradePart.OfferId = tradeObj.OfferId;
                        tradePart.TradeStatus = tradeObj.Status;
                        tradePart.CurrencyOfTrade = tradeObj.CurrencyOfTrade;
                        tradePart.Duration = tradeObj.Duration;
                        tradePart.FeeBTCAmount = tradeObj.FeeBTCAmount;
                        tradePart.FeeETHAmount = tradeObj.FeeETHAmount;
                        tradePart.FeeType = tradeObj.FeeType;
                        tradePart.FeeUSDT20Amount = tradeObj.FeeUSDT20Amount;
                        tradePart.FeeVNDAmount = tradeObj.FeeVNDAmount;
                        tradePart.PaymentMethod = tradeObj.PaymentMethod;
                        tradePart.OfferType = tradeObj.OfferType;
                        tradePart.OfferWallet = tradeObj.OfferWallet;
                        tradePart.Seller = tradeObj.Seller;
                        tradePart.SellerContentId = tradeObj.SellerContentId;
                        tradePart.TradeBTCAmount = tradeObj.TradeBTCAmount;
                        tradePart.TradeETHAmount = tradeObj.TradeETHAmount;
                        tradePart.TradeUSDT20Amount = tradeObj.TradeUSDT20Amount;
                        tradePart.TradeVNDAmount = tradeObj.TradeVNDAmount;
                        tradePart.TradeType = tradeObj.TradeType;
                        tradePart.DateTime= tradeObj.DateTime;

                        tradeContent.Apply(tradePart);
                        await _contentManager.UpdateAsync(tradeContent);

                        var resultUpdateTrade = await _contentManager.ValidateAsync(tradeContent);

                        if (resultUpdateTrade.Succeeded)
                        {
                            await _contentManager.PublishAsync(tradeContent);

                            // Update Balance of Buyer
                            decimal updatingAmount = tradeObj.TradeETHAmount + tradeObj.FeeETHAmount;
                            string buyerContentId = tradeObj.BuyerContentId;
                            var traderBuyer = await _session.Query<ContentItem, ContentItemIndex>(x => x.Published && x.Latest && x.ContentItemId == buyerContentId && x.ContentType == "TraderPage").FirstOrDefaultAsync();

                            dynamic traderJsonObj = traderBuyer.Content;
                            var traderObj = traderJsonObj["TraderForFilteringPart"];

                            decimal currentBuyerETHBalance = traderObj.ETHBalance;
                            var newBuyerETHBalance = currentBuyerETHBalance + updatingAmount;

                            traderObj.ETHBalance = newBuyerETHBalance;

                            traderObj.Latest = true;
                            await _contentManager.UpdateAsync(traderBuyer);

                            var resultUpdateTrader = await _contentManager.ValidateAsync(traderBuyer);

                            if (resultUpdateTrader.Succeeded)
                            {
                                await _contentManager.PublishAsync(traderBuyer);

                                return Ok(true);
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    ex.ToString();
                    return Problem(
                        title: S["One or more validation errors occurred."],
                        detail: "storeName is not found.",
                        statusCode: (int)HttpStatusCode.BadRequest);
                }
            }
            return Ok(false);
        }
        [HttpGet]

        [ActionName("ReleaseUSDT20")]
        public async Task<IActionResult> ReleaseUSDT20(string tradeId, string twoFaCode)
        {
            // Make trade completed
            var tradeContent = await _contentManager.GetAsync(tradeId);
            if (tradeContent != null)
            {
                dynamic jsonObj = tradeContent.Content;
                var tradeObj = jsonObj["TradeFilteringPart"];
                string sellerContentId = tradeObj.SellerContentId;
                var trader = await _contentManager.GetAsync(sellerContentId);

                string sellerEmail = trader.Content["TraderForFilteringPart"].Email;

                var btcApiCheck2FACodeUrl = $"{btcPayliteApiUrl}/api/v1/users/check2facode/{twoFaCode}/{sellerEmail}";

                try
                {
                    // Check seller 2FA code                                                          
                    using var client = new RestClient(btcApiCheck2FACodeUrl);
                    var request = new RestRequest();

                    request.AddHeader("Authorization", btcPayliteToken);

                    var twoFACodeString = await client.ExecuteGetAsync(request);
                    var twoFACodeStatus = Convert.ToBoolean(twoFACodeString.Content);

                    if (twoFACodeStatus == true)
                    {
                        tradeObj.Status = "completed";

                        var tradePart = new TradeFilteringPart();

                        tradePart.Buyer = tradeObj.Buyer;
                        tradePart.BuyerContentId = tradeObj.BuyerContentId;
                        tradePart.OfferId = tradeObj.OfferId;
                        tradePart.TradeStatus = tradeObj.Status;
                        tradePart.CurrencyOfTrade = tradeObj.CurrencyOfTrade;
                        tradePart.Duration = tradeObj.Duration;
                        tradePart.FeeBTCAmount = tradeObj.FeeBTCAmount;
                        tradePart.FeeETHAmount = tradeObj.FeeETHAmount;
                        tradePart.FeeType = tradeObj.FeeType;
                        tradePart.FeeUSDT20Amount = tradeObj.FeeUSDT20Amount;
                        tradePart.FeeVNDAmount = tradeObj.FeeVNDAmount;
                        tradePart.PaymentMethod = tradeObj.PaymentMethod;
                        tradePart.OfferType = tradeObj.OfferType;
                        tradePart.OfferWallet = tradeObj.OfferWallet;
                        tradePart.Seller = tradeObj.Seller;
                        tradePart.SellerContentId = tradeObj.SellerContentId;
                        tradePart.TradeBTCAmount = tradeObj.TradeBTCAmount;
                        tradePart.TradeETHAmount = tradeObj.TradeETHAmount;
                        tradePart.TradeUSDT20Amount = tradeObj.TradeUSDT20Amount;
                        tradePart.TradeVNDAmount = tradeObj.TradeVNDAmount;
                        tradePart.TradeType = tradeObj.TradeType;
                        tradePart.DateTime = tradeObj.DateTime;

                        tradeContent.Apply(tradePart);
                        await _contentManager.UpdateAsync(tradeContent);

                        var resultUpdateTrade = await _contentManager.ValidateAsync(tradeContent);

                        if (resultUpdateTrade.Succeeded)
                        {
                            await _contentManager.PublishAsync(tradeContent);

                            // Update Balance of Buyer
                            decimal updatingAmount = tradeObj.TradeUSDT20Amount + tradeObj.FeeUSDT20Amount;
                            string buyerContentId = tradeObj.BuyerContentId;
                            //var traderSeller = await _session.Query<ContentItem, ContentItemIndex>(x => x.Published && x.Latest && x.ContentItemId == tradeContent.SellerContentId.Text && x.ContentType == "TraderPage").FirstOrDefaultAsync();
                            var traderBuyer = await _session.Query<ContentItem, ContentItemIndex>(x => x.Published && x.Latest && x.ContentItemId == buyerContentId && x.ContentType == "TraderPage").FirstOrDefaultAsync();

                            dynamic traderJsonObj = traderBuyer.Content;
                            var traderObj = traderJsonObj["TraderForFilteringPart"];

                            decimal currentBuyerUSDT20Balance = traderObj.USDT20Balance;
                            var newBuyerUSDT20Balance = currentBuyerUSDT20Balance + updatingAmount;

                            traderObj.USDT20Balance = newBuyerUSDT20Balance;

                            traderObj.Latest = true;
                            await _contentManager.UpdateAsync(traderBuyer);

                            var resultUpdateTrader = await _contentManager.ValidateAsync(traderBuyer);

                            if (resultUpdateTrader.Succeeded)
                            {
                                await _contentManager.PublishAsync(traderBuyer);

                                return Ok(true);
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    ex.ToString();
                    return Problem(
                        title: S["One or more validation errors occurred."],
                        detail: "storeName is not found.",
                        statusCode: (int)HttpStatusCode.BadRequest);
                }
            }
            return Ok(false);
        }

        [HttpGet]
        [ActionName("ReleaseVND")]
        public async Task<IActionResult> ReleaseVND(string tradeId, string twoFaCode)
        {
            // Make trade completed
            var tradeContent = await _contentManager.GetAsync(tradeId);
            if (tradeContent != null)
            {
                dynamic jsonObj = tradeContent.Content;
                var tradeObj = jsonObj["TradeFilteringPart"];
                string sellerContentId = tradeObj.SellerContentId;
                var trader = await _contentManager.GetAsync(sellerContentId);

                string sellerEmail = trader.Content["TraderForFilteringPart"].Email;

                var btcApiCheck2FACodeUrl = $"{btcPayliteApiUrl}/api/v1/users/check2facode/{twoFaCode}/{sellerEmail}";

                try
                {
                    // Check seller 2FA code                                                          
                    using var client = new RestClient(btcApiCheck2FACodeUrl);
                    var request = new RestRequest();

                    request.AddHeader("Authorization", btcPayliteToken);

                    var twoFACodeString = await client.ExecuteGetAsync(request);
                    var twoFACodeStatus = Convert.ToBoolean(twoFACodeString.Content);

                    if (twoFACodeStatus == true)
                    {
                        tradeObj.Status = "completed";

                        var tradePart = new TradeFilteringPart();

                        tradePart.Buyer = tradeObj.Buyer;
                        tradePart.BuyerContentId = tradeObj.BuyerContentId;
                        tradePart.OfferId = tradeObj.OfferId;
                        tradePart.TradeStatus = tradeObj.Status;
                        tradePart.CurrencyOfTrade = tradeObj.CurrencyOfTrade;
                        tradePart.Duration = tradeObj.Duration;
                        tradePart.FeeBTCAmount = tradeObj.FeeBTCAmount;
                        tradePart.FeeETHAmount = tradeObj.FeeETHAmount;
                        tradePart.FeeType = tradeObj.FeeType;
                        tradePart.FeeUSDT20Amount = tradeObj.FeeUSDT20Amount;
                        tradePart.FeeVNDAmount = tradeObj.FeeVNDAmount;
                        tradePart.PaymentMethod = tradeObj.PaymentMethod;
                        tradePart.OfferType = tradeObj.OfferType;
                        tradePart.OfferWallet = tradeObj.OfferWallet;
                        tradePart.Seller = tradeObj.Seller;
                        tradePart.SellerContentId = tradeObj.SellerContentId;
                        tradePart.TradeBTCAmount = tradeObj.TradeBTCAmount;
                        tradePart.TradeETHAmount = tradeObj.TradeETHAmount;
                        tradePart.TradeUSDT20Amount = tradeObj.TradeUSDT20Amount;
                        tradePart.TradeVNDAmount = tradeObj.TradeVNDAmount;
                        tradePart.TradeType = tradeObj.TradeType;
                        tradePart.DateTime = tradeObj.DateTime;

                        tradeContent.Apply(tradePart);
                        await _contentManager.UpdateAsync(tradeContent);

                        var resultUpdateTrade = await _contentManager.ValidateAsync(tradeContent);

                        if (resultUpdateTrade.Succeeded)
                        {
                            await _contentManager.PublishAsync(tradeContent);

                            // Update Balance of Buyer
                            decimal updatingAmount = tradeObj.TradeVNDAmount + tradeObj.FeeVNDAmount;
                            string buyerContentId = tradeObj.BuyerContentId;
                            var traderBuyer = await _session.Query<ContentItem, ContentItemIndex>(x => x.Published && x.Latest && x.ContentItemId == buyerContentId && x.ContentType == "TraderPage").FirstOrDefaultAsync();

                            dynamic traderJsonObj = traderBuyer.Content;
                            var traderObj = traderJsonObj["TraderForFilteringPart"];

                            decimal currentBuyerVNDBalance = traderObj.VNDBalance;
                            var newBuyerVNDBalance = currentBuyerVNDBalance + updatingAmount;

                            traderObj.VNDBalance = newBuyerVNDBalance;

                            traderObj.Latest = true;
                            await _contentManager.UpdateAsync(traderBuyer);

                            var resultUpdateTrader = await _contentManager.ValidateAsync(traderBuyer);

                            if (resultUpdateTrader.Succeeded)
                            {
                                await _contentManager.PublishAsync(traderBuyer);

                                return Ok(true);
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    ex.ToString();
                    return Problem(
                        title: S["One or more validation errors occurred."],
                        detail: "storeName is not found.",
                        statusCode: (int)HttpStatusCode.BadRequest);
                }
            }
            return Ok(false);
        }

        [HttpGet]
        [ActionName("GetTransactions")]
        public async Task<IActionResult> GetTransactions()
        {
            var user = await _userManager.FindByNameAsync(User.Identity.Name) as User;
            var btcApiCheckAddressUrl = $"{btcPayliteApiUrl}/api/v1/wallet/gettransactions/{user.Email}";

            using var client = new RestClient(btcApiCheckAddressUrl);
            var request = new RestRequest();

            request.AddHeader("Authorization", btcPayliteToken);

            try
            {
                var response = await client.ExecuteGetAsync(request);
                return Ok(response.Content);
            }
            catch
            {
                return Problem(
                                title: S["One or more validation errors occurred."],
                                detail: "BTC userName is not found.",
                                statusCode: (int)HttpStatusCode.BadRequest);
            }
        }

        [HttpGet]
        [ActionName("GetEthTransactions")]
        public async Task<IActionResult> GetEthTransactions()
        {
            var user = await _userManager.FindByNameAsync(User.Identity.Name) as User;
            var btcApiCheckAddressUrl = $"{btcPayliteApiUrl}/api/v1/wallet/gettransactions/{user.Email}";

            using var client = new RestClient(btcApiCheckAddressUrl);
            var request = new RestRequest();

            request.AddHeader("Authorization", btcPayliteToken);

            try
            {
                var response = await client.ExecuteGetAsync(request);
                return Ok(response.Content);
            }
            catch
            {
                return Problem(
                                title: S["One or more validation errors occurred."],
                                detail: "BTC userName is not found.",
                                statusCode: (int)HttpStatusCode.BadRequest);
            }
        }

        [HttpGet]
        [ActionName("GetUsdtTransactions")]
        public async Task<IActionResult> GetUsdtTransactions()
        {
            var user = await _userManager.FindByNameAsync(User.Identity.Name) as User;
            var btcApiCheckAddressUrl = $"{btcPayliteApiUrl}/api/v1/wallet/gettransactions/{user.Email}";

            using var client = new RestClient(btcApiCheckAddressUrl);
            var request = new RestRequest();

            request.AddHeader("Authorization", btcPayliteToken);

            try
            {
                var response = await client.ExecuteGetAsync(request);
                return Ok(response.Content);
            }
            catch
            {
                return Problem(
                                title: S["One or more validation errors occurred."],
                                detail: "BTC userName is not found.",
                                statusCode: (int)HttpStatusCode.BadRequest);
            }
        }

        [HttpGet]
        [ActionName("GetBtcTransactions")]
        public async Task<IActionResult> GetBtcTransactions()
        {
            var user = await _userManager.FindByNameAsync(User.Identity.Name) as User;
            var btcApiCheckAddressUrl = $"{btcPayliteApiUrl}/api/v1/wallet/gettransactions/{user.Email}";

            using var client = new RestClient(btcApiCheckAddressUrl);
            var request = new RestRequest();

            request.AddHeader("Authorization", btcPayliteToken);

            try
            {
                var response = await client.ExecuteGetAsync(request);
                return Ok(response.Content);
            }
            catch
            {
                return Problem(
                                title: S["One or more validation errors occurred."],
                                detail: "BTC userName is not found.",
                                statusCode: (int)HttpStatusCode.BadRequest);
            }
        }

        [HttpGet]
        [ActionName("GetVndTransactions")]
        public async Task<IActionResult> GetVndTransactions()
        {
            var user = await _userManager.FindByNameAsync(User.Identity.Name) as User;
            var btcApiCheckAddressUrl = $"{btcPayliteApiUrl}/api/v1/wallet/gettransactions/{user.Email}";

            using var client = new RestClient(btcApiCheckAddressUrl);
            var request = new RestRequest();

            request.AddHeader("Authorization", btcPayliteToken);

            try
            {
                var response = await client.ExecuteGetAsync(request);
                return Ok(response.Content);
            }
            catch
            {
                return Problem(
                                title: S["One or more validation errors occurred."],
                                detail: "BTC userName is not found.",
                                statusCode: (int)HttpStatusCode.BadRequest);
            }
        }

        [HttpGet]
        [ActionName("CheckReceiveAddress")]
        public async Task<ActionResult> CheckReceiveAddressAsync(string address)
        {
            var btcApiCheckAddressUrl = $"{btcPayliteApiUrl}/api/v1/wallet/checkaddress/{address}";

            using var client = new RestClient(btcApiCheckAddressUrl);
            var request = new RestRequest();

            request.AddHeader("Authorization", btcPayliteToken);

            try
            {
                var response = await client.ExecuteGetAsync(request);
                return Ok(response.Content);
            }
            catch
            {
                return Problem(
                                title: S["One or more validation errors occurred."],
                                detail: "BTC userName is not found.",
                                statusCode: (int)HttpStatusCode.BadRequest);
            }
        }

        [HttpPost]
        [ActionName("SendBitcoin")]
        public async Task<IActionResult> SendBitcoin(WalletModel request)
        {
            var user = await _userManager.FindByNameAsync(User.Identity.Name) as User;
            var btcApiCheck2FACodeUrl = $"{btcPayliteApiUrl}/api/v1/users/check2facode/{request.TwoFaCode}/{user.Email}";
            var btcApiSendBitcoinUrl = $"{btcPayliteApiUrl}/api/v1/wallet/sendbitcoin";

            try
            {
                // Check seller 2FA code                                                          
                using var clientFA = new RestClient(btcApiSendBitcoinUrl);
                var restRequestFA = new RestRequest();

                restRequestFA.AddHeader("Authorization", btcPayliteToken);

                var twoFACodeStatus = await clientFA.ExecuteAsync(restRequestFA);

                if (!Convert.ToBoolean(twoFACodeStatus.Content))
                {
                    return Ok(false);
                }

                var data = new
                {
                    StoreName = user.Email,
                    request.TargetAddress,
                    request.Amount
                };
                var dataString = JsonConvert.SerializeObject(data);

                using var client = new RestClient(btcApiSendBitcoinUrl);
                var restRequest = new RestRequest();

                restRequest.AddHeader("Authorization", btcPayliteToken);
                restRequest.AddHeader("ContentType", "application/json");
                restRequest.AddJsonBody(dataString);

                var apiResult = await client.ExecutePostAsync(restRequest);
                return Ok(apiResult);
            }
            catch
            {
                return Problem(
                        title: S["One or more validation errors occurred."],
                        detail: "Something error when try to send bitcoin.",
                        statusCode: (int)HttpStatusCode.BadRequest);
            }

        }

        [HttpPost]
        [ActionName("SendBitcoinByAdmin")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> SendBitcoinByAdmin(WalletForAdminModel request)
        {
            var user = await _userManager.FindByNameAsync(User.Identity.Name) as User;

            if (!await _userManager.IsInRoleAsync(user, _userManager.NormalizeName("admin")))
            {
                return Problem(
                        title: S["One or more validation errors occurred."],
                        detail: "no permission to send bitcoin.",
                        statusCode: (int)HttpStatusCode.BadRequest);
            }

            var btcApiCheck2FACodeUrl = $"{btcPayliteApiUrl}/api/v1/users/check2facode/{request.AdminTwoFaCode}/{user.Email}";
            var btcApiSendBitcoinUrl = $"{btcPayliteApiUrl}/api/v1/wallet/sendbitcoin";

            try
            {
                // Check seller 2FA code                                                          
                using var client = new RestClient(btcApiCheck2FACodeUrl);
                var restRequest = new RestRequest();

                restRequest.AddHeader("Authorization", btcPayliteToken);

                var twoFACodeStatus = await client.ExecuteGetAsync(restRequest);

                if (!Convert.ToBoolean(twoFACodeStatus.Content))
                {
                    return Ok(false);
                }

                var data = new
                {
                    StoreName = request.SenderEmail,
                    request.TargetAddress,
                    request.Amount
                };
                var dataString = JsonConvert.SerializeObject(data);

                restRequest.AddHeader("ContentType", "application/json");
                restRequest.AddJsonBody(dataString);

                var apiResult = await client.ExecutePostAsync(restRequest);
                return Ok(apiResult.Content);
            }
            catch
            {
                return Problem(
                        title: S["One or more validation errors occurred."],
                        detail: "Something error when try to send bitcoin.",
                        statusCode: (int)HttpStatusCode.BadRequest);
            }

        }

        [HttpPost]
        [ActionName("SendEth")]
        public async Task<IActionResult> SendEth(WalletModel request)
        {
            var user = await _userManager.FindByNameAsync(User.Identity.Name) as User;
            var ethApiSendEthUrl = $"{btcPayliteApiUrl}/api/v1/stores/{user.Email}/transferEthUserName";

            var data = new
            {
                CryptoCode = "ETH",
                request.Amount,
                ToAddress = request.TargetAddress,
                StoreId = _config["StoreId"],
                CurrencyOfTrade = "USD"
            };

            var dataString = JsonConvert.SerializeObject(data);

            using var client = new RestClient(ethApiSendEthUrl);
            var restRequest = new RestRequest();

            restRequest.AddHeader("Authorization", btcPayliteToken);
            restRequest.AddHeader("ContentType", "application/json");
            restRequest.AddJsonBody(dataString);

            try
            {
                var apiResult = await client.ExecutePostAsync(restRequest);

                if (!string.IsNullOrEmpty(apiResult.Content))
                {
                    var returnObject = JsonConvert.DeserializeObject<SendCoinReturnModel>(apiResult.Content);

                    var updateBalanceModel = new UpdateBalanceModel()
                    {
                        ContenItemId = request.TraderId,
                        TransactionType = 0,// withdraw
                        Amount = request.Amount.ToString(),
                        CreatedDate = DateTimeOffset.UtcNow,
                        Crypto = "ETH",
                        BlockChainLink = returnObject.BlockChainLink,
                        Rate = returnObject.Rate
                    };

                    var result = await ApiCommon.UpdateBalance(_contentManager, _session, updateBalanceModel, user);

                    return Ok(result);
                }

                return Ok(false);
            }
            catch
            {
                return Problem(
                        title: S["One or more validation errors occurred."],
                        detail: "Something error when try to send bitcoin.",
                        statusCode: (int)HttpStatusCode.BadRequest);
            }

        }

        [HttpGet]
        [ActionName("GenerateEthAddress")]
        public async Task<IActionResult> GenerateEthAddress()
        {
            var user = await _userManager.FindByNameAsync(User.Identity.Name) as User;
            var ethApiGetNewAddressUrl = $"{btcPayliteApiUrl}/api/v1/stores/{user.Email}/eth-getwalletname";

            using var client = new RestClient(ethApiGetNewAddressUrl);
            var restRequest = new RestRequest();

            restRequest.AddHeader("Authorization", btcPayliteToken);
            restRequest.AddHeader("ContentType", "application/json");


            try
            {
                var data = new
                {
                    CryptoCode = "ETH"
                };
                var dataString = JsonConvert.SerializeObject(data);

                restRequest.AddJsonBody(dataString);

                var userEthBalanceResult = await client.ExecutePostAsync(restRequest);
                var ethBalanceObj = JsonConvert.DeserializeObject<dynamic>(userEthBalanceResult.Content);

                return Ok(ethBalanceObj);
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

            var updateBalanceUrl = _config["PaxHubUrl"] + "/UpdateBalance";
            var ethApiGetNewAddressUrl = $"{btcPayliteApiUrl}/api/v1/stores/{_config["StoreId"]}/invoices";

            using var client = new RestClient(ethApiGetNewAddressUrl);
            var request = new RestRequest();

            request.AddHeader("Authorization", btcPayliteToken);
            request.AddHeader("ContentType", "application/json");

            var bearerToken = Request.Headers[HeaderNames.Authorization].ToString().Replace("Bearer ", "");

            try
            {
                var data = new
                {
                    Metadata = new { OfferWallet = "ethereum", BuyerEmail = user.Email, TraderContentId = traderContentId, PaxHubToken = bearerToken, PaxHubUrl = updateBalanceUrl, CurrencyOfTrade = "USD" },
                    Checkout = new
                    {
                        SpeedPolicy = "HighSpeed",
                        PaymentMethods = new List<string>() { "ETH_EthereumLike" },
                        ExpirationMinutes = 45,
                        MonitoringMinutes = 45,
                        PaymentTolerance = 0,
                    },
                    Amount = 1,
                    Currency = "USD"
                };

                var dataString = JsonConvert.SerializeObject(data);
                request.AddJsonBody(dataString);

                var ethInvoiceResult = await client.ExecutePostAsync(request);
                var ethInvoiceObj = JsonConvert.DeserializeObject<dynamic>(ethInvoiceResult.Content);

                //var apiResult = client.DownloadString(new Uri(ethApiGetNewAddressUrl));
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

            var updateBalanceUrl = _config["PaxHubUrl"] + "/UpdateBalance";
            var ethApiGetNewAddressUrl = $"{btcPayliteApiUrl}/api/v1/stores/{_config["StoreId"]}/invoices";

            using var client = new RestClient(ethApiGetNewAddressUrl);
            var request = new RestRequest();

            request.AddHeader("Authorization", btcPayliteToken);
            request.AddHeader("ContentType", "application/json");

            var bearerToken = Request.Headers[HeaderNames.Authorization].ToString().Replace("Bearer ", "");

            try
            {
                var data = new
                {
                    Metadata = new { OfferWallet = "bitcoin", BuyerEmail = user.Email, TraderContentId = traderContentId, PaxHubToken = bearerToken, PaxHubUrl = updateBalanceUrl, CurrencyOfTrade = "USD" },
                    Checkout = new
                    {
                        SpeedPolicy = "HighSpeed",
                        PaymentMethods = new List<string>() { "BTC" },
                        ExpirationMinutes = 45,
                        MonitoringMinutes = 45,
                        PaymentTolerance = 0,
                    },
                    Amount = 1,
                    Currency = "USD"
                };

                var dataString = JsonConvert.SerializeObject(data);
                request.AddJsonBody(dataString);

                var ethInvoiceResult = await client.ExecutePostAsync(request);
                var ethInvoiceObj = JsonConvert.DeserializeObject<dynamic>(ethInvoiceResult.Content);

                //var apiResult = client.DownloadString(new Uri(ethApiGetNewAddressUrl));
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

            var updateBalanceUrl = _config["PaxHubUrl"] + "/UpdateBalance";
            var ethApiGetNewAddressUrl = $"{btcPayliteApiUrl}/api/v1/stores/{_config["StoreId"]}/invoices";

            using var client = new RestClient(ethApiGetNewAddressUrl);
            var request = new RestRequest();

            request.AddHeader("Authorization", btcPayliteToken);
            request.AddHeader("ContentType", "application/json");

            var bearerToken = Request.Headers[HeaderNames.Authorization].ToString().Replace("Bearer ", "");

            try
            {
                var data = new
                {
                    Metadata = new { OfferWallet = "usdt20", BuyerEmail = user.Email, TraderContentId = traderContentId, PaxHubToken = bearerToken, PaxHubUrl = updateBalanceUrl, CurrencyOfTrade = "USD" },
                    Checkout = new
                    {
                        SpeedPolicy = "HighSpeed",
                        PaymentMethods = new List<string>() { "FAU_EthereumLike" },
                        ExpirationMinutes = 45,
                        MonitoringMinutes = 45,
                        PaymentTolerance = 0,
                    },
                    Amount = 1,
                    Currency = "USD"
                };

                var dataString = JsonConvert.SerializeObject(data);
                request.AddJsonBody(dataString);

                var ethInvoiceResult = await client.ExecutePostAsync(request);
                var ethInvoiceObj = JsonConvert.DeserializeObject<dynamic>(ethInvoiceResult.Content);

                //var apiResult = client.DownloadString(new Uri(ethApiGetNewAddressUrl));
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
        [ActionName("GetInvoicesEth")]
        public async Task<IActionResult> GetInvoicesEth()
        {
            var user = await _userManager.FindByNameAsync(User.Identity.Name) as User;
            var ethApiGetInvoicesUrl = $"{btcPayliteApiUrl}/api/v1/stores/{_config["StoreId"]}/{user.Email}/invoices";

            using var client = new RestClient(ethApiGetInvoicesUrl);
            var request = new RestRequest();

            request.AddHeader("Authorization", btcPayliteToken);
            request.AddHeader("ContentType", "application/json");

            try
            {
                var ethInvoiceResult = await client.ExecuteGetAsync(request);
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

        [HttpPost]
        [ActionName("SendUsdt20")]
        public async Task<IActionResult> SendUsdt20(WalletModel request)
        {
            var user = await _userManager.FindByNameAsync(User.Identity.Name) as User;
            var ethApiSendBitcoinUrl = $"{btcPayliteApiUrl}/api/v1/stores/{user.Email}/transferEthUserName";

            var data = new
            {
                CryptoCode = "FAU",
                //CryptoCode = "USDT20",
                Amount = request.Amount,
                ToAddress = request.TargetAddress,
            };
            var dataString = JsonConvert.SerializeObject(data);

            using var client = new RestClient(ethApiSendBitcoinUrl);
            var restRequest = new RestRequest();

            restRequest.AddJsonBody(dataString);
            restRequest.AddHeader("Authorization", btcPayliteToken);
            restRequest.AddHeader("ContentType", "application/json");

            try
            {
                var apiResult = await client.ExecutePostAsync(restRequest);
                return Ok(apiResult.Content);
            }
            catch
            {
                return Problem(
                        title: S["One or more validation errors occurred."],
                        detail: "Something error when try to send bitcoin.",
                        statusCode: (int)HttpStatusCode.BadRequest);
            }

        }

        [HttpGet]
        [ActionName("GenerateUsdt20Address")]
        public async Task<IActionResult> GenerateUsdt20Address()
        {
            var user = await _userManager.FindByNameAsync(User.Identity.Name) as User;
            var ethApiGetNewAddressUrl = $"{btcPayliteApiUrl}/api/v1/stores/{user.Email}/usdt20-getwalletname";

            using var client = new RestClient(ethApiGetNewAddressUrl);
            var restRequest = new RestRequest();

            restRequest.AddHeader("Authorization", btcPayliteToken);
            restRequest.AddHeader("ContentType", "application/json");

            try
            {
                var data = new
                {
                    CryptoCode = "FAU"
                    //CryptoCode = "USDT20"
                };
                var dataString = JsonConvert.SerializeObject(data);
                restRequest.AddJsonBody(dataString);

                var userEthBalanceResult = await client.ExecutePostAsync(restRequest);
                var ethBalanceObj = JsonConvert.DeserializeObject<dynamic>(userEthBalanceResult.Content);

                //var apiResult = client.DownloadString(new Uri(ethApiGetNewAddressUrl));
                return Ok(ethBalanceObj);
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
        [ActionName("GetNewReceiveAddress")]
        public async Task<IActionResult> GetNewReceiveAddress()
        {
            var user = await _userManager.FindByNameAsync(User.Identity.Name) as User;
            var btcApiGetNewAddressUrl = $"{btcPayliteApiUrl}/api/v1/wallet/getnewaddress/{user.Email}";

            using var client = new RestClient(btcApiGetNewAddressUrl);
            var restRequest = new RestRequest();

            restRequest.AddHeader("Authorization", btcPayliteToken);
            restRequest.AddHeader("ContentType", "application/json");

            try
            {
                var apiResult = await client.ExecuteGetAsync(restRequest);
                return Ok(apiResult.Content);
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
        [ActionName("GetReceiveAddress")]
        public async Task<IActionResult> GetReceiveAddress()
        {
            var user = await _userManager.FindByNameAsync(User.Identity.Name) as User;
            var btcApiGetAddressUrl = $"{btcPayliteApiUrl}/api/v1/wallet/getaddress/{user.Email}";

            using var client = new RestClient(btcApiGetAddressUrl);
            var restRequest = new RestRequest();

            restRequest.AddHeader("Authorization", btcPayliteToken);
            restRequest.AddHeader("ContentType", "application/json");

            try
            {
                var apiResult = await client.ExecuteGetAsync(restRequest);
                if (String.IsNullOrEmpty(apiResult.Content))
                {
                    return Problem(
                        title: S["One or more validation errors occurred."],
                        detail: "BTC Address is empty.",
                        statusCode: (int)HttpStatusCode.BadRequest);
                }
                return Ok(apiResult);
            }
            catch
            {

                return Problem(
                    title: S["One or more validation errors occurred."],
                    detail: "BTC storeName is not found.",
                    statusCode: (int)HttpStatusCode.BadRequest);
            }

        }

    }
}
