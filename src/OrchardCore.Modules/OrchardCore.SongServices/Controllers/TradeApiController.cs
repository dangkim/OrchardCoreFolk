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
using OrchardCore.ContentManagement.Records;
using OrchardCore.Users.Services;
using OrchardCore.SongServices.ApiModels;
using System.Collections.Generic;
using OrchardCore.SongServices.Models;
using OrchardCore.SongServices.Controllers;
using CommonPermissions = OrchardCore.Contents.CommonPermissions;
using OrchardCore.SongServices.ViewModels;
using OrchardCore.SongServices.ContentParts;
using OrchardCore.SongServices.Indexing;

namespace OrchardCore.Content.Controllers
{
    [Route("api/content/[action]")]
    [ApiController]
    [Authorize(AuthenticationSchemes = "Api"), IgnoreAntiforgeryToken, AllowAnonymous]
    public class TradeApiController : Controller
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
        private readonly ISession _session;
        private readonly IUserService _userService;

        public string btcPayliteApiUrl;
        public string btcPayliteToken;

        public TradeApiController(
            IUserService userService,
            Microsoft.Extensions.Configuration.IConfiguration config,
            ISession session,
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
        [ActionName("GetTradeByPartHome")]
        [AllowAnonymous]
        public async Task<IActionResult> GetTradeByPartHome()
        {
            var tradeHome = await _session
                .Query<ContentItem, ContentItemIndex>(index => index.ContentType == "TradePage" && index.Latest && index.Published).OrderByDescending(index => index.CreatedUtc)
                .With<TradeFilteringPartIndex>(p => p.Status == "completed")
                .Take(50)
                .ListAsync();

            var listTrades = new List<object>();

            foreach (var item in tradeHome)
            {
                var tradeObj = item.As<TradeFilteringPart>();

                var tradeModel = new
                {
                    item.ContentItemId,
                    tradeObj.Buyer,
                    tradeObj.BuyerContentId,
                    tradeObj.OfferId,
                    tradeObj.TradeStatus,
                    tradeObj.CurrencyOfTrade,
                    tradeObj.Duration,
                    tradeObj.FeeBTCAmount,
                    tradeObj.FeeETHAmount,
                    tradeObj.FeeType,
                    tradeObj.FeeUSDT20Amount,
                    tradeObj.FeeVNDAmount,
                    tradeObj.PaymentMethod,
                    tradeObj.OfferType,
                    tradeObj.OfferWallet,
                    tradeObj.Seller,
                    tradeObj.SellerContentId,
                    tradeObj.TradeBTCAmount,
                    tradeObj.TradeETHAmount,
                    tradeObj.TradeUSDT20Amount,
                    tradeObj.TradeVNDAmount,
                    tradeObj.TradeType,
                    item.CreatedUtc,
                    BlockExplorerLink = ""
                };

                listTrades.Add(tradeModel);
            }

            return Ok(listTrades);
        }

        [HttpGet]
        [ActionName("GetTradeById")]
        public async Task<IActionResult> GetTradeById(string id)
        {
            var tradeById = await _session
                .Query<ContentItem, ContentItemIndex>(index => index.ContentType == "TradePage" && index.ContentItemId == id && index.Published && index.Latest)
                .ListAsync();

            var listTrades = new List<object>();

            foreach (var item in tradeById)
            {
                var tradeObj = item.As<TradeFilteringPart>();

                var tradeModel = new
                {
                    item.ContentItemId,
                    tradeObj.Buyer,
                    tradeObj.BuyerContentId,
                    tradeObj.OfferId,
                    tradeObj.TradeStatus,
                    tradeObj.CurrencyOfTrade,
                    tradeObj.Duration,
                    tradeObj.FeeBTCAmount,
                    tradeObj.FeeETHAmount,
                    tradeObj.FeeType,
                    tradeObj.FeeUSDT20Amount,
                    tradeObj.FeeVNDAmount,
                    tradeObj.PaymentMethod,
                    tradeObj.OfferType,
                    tradeObj.OfferWallet,
                    tradeObj.Seller,
                    tradeObj.SellerContentId,
                    tradeObj.TradeBTCAmount,
                    tradeObj.TradeETHAmount,
                    tradeObj.TradeUSDT20Amount,
                    tradeObj.TradeVNDAmount,
                    tradeObj.TradeType,
                    item.CreatedUtc,
                    BlockExplorerLink = ""
                };

                listTrades.Add(tradeModel);
            }

            return Ok(listTrades.FirstOrDefault());
        }

        [HttpGet]
        [ActionName("GetTradeByStatus")]
        public async Task<IActionResult> GetTradeByStatus(string status, string userName)
        {
            var tradeById = await _session
                            .Query<ContentItem, ContentItemIndex>(index => index.ContentType == "TradePage" && index.Published && index.Latest)
                            .With<TradeFilteringPartIndex>(p => p.Status == status && p.Seller == userName)
                            .ListAsync();

            var listTrades = new List<object>();

            foreach (var item in tradeById)
            {
                var tradeObj = item.As<TradeFilteringPart>();

                var tradeModel = new
                {
                    item.ContentItemId,
                    tradeObj.Buyer,
                    tradeObj.BuyerContentId,
                    tradeObj.OfferId,
                    tradeObj.TradeStatus,
                    tradeObj.CurrencyOfTrade,
                    tradeObj.Duration,
                    tradeObj.FeeBTCAmount,
                    tradeObj.FeeETHAmount,
                    tradeObj.FeeType,
                    tradeObj.FeeUSDT20Amount,
                    tradeObj.FeeVNDAmount,
                    tradeObj.PaymentMethod,
                    tradeObj.OfferType,
                    tradeObj.OfferWallet,
                    tradeObj.Seller,
                    tradeObj.SellerContentId,
                    tradeObj.TradeBTCAmount,
                    tradeObj.TradeETHAmount,
                    tradeObj.TradeUSDT20Amount,
                    tradeObj.TradeVNDAmount,
                    tradeObj.TradeType,
                    item.CreatedUtc,
                    BlockExplorerLink = ""
                };

                listTrades.Add(tradeModel);
            }

            return Ok(listTrades.FirstOrDefault());
        }

        [HttpGet]
        [ActionName("GetAllTradeOfUser")]
        public async Task<IActionResult> GetAllTradeOfUser(string wallet, string userName)
        {
            var tradeById = await _session
                .Query<ContentItem, ContentItemIndex>(index => index.ContentType == "TradePage" && index.Published && index.Latest)
                .With<TradeFilteringPartIndex>(p => p.CurrencyOfTrade == wallet && p.Seller == userName)
                .With<TradeFilteringPartIndex>(p => p.Status == "completed" || p.Status == "cancel")
                .ListAsync();

            var listTrades = new List<object>();

            foreach (var item in tradeById)
            {
                var tradeObj = item.As<TradeFilteringPart>();

                var tradeModel = new
                {
                    item.ContentItemId,
                    tradeObj.Buyer,
                    tradeObj.BuyerContentId,
                    tradeObj.OfferId,
                    tradeObj.TradeStatus,
                    tradeObj.CurrencyOfTrade,
                    tradeObj.Duration,
                    tradeObj.FeeBTCAmount,
                    tradeObj.FeeETHAmount,
                    tradeObj.FeeType,
                    tradeObj.FeeUSDT20Amount,
                    tradeObj.FeeVNDAmount,
                    tradeObj.PaymentMethod,
                    tradeObj.OfferType,
                    tradeObj.OfferWallet,
                    tradeObj.Seller,
                    tradeObj.SellerContentId,
                    tradeObj.TradeBTCAmount,
                    tradeObj.TradeETHAmount,
                    tradeObj.TradeUSDT20Amount,
                    tradeObj.TradeVNDAmount,
                    tradeObj.TradeType,
                    item.CreatedUtc,
                    BlockExplorerLink = ""
                };

                listTrades.Add(tradeModel);
            }

            return Ok(listTrades.FirstOrDefault());
        }

        [HttpGet]
        [ActionName("MakeExpiredTrade")]
        public async Task<IActionResult> MakeExpiredTrade(string tradeId)
        {
            var tradeContent = await _contentManager.GetAsync(tradeId);
            if (tradeContent != null)
            {
                var tradePart = tradeContent.As<TradeFilteringPart>();

                if (!await _authorizationService.AuthorizeAsync(User, CommonPermissions.PublishOwnContent, tradeContent))
                {
                    return Forbid();
                }

                tradePart.TradeStatus = "expired";

                tradeContent.Apply(tradePart);

                tradeContent.Latest = true;

                await _contentManager.UpdateAsync(tradeContent);

                var result = await _contentManager.ValidateAsync(tradeContent);

                if (result.Succeeded)
                {
                    await _contentManager.PublishAsync(tradeContent);
                }
                else
                {
                    return Ok(false);
                }

                return Ok(tradeContent);
            }

            return Ok(false);
        }

        [HttpGet]
        [ActionName("MakeCancelTrade")]
        public async Task<IActionResult> MakeCancelTrade(string tradeId)
        {
            var tradeContent = await _contentManager.GetAsync(tradeId, VersionOptions.DraftRequired);
            if (tradeContent != null)
            {
                dynamic jsonObj = tradeContent.Content;
                var tradeObj = jsonObj;

                if (!await _authorizationService.AuthorizeAsync(User, CommonPermissions.PublishContent, tradeContent))
                {
                    return Forbid();
                }

                string offerContentItemId = tradeObj.OfferId;
                string traderSellerContentItemId = tradeObj.SellerContentId;

                var traderSeller = await _session.Query<ContentItem, ContentItemIndex>(x => x.Published && x.Latest && x.ContentItemId == traderSellerContentItemId && x.ContentType == "TraderPage").FirstOrDefaultAsync();

                var traderObj = traderSeller.Content["TraderForFilteringPart"];
                decimal currentETHBalance = traderObj.ETHBalance;

                var tradeAmount = tradeObj.TradeETHAmount;
                decimal vxfulFee = tradeObj.FeeETHAmount;

                tradeObj.TradeStatus = "cancel";

                var tradePart = new TradeFilteringPart();

                tradePart.Buyer = tradeObj.Buyer;
                tradePart.BuyerContentId = tradeObj.BuyerContentId;
                tradePart.OfferId = tradeObj.OfferId;
                tradePart.TradeStatus = tradeObj.TradeStatus;
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

                await _contentManager.PublishAsync(tradeContent);

                currentETHBalance += tradeAmount + vxfulFee;

                traderObj.ETHBalance = currentETHBalance;

                var traderPart = new TraderForFilteringPart()
                {
                    Name = traderObj.Name,
                    IsActivatedTele = traderObj.IsActivatedTele,
                    BondVndBalance = traderObj.BondVndBalance,
                    VndBalance = traderObj.VndBalance,
                    BTCBalance = traderObj.BTCBalance,
                    ETHBalance = traderObj.ETHBalance,
                    USDT20Balance = traderObj.USDT20Balance,
                    WithdrawVNDStatus = traderObj.WithdrawVNDStatus,
                    ReferenceCode = traderObj.ReferenceCode,
                    DateSend = traderObj.DateSend,
                    UserId = traderObj.UserId,
                    Email = traderObj.Email,
                    PhoneNumber = traderObj.PhoneNumber,
                    BankAccounts = traderObj.NBankAccountsame,
                    ChatIdTele = traderObj.ChatIdTele,
                    DeviceId = traderObj.DeviceId,
                    MoneyStatus = traderObj.MoneyStatus,
                    Amount = traderObj.Amount,
                    TotalFeeBTC = traderObj.TotalFeeBTC,
                    TotalFeeETH = traderObj.totalFeeETH,
                    TotalFeeUSDT = traderObj.totalTimeUSDT,
                    TotalFeeVND = traderObj.TotalFeeVND,
                    BookmarkOffers = traderObj.BookmarkOffers,
                    DateTime = traderObj.DateTime
                };

                traderSeller.Apply(traderPart);

                await _contentManager.UpdateAsync(traderSeller);

                return Ok(tradeContent);
            }

            return Ok(false);
        }

        [HttpGet]
        [ActionName("MakePaidTrade")]
        public async Task<IActionResult> MakePaidTrade(string tradeId)
        {
            var user = await _userManager.FindByNameAsync(User.Identity.Name) as User;

            if (user == null)
            {
                return Problem(
                        title: S["One or more validation errors occurred."],
                        detail: "User not found",
                        statusCode: (int)HttpStatusCode.BadRequest);
            }

            var tradeContent = await _contentManager.GetAsync(tradeId, VersionOptions.DraftRequired);
            if (tradeContent != null)
            {
                dynamic jsonObj = tradeContent.Content;
                var tradeObj = jsonObj;
                tradeObj.TradeStatus = "paid";

                var tradePart = new TradeFilteringPart();

                tradePart.Buyer = tradeObj.Buyer;
                tradePart.BuyerContentId = tradeObj.BuyerContentId;
                tradePart.OfferId = tradeObj.OfferId;
                tradePart.TradeStatus = tradeObj.TradeStatus;
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

                if (!await _authorizationService.AuthorizeAsync(User, CommonPermissions.PublishContent, tradeContent))
                {
                    return Forbid();
                }

                await _contentManager.PublishAsync(tradeContent);

                return Ok(tradeContent);
            }

            return Ok(false);
        }

        [HttpGet]
        [ActionName("MakePaidTradeVND")]
        public async Task<IActionResult> MakePaidTradeVND(string tradeId)
        {
            var trade = await _contentManager.GetAsync(tradeId);
            if (trade != null)
            {
                dynamic jsonObj = trade.Content;
                var tradeObj = jsonObj["TradePage"];
                tradeObj["Status"]["Text"] = "paid";

                await _contentManager.UpdateAsync(trade);
            }
            return Ok(true);
        }

        [HttpPost]
        [ActionName("CreateTradeETH")]
        public async Task<IActionResult> CreateTradeETH(ContentItem modelOfTrade)
        {
            // Find existing trade                    
            var isExisting = await CheckExistingTrade();
            if (isExisting)
            {
                return Problem(
                        title: S["One or more validation errors occurred."],
                        detail: "Only one trade active at the time , you can not create another trade",
                        statusCode: (int)HttpStatusCode.BadRequest);
            }

            if (!await _authorizationService.AuthorizeAsync(User, CommonPermissions.PublishOwnContent, modelOfTrade))
            {
                return Forbid();
            }

            var tradePart = modelOfTrade.As<TradeFilteringPart>();
            var offerContentItemId = tradePart.OfferId;
            var traderSellerContentItemId = tradePart.SellerContentId;

            var offer = await _session.Query<ContentItem, ContentItemIndex>(x => x.ContentItemId == offerContentItemId && x.ContentType == "OfferPage" && x.Published).FirstOrDefaultAsync();
            var traderSeller = await _session.Query<ContentItem, ContentItemIndex>(x => x.Published && x.Latest && x.ContentItemId == traderSellerContentItemId && x.ContentType == "TraderPage").FirstOrDefaultAsync();

            var offerPart = offer.As<OfferFilteringPart>();
            var paymentMethod = offerPart.PaymentMethod;
            var offerType = offerPart.OfferType;
            var offerWallet = offerPart.Wallet;

            traderSeller.Latest = true;
            var traderPart = traderSeller.As<TraderForFilteringPart>();
            var currentETHBalance = traderPart.ETHBalance;

            var tradeAmount = tradePart.TradeETHAmount;
            decimal vxfulFee = 1;

            // Check fee
            if (paymentMethod == "Free Mode")
            {
                vxfulFee = 1m * tradeAmount / 100;
            }
            else
            {
                vxfulFee = 0.5m * tradeAmount / 100;
            }

            var transactionFee = Decimal.Parse("0.00000");// Internal transaction -> no network fee (transaction fee)

            tradePart.FeeETHAmount = vxfulFee;
            tradePart.TradeStatus = "pending";
            tradePart.PaymentMethod = paymentMethod;

            if (currentETHBalance < (tradeAmount + vxfulFee + transactionFee))
            {
                return Problem(
                        title: S["One or more validation errors occurred."],
                        detail: "Seller is not enough ETH to start a trade , decrease amount to start a trade.",
                        statusCode: (int)HttpStatusCode.BadRequest);
            }

            var contentItem = await _contentManager.GetAsync(modelOfTrade.ContentItemId, VersionOptions.DraftRequired);
            if (contentItem == null)
            {
                var newTradeContentItem = await _contentManager.NewAsync("TradePage");

                tradePart.OfferType = offerType;
                tradePart.OfferWallet = offerWallet;

                newTradeContentItem.Apply(tradePart);
                newTradeContentItem.Merge(modelOfTrade);

                var result = await _contentManager.UpdateValidateAndCreateAsync(newTradeContentItem, VersionOptions.Published);
                if (result.Succeeded)
                {
                    contentItem = newTradeContentItem;

                    // Create user notify
                    _rabbitMQProducer.CreateUserNotify(new CreateNotifyModel()
                    {
                        UserName = tradePart.Seller,
                        Content = $"New trade started {contentItem.ContentItemId}",
                        TradeId = contentItem.ContentItemId
                    });

                    _rabbitMQProducer.CreateUserNotify(new CreateNotifyModel()
                    {
                        UserName = tradePart.Buyer,
                        Content = $"New trade started {contentItem.ContentItemId}",
                        TradeId = contentItem.ContentItemId
                    });

                }
                else
                {
                    return Problem(
                        title: S["One or more validation errors occurred."],
                        detail: String.Join(',', result.Errors),
                        statusCode: (int)HttpStatusCode.BadRequest);
                }
            }
            return Ok(contentItem);
        }

        [HttpPost]
        [ActionName("CreateTradeUSDT20")]
        public async Task<IActionResult> CreateTradeUSDT20(ContentItem modelOfTrade)
        {
            // Find existing trade                    
            var isExisting = await CheckExistingTrade();
            if (isExisting)
            {
                return Problem(
                        title: S["One or more validation errors occurred."],
                        detail: "Only one trade active at the time , you can not create another trade",
                        statusCode: (int)HttpStatusCode.BadRequest);
            }

            if (!await _authorizationService.AuthorizeAsync(User, CommonPermissions.PublishOwnContent, modelOfTrade))
            {
                return Forbid();
            }

            var tradePart = modelOfTrade.As<TradeFilteringPart>();
            var offerContentItemId = tradePart.OfferId;
            var traderSellerContentItemId = tradePart.SellerContentId;

            var offer = await _session.Query<ContentItem, ContentItemIndex>(x => x.ContentItemId == offerContentItemId && x.ContentType == "OfferPage" && x.Published).FirstOrDefaultAsync();
            var traderSeller = await _session.Query<ContentItem, ContentItemIndex>(x => x.Published && x.Latest && x.ContentItemId == traderSellerContentItemId && x.ContentType == "TraderPage").FirstOrDefaultAsync();

            var offerPart = offer.As<OfferFilteringPart>();
            var paymentMethod = offerPart.PaymentMethod;
            var offerType = offerPart.OfferType;
            var offerWallet = offerPart.Wallet;

            traderSeller.Latest = true;
            var traderPart = traderSeller.As<TraderForFilteringPart>();
            var currentUSDT20Balance = traderPart.USDT20Balance;

            var tradeAmount = tradePart.TradeUSDT20Amount;
            decimal vxfulFee = 1;

            // Check fee
            if (paymentMethod == "Free Mode")
            {
                vxfulFee = 1m * tradeAmount / 100;
            }
            else
            {
                vxfulFee = 0.5m * tradeAmount / 100;
            }

            var transactionFee = Decimal.Parse("0.00000");// Internal transaction -> no network fee (transaction fee)

            tradePart.FeeUSDT20Amount = vxfulFee;
            tradePart.TradeStatus = "pending";
            tradePart.PaymentMethod = paymentMethod;

            if (currentUSDT20Balance < (tradeAmount + vxfulFee + transactionFee))
            {
                return Problem(
                        title: S["One or more validation errors occurred."],
                        detail: "Seller is not enough USDT20 to start a trade , decrease amount to start a trade.",
                        statusCode: (int)HttpStatusCode.BadRequest);
            }

            var contentItem = await _contentManager.GetAsync(modelOfTrade.ContentItemId, VersionOptions.DraftRequired);
            if (contentItem == null)
            {
                var newTradeContentItem = await _contentManager.NewAsync("TradePage");

                tradePart.OfferType = offerType;
                tradePart.OfferWallet = offerWallet;

                newTradeContentItem.Apply(tradePart);
                newTradeContentItem.Merge(modelOfTrade);

                var result = await _contentManager.UpdateValidateAndCreateAsync(newTradeContentItem, VersionOptions.Published);
                if (result.Succeeded)
                {
                    contentItem = newTradeContentItem;

                    // Create user notify
                    _rabbitMQProducer.CreateUserNotify(new CreateNotifyModel()
                    {
                        UserName = tradePart.Seller,
                        Content = $"New trade started {contentItem.ContentItemId}",
                        TradeId = contentItem.ContentItemId
                    });

                    _rabbitMQProducer.CreateUserNotify(new CreateNotifyModel()
                    {
                        UserName = tradePart.Buyer,
                        Content = $"New trade started {contentItem.ContentItemId}",
                        TradeId = contentItem.ContentItemId
                    });

                }
                else
                {
                    return Problem(
                        title: S["One or more validation errors occurred."],
                        detail: String.Join(',', result.Errors),
                        statusCode: (int)HttpStatusCode.BadRequest);
                }
            }
            return Ok(contentItem);
        }

        [HttpPost]
        [ActionName("CreateTradeVND")]
        public async Task<IActionResult> CreateTradeVND(ContentItem modelOfTrade)
        {
            // Find existing trade                    
            var isExisting = await CheckExistingTrade();
            if (isExisting)
            {
                return Problem(
                        title: S["One or more validation errors occurred."],
                        detail: "Only one trade active at the time , you can not create another trade",
                        statusCode: (int)HttpStatusCode.BadRequest);
            }

            if (!await _authorizationService.AuthorizeAsync(User, CommonPermissions.PublishOwnContent, modelOfTrade))
            {
                return Forbid();
            }

            var tradePart = modelOfTrade.As<TradeFilteringPart>();
            var offerContentItemId = tradePart.OfferId;
            var traderSellerContentItemId = tradePart.SellerContentId;

            var offer = await _session.Query<ContentItem, ContentItemIndex>(x => x.ContentItemId == offerContentItemId && x.ContentType == "OfferPage" && x.Published).FirstOrDefaultAsync();
            var traderSeller = await _session.Query<ContentItem, ContentItemIndex>(x => x.Published && x.Latest && x.ContentItemId == traderSellerContentItemId && x.ContentType == "TraderPage").FirstOrDefaultAsync();

            var offerPart = offer.As<OfferFilteringPart>();
            var paymentMethod = offerPart.PaymentMethod;
            var offerType = offerPart.OfferType;
            var offerWallet = offerPart.Wallet;

            traderSeller.Latest = true;
            var traderPart = traderSeller.As<TraderForFilteringPart>();
            var currentVNDBalance = traderPart.VndBalance;

            var tradeAmount = tradePart.TradeVNDAmount;
            decimal vxfulFee = 1;

            // Check fee
            if (paymentMethod == "Free Mode")
            {
                vxfulFee = 1m * tradeAmount / 100;
            }
            else
            {
                vxfulFee = 0.5m * tradeAmount / 100;
            }

            var transactionFee = Decimal.Parse("0.00000");// Internal transaction -> no network fee (transaction fee)

            tradePart.FeeVNDAmount = vxfulFee;
            tradePart.TradeStatus = "pending";
            tradePart.PaymentMethod = paymentMethod;

            if (currentVNDBalance < (tradeAmount + vxfulFee + transactionFee))
            {
                return Problem(
                        title: S["One or more validation errors occurred."],
                        detail: "Seller is not enough VND to start a trade , decrease amount to start a trade.",
                        statusCode: (int)HttpStatusCode.BadRequest);
            }

            var contentItem = await _contentManager.GetAsync(modelOfTrade.ContentItemId, VersionOptions.DraftRequired);
            if (contentItem == null)
            {
                var newTradeContentItem = await _contentManager.NewAsync("TradePage");

                tradePart.OfferType = offerType;
                tradePart.OfferWallet = offerWallet;

                newTradeContentItem.Apply(tradePart);
                newTradeContentItem.Merge(modelOfTrade);

                var result = await _contentManager.UpdateValidateAndCreateAsync(newTradeContentItem, VersionOptions.Published);
                if (result.Succeeded)
                {
                    contentItem = newTradeContentItem;

                    // Create user notify
                    _rabbitMQProducer.CreateUserNotify(new CreateNotifyModel()
                    {
                        UserName = tradePart.Seller,
                        Content = $"New trade started {contentItem.ContentItemId}",
                        TradeId = contentItem.ContentItemId
                    });

                    _rabbitMQProducer.CreateUserNotify(new CreateNotifyModel()
                    {
                        UserName = tradePart.Buyer,
                        Content = $"New trade started {contentItem.ContentItemId}",
                        TradeId = contentItem.ContentItemId
                    });

                }
                else
                {
                    return Problem(
                        title: S["One or more validation errors occurred."],
                        detail: String.Join(',', result.Errors),
                        statusCode: (int)HttpStatusCode.BadRequest);
                }
            }
            return Ok(contentItem);
        }

        [HttpPost]
        [ActionName("CreateTradeBTC")]
        public async Task<IActionResult> CreateTradeBTC(ContentItem modelOfTrade)
        {
            // Find existing trade                    
            var isExisting = await CheckExistingTrade();
            if (isExisting)
            {
                return Problem(
                        title: S["One or more validation errors occurred."],
                        detail: "Only one trade active at the time , you can not create another trade",
                        statusCode: (int)HttpStatusCode.BadRequest);
            }

            if (!await _authorizationService.AuthorizeAsync(User, CommonPermissions.PublishOwnContent, modelOfTrade))
            {
                return Forbid();
            }

            var tradePart = modelOfTrade.As<TradeFilteringPart>();
            var offerContentItemId = tradePart.OfferId;
            var traderSellerContentItemId = tradePart.SellerContentId;

            var offer = await _session.Query<ContentItem, ContentItemIndex>(x => x.ContentItemId == offerContentItemId && x.ContentType == "OfferPage" && x.Published).FirstOrDefaultAsync();
            var traderSeller = await _session.Query<ContentItem, ContentItemIndex>(x => x.Published && x.Latest && x.ContentItemId == traderSellerContentItemId && x.ContentType == "TraderPage").FirstOrDefaultAsync();

            var offerPart = offer.As<OfferFilteringPart>();
            var paymentMethod = offerPart.PaymentMethod;
            var offerType = offerPart.OfferType;
            var offerWallet = offerPart.Wallet;

            traderSeller.Latest = true;
            var traderPart = traderSeller.As<TraderForFilteringPart>();
            var currentBTCBalance = traderPart.BTCBalance;

            var tradeAmount = tradePart.TradeBTCAmount;
            decimal vxfulFee = 1;

            // Check fee
            if (paymentMethod == "Free Mode")
            {
                vxfulFee = 1m * tradeAmount / 100;
            }
            else
            {
                vxfulFee = 0.5m * tradeAmount / 100;
            }

            var transactionFee = Decimal.Parse("0.00000");// Internal transaction -> no network fee (transaction fee)

            tradePart.FeeBTCAmount = vxfulFee;
            tradePart.TradeStatus = "pending";
            tradePart.PaymentMethod = paymentMethod;

            if (currentBTCBalance < (tradeAmount + vxfulFee + transactionFee))
            {
                return Problem(
                        title: S["One or more validation errors occurred."],
                        detail: "Seller is not enough BTC to start a trade , decrease amount to start a trade.",
                        statusCode: (int)HttpStatusCode.BadRequest);
            }

            var contentItem = await _contentManager.GetAsync(modelOfTrade.ContentItemId, VersionOptions.DraftRequired);
            if (contentItem == null)
            {
                var newTradeContentItem = await _contentManager.NewAsync("TradePage");

                tradePart.OfferType = offerType;
                tradePart.OfferWallet = offerWallet;

                newTradeContentItem.Apply(tradePart);
                newTradeContentItem.Merge(modelOfTrade);

                var result = await _contentManager.UpdateValidateAndCreateAsync(newTradeContentItem, VersionOptions.Published);
                if (result.Succeeded)
                {
                    //currentBTCBalance -= tradeAmount + vxfulFee + transactionFee;

                    //traderPart.BTCBalance = currentBTCBalance;

                    //traderSeller.Apply(traderPart);

                    //var resultUpdateTrader = await _contentManager.ValidateAsync(traderSeller);

                    //if (resultUpdateTrader.Succeeded)
                    //{
                    //    await _contentManager.UpdateAsync(traderSeller);
                    //}

                    contentItem = newTradeContentItem;

                    //dynamic jsonNewObj = newTradeContentItem.Content;
                    //var tradeInfo = jsonNewObj;
                    //string seller = tradeInfo.Seller;
                    //string buyer = tradeInfo.Buyer;

                    // Create user notify
                    _rabbitMQProducer.CreateUserNotify(new CreateNotifyModel()
                    {
                        UserName = tradePart.Seller,
                        Content = $"New trade started {contentItem.ContentItemId}",
                        TradeId = contentItem.ContentItemId
                    });

                    _rabbitMQProducer.CreateUserNotify(new CreateNotifyModel()
                    {
                        UserName = tradePart.Buyer,
                        Content = $"New trade started {contentItem.ContentItemId}",
                        TradeId = contentItem.ContentItemId
                    });

                }
                else
                {
                    return Problem(
                        title: S["One or more validation errors occurred."],
                        detail: String.Join(',', result.Errors),
                        statusCode: (int)HttpStatusCode.BadRequest);
                }
            }
            return Ok(contentItem);
        }

        [HttpGet]
        [ActionName("GetTradeHome")]
        public async Task<IActionResult> GetTradeHome()
        {
            try
            {
                using var session = _session.Store.CreateSession();

                var trades = await _session
                .Query<ContentItem, ContentItemIndex>(index => index.ContentType == "TradePage")
                .OrderByDescending(o => o.CreatedUtc)
                .Take(50)
                .With<TradeFilteringPartIndex>(p => p.Status == "completed")
                .ListAsync();

                List<TradeFilteringPartViewModel> listOfTrade = JsonConvert.DeserializeObject<List<TradeFilteringPartViewModel>>(trades.ToString());

                return Ok(listOfTrade);
            }
            catch (Exception ex)
            {
                ex.ToString();
                return Problem(
                    title: S["One or more validation errors occurred."],
                    detail: "BTC userName is not found.",
                    statusCode: (int)HttpStatusCode.BadRequest);
            }
        }

        private async Task<bool> CheckExistingTrade()
        {
            var user = await _userManager.FindByNameAsync(User.Identity.Name) as User;

            var trader = await _session
                .Query<ContentItem, ContentItemIndex>(index => index.ContentType == "TraderPage" && index.Published && index.Latest)
                .With<TraderForFilteringPartIndex>(p => p.UserId == user.Id)
                .FirstOrDefaultAsync();

            string name = trader.Content["TraderForFilteringPart"].Name;

            using var session = _session.Store.CreateSession();

            var trades = await _session
            .Query<ContentItem, ContentItemIndex>(x => x.ContentType == "TradePage" && x.Published && (x.Author == User.Identity.Name || x.Owner == User.Identity.Name))
            .With<TradeFilteringPartIndex>(p => p.Status == "pending" && (p.Buyer == name || p.Seller == name))
            .ListAsync();

            if (trades.Count() > 0)
            {
                return true;
            }

            return false;

        }
    }
}
