using System;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using Newtonsoft.Json.Linq;
using OrchardCore.ContentManagement;
using OrchardCore.SongServices.Models;
using OrchardCore.Mvc.Utilities;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using OrchardCore.Email;
using OrchardCore.Users;
using Permissions = OrchardCore.SongServices.Permissions;
using Microsoft.AspNetCore.Cors;
using Newtonsoft.Json;
using System.Text.RegularExpressions;
using System.Web;
using YesSql;
using OrchardCore.ContentManagement.Records;
using OrchardCore.Users.Services;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using System.Collections.Generic;
using YesSql.Services;
using OrchardCore.Settings;
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.DependencyInjection;
using OrchardCore.Entities;
using OrchardCore.Modules;
using IWorkflowManager = OrchardCore.Workflows.Services.IWorkflowManager;
using SignInResult = Microsoft.AspNetCore.Identity.SignInResult;
using OrchardCore.Users.Events;
using OrchardCore.Users.Handlers;
using Newtonsoft.Json.Serialization;
using OrchardCore.Scripting;
using OrchardCore.SongServices.ApiModels;
using OrchardCore.SongServices.ViewModels;
using OrchardCore.SongServices.ApiCommonFunctions;
using User = OrchardCore.Users.Models.User;
using OrchardCore.SongServices.Permissions;
using CommonPermissions = OrchardCore.Contents.CommonPermissions;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using OrchardCore.SongServices.ContentParts;
using OrchardCore.Users.Models;
using OrchardCore.Users.ViewModels;
using Google.Authenticator;
using OrchardCore.SongServices.Indexing;

namespace OrchardCore.SongServices.Controllers
{
    [Route("api/content/[action]")]
    [ApiController]
    [Authorize(AuthenticationSchemes = "Api"), IgnoreAntiforgeryToken, AllowAnonymous]
    public class ApiController : Controller
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
        //private readonly ITeleUpdateService _updateService;
        private readonly ISiteService _siteService;
        private readonly SignInManager<IUser> _signInManager;
        private readonly IEnumerable<ILoginFormEvent> _accountEvents;
        private readonly IScriptingManager _scriptingManager;
        private readonly IEnumerable<IExternalLoginEventHandler> _externalLoginHandlers;
        private readonly IDataProtectionProvider _dataProtectionProvider;
        private readonly IDistributedCache _distributedCache;
        private readonly IClock _clock;

        public string btcPayliteApiUrl;
        public string btcPayliteToken;


        public ApiController(
            IUserService userService,
            Microsoft.Extensions.Configuration.IConfiguration config,
            ISession session,
            IContentManager contentManager,
            IAuthorizationService authorizationService,
            UserManager<IUser> userManager,
            IEmailAddressValidator emailAddressValidator,
            ILogger<ApiController> logger,
            IRabbitMQProducer rabbitMQProducer,
            //ITeleUpdateService updateService,
            ISiteService siteService,
            SignInManager<IUser> signInManager,
            IEnumerable<ILoginFormEvent> accountEvents,
            IScriptingManager scriptingManager,
            IEnumerable<IExternalLoginEventHandler> externalLoginHandlers,
            IDataProtectionProvider dataProtectionProvider,
            IDistributedCache distributedCache,
            IClock clock,
            IStringLocalizer<ApiController> stringLocalizer)
        {
            _userService = userService;
            _config = config;
            _contentManager = contentManager;
            _authorizationService = authorizationService;
            _userManager = userManager;
            _emailAddressValidator = emailAddressValidator ?? throw new ArgumentNullException(nameof(emailAddressValidator));
            _logger = logger;
            _rabbitMQProducer = rabbitMQProducer;
            //_updateService = updateService;
            _session = session;
            _signInManager = signInManager;
            _siteService = siteService;
            _accountEvents = accountEvents;
            _scriptingManager = scriptingManager;
            _externalLoginHandlers = externalLoginHandlers;
            _dataProtectionProvider = dataProtectionProvider;
            _distributedCache = distributedCache;
            _clock = clock;
            S = stringLocalizer;

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

        [Route("{contentItemId}"), HttpGet]
        public async Task<IActionResult> Get(string contentItemId)
        {
            if (!await _authorizationService.AuthorizeAsync(User, AccessApiPermissions.AccessContentApi))
            {
                return this.ChallengeOrForbid();
            }

            var contentItem = await _contentManager.GetAsync(contentItemId);

            if (contentItem == null)
            {
                return NotFound();
            }

            return Ok(contentItem);
        }

        [HttpPost]
        public async Task<IActionResult> Post(ContentItem model, bool draft = false)
        {
            if (!await _authorizationService.AuthorizeAsync(User, AccessApiPermissions.AccessContentApi))
            {
                return this.ChallengeOrForbid();
            }

            //dynamic jsonObj = contentItem.Content;

            //string followerChart = Convert.ToString(jsonObj["Influencer"]["FollowerChart"]["Text"]);
            //string chartDate = Convert.ToString(jsonObj["Influencer"]["ChartCategoryDate"]["Text"]);
            //followerChart = followerChart + ";" + followerAndPhotoModel.NumberOfFollowers;
            //chartDate = chartDate + ";" + followerAndPhotoModel.ChartDate;

            //jsonObj["Influencer"]["NumberOfFollowers"]["Value"] = followerAndPhotoModel.NumberOfFollowers;
            //jsonObj["Influencer"]["FollowerChart"]["Text"] = followerChart.Trim(charsToTrim);
            //jsonObj["Influencer"]["ChartCategoryDate"]["Text"] = chartDate.Trim(charsToTrim);

            //var photos = jsonObj["Influencer"]["Photo"]["Paths"];

            //foreach (var item in followerAndPhotoModel.PhotoPaths)
            //{
            //    photos.Add(item);
            //}
            // It is really important to keep the proper method calls order with the ContentManager
            // so that all event handlers gets triggered in the right sequence.

            var contentItem = await _contentManager.GetAsync(model.ContentItemId, VersionOptions.DraftRequired);

            if (contentItem == null)
            {
                if (!await _authorizationService.AuthorizeAsync(User, CommonPermissions.PublishContent))
                {
                    return this.ChallengeOrForbid();
                }

                var newContentItem = await _contentManager.NewAsync(model.ContentType);
                newContentItem.Merge(model);

                var result = await _contentManager.UpdateValidateAndCreateAsync(newContentItem, draft ? VersionOptions.DraftRequired : VersionOptions.Published);
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
                if (!await _authorizationService.AuthorizeAsync(User, CommonPermissions.EditContent, contentItem))
                {
                    return this.ChallengeOrForbid();
                }

                contentItem.Merge(model, UpdateJsonMergeSettings);

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
        [ActionName("Verify2FACode")]
        [AllowAnonymous]
        public async Task<IActionResult> Verify2FACode(VerifyTwoFaModel request)
        {
            try
            {
                var user = await _userManager.FindByIdAsync(request.UserId) as User;

                var authenticatorCode = request.TwoFaCode.Replace(" ", string.Empty).Replace("-", string.Empty);

                var result = CheckTwoFactor(user, authenticatorCode);

                if (result)
                {
                    var resultConfirm = await _userManager.ConfirmEmailAsync(user, HttpUtility.UrlDecode(request.ConfirmEmailCode));
                    if (resultConfirm.Succeeded)
                    {
                        return Ok(true);
                    }
                    else
                    {
                        return Problem(
                                title: S["One or more validation errors occurred."],
                                detail: "Invalid token , this token was expired. Try resend another confirmation email",
                                statusCode: (int)HttpStatusCode.BadRequest);
                    }
                }
                else
                {
                    return Problem(
                                title: S["One or more validation errors occurred."],
                                detail: "Verification code is not valid, try another code",
                                statusCode: (int)HttpStatusCode.BadRequest);
                }
            }
            catch
            {
                return Problem(
                                title: S["One or more validation errors occurred."],
                                detail: "userName is not found.",
                                statusCode: (int)HttpStatusCode.BadRequest);
            }
        }

        [HttpPost]
        [ActionName("Check2FACode")]
        public async Task<IActionResult> Check2FACode(TwoFaCodeModel request)
        {
            if (!await _authorizationService.AuthorizeAsync(User, Permissions.AccessApiPermissions.AccessContentApi))
            {
                return this.ChallengeOrForbid();
            }

            var user = await _userManager.FindByNameAsync(User.Identity.Name) as User;

            try
            {
                var authenticatorCode = request.TwoFaCode.Replace(" ", string.Empty).Replace("-", string.Empty);

                var result = CheckTwoFactor(user, authenticatorCode);

                if (result)
                {
                    return Ok(true);
                }
                else
                {
                    return Ok(false);
                }
            }
            catch
            {
                return Problem(
                                title: S["One or more validation errors occurred."],
                                detail: "userName is not found.",
                                statusCode: (int)HttpStatusCode.BadRequest);
            }
        }

        [HttpGet]
        [ActionName("Check2FACode3")]
        public async Task<IActionResult> Check2FACode3()
        {
            if (!await _authorizationService.AuthorizeAsync(User, Permissions.AccessApiPermissions.AccessContentApi))
            {
                return this.ChallengeOrForbid();
            }

            var user = await _userManager.FindByNameAsync(User.Identity.Name) as User;

            try
            {
                //var authenticatorCode = "123456";

                var result = true;

                if (result)
                {
                    return Ok(true);
                }
                else
                {
                    return Ok(false);
                }
            }
            catch
            {
                return Problem(
                                title: S["One or more validation errors occurred."],
                                detail: "userName is not found.",
                                statusCode: (int)HttpStatusCode.BadRequest);
            }
        }

        [HttpGet]
        [ActionName("GetUser2FAQrCode")]
        [AllowAnonymous]
        public async Task<IActionResult> GetUser2FAQrCode(string userId)
        {
            var user = await _userManager.FindByIdAsync(userId) as User;

            var twoFactor = new TwoFactorAuthenticator();

            var setupInfo = twoFactor.GenerateSetupCode("EasierLife", user.Email, TwoFactorKey(user), false, 3);

            var model = new TwoFactorAuthenticationModel();

            model.SharedKey = setupInfo.ManualEntryKey;
            model.AuthenticatorUri = setupInfo.QrCodeSetupImageUrl;

            return Ok(model);

        }

        [HttpGet]
        [ActionName("PaymentMethodDetail")]
        [AllowAnonymous]
        public async Task<IActionResult> GetPaymentMethodDetail()
        {
            using var session = _session.Store.CreateSession();
            var method = await session.Query<ContentItem, ContentItemIndex>(x => x.ContentType == "PaymentMethod" && x.Published && x.Latest).FirstOrDefaultAsync();

            if (method != null)
            {
                return Ok(method);
            }

            // If we got this far, something failed, redisplay form
            return Problem(title: S["One or more validation errors occurred."],
                        detail: String.Join(',', ModelState.Values.FirstOrDefault().Errors.Select(e => e.ErrorMessage)),
                        statusCode: (int)HttpStatusCode.BadRequest);

        }

        [HttpPost]
        [ActionName("RegisterSptb")]
        [AllowAnonymous]
        public async Task<IActionResult> Register(RegisterModel model)
        {
            if (String.IsNullOrWhiteSpace(model.UserName))
            {
                ModelState.AddModelError("UserName", S["Username is required."]);
            }

            if (String.IsNullOrWhiteSpace(model.Email))
            {
                ModelState.AddModelError("Email", S["Email is required."]);
            }
            else if (!IsEmail(model.Email))
            {
                ModelState.AddModelError("Email", S["Invalid Email."]);
            }

            if (String.IsNullOrWhiteSpace(model.Password))
            {
                ModelState.AddModelError("Password", S["Password is required."]);
            }

            if (model.Password != model.ConfirmPassword)
            {
                ModelState.AddModelError("ConfirmPassword", S["The new password and confirmation password do not match."]);
            }

            if (ModelState.IsValid)
            {
                // Check if user with same email already exists
                var userWithEmail = await _userManager.FindByEmailAsync(model.Email);

                if (userWithEmail != null)
                {
                    ModelState.AddModelError("Email", S["A user with the same email already exists."]);
                }
            }

            // Get existing trader
            var trader = await _session.Query<ContentItem, ContentItemIndex>(x => x.Published && x.Latest && x.DisplayText.Contains(model.UserName) && x.ContentType == "TraderPage").FirstOrDefaultAsync();

            if (trader != null)
            {
                ModelState.AddModelError("UserName", S["A user with the same name already exists."]);
            }

            //if (!String.IsNullOrEmpty(model.UserName))
            //{
            //    // Check if user with same name already exists
            //    var userWithName = await _userManager.FindByNameAsync(model.UserName);

            //    if (userWithName != null)
            //    {
            //        ModelState.AddModelError("UserName", S["A user with the same name already exists."]);
            //    }
            //}

            if (TryValidateModel(model) && ModelState.IsValid)
            {
                var redirectUrl = _config["VxFulFrontendUrl"];
                var iuser = await this.RegisterUser(model, S["Confirm your account"], _logger, redirectUrl);
                var user = iuser as User;
                // If we get a user, redirect to returnUrl
                if (user != null)
                {
                    // create trader
                    var newContentItem = await _contentManager.NewAsync("TraderPage");
                    newContentItem.Owner = model.UserName;
                    newContentItem.Author = model.UserName;

                    var traderPart = new TraderForFilteringPart()
                    {
                        Name = model.UserName,
                        IsActivatedTele = false,
                        BondVndBalance = 0,
                        VndBalance = 0,
                        BTCBalance = 0,
                        ETHBalance = 0,
                        USDT20Balance = 0,
                        WithdrawVNDStatus = "WithdrawNA",
                        ReferenceCode = -1,
                        DateSend = string.Empty,
                        UserId = user.Id,
                        Email = model.Email,
                        PhoneNumber = string.Empty,
                        BankAccounts = string.Empty,
                        ChatIdTele = string.Empty,
                        DeviceId = string.Empty,
                        MoneyStatus = string.Empty,
                        Amount = 0,
                        TotalFeeBTC = 0,
                        TotalFeeETH = 0,
                        TotalFeeUSDT = 0,
                        TotalFeeVND = 0,
                        BookmarkOffers = string.Empty,
                        DateTime = DateTime.UtcNow
                    };

                    newContentItem.Apply(traderPart);

                    var result = await _contentManager.ValidateAsync(newContentItem);

                    if (result.Succeeded)
                    {
                        await _contentManager.CreateAsync(newContentItem, VersionOptions.Published);

                        return Ok(newContentItem);
                    }
                    else
                    {
                        return Problem(
                            title: S["One or more validation errors occurred."],
                            detail: String.Join(',', result.Errors),
                            statusCode: (int)HttpStatusCode.BadRequest);
                    }
                }

            }

            // If we got this far, something failed, redisplay form
            return Problem(title: S["One or more validation errors occurred."],
                        detail: String.Join(',', ModelState.Values.FirstOrDefault().Errors.Select(e => e.ErrorMessage)),
                        statusCode: (int)HttpStatusCode.BadRequest);

        }

        [ActionName("ForgetPasswordSptb")]
        [AllowAnonymous]
        public async Task<IActionResult> ForgetPassword(RegisterModel model)
        {
            if (String.IsNullOrWhiteSpace(model.Email))
            {
                ModelState.AddModelError("Email", S["Email is required."]);
            }
            else if (!IsEmail(model.Email))
            {
                ModelState.AddModelError("Email", S["Invalid Email."]);
            }

            if (ModelState.IsValid)
            {
                // Check if user with same email already exists
                var userWithEmail = await _userManager.FindByEmailAsync(model.Email) as User;

                if (TryValidateModel(model) && ModelState.IsValid)
                {
                    var redirectUrl = _config["VxFulFrontendUrl"];
                    var iuser = await this.ForgetPasswordUser(userWithEmail, S["Reset your password"], _logger, redirectUrl);
                    //var user = iuser as User;
                    return Ok(iuser);
                }
            }

            // If we got this far, something failed, redisplay form
            return Problem(title: S["One or more validation errors occurred."],
                        detail: String.Join(',', ModelState.Values.FirstOrDefault().Errors.Select(e => e.ErrorMessage)),
                        statusCode: (int)HttpStatusCode.BadRequest);

        }

        [HttpPost]
        [ActionName("ResetPasswordSptb")]
        [AllowAnonymous]
        public async Task<IActionResult> ResetPassword(ResetPasswordModel model)
        {
            if (String.IsNullOrWhiteSpace(model.Password))
            {
                ModelState.AddModelError("Password", S["Password is required."]);
            }

            if (model.Password != model.ConfirmPassword)
            {
                ModelState.AddModelError("ConfirmPassword", S["The new password and confirmation password do not match."]);
            }

            if (ModelState.IsValid)
            {
                if (TryValidateModel(model) && ModelState.IsValid)
                {
                    var user = await _userManager.FindByIdAsync(model.UserId);

                    if (user == null)
                    {
                        return NotFound();
                    }

                    var token = HttpUtility.UrlDecode(model.ResetToken);

                    if (await _userService.ResetPasswordAsync(user.UserName, token, model.Password, ModelState.AddModelError))
                    {
                        return Ok(true);
                    }
                }
            }

            // If we got this far, something failed, redisplay form
            return Problem(title: S["One or more validation errors occurred."],
                        detail: String.Join(',', ModelState.Values.FirstOrDefault().Errors.Select(e => e.ErrorMessage)),
                        statusCode: (int)HttpStatusCode.BadRequest);

        }

        [HttpGet]
        [ActionName("GetUserInfo")]
        public async Task<IActionResult> GetUserInfo()
        {
            Dictionary<string, decimal> dicBalance = null;
            var traderId = "";

            if (!await _authorizationService.AuthorizeAsync(User, AccessApiPermissions.AccessContentApi))
            {
                return this.ChallengeOrForbid();
            }

            var user = await _userManager.FindByNameAsync(User.Identity.Name) as User;

            if (user == null)
            {
                return Problem(
                        title: S["One or more validation errors occurred."],
                        detail: "User not found",
                        statusCode: (int)HttpStatusCode.BadRequest);
            }

            try
            {
                decimal btcBalanceValue = 0;
                decimal ethBalanceValue = 0;
                decimal usdtBalanceValue = 0;
                decimal vndBalanceValue = 0;


                var currentTrader = await _session
                .Query<ContentItem, ContentItemIndex>(index => index.ContentType == "TraderPage" && index.Published && index.Latest)
                .With<TraderForFilteringPartIndex>(p => p.UserId == user.Id)
                .FirstOrDefaultAsync();

                var btcBalanceString = Convert.ToString(currentTrader.Content["TraderForFilteringPart"].BTCBalance);
                var ethBalanceString = Convert.ToString(currentTrader.Content["TraderForFilteringPart"].ETHBalance);
                var usdtBalanceString = Convert.ToString(currentTrader.Content["TraderForFilteringPart"].USDT20Balance);
                var vndBalanceString = Convert.ToString(currentTrader.Content["TraderForFilteringPart"].VndBalance);
                var userName = Convert.ToString(currentTrader.Content["TraderForFilteringPart"].Name);

                btcBalanceValue = btcBalanceString == null ? 0 : Decimal.Parse(btcBalanceString);
                ethBalanceValue = ethBalanceString == null ? 0 : Decimal.Parse(ethBalanceString);
                usdtBalanceValue = usdtBalanceString == null ? 0 : Decimal.Parse(usdtBalanceString);
                vndBalanceValue = vndBalanceString == null ? 0 : Decimal.Parse(vndBalanceString);

                traderId = currentTrader.ContentItemId;

                dicBalance = await ApiCommon.CalculateBalanceAsync(btcBalanceValue, ethBalanceValue, usdtBalanceValue, vndBalanceValue, user.UserName, _session);

                user.UserName = userName;
            }
            catch (Exception ex)
            {
                ex.ToString();
                return Problem(
                    title: S["One or more validation errors occurred."],
                    detail: "BTC userName is not found.",
                    statusCode: (int)HttpStatusCode.BadRequest);
            }

            var userInfo = new
            {
                user.Id,
                user.UserName,
                user.Email,
                user.EmailConfirmed,
                BalanceBTC = dicBalance == null ? "0" : String.Format("{0:F8}", dicBalance["btc"]),
                BalanceETH = dicBalance == null ? "0" : String.Format("{0:F8}", dicBalance["eth"]),
                BalanceUSDT20 = dicBalance == null ? "0" : String.Format("{0:F8}", dicBalance["usdt20"]),
                BalanceVND = dicBalance == null ? "0" : String.Format("{0:F8}", dicBalance["vnd"]),
                user.IsEnabled,
                TraderId = traderId
            };

            return Ok(userInfo);
        }

        [HttpPost]
        [ActionName("UpdateBalance")]
        public async Task<IActionResult> UpdateBalance(UpdateBalanceModel updateBalanceModel)
        {
            var user = await _userManager.FindByNameAsync(User.Identity.Name) as User;
            if (user == null)
            {
                return Problem(
                        title: S["One or more validation errors occurred."],
                        detail: "User not found",
                        statusCode: (int)HttpStatusCode.BadRequest);
            }

            try
            {
                var result = await ApiCommon.UpdateBalance(_contentManager, _session, updateBalanceModel, user);

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
        [ActionName("CheckUserConfirmEmailById")]
        [AllowAnonymous]
        public async Task<IActionResult> CheckUserConfirmEmailById(string userId)
        {
            var user = await _userManager.FindByIdAsync(userId) as User;
            return Ok(user.EmailConfirmed);
        }

        [HttpGet]
        [ActionName("ConfirmEmailSptb")]
        [AllowAnonymous]
        public async Task<IActionResult> ConfirmEmail(string userId, string code)
        {
            if (userId == null || code == null)
            {
                return RedirectToAction(nameof(this.Register), "Registration");
            }

            var user = await _userManager.FindByIdAsync(userId);

            if (user == null)
            {
                return NotFound();
            }

            var result = await _userManager.ConfirmEmailAsync(user, code);

            if (result.Succeeded)
            {
                var content = "<html><h2>" +
                    S["Email confirmation"] +
                    "</h2></html> <div><p>" +
                    S["Thank you for confirming your email."] +
                    $"</p><p><a href = '' >" +
                    S["Home"] +
                    "</a></p></div>";
                return new ContentResult
                {
                    ContentType = "text/html",
                    StatusCode = (int)HttpStatusCode.OK,
                    Content = content
                };
            }

            return NotFound();
        }

        [HttpGet]
        [ActionName("GetPaymentMethods")]
        [AllowAnonymous]
        public async Task<IActionResult> GetPaymentMethods()
        {
            var paymentCategories = await _session
                .Query<ContentItem, ContentItemIndex>(index => index.ContentType == "PaymentCategory" && index.Published && index.Latest).OrderBy(index => index.DisplayText)
                .ListAsync();

            var listIds = paymentCategories.Select(p => p.ContentItemId).ToList();

            var paymentMethodNews = await _session
                .Query<ContentItem, ContentItemIndex>(index => index.ContentType == "PaymentMethod" && index.Published && index.Latest).OrderBy(index => index.DisplayText)
                .ListAsync();

            var listPaymentCategories = new List<PaymentCategoryReturnModel>();

            foreach (var item in paymentCategories)
            {
                dynamic categoryObj = item.Content;

                var category = new PaymentCategoryReturnModel()
                {
                    DisplayText = item.DisplayText,
                    IconPath = categoryObj["PaymentCategory"]["IconPath"]["Text"].Value,
                    Color = categoryObj["PaymentCategory"]["Color"]["Text"].Value,
                    ContentItemId = item.ContentItemId
                };

                listPaymentCategories.Add(category);
            }

            foreach (var item in paymentMethodNews)
            {
                dynamic methodObj = item.Content;

                string contentItemId = methodObj["ContainedPart"]["ListContentItemId"];

                var category = listPaymentCategories.Where(p => p.ContentItemId == contentItemId).FirstOrDefault();

                if (category != null)
                {
                    var listMethods = item.DisplayText.Trim(';').Split(';');
                    category.PaymentMethodNew = listMethods;
                }
            }

            return Ok(listPaymentCategories);
        }

        [HttpPost]
        [ActionName("ExternalLoginSptb")]
        [AllowAnonymous]
        public IActionResult ExternalLogin(string provider, string returnUrl = null)
        {
            // Request a redirect to the external login provider.
            var redirectUrl = _config["PaxHubUrl"] + "/" + nameof(ExternalLoginCallback) + "?returnUrl=" + returnUrl;
            var properties = _signInManager.ConfigureExternalAuthenticationProperties(provider, redirectUrl);
            return Challenge(properties, provider);
        }

        [HttpGet]
        [ActionName("ExternalLoginCallbackSptb")]
        [AllowAnonymous]
        public async Task<IActionResult> ExternalLoginCallback(string returnUrl = null, string remoteError = null)
        {
            if (remoteError != null)
            {
                _logger.LogError("Error from external provider: {Error}", remoteError);
                return RedirectToAction(nameof(Login));
            }

            var info = await _signInManager.GetExternalLoginInfoAsync();
            if (info == null)
            {
                _logger.LogError("Could not get external login info.");
                return RedirectToAction(nameof(Login));
            }

            var registrationSettings = (await _siteService.GetSiteSettingsAsync()).As<RegistrationSettings>();
            var user = await _userManager.FindByLoginAsync(info.LoginProvider, info.ProviderKey);

            if (user != null)
            {
                if (!await AddConfirmEmailError(user))
                {
                    await _accountEvents.InvokeAsync((e, user, modelState) => e.LoggingInAsync(user.UserName, (key, message) => modelState.AddModelError(key, message)), user, ModelState, _logger);

                    var signInResult = await ExternalLoginSignInAsync(user, info);
                    if (signInResult.Succeeded)
                    {
                        return RedirectToLocal(returnUrl);
                        //return LoggedInActionResult(user, returnUrl, info);
                    }
                    else
                    {
                        ModelState.AddModelError(string.Empty, S["Invalid login attempt."]);
                    }
                }
            }
            else
            {
                var email = info.Principal.FindFirstValue(ClaimTypes.Email) ?? info.Principal.FindFirstValue("email");

                if (!string.IsNullOrWhiteSpace(email))
                    user = await _userManager.FindByEmailAsync(email);

                ViewData["ReturnUrl"] = returnUrl;
                ViewData["LoginProvider"] = info.LoginProvider;

                if (user != null)
                {
                    // Link external login to an existing user
                    ViewData["UserName"] = user.UserName;
                    ViewData["Email"] = email;

                    return View("LinkExternalLogin");
                }
                else
                {
                    // no user could be matched, check if a new user can register
                    if (registrationSettings.UsersCanRegister == UserRegistrationType.NoRegistration)
                    {
                        string message = S["Site does not allow user registration."];
                        _logger.LogWarning("{Message}", message);
                        ModelState.AddModelError("", message);
                    }
                    else
                    {
                        var externalLoginViewModel = new RegisterExternalLoginViewModel();

                        externalLoginViewModel.NoPassword = registrationSettings.NoPasswordForExternalUsers;
                        externalLoginViewModel.NoEmail = registrationSettings.NoEmailForExternalUsers;
                        externalLoginViewModel.NoUsername = registrationSettings.NoUsernameForExternalUsers;

                        // If registrationSettings.NoUsernameForExternalUsers is true, this username will not be used
                        externalLoginViewModel.UserName = await GenerateUsername(info);
                        externalLoginViewModel.Email = email;

                        user = await this.RegisterUser(new RegisterViewModel()
                        {
                            UserName = externalLoginViewModel.UserName,
                            Email = externalLoginViewModel.Email,
                            Password = null,
                            ConfirmPassword = null
                        }, S["Confirm your account"], _logger);

                        // If the registration was successful we can link the external provider and redirect the user
                        if (user != null)
                        {
                            var identityResult = await _signInManager.UserManager.AddLoginAsync(user, new UserLoginInfo(info.LoginProvider, info.ProviderKey, info.ProviderDisplayName));
                            if (identityResult.Succeeded)
                            {
                                _logger.LogInformation(3, "User account linked to {LoginProvider} provider.", info.LoginProvider);

                                // We have created/linked to the local user, so we must verify the login.
                                // If it does not succeed, the user is not allowed to login
                                var signInResult = await ExternalLoginSignInAsync(user, info);
                                if (signInResult.Succeeded)
                                {
                                    return RedirectToLocal(returnUrl);
                                    //return LoggedInActionResult(user, returnUrl, info);
                                }
                                else
                                {
                                    ModelState.AddModelError(string.Empty, S["Invalid login attempt."]);
                                    return View(nameof(Login));
                                }
                            }
                            AddIdentityErrors(identityResult);
                        }

                        return View("RegisterExternalLogin", externalLoginViewModel);
                    }
                }
            }
            return RedirectToAction(nameof(Login));
        }

        [HttpGet]
        [ActionName("LoginSptb")]
        [AllowAnonymous]
        public async Task<IActionResult> Login(string returnUrl = null)
        {
            if (HttpContext.User != null && HttpContext.User.Identity.IsAuthenticated)
            {
                returnUrl = null;
            }

            // Clear the existing external cookie to ensure a clean login process
            await HttpContext.SignOutAsync(IdentityConstants.ExternalScheme);

            var loginSettings = (await _siteService.GetSiteSettingsAsync()).As<LoginSettings>();
            if (loginSettings.UseExternalProviderIfOnlyOneDefined)
            {
                var schemes = await _signInManager.GetExternalAuthenticationSchemesAsync();
                if (schemes.Count() == 1)
                {
                    var provider = schemes.First().Name;

                    var dataProtector = _dataProtectionProvider.CreateProtector(nameof(DefaultExternalLogin))
                                            .ToTimeLimitedDataProtector();

                    var token = Guid.NewGuid();
                    var expiration = new TimeSpan(0, 0, 5);
                    var protectedToken = dataProtector.Protect(token.ToString(), _clock.UtcNow.Add(expiration));
                    await _distributedCache.SetAsync(token.ToString(), token.ToByteArray(), new DistributedCacheEntryOptions() { AbsoluteExpirationRelativeToNow = expiration });
                    return RedirectToAction(nameof(DefaultExternalLogin), new { protectedToken, returnUrl });
                }
            }

            ViewData["ReturnUrl"] = returnUrl;
            return View();
        }

        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> DefaultExternalLogin(string protectedToken, string returnUrl = null)
        {
            var loginSettings = (await _siteService.GetSiteSettingsAsync()).As<LoginSettings>();
            if (loginSettings.UseExternalProviderIfOnlyOneDefined)
            {
                var schemes = await _signInManager.GetExternalAuthenticationSchemesAsync();
                if (schemes.Count() == 1)
                {
                    var dataProtector = _dataProtectionProvider.CreateProtector(nameof(DefaultExternalLogin))
                                            .ToTimeLimitedDataProtector();
                    try
                    {
                        Guid token;
                        if (Guid.TryParse(dataProtector.Unprotect(protectedToken), out token))
                        {
                            byte[] tokenBytes = await _distributedCache.GetAsync(token.ToString());
                            var cacheToken = new Guid(tokenBytes);
                            if (token.Equals(cacheToken))
                            {
                                return ExternalLogin(schemes.First().Name, returnUrl);
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "An error occurred while validating DefaultExternalLogin token");
                    }
                }
            }
            return RedirectToAction(nameof(Login));
        }

        private static bool IsEmail(string str)
        {
            if (String.IsNullOrWhiteSpace(str))
                return false;

            var _Email = new Regex("^((([a-z]|\\d|[!#\\$%&'\\*\\+\\-\\/=\\?\\^_`{\\|}~]|[\\u00A0-\\uD7FF\\uF900-\\uFDCF\\uFDF0-\\uFFEF])+(\\.([a-z]|\\d|[!#\\$%&'\\*\\+\\-\\/=\\?\\^_`{\\|}~]|[\\u00A0-\\uD7FF\\uF900-\\uFDCF\\uFDF0-\\uFFEF])+)*)|((\\x22)((((\\x20|\\x09)*(\\x0d\\x0a))?(\\x20|\\x09)+)?(([\\x01-\\x08\\x0b\\x0c\\x0e-\\x1f\\x7f]|\\x21|[\\x23-\\x5b]|[\\x5d-\\x7e]|[\\u00A0-\\uD7FF\\uF900-\\uFDCF\\uFDF0-\\uFFEF])|(\\\\([\\x01-\\x09\\x0b\\x0c\\x0d-\\x7f]|[\\u00A0-\\uD7FF\\uF900-\\uFDCF\\uFDF0-\\uFFEF]))))*(((\\x20|\\x09)*(\\x0d\\x0a))?(\\x20|\\x09)+)?(\\x22)))@((([a-z]|\\d|[\\u00A0-\\uD7FF\\uF900-\\uFDCF\\uFDF0-\\uFFEF])|(([a-z]|\\d|[\\u00A0-\\uD7FF\\uF900-\\uFDCF\\uFDF0-\\uFFEF])([a-z]|\\d|-|\\.|_|~|[\\u00A0-\\uD7FF\\uF900-\\uFDCF\\uFDF0-\\uFFEF])*([a-z]|\\d|[\\u00A0-\\uD7FF\\uF900-\\uFDCF\\uFDF0-\\uFFEF])))\\.)+(([a-z]|[\\u00A0-\\uD7FF\\uF900-\\uFDCF\\uFDF0-\\uFFEF])|(([a-z]|[\\u00A0-\\uD7FF\\uF900-\\uFDCF\\uFDF0-\\uFFEF])([a-z]|\\d|-|\\.|_|~|[\\u00A0-\\uD7FF\\uF900-\\uFDCF\\uFDF0-\\uFFEF])*([a-z]|[\\u00A0-\\uD7FF\\uF900-\\uFDCF\\uFDF0-\\uFFEF])))\\.?$", RegexOptions.IgnoreCase | RegexOptions.ExplicitCapture | RegexOptions.Compiled, TimeSpan.FromSeconds(2.0));
            return _Email.IsMatch(str);
        }

        private async Task<bool> AddConfirmEmailError(IUser user)
        {
            var registrationSettings = (await _siteService.GetSiteSettingsAsync()).As<RegistrationSettings>();
            if (registrationSettings.UsersMustValidateEmail == true)
            {
                // Require that the users have a confirmed email before they can log on.
                if (!await _userManager.IsEmailConfirmedAsync(user))
                {
                    ModelState.AddModelError(string.Empty, S["You must confirm your email."]);
                    return true;
                }
            }

            return false;
        }

        private async Task<SignInResult> ExternalLoginSignInAsync(IUser user, ExternalLoginInfo info)
        {
            var claims = info.Principal.GetSerializableClaims();
            var userRoles = await _userManager.GetRolesAsync(user);
            var context = new UpdateRolesContext(user, info.LoginProvider, claims, userRoles);

            var rolesToAdd = new List<string>();
            var rolesToRemove = new List<string>();

            var loginSettings = (await _siteService.GetSiteSettingsAsync()).As<LoginSettings>();
            if (loginSettings.UseScriptToSyncRoles)
            {
                try
                {
                    var jsonSerializerSettings = new JsonSerializerSettings()
                    {
                        ContractResolver = new CamelCasePropertyNamesContractResolver()
                    };
                    var script = $"js: function syncRoles(context) {{\n{loginSettings.SyncRolesScript}\n}}\nvar context={JsonConvert.SerializeObject(context, jsonSerializerSettings)};\nsyncRoles(context);\nreturn context;";
                    dynamic evaluationResult = _scriptingManager.Evaluate(script, null, null, null);
                    rolesToAdd = (evaluationResult.rolesToAdd as object[]).Select(i => i.ToString()).ToList();
                    rolesToRemove = (evaluationResult.rolesToRemove as object[]).Select(i => i.ToString()).ToList();
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error Syncing Roles From External Provider {info.LoginProvider}", info.LoginProvider);
                }
            }
            else
            {
                foreach (var item in _externalLoginHandlers)
                {
                    try
                    {
                        await item.UpdateRoles(context);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "{externalLoginHandler} - IExternalLoginHandler.UpdateRoles threw an exception", item.GetType());
                    }
                }
                rolesToAdd = context.RolesToAdd;
                rolesToRemove = context.RolesToRemove;
            }

            await _userManager.AddToRolesAsync(user, rolesToAdd.Distinct());
            await _userManager.RemoveFromRolesAsync(user, rolesToRemove.Distinct());

            var result = await _signInManager.ExternalLoginSignInAsync(info.LoginProvider, info.ProviderKey, isPersistent: false, bypassTwoFactor: true);

            if (result.Succeeded)
            {
                var identityResult = await _signInManager.UpdateExternalAuthenticationTokensAsync(info);
                if (!identityResult.Succeeded)
                {
                    _logger.LogError("Error updating the external authentication tokens.");
                }
            }

            return result;
        }

        private async Task<string> GenerateUsername(ExternalLoginInfo info)
        {
            var now = new TimeSpan(_clock.UtcNow.Ticks) - new TimeSpan(DateTime.UnixEpoch.Ticks);
            var ret = string.Concat("u" + Convert.ToInt32(now.TotalSeconds).ToString());

            var registrationSettings = (await _siteService.GetSiteSettingsAsync()).As<RegistrationSettings>();

            var externalClaims = info == null ? null : info.Principal.GetSerializableClaims();

            if (registrationSettings.UseScriptToGenerateUsername)
            {
                var context = new { userName = string.Empty, loginProvider = info?.LoginProvider, externalClaims };
                var jsonSerializerSettings = new JsonSerializerSettings()
                {
                    ContractResolver = new CamelCasePropertyNamesContractResolver()
                };
                var script = $"js: function generateUsername(context) {{\n{registrationSettings.GenerateUsernameScript}\n}}\nvar context = {JsonConvert.SerializeObject(context, jsonSerializerSettings)};\ngenerateUsername(context);\nreturn context;";
                try
                {
                    dynamic evaluationResult = _scriptingManager.Evaluate(script, null, null, null);
                    if (evaluationResult?.userName == null)
                        throw new Exception("GenerateUsernameScript did not return a username");
                    return evaluationResult.userName;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error evaluating GenerateUsernameScript({context})", context);
                }
            }
            else
            {
                var userNames = new Dictionary<Type, string>();
                foreach (var item in _externalLoginHandlers)
                {
                    try
                    {
                        var userName = await item.GenerateUserName(info.LoginProvider, externalClaims.ToArray());
                        if (!string.IsNullOrWhiteSpace(userName))
                        {
                            if (userNames.Count == 0)
                            {
                                ret = userName;
                            }
                            userNames.Add(item.GetType(), userName);
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "{externalLoginHandler} - IExternalLoginHandler.GenerateUserName threw an exception", item.GetType());
                    }
                }
                if (userNames.Count > 1)
                {
                    _logger.LogWarning("More than one IExternalLoginHandler generated username. Used first one registered, {externalLoginHandler}", userNames.FirstOrDefault().Key);
                }
            }

            return ret;
        }

        private RedirectResult LoggedInActionResult(IUser user, string returnUrl = null, ExternalLoginInfo info = null)
        {
            var workflowManager = HttpContext.RequestServices.GetService<IWorkflowManager>();
            if (workflowManager != null)
            {
                var input = new Dictionary<string, object>();
                input["UserName"] = user.UserName;
                input["ExternalClaims"] = info == null ? Enumerable.Empty<SerializableClaim>() : info.Principal.GetSerializableClaims();
                input["Roles"] = ((User)user).RoleNames;
                input["Provider"] = info?.LoginProvider;
                //await workflowManager.TriggerEventAsync(nameof(Workflows.Activities.UserLoggedInEvent),
                //    input: input, correlationId: ((Users.Models.User)user).Id.ToString());
            }

            return Redirect(returnUrl);
        }

        private void AddIdentityErrors(IdentityResult result)
        {
            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }
        }

        private RedirectResult RedirectToLocal(string returnUrl)
        {
            var url = _config["VxFulFrontendUrl"] + returnUrl;

            return Redirect(url);
            //if (Url.IsLocalUrl(returnUrl))
            //{
            //    return Redirect(returnUrl);
            //}
            //else
            //{
            //    return Redirect("~/");
            //}
        }

        private static string TwoFactorKey(User user)
        {
            return $"EasierLife+{user.Email}";
        }

        private static bool CheckTwoFactor(User user, string authenticatorCode)
        {
            var accountSecretKey = $"EasierLife+{user.Email}";

            var twoFactorAuthenticator = new TwoFactorAuthenticator();

            var result = twoFactorAuthenticator.ValidateTwoFactorPIN(accountSecretKey, authenticatorCode);

            return result;
        }

    }
}
