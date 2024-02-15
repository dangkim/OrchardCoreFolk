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
using Permissions = OrchardCore.SongServices.Permissions;
using CommonPermissions = OrchardCore.Contents.CommonPermissions;
using Microsoft.AspNetCore.Cors;
using Newtonsoft.Json;
using System.Text.RegularExpressions;
using System.Web;
using YesSql;
using OrchardCore.ContentManagement.Records;
using System.Text;
using OrchardCore.Users.Services;
using OrchardCore.SongServices.Models;
using OrchardCore.SongServices.Controllers;
using RestSharp;
using OrchardCore.SongServices.Indexing;

namespace OrchardCore.Content.Controllers
{
    [Route("api/content/[action]")]
    [ApiController]
    [Authorize(AuthenticationSchemes = "Api"), IgnoreAntiforgeryToken, AllowAnonymous]
    public class TraderApiController : Controller
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


        public TraderApiController(
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
        [ActionName("UpdateDepositTrader")]
        public async Task<IActionResult> UpdateDepositTrader(UpdateTraderModel model, bool draft = false)
        {
            var contentItemTrader = await _contentManager.GetAsync(model.TraderContentId, VersionOptions.DraftRequired);

            if (contentItemTrader == null)
            {
                return this.ChallengeOrForbid();
            }
            else
            {
                dynamic jsonObjTrader = contentItemTrader.Content;

                var traderObj = jsonObjTrader["TraderPage"];
                string referenceCode = traderObj["ReferenceCode"]["Text"];

                var currentBalance = traderObj["VndBalance"]["Text"] == null ? 0 : traderObj["VndBalance"]["Text"];
                currentBalance += model.Amount;

                if (model.Amount >= 0m && !string.IsNullOrEmpty(referenceCode) && !string.IsNullOrWhiteSpace(referenceCode) && referenceCode != model.ReferenceCode)
                {
                    //var currentBalance = traderObj["VndBalance"]["Text"] == null ? 0 : traderObj["VndBalance"]["Text"];
                    //currentBalance += model.Amount;

                    traderObj["VndBalance"]["Text"] = currentBalance;
                    traderObj["MoneyStatus"]["Text"] = "D";
                    traderObj["Amount"]["Text"] = model.Amount;
                    traderObj["ReferenceCode"]["Text"] = model.ReferenceCode;

                    model.MoneyStatus = "D";
                    //model.VndBalance = decimal.Parse(currentBalance);

                    await _contentManager.UpdateAsync(contentItemTrader);
                    var result = await _contentManager.ValidateAsync(contentItemTrader);

                    if (result.Succeeded)
                    {
                        if (!draft)
                        {
                            await _contentManager.PublishAsync(contentItemTrader);
                        }

                        model.VndBalance = currentBalance;

                        _rabbitMQProducer.CreateUserNotify(new CreateNotifyModel()
                        {
                            UserName = User.Identity.Name,
                            Content = $"Deposit successfully",
                            TradeId = ""
                        });
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
                    return Problem(
                            title: S["Please check amount and reference code"],
                            statusCode: (int)HttpStatusCode.BadRequest);
                }
            }

            return Ok(model);
        }

        [HttpPost]
        [ActionName("UpdateBondTrader")]
        public async Task<IActionResult> UpdateBondTrader(UpdateTraderModel model, bool draft = false)
        {
            var contentItemTrader = await _contentManager.GetAsync(model.TraderContentId, VersionOptions.DraftRequired);

            if (contentItemTrader == null)
            {
                return this.ChallengeOrForbid();
            }
            else
            {
                dynamic jsonObjTrader = contentItemTrader.Content;

                var traderObj = jsonObjTrader["TraderPage"];
                string referenceCode = traderObj["ReferenceCode"]["Text"];

                if (model.Amount >= 0m && !string.IsNullOrEmpty(referenceCode) && !string.IsNullOrWhiteSpace(referenceCode) && referenceCode != model.ReferenceCode)
                {
                    var currentBondBalance = traderObj["BondVndBalance"]["Text"] == null ? 0 : traderObj["BondVndBalance"]["Text"];
                    currentBondBalance += model.Amount;

                    traderObj["BondVndBalance"]["Text"] = currentBondBalance;
                    traderObj["MoneyStatus"]["Text"] = "D";
                    traderObj["Amount"]["Text"] = model.Amount;
                    traderObj["ReferenceCode"]["Text"] = model.ReferenceCode;

                    model.MoneyStatus = "D";

                    await _contentManager.UpdateAsync(contentItemTrader);
                    var result = await _contentManager.ValidateAsync(contentItemTrader);

                    if (result.Succeeded)
                    {
                        if (!draft)
                        {
                            await _contentManager.PublishAsync(contentItemTrader);
                        }

                        _rabbitMQProducer.CreateUserNotify(new CreateNotifyModel()
                        {
                            UserName = User.Identity.Name,
                            Content = $"Deposit bond successfully",
                            TradeId = ""
                        });
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
                    return Problem(
                            title: S["Please check amount and reference code"],
                            statusCode: (int)HttpStatusCode.BadRequest);
                }
            }

            return Ok(model);
        }

        [HttpPost]
        [ActionName("UpdateWithdrawTrader")]
        public async Task<IActionResult> UpdateWithdrawTrader(UpdateTraderModel model, bool draft = false)
        {
            if (!await _authorizationService.AuthorizeAsync(User, Permissions.AccessApiPermissions.AccessContentApi))
            {
                return this.ChallengeOrForbid();
            }

            var user = await _userManager.FindByNameAsync(User.Identity.Name) as User;
            var btcApiCheck2FACodeUrl = $"{btcPayliteApiUrl}/api/v1/users/check2facode/{model.TwoFaCode}/{user.Email}";

            using var client = new RestClient(btcApiCheck2FACodeUrl);
            var request = new RestRequest();

            request.AddHeader("Authorization", btcPayliteToken);

            try
            {
                var response = await client.ExecuteGetAsync(request);

                if (bool.Parse(response.Content))
                {
                    var contentItemTrader = await _contentManager.GetAsync(model.TraderContentId, VersionOptions.DraftRequired);

                    if (contentItemTrader == null)
                    {
                        return this.ChallengeOrForbid();
                    }
                    else
                    {
                        dynamic jsonObjTrader = contentItemTrader.Content;

                        var traderObj = jsonObjTrader["TraderPage"];

                        var currentVndBalance = traderObj["VndBalance"]["Text"] ?? 0;

                        if (model.Amount >= 0m && currentVndBalance <= model.Amount)
                        {
                            currentVndBalance += model.Amount;
                            traderObj["VndBalance"]["Text"] = currentVndBalance;
                            traderObj["MoneyStatus"]["Text"] = "W";
                            traderObj["Amount"]["Text"] = model.Amount;
                            traderObj["WithdrawVNDStatus"]["Text"] = "Completed";

                            model.MoneyStatus = "W";

                            await _contentManager.UpdateAsync(contentItemTrader);
                            var result = await _contentManager.ValidateAsync(contentItemTrader);

                            if (result.Succeeded)
                            {
                                if (!draft)
                                {
                                    await _contentManager.PublishAsync(contentItemTrader);
                                }

                                _rabbitMQProducer.CreateUserNotify(new CreateNotifyModel()
                                {
                                    UserName = User.Identity.Name,
                                    Content = $"Withdraw successfully",
                                    TradeId = ""
                                });
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
                            return Problem(
                                    title: S["Please check amount"],
                                    detail: S["Please check amount"],
                                    statusCode: (int)HttpStatusCode.BadRequest);
                        }
                    }
                }
                else
                {
                    return Problem(
                                title: S["One or more validation errors occurred."],
                                detail: S["Please check your 2FA code"],
                                statusCode: (int)HttpStatusCode.BadRequest);
                }
            }
            catch (Exception)
            {
                return Problem(
                                title: S["One or more validation errors occurred."],
                                detail: S["Please check your 2FA code"],
                                statusCode: (int)HttpStatusCode.BadRequest);
            }

            // It is really important to keep the proper method calls order with the ContentManager
            // so that all event handlers gets triggered in the right sequence.

            return Ok(model);
        }

        [HttpPost]
        [ActionName("UpdateWithdrawBondTrader")]
        public async Task<IActionResult> UpdateWithdrawBondTrader(UpdateTraderModel model, bool draft = false)
        {
            if (!await _authorizationService.AuthorizeAsync(User, Permissions.AccessApiPermissions.AccessContentApi))
            {
                return this.ChallengeOrForbid();
            }

            var user = await _userManager.FindByNameAsync(User.Identity.Name) as User;
            var btcApiCheck2FACodeUrl = $"{btcPayliteApiUrl}/api/v1/users/check2facode/{model.TwoFaCode}/{user.Email}";

            using var client = new RestClient(btcApiCheck2FACodeUrl);
            var request = new RestRequest();

            request.AddHeader("Authorization", btcPayliteToken);

            try
            {
                var response = await client.ExecuteGetAsync(request);

                if (bool.Parse(response.Content))
                {
                    // It is really important to keep the proper method calls order with the ContentManager
                    // so that all event handlers gets triggered in the right sequence.

                    var contentItemTrader = await _contentManager.GetAsync(model.TraderContentId, VersionOptions.DraftRequired);

                    if (contentItemTrader == null)
                    {
                        return this.ChallengeOrForbid();
                    }
                    else
                    {
                        dynamic jsonObjTrader = contentItemTrader.Content;

                        var traderObj = jsonObjTrader["TraderPage"];

                        var currentBondBalance = traderObj["BondVndBalance"]["Text"] ?? 0;

                        if (model.Amount >= 0m && currentBondBalance <= model.Amount)
                        {
                            currentBondBalance += model.Amount;
                            traderObj["BondVndBalance"]["Text"] = currentBondBalance;
                            traderObj["MoneyStatus"]["Text"] = "W";
                            traderObj["Amount"]["Text"] = model.Amount;

                            model.MoneyStatus = "W";

                            await _contentManager.UpdateAsync(contentItemTrader);
                            var result = await _contentManager.ValidateAsync(contentItemTrader);

                            if (result.Succeeded)
                            {
                                if (!draft)
                                {
                                    await _contentManager.PublishAsync(contentItemTrader);
                                }

                                _rabbitMQProducer.CreateUserNotify(new CreateNotifyModel()
                                {
                                    UserName = User.Identity.Name,
                                    Content = $"Withdraw bond successfully",
                                    TradeId = ""
                                });
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
                            return Problem(
                                    title: S["Please check amount"],
                                    detail: S["Please check amount"],
                                    statusCode: (int)HttpStatusCode.BadRequest);
                        }
                    }
                }
                else
                {
                    return Problem(
                                title: S["One or more validation errors occurred."],
                                detail: S["Please check your 2FA code"],
                                statusCode: (int)HttpStatusCode.BadRequest);
                }
            }
            catch (Exception)
            {
                return Problem(title: S["One or more validation errors occurred."],
                                detail: S["Please check your 2FA code"],
                                statusCode: (int)HttpStatusCode.BadRequest);
            }

            return Ok(model);
        }

        [HttpPost]
        [ActionName("StartWithdrawTrader")]
        public async Task<IActionResult> StartWithdrawTrader(UpdateTraderModel model, bool draft = false)
        {
            if (!await _authorizationService.AuthorizeAsync(User, Permissions.AccessApiPermissions.AccessContentApi))
            {
                return this.ChallengeOrForbid();
            }

            var user = await _userManager.FindByNameAsync(User.Identity.Name) as User;
            var btcApiCheck2FACodeUrl = $"{btcPayliteApiUrl}/api/v1/users/check2facode/{model.TwoFaCode}/{user.Email}";

            using var client = new RestClient(btcApiCheck2FACodeUrl);
            var request = new RestRequest();

            request.AddHeader("Authorization", btcPayliteToken);

            try
            {
                var response = await client.ExecuteGetAsync(request);

                // It is really important to keep the proper method calls order with the ContentManager
                // so that all event handlers gets triggered in the right sequence.

                if (bool.Parse(response.Content))
                {
                    var contentItemTrader = await _contentManager.GetAsync(model.TraderContentId, VersionOptions.DraftRequired);

                    if (contentItemTrader == null)
                    {
                        return this.ChallengeOrForbid();
                    }
                    else
                    {
                        dynamic jsonObjTrader = contentItemTrader.Content;

                        var traderObj = jsonObjTrader["TraderPage"];
                        //string referenceCode = traderObj["ReferenceCode"]["Text"];

                        var currentVndBalance = traderObj["VndBalance"]["Text"] ?? 0;

                        if (model.Amount >= 0m &&
                            currentVndBalance <= model.Amount &&
                            !string.IsNullOrEmpty(model.WithdrawVNDStatus) &&
                            !string.IsNullOrEmpty(model.BankAccounts))
                        {
                            traderObj["WithdrawVNDStatus"]["Text"] = model.WithdrawVNDStatus;
                            traderObj["BankAccounts"]["Text"] = model.BankAccounts;
                            traderObj["Amount"]["Text"] = model.Amount;

                            await _contentManager.UpdateAsync(contentItemTrader);
                            var result = await _contentManager.ValidateAsync(contentItemTrader);

                            if (result.Succeeded)
                            {
                                if (!draft)
                                {
                                    await _contentManager.PublishAsync(contentItemTrader);
                                }

                                _rabbitMQProducer.CreateUserNotify(new CreateNotifyModel()
                                {
                                    UserName = User.Identity.Name,
                                    Content = $"Withdraw in process, please wait a few minutes",
                                    TradeId = ""
                                });
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
                            return Problem(
                                    title: S["Please check amount"],
                                    detail: S["Please check amount"],
                                    statusCode: (int)HttpStatusCode.BadRequest);
                        }
                    }
                }
                else
                {
                    return Problem(
                                title: S["One or more validation errors occurred."],
                                detail: S["Please check your 2FA code"],
                                statusCode: (int)HttpStatusCode.BadRequest);
                }
            }
            catch (Exception)
            {
                return Problem(
                                title: S["One or more validation errors occurred."],
                                detail: S["Please check your 2FA code"],
                                statusCode: (int)HttpStatusCode.BadRequest);
            }

            return Ok(model);
        }

        [HttpGet]
        [ActionName("GetOnlineStatus")]
        public async Task<IActionResult> GetOnlineStatus(string userName)
        {
            if (!await _authorizationService.AuthorizeAsync(User, Permissions.AccessApiPermissions.AccessContentApi))
            {
                return this.ChallengeOrForbid();
            }

            try
            {
                _rabbitMQProducer.ConnectedSignalR(userName);
            }
            catch (Exception)
            {
                return Problem(
                                title: S["One or more validation errors occurred."],
                                detail: S["Please check your 2FA code"],
                                statusCode: (int)HttpStatusCode.BadRequest);
            }

            return Ok(true);
        }

        [HttpGet]
        [ActionName("GetUserContentId")]
        public async Task<IActionResult> GetUserContentId(string userName)
        {
            if (!await _authorizationService.AuthorizeAsync(User, Permissions.AccessApiPermissions.AccessContentApi))
            {
                return this.ChallengeOrForbid();
            }

            try
            {
                var trader = await _session
                .Query<ContentItem, ContentItemIndex>(index => index.ContentType == "TraderPage" && index.Published && index.Latest)
                .With<TraderForFilteringPartIndex>(p => p.Name == userName)
                .FirstOrDefaultAsync();

                var result = new
                {
                    ContentItemId = trader.ContentItemId,
                    UserId = trader.Content["TraderForFilteringPart"].UserId
                };

                return Ok(result);
            }
            catch (Exception)
            {
                return Problem(
                                title: S["One or more validation errors occurred."],
                                detail: S["Please check your user name"],
                                statusCode: (int)HttpStatusCode.BadRequest);
            }
        }
    }
}
