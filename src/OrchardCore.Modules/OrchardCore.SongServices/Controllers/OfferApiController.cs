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
using Permissions = OrchardCore.SongServices.Permissions;
using CommonPermissions = OrchardCore.Contents.CommonPermissions;
using Microsoft.AspNetCore.Cors;
using YesSql;
using YesSql.Services;
using OrchardCore.ContentManagement.Records;
using OrchardCore.Users.Services;
using System.Collections.Generic;
using OrchardCore.SongServices.Models;
using OrchardCore.SongServices.Controllers;
using OrchardCore.SongServices.ApiModels;
using OrchardCore.Flows.Models;
using OrchardCore.SongServices.ContentParts;
using OrchardCore.SongServices.Indexing;
using Microsoft.AspNetCore.Mvc.Filters;

namespace OrchardCore.Content.Controllers
{
    [Route("api/content/[action]")]
    [ApiController]
    [Authorize(AuthenticationSchemes = "Api"), IgnoreAntiforgeryToken, AllowAnonymous]
    public class OfferApiController : Controller
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

        public OfferApiController(
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


        [HttpPost]
        [ActionName("RegisterOffer")]
        public async Task<IActionResult> RegisterOffer(RegisterOfferModel model)
        {
            if (!await _authorizationService.AuthorizeAsync(User, Permissions.AccessApiPermissions.AccessContentApi))
            {
                return this.ChallengeOrForbid();
            }

            var contentItem = await _contentManager.GetAsync(model.Offer.ContentItemId, VersionOptions.DraftRequired);

            if (contentItem == null)
            {
                if (!await _authorizationService.AuthorizeAsync(User, CommonPermissions.PublishOwnContent))
                {
                    return this.ChallengeOrForbid();
                }

                var newContentItem = await _contentManager.NewAsync(model.Offer.ContentType);
                newContentItem.Merge(model.Offer);

                var result = await _contentManager.UpdateValidateAndCreateAsync(newContentItem, VersionOptions.Published);
                if (result.Succeeded)
                {
                    contentItem = newContentItem;
                }
                else
                {
                    return Problem(
                        title: S["One or more validation errors occurred."],
                        detail: string.Join(',', result.Errors),
                        statusCode: (int)HttpStatusCode.BadRequest);
                }
            }
            else
            {
                if (!await _authorizationService.AuthorizeAsync(User, CommonPermissions.EditOwnContent, contentItem))
                {
                    return this.ChallengeOrForbid();
                }

                contentItem.Merge(model, UpdateJsonMergeSettings);

                await _contentManager.UpdateAsync(contentItem);
                var result = await _contentManager.ValidateAsync(contentItem);

                if (result.Succeeded)
                {
                    await _contentManager.PublishAsync(contentItem);
                }
                else
                {
                    return Problem(
                        title: S["One or more validation errors occurred."],
                        detail: string.Join(',', result.Errors),
                        statusCode: (int)HttpStatusCode.BadRequest);
                }
            }

            return Ok(contentItem);
        }

        [HttpPost]
        [ActionName("UpdateOffer")]
        public async Task<IActionResult> UpdateOffer(RegisterOfferModel model, bool draft = false)
        {
            if (!await _authorizationService.AuthorizeAsync(User, Permissions.AccessApiPermissions.AccessContentApi))
            {
                return this.ChallengeOrForbid();
            }

            // It is really important to keep the proper method calls order with the ContentManager
            // so that all event handlers gets triggered in the right sequence.

            var contentItem = await _contentManager.GetAsync(model.Offer.ContentItemId);

            if (contentItem == null)
            {
                return this.ChallengeOrForbid();
            }
            else
            {
                if (!await _authorizationService.AuthorizeAsync(User, CommonPermissions.EditOwnContent, contentItem))
                {
                    return this.ChallengeOrForbid();
                }

                //model.DisplayText = contentItem.DisplayText;
                model.Offer.Owner = contentItem.Owner;
                model.Offer.Author = contentItem.Author;

                contentItem.Merge(model.Offer);

                await _contentManager.UpdateAsync(contentItem);
                var result = await _contentManager.ValidateAsync(contentItem);

                if (result.Succeeded)
                {
                    if (!draft)
                    {
                        await _contentManager.PublishAsync(contentItem);
                    }
                }
                else
                {
                    return Problem(
                        title: S["One or more validation errors occurred."],
                        detail: string.Join(',', result.Errors),
                        statusCode: (int)HttpStatusCode.BadRequest);
                }
            }

            return Ok(contentItem);
        }

        [HttpPost]
        [ActionName("GetOfferConsignment")]
        public async Task<IActionResult> GetOfferConsignment(SearchingOfferModel offerModel)
        {
            if (!await _authorizationService.AuthorizeAsync(User, Permissions.AccessApiPermissions.AccessContentApi))
            {
                return this.ChallengeOrForbid();
            }

            IEnumerable<ContentItem> offers = null;

            if (offerModel.Method.Equals("all", StringComparison.OrdinalIgnoreCase) && offerModel.Currency.Equals("all", StringComparison.OrdinalIgnoreCase))
            {
                offers = await _session
                .Query<ContentItem, ContentItemIndex>(index => index.ContentType == "OfferPage" && index.Published && index.Latest && index.Author != User.Identity.Name).OrderByDescending(index => index.CreatedUtc)
                .With<OfferFilteringPartIndex>(p => p.Wallet == offerModel.Wallet
                                                        && p.OfferType == "consign"
                                                        && p.Status != "disabled")
                .Take(offerModel.Take)
                .ListAsync();
            }
            else
            {
                if (offerModel.MinAmount == 0)
                {
                    offers = await _session
                    .Query<ContentItem, ContentItemIndex>(index => index.ContentType == "OfferPage" && index.Published && index.Latest && index.Author != User.Identity.Name).OrderByDescending(index => index.CreatedUtc)
                    .With<OfferFilteringPartIndex>(p => p.PaymentMethod == offerModel.Method
                                                            && p.Wallet == offerModel.Wallet
                                                            && p.PreferredCurrency == offerModel.Currency
                                                            && p.OfferType == "consign"
                                                            && p.Status != "disabled")

                    .Take(offerModel.Take)
                    .ListAsync();
                }
                else
                {
                    offers = await _session
                    .Query<ContentItem, ContentItemIndex>(index => index.ContentType == "OfferPage" && index.Published && index.Latest && index.Author != User.Identity.Name).OrderByDescending(index => index.CreatedUtc)
                    .With<OfferFilteringPartIndex>(p => p.MinAmount >= offerModel.MinAmount
                                                            && p.MaxAmount <= offerModel.MaxAmount
                                                            && p.PaymentMethod == offerModel.Method
                                                            && p.Wallet == offerModel.Wallet
                                                            && p.PreferredCurrency == offerModel.Currency
                                                            && p.OfferType == "consign"
                                                            && p.Status != "disabled")

                    .Take(offerModel.Take)
                    .ListAsync();
                }
            }

            var listOffers = new List<OfferHomeReturnModel>();

            foreach (var item in offers)
            {
                var offerPart = item.As<OfferFilteringPart>();

                var trader = await _session
                .Query<ContentItem, ContentItemIndex>(index => index.ContentType == "TraderPage" && index.Published && index.Latest).OrderByDescending(index => index.CreatedUtc)
                .With<TraderForFilteringPartIndex>(p => p.Name == item.Owner)
                .FirstOrDefaultAsync();

                if (trader != null)
                {
                    var traderPart = trader.As<TraderForFilteringPart>();

                    var bondVndBalance = traderPart.BondVndBalance;

                    var offer = new OfferHomeReturnModel()
                    {
                        ContentItemId = item.ContentItemId,
                        CreatedUtc = item.CreatedUtc.Value.ToString(),
                        MaxAmount = offerPart.MaxAmount,
                        MinAmount = offerPart.MinAmount,
                        OfferLabel = offerPart.OfferLabel,
                        OfferTerms = offerPart.OfferTerms,
                        OfferPrice = offerPart.OfferPrice,
                        OfferGet = offerPart.OfferGet,
                        OfferType = offerPart.OfferType,
                        Owner = item.Owner,
                        PaymentMethod = offerPart.PaymentMethod,
                        Percentage = offerPart.Percentage,
                        PreferredCurrency = offerPart.PreferredCurrency,
                        Status = offerPart.OfferStatus,
                        TradeInstructions = offerPart.TradeInstructions,
                        Wallet = offerPart.Wallet,
                        BondVndBalance = bondVndBalance,
                    };

                    listOffers.Add(offer);
                }
            }

            return Ok(listOffers);
        }

        [HttpPost]
        [ActionName("GetOfferFiltering")]
        public async Task<IActionResult> GetOfferFiltering(SearchingOfferModel offerModel)
        {
            if (!await _authorizationService.AuthorizeAsync(User, Permissions.AccessApiPermissions.AccessContentApi))
            {
                return this.ChallengeOrForbid();
            }

            IEnumerable<ContentItem> offers = null;

            if (offerModel.Method.Equals("all", StringComparison.OrdinalIgnoreCase) && offerModel.Currency.Equals("all", StringComparison.OrdinalIgnoreCase))
            {
                offers = await _session
                .Query<ContentItem, ContentItemIndex>(index => index.ContentType == "OfferPage" && index.Published && index.Latest && index.Author != User.Identity.Name).OrderByDescending(index => index.CreatedUtc)
                .With<OfferFilteringPartIndex>(p => p.Wallet == offerModel.Wallet
                                                        && p.OfferType == offerModel.OfferType
                                                        && p.Status != "disabled")
                .Take(offerModel.Take)
                .ListAsync();
            }
            else
            {
                if (offerModel.MinAmount == 0)
                {
                    offers = await _session
                    .Query<ContentItem, ContentItemIndex>(index => index.ContentType == "OfferPage" && index.Published && index.Latest && index.Author != User.Identity.Name).OrderByDescending(index => index.CreatedUtc)
                    .With<OfferFilteringPartIndex>(p => p.PaymentMethod == offerModel.Method
                                                            && p.Wallet == offerModel.Wallet
                                                            && p.PreferredCurrency == offerModel.Currency
                                                            && p.OfferType == offerModel.OfferType
                                                            && p.Status != "disabled")

                    .Take(offerModel.Take)
                    .ListAsync();
                }
                else
                {
                    offers = await _session
                    .Query<ContentItem, ContentItemIndex>(index => index.ContentType == "OfferPage" && index.Published && index.Latest && index.Author != User.Identity.Name).OrderByDescending(index => index.CreatedUtc)
                    .With<OfferFilteringPartIndex>(p => p.MinAmount <= offerModel.MinAmount
                                                            && p.MaxAmount >= offerModel.MinAmount
                                                            && p.PaymentMethod == offerModel.Method
                                                            && p.Wallet == offerModel.Wallet
                                                            && p.PreferredCurrency == offerModel.Currency
                                                            && p.OfferType == offerModel.OfferType
                                                            && p.Status != "disabled")

                    .Take(offerModel.Take)
                    .ListAsync();
                }
            }

            var listOffers = new List<OfferHomeReturnModel>();
            if (offers != null)
            {
                foreach (var item in offers)
                {
                    var offerPart = item.As<OfferFilteringPart>();

                    var trader = await _session
                    .Query<ContentItem, ContentItemIndex>(index => index.ContentType == "TraderPage" && index.Published && index.Latest).OrderByDescending(index => index.CreatedUtc)
                    .With<TraderForFilteringPartIndex>(p => p.Name == item.Owner)
                    .FirstOrDefaultAsync();

                    if (trader != null)
                    {
                        var traderPart = trader.As<TraderForFilteringPart>();

                        var bondVndBalance = traderPart.BondVndBalance;

                        var offer = new OfferHomeReturnModel()
                        {
                            ContentItemId = item.ContentItemId,
                            CreatedUtc = item.CreatedUtc.Value.ToString(),
                            MaxAmount = offerPart.MaxAmount,
                            MinAmount = offerPart.MinAmount,
                            OfferLabel = offerPart.OfferLabel,
                            OfferTerms = offerPart.OfferTerms,
                            OfferPrice = offerPart.OfferPrice,
                            OfferGet = offerPart.OfferGet,
                            OfferType = offerPart.OfferType,
                            Owner = item.Owner,
                            PaymentMethod = offerPart.PaymentMethod,
                            Percentage = offerPart.Percentage,
                            PreferredCurrency = offerPart.PreferredCurrency,
                            Status = offerPart.OfferStatus,
                            TradeInstructions = offerPart.TradeInstructions,
                            Wallet = offerPart.Wallet,
                            BondVndBalance = bondVndBalance,
                        };

                        listOffers.Add(offer);
                    }
                }
            }

            return Ok(listOffers);
        }

        [HttpPost]
        [ActionName("GetOfferConsignmentHome")]
        [AllowAnonymous]
        public async Task<IActionResult> GetOfferConsignmentHome(SearchingOfferModel offerModel)
        {
            IEnumerable<ContentItem> offers = null;

            if (offerModel.Wallet.Equals("all", StringComparison.OrdinalIgnoreCase)
                && offerModel.Currency.Equals("all", StringComparison.OrdinalIgnoreCase)
                && offerModel.Method.Equals("all", StringComparison.OrdinalIgnoreCase)
                && offerModel.OfferType.Equals("all", StringComparison.OrdinalIgnoreCase))
            {
                offers = await _session
                .Query<ContentItem, ContentItemIndex>(index => index.ContentType == "OfferPage" && index.Published && index.Latest).OrderByDescending(index => index.CreatedUtc)
                .With<OfferFilteringPartIndex>(p => p.OfferType == "consign" && p.Status != "disabled")
                .Take(offerModel.Take)
                .ListAsync();
            }
            else
            {
                offers = await _session
                .Query<ContentItem, ContentItemIndex>(index => index.ContentType == "OfferPage" && index.Published && index.Latest).OrderByDescending(index => index.CreatedUtc)
                .With<OfferFilteringPartIndex>(p => p.OfferType == "consign" && p.Status != "disabled" && p.Wallet == offerModel.Wallet)
                .Take(offerModel.Take)
                .ListAsync();
            }

            var listOffers = new List<OfferHomeReturnModel>();

            foreach (var item in offers)
            {
                var offerPart = item.As<OfferFilteringPart>();

                var trader = await _session
                .Query<ContentItem, ContentItemIndex>(index => index.ContentType == "TraderPage" && index.Published && index.Latest).OrderByDescending(index => index.CreatedUtc)
                .With<TraderForFilteringPartIndex>(p => p.Name == item.Owner)
                .FirstOrDefaultAsync();

                if (trader != null)
                {
                    var traderPart = trader.As<TraderForFilteringPart>();

                    var bondVndBalance = traderPart.BondVndBalance;

                    var offer = new OfferHomeReturnModel()
                    {
                        ContentItemId = item.ContentItemId,
                        CreatedUtc = item.CreatedUtc.Value.ToString(),
                        MaxAmount = offerPart.MaxAmount,
                        MinAmount = offerPart.MinAmount,
                        OfferLabel = offerPart.OfferLabel,
                        OfferTerms = offerPart.OfferTerms,
                        OfferPrice = offerPart.OfferPrice,
                        OfferGet = offerPart.OfferGet,
                        OfferType = offerPart.OfferType,
                        Owner = item.Owner,
                        PaymentMethod = offerPart.PaymentMethod,
                        Percentage = offerPart.Percentage,
                        PreferredCurrency = offerPart.PreferredCurrency,
                        Status = offerPart.OfferStatus,
                        TradeInstructions = offerPart.TradeInstructions,
                        Wallet = offerPart.Wallet,
                        BondVndBalance = bondVndBalance,
                    };

                    listOffers.Add(offer);
                }
            }

            return Ok(listOffers);
        }

        [HttpPost]
        [ActionName("GetOfferConsignmentHomeWithUser")]
        [AllowAnonymous]
        public async Task<IActionResult> GetOfferConsignmentHomeWithUser(SearchingOfferModel offerModel)
        {
            var offers = await _session
                .Query<ContentItem, ContentItemIndex>(index => index.ContentType == "OfferPage" && index.Published && index.Latest && index.Author != User.Identity.Name).OrderByDescending(index => index.CreatedUtc)
                .With<OfferFilteringPartIndex>(p => p.OfferType == "consign" && p.Status != "disabled")
                .Take(offerModel.Take)
                .ListAsync();

            var listOffers = new List<OfferHomeReturnModel>();

            foreach (var item in offers)
            {
                var offerPart = item.As<OfferFilteringPart>();

                var trader = await _session
                .Query<ContentItem, ContentItemIndex>(index => index.ContentType == "TraderPage" && index.Published && index.Latest).OrderByDescending(index => index.CreatedUtc)
                .With<TraderForFilteringPartIndex>(p => p.Name == item.Owner)
                .FirstOrDefaultAsync();

                if (trader != null)
                {
                    var traderPart = trader.As<TraderForFilteringPart>();

                    var bondVndBalance = traderPart.BondVndBalance;

                    var offer = new OfferHomeReturnModel()
                    {
                        ContentItemId = item.ContentItemId,
                        CreatedUtc = item.CreatedUtc.Value.ToString(),
                        MaxAmount = offerPart.MaxAmount,
                        MinAmount = offerPart.MinAmount,
                        OfferLabel = offerPart.OfferLabel,
                        OfferTerms = offerPart.OfferTerms,
                        OfferPrice = offerPart.OfferPrice,
                        OfferGet = offerPart.OfferGet,
                        OfferType = offerPart.OfferType,
                        Owner = item.Owner,
                        PaymentMethod = offerPart.PaymentMethod,
                        Percentage = offerPart.Percentage,
                        PreferredCurrency = offerPart.PreferredCurrency,
                        Status = offerPart.OfferStatus,
                        TradeInstructions = offerPart.TradeInstructions,
                        Wallet = offerPart.Wallet,
                        BondVndBalance = bondVndBalance,
                    };

                    listOffers.Add(offer);
                }
            }

            return Ok(listOffers);
        }

        [HttpPost]
        [ActionName("GetOfferDashBoard")]
        public async Task<IActionResult> GetOfferDashBoard(SearchingOfferModel offerModel)
        {
            var offers = await _session
                .Query<ContentItem, ContentItemIndex>(index => index.ContentType == "OfferPage" && index.Published && index.Latest && index.Owner == User.Identity.Name).OrderByDescending(index => index.CreatedUtc)
                .With<OfferFilteringPartIndex>(p => p.Wallet == offerModel.Wallet)
                .ListAsync();

            var listOffers = new List<OfferHomeReturnModel>();

            foreach (var item in offers)
            {
                var offerPart = item.As<OfferFilteringPart>();

                var trader = await _session
                .Query<ContentItem, ContentItemIndex>(index => index.ContentType == "TraderPage" && index.Published && index.Latest).OrderByDescending(index => index.CreatedUtc)
                .With<TraderForFilteringPartIndex>(p => p.Name == item.Owner)
                .FirstOrDefaultAsync();

                if (trader != null)
                {
                    var traderPart = trader.As<TraderForFilteringPart>();

                    var bondVndBalance = traderPart.BondVndBalance;

                    var offer = new OfferHomeReturnModel()
                    {
                        ContentItemId = item.ContentItemId,
                        CreatedUtc = item.CreatedUtc.Value.ToString(),
                        MaxAmount = offerPart.MaxAmount,
                        MinAmount = offerPart.MinAmount,
                        OfferLabel = offerPart.OfferLabel,
                        OfferTerms = offerPart.OfferTerms,
                        OfferPrice = offerPart.OfferPrice,
                        OfferGet = offerPart.OfferGet,
                        OfferType = offerPart.OfferType,
                        Owner = item.Owner,
                        PaymentMethod = offerPart.PaymentMethod,
                        Percentage = offerPart.Percentage,
                        PreferredCurrency = offerPart.PreferredCurrency,
                        Status = offerPart.OfferStatus,
                        TradeInstructions = offerPart.TradeInstructions,
                        Wallet = offerPart.Wallet,
                        BondVndBalance = bondVndBalance,
                    };

                    listOffers.Add(offer);
                }
            }

            return Ok(listOffers);
        }

        [HttpPost]
        [ActionName("GetOfferConsignmentDashBoard")]
        public async Task<IActionResult> GetOfferConsignmentDashBoard(SearchingOfferModel offerModel)
        {
            var offers = await _session
                .Query<ContentItem, ContentItemIndex>(index => index.ContentType == "OfferPage" && index.Published && index.Latest && index.Author == User.Identity.Name).OrderByDescending(index => index.CreatedUtc)
                .With<OfferFilteringPartIndex>(p => p.OfferType == "consign" && p.Wallet == offerModel.Wallet)
                .ListAsync();

            var listOffers = new List<OfferHomeReturnModel>();

            foreach (var item in offers)
            {
                var offerPart = item.As<OfferFilteringPart>();

                var trader = await _session
                .Query<ContentItem, ContentItemIndex>(index => index.ContentType == "TraderPage" && index.Published && index.Latest).OrderByDescending(index => index.CreatedUtc)
                .With<TraderForFilteringPartIndex>(p => p.Name == item.Owner)
                .FirstOrDefaultAsync();

                if (trader != null)
                {
                    var traderPart = trader.As<TraderForFilteringPart>();

                    var bondVndBalance = traderPart.BondVndBalance;

                    var offer = new OfferHomeReturnModel()
                    {
                        ContentItemId = item.ContentItemId,
                        CreatedUtc = item.CreatedUtc.Value.ToString(),
                        MaxAmount = offerPart.MaxAmount,
                        MinAmount = offerPart.MinAmount,
                        OfferLabel = offerPart.OfferLabel,
                        OfferTerms = offerPart.OfferTerms,
                        OfferPrice = offerPart.OfferPrice,
                        OfferGet = offerPart.OfferGet,
                        OfferType = offerPart.OfferType,
                        Owner = item.Owner,
                        PaymentMethod = offerPart.PaymentMethod,
                        Percentage = offerPart.Percentage,
                        PreferredCurrency = offerPart.PreferredCurrency,
                        Status = offerPart.OfferStatus,
                        TradeInstructions = offerPart.TradeInstructions,
                        Wallet = offerPart.Wallet,
                        BondVndBalance = bondVndBalance,
                    };

                    listOffers.Add(offer);
                }
            }

            return Ok(listOffers);
        }

        [HttpPost]
        [ActionName("GetOfferFilteringHome")]
        [AllowAnonymous]
        public async Task<IActionResult> GetOfferFilteringHome(SearchingOfferModel offerModel)
        {
            IEnumerable<ContentItem> offers = null;

            if (offerModel.MinAmount == 0)
            {
                offers = await _session
                .Query<ContentItem, ContentItemIndex>(index => index.ContentType == "OfferPage" && index.Published && index.Latest).OrderByDescending(index => index.CreatedUtc)
                .With<OfferFilteringPartIndex>(p => p.PaymentMethod == offerModel.Method
                                                        && p.Wallet == offerModel.Wallet
                                                        && p.PreferredCurrency == offerModel.Currency
                                                        && p.OfferType == offerModel.OfferType
                                                        && p.Status != "disabled")

                .Take(offerModel.Take)
                .ListAsync();
            }
            else
            {
                offers = await _session
                .Query<ContentItem, ContentItemIndex>(index => index.ContentType == "OfferPage" && index.Published && index.Latest).OrderByDescending(index => index.CreatedUtc)
                .With<OfferFilteringPartIndex>(p => p.MinAmount <= offerModel.MinAmount
                                                        && p.MaxAmount >= offerModel.MinAmount
                                                        && p.PaymentMethod == offerModel.Method
                                                        && p.Wallet == offerModel.Wallet
                                                        && p.PreferredCurrency == offerModel.Currency
                                                        && p.OfferType == offerModel.OfferType
                                                        && p.Status != "disabled")

                .Take(offerModel.Take)
                .ListAsync();
            }

            var listOffers = new List<OfferHomeReturnModel>();

            foreach (var item in offers)
            {
                var offerPart = item.As<OfferFilteringPart>();

                var trader = await _session
                .Query<ContentItem, ContentItemIndex>(index => index.ContentType == "TraderPage" && index.Published && index.Latest).OrderByDescending(index => index.CreatedUtc)
                .With<TraderForFilteringPartIndex>(p => p.Name == item.Owner)
                .FirstOrDefaultAsync();

                if (trader != null)
                {
                    var traderPart = trader.As<TraderForFilteringPart>();

                    var bondVndBalance = traderPart.BondVndBalance;

                    var offer = new OfferHomeReturnModel()
                    {
                        ContentItemId = item.ContentItemId,
                        CreatedUtc = item.CreatedUtc.Value.ToString(),
                        MaxAmount = offerPart.MaxAmount,
                        MinAmount = offerPart.MinAmount,
                        OfferLabel = offerPart.OfferLabel,
                        OfferTerms = offerPart.OfferTerms,
                        OfferPrice = offerPart.OfferPrice,
                        OfferGet = offerPart.OfferGet,
                        OfferType = offerPart.OfferType,
                        Owner = item.Owner,
                        PaymentMethod = offerPart.PaymentMethod,
                        Percentage = offerPart.Percentage,
                        PreferredCurrency = offerPart.PreferredCurrency,
                        Status = offerPart.OfferStatus,
                        TradeInstructions = offerPart.TradeInstructions,
                        Wallet = offerPart.Wallet,
                        BondVndBalance = bondVndBalance,
                    };

                    listOffers.Add(offer);
                }
            }

            return Ok(listOffers);
        }

        [HttpPost]
        [ActionName("GetOfferByPartHome")]
        [AllowAnonymous]
        public async Task<IActionResult> GetOfferByPartHome(SearchingOfferModel offerModel)
        {
            var offerHome = await _session
                .Query<ContentItem, ContentItemIndex>(index => index.ContentType == "OfferPage" && index.Published && index.Latest).OrderByDescending(index => index.CreatedUtc)
                .With<OfferFilteringPartIndex>(p => p.OfferType != "consign" && p.Status != "disabled")
                .Take(offerModel.Take)
                .ListAsync();

            var listOffers = new List<OfferHomeReturnModel>();

            foreach (var item in offerHome)
            {
                var offerPart = item.As<OfferFilteringPart>();

                var trader = await _session
                .Query<ContentItem, ContentItemIndex>(index => index.ContentType == "TraderPage" && index.Published && index.Latest).OrderByDescending(index => index.CreatedUtc)
                .With<TraderForFilteringPartIndex>(p => p.Name == item.Owner)
                .FirstOrDefaultAsync();

                if (trader != null)
                {
                    var traderPart = trader.As<TraderForFilteringPart>();

                    var bondVndBalance = traderPart.BondVndBalance;

                    var offer = new OfferHomeReturnModel()
                    {
                        ContentItemId = item.ContentItemId,
                        CreatedUtc = item.CreatedUtc.Value.ToString(),
                        MaxAmount = offerPart.MaxAmount,
                        MinAmount = offerPart.MinAmount,
                        OfferLabel = offerPart.OfferLabel,
                        OfferTerms = offerPart.OfferTerms,
                        OfferPrice = offerPart.OfferPrice,
                        OfferGet = offerPart.OfferGet,
                        OfferType = offerPart.OfferType,
                        Owner = item.Owner,
                        PaymentMethod = offerPart.PaymentMethod,
                        Percentage = offerPart.Percentage,
                        PreferredCurrency = offerPart.PreferredCurrency,
                        Status = offerPart.OfferStatus,
                        TradeInstructions = offerPart.TradeInstructions,
                        Wallet = offerPart.Wallet,
                        BondVndBalance = bondVndBalance,
                    };

                    listOffers.Add(offer);
                }
            }

            return Ok(listOffers);
        }

        [HttpPost]
        [ActionName("GetOfferByPartHomeWithUser")]
        public async Task<IActionResult> GetOfferByPartHomeWithUser(SearchingOfferModel offerModel)
        {
            var offerHome = await _session
                .Query<ContentItem, ContentItemIndex>(index => index.ContentType == "OfferPage" && index.Published && index.Latest && index.Author != User.Identity.Name).OrderByDescending(index => index.CreatedUtc)
                .With<OfferFilteringPartIndex>(p => p.OfferType != "consign" && p.Status != "disabled")
                .Take(offerModel.Take)
                .ListAsync();

            var listOffers = new List<OfferHomeReturnModel>();

            foreach (var item in offerHome)
            {
                var offerPart = item.As<OfferFilteringPart>();

                var trader = await _session
                .Query<ContentItem, ContentItemIndex>(index => index.ContentType == "TraderPage" && index.Published && index.Latest).OrderByDescending(index => index.CreatedUtc)
                .With<TraderForFilteringPartIndex>(p => p.Name == item.Owner)
                .FirstOrDefaultAsync();

                if (trader != null)
                {
                    var traderPart = trader.As<TraderForFilteringPart>();

                    var bondVndBalance = traderPart.BondVndBalance;

                    var offer = new OfferHomeReturnModel()
                    {
                        ContentItemId = item.ContentItemId,
                        CreatedUtc = item.CreatedUtc.Value.ToString(),
                        MaxAmount = offerPart.MaxAmount,
                        MinAmount = offerPart.MinAmount,
                        OfferLabel = offerPart.OfferLabel,
                        OfferTerms = offerPart.OfferTerms,
                        OfferPrice = offerPart.OfferPrice,
                        OfferGet = offerPart.OfferGet,
                        OfferType = offerPart.OfferType,
                        Owner = item.Owner,
                        PaymentMethod = offerPart.PaymentMethod,
                        Percentage = offerPart.Percentage,
                        PreferredCurrency = offerPart.PreferredCurrency,
                        Status = offerPart.OfferStatus,
                        TradeInstructions = offerPart.TradeInstructions,
                        Wallet = offerPart.Wallet,
                        BondVndBalance = bondVndBalance,
                    };

                    listOffers.Add(offer);
                }
            }

            return Ok(listOffers);
        }

        [HttpGet]
        [ActionName("GetOfferDetail")]
        [AllowAnonymous]
        public async Task<IActionResult> GetOfferDetail(string wallet, string owner, string currency)
        {

            var offerMinMax1 = await _session
                .Query<ContentItem, ContentItemIndex>(index => index.ContentType == "OfferPage" && index.Published && index.Latest)
                //.With<OfferFilteringPartIndex>(p => p.MinAmount > 4000000m && p.MaxAmount <= 12000000m)
                .ListAsync();

            var offerMinMax2 = await _session
                .Query<ContentItem, ContentItemIndex>(index => index.ContentType == "OfferPage" && index.Published && index.Latest)
                .With<OfferFilteringPartIndex>(p => p.MinAmount > 0 && p.MaxAmount <= 10000000)
                .ListAsync();

            var offerMinMax3 = await _session
                .Query<ContentItem, ContentItemIndex>(index => index.ContentType == "OfferPage" && index.Published && index.Latest)
                .With<OfferFilteringPartIndex>(p => p.MinAmount >= 0m && p.MaxAmount <= 10000000)
                .ListAsync();

            return Ok(offerMinMax1);
        }

        [HttpGet]
        [ActionName("GetOfferById")]
        public async Task<IActionResult> GetOfferById(string id)
        {
            var offer = await _contentManager.GetAsync(id);
            var offerModel = new OfferHomeReturnModel();
            var offerPart = offer.As<OfferFilteringPart>();

            var trader = await _session
                .Query<ContentItem, ContentItemIndex>(index => index.ContentType == "TraderPage" && index.Published && index.Latest).OrderByDescending(index => index.CreatedUtc)
                .With<TraderForFilteringPartIndex>(p => p.Name == offer.Owner)
                .FirstOrDefaultAsync();

            if (trader != null)
            {
                var traderPart = trader.As<TraderForFilteringPart>();

                var bondVndBalance = traderPart.BondVndBalance;

                offerModel = new OfferHomeReturnModel()
                {
                    ContentItemId = offer.ContentItemId,
                    CreatedUtc = offer.CreatedUtc.Value.ToString(),
                    MaxAmount = offerPart.MaxAmount,
                    MinAmount = offerPart.MinAmount,
                    OfferLabel = offerPart.OfferLabel,
                    OfferTerms = offerPart.OfferTerms,
                    OfferPrice = offerPart.OfferPrice,
                    OfferGet = offerPart.OfferGet,
                    OfferType = offerPart.OfferType,
                    Owner = offer.Owner,
                    PaymentMethod = offerPart.PaymentMethod,
                    Percentage = offerPart.Percentage,
                    PreferredCurrency = offerPart.PreferredCurrency,
                    Status = offerPart.OfferStatus,
                    TradeInstructions = offerPart.TradeInstructions,
                    Wallet = offerPart.Wallet,
                    BondVndBalance = bondVndBalance,
                };
            }



            return Ok(offerModel);
        }

        [HttpPost]
        [ActionName("UpdateOfferVisible")]
        public async Task<IActionResult> UpdateOfferVisible(OfferStatusModel offerStatusModel)
        {
            if (!await _authorizationService.AuthorizeAsync(User, Permissions.AccessApiPermissions.AccessContentApi))
            {
                return this.ChallengeOrForbid();
            }

            var query = _session.Query<ContentItem>();

            query.With<ContentItemIndex>(x => x.Published && x.Latest && x.ContentItemId == offerStatusModel.OfferItemId && x.ContentType == "OfferPage");

            var contentItemOffer = await query.FirstOrDefaultAsync();

            if (contentItemOffer == null)
            {
                return this.ChallengeOrForbid();
            }
            else
            {
                var offerPart = contentItemOffer.As<OfferFilteringPart>();

                offerPart.OfferStatus = offerStatusModel.IsDisabled == true ? "disabled" : offerStatusModel.Status;

                contentItemOffer.Apply(offerPart);

                contentItemOffer.Latest = true;
                await _contentManager.UpdateAsync(contentItemOffer);

                var result = await _contentManager.ValidateAsync(contentItemOffer);

                if (result.Succeeded)
                {
                    await _contentManager.PublishAsync(contentItemOffer);
                }
                else
                {
                    return NotFound();
                }
            }

            var offerStatus = new
            {
                ContentItemId = offerStatusModel.OfferItemId,
                Status = offerStatusModel.IsDisabled == true ? "disabled" : offerStatusModel.Status,
            };

            return Ok(offerStatus);
        }

    }
}
