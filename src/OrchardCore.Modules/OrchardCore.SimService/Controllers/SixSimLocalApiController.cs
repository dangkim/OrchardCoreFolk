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
using System.Text;
using OrchardCore.Users.Services;
using OrchardCore.SimService.ApiModels;
using OrchardCore.SimService.Models;
using OrchardCore.SimService.Indexes;
using NSwag.Annotations;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Primitives;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using OrchardCore.SimService.ViewModels;
using RestSharp;
using OrchardCore.SimService.Permissions;
using OrchardCore.Data;

namespace OrchardCore.SimService.Controllers
{
    [Route("api/content/[action]")]
    [ApiController]
    [Authorize(AuthenticationSchemes = "Api"), IgnoreAntiforgeryToken]
    [OpenApiIgnore]
    public class SixSimLocalApiController : Controller
    {
        private static readonly JsonMergeSettings UpdateJsonMergeSettings = new JsonMergeSettings { MergeArrayHandling = MergeArrayHandling.Replace };
        private readonly Microsoft.Extensions.Configuration.IConfiguration _config;
        private readonly IContentManager _contentManager;
        private readonly IAuthorizationService _authorizationService;
        private readonly IStringLocalizer S;
        private readonly UserManager<IUser> _userManager;
        private readonly IEmailAddressValidator _emailAddressValidator;
        private readonly ILogger _logger;
        private readonly YesSql.ISession _session;
        private readonly YesSql.ISession _sessionReadOnly;
        private readonly IUserService _userService;
        public string fiveSimToken;

        public string btcPayliteApiUrl;
        public string btcPayliteToken;


        public SixSimLocalApiController(
            IUserService userService,
            Microsoft.Extensions.Configuration.IConfiguration config,
            YesSql.ISession session,
            IReadOnlySession sessionReadOnly,
            IContentManager contentManager,
            IAuthorizationService authorizationService,
            UserManager<IUser> userManager,
            IEmailAddressValidator emailAddressValidator,
            ILogger<ApiController> logger,
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
            _session = session;
            _sessionReadOnly = sessionReadOnly;

            fiveSimToken = _config["FiveSimToken"];

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
        [ActionName("RateVndToUsd")]
        [AllowAnonymous]
        public async Task<IActionResult> RateVndToUsd()
        {
            var exchangeRateVND = await _sessionReadOnly
                .Query<ContentItem, ContentItemIndex>(index => index.ContentType == "ExchangeRate" && index.DisplayText == "VND" && index.Published && index.Latest)
                .FirstOrDefaultAsync();
            //var exchangeRateVND = await _session
            //    .Query<ContentItem, ContentItemIndex>(index => index.ContentType == "ExchangeRate" && index.DisplayText == "VND" && index.Published && index.Latest)
            //    .FirstOrDefaultAsync();

            if (exchangeRateVND != null)
            {
                decimal vndToUSD = exchangeRateVND.Content["ExchangeRate"]["RateToUsd"]["Text"];

                if (vndToUSD == 0)
                {
                    return Ok(24000);
                }
                else
                {
                    vndToUSD = 1 / vndToUSD;
                    var rounded = Math.Round(vndToUSD);
                    return Ok(rounded);
                }
            }
            else
            {
                // If we got this far, something failed, redisplay form
                return Problem(title: S["One or more validation errors occurred."],
                            detail: "There's no user",
                            statusCode: (int)HttpStatusCode.BadRequest);
            }
        }

        [HttpPost]
        [EnableCors("MyPolicy")]
        [ActionName("CancelPM")]
        [AllowAnonymous]
        public async Task<IActionResult> CancelPerfectMoneyAsync([FromForm] PerfectMoneyModel model)
        {
            // check user Id by payment Id from PM
            var userContent = await _session
                .Query<ContentItem, ContentItemIndex>(index => index.ContentType == "UserProfileType" && index.Published && index.Latest)
                .With<UserProfilePartIndex>(p => p.UserId == model.PAYMENT_ID)
                .FirstOrDefaultAsync();

            //if (userContent != null)
            //{
            //    // Check syntax
            //    if (model.SUGGESTED_MEMO == ("BMZ" + model.PAYMENT_ID + "BMZ"))
            //    {
            //        // Update Balance
            //        await UpdateSixSimBalanceByPMAsync(model.PAYMENT_AMOUNT, model.PAYMENT_ID);
            //    }
            //}

            return Redirect("https://simforrent.com/payment");
        }

        [HttpPost]
        [EnableCors("MyPolicy")]
        [ActionName("SuccessPM")]
        [AllowAnonymous]
        public async Task<IActionResult> SuccessPerfectMoneyAsync([FromForm] PerfectMoneyModel model)
        {
            // check user Id by payment Id from PM
            var userContent = await _session
                .Query<ContentItem, ContentItemIndex>(index => index.ContentType == "UserProfileType" && index.Published && index.Latest)
                .With<UserProfilePartIndex>(p => p.UserId == model.PAYMENT_ID)
                .FirstOrDefaultAsync();

            if (userContent != null)
            {
                // Check syntax
                if (model.SUGGESTED_MEMO == ("BMZ" + model.PAYMENT_ID + "BMZ"))
                {
                    // Update Balance
                    await UpdateSixSimBalanceByPMAsync(model.PAYMENT_AMOUNT, model.PAYMENT_ID);
                }
            }

            return Redirect("https://simforrent.com/payment");
        }

        [HttpGet]
        [EnableCors("MyPolicy")]
        [ActionName("GetBankAccount")]
        [AllowAnonymous]
        public async Task<IActionResult> GetBankAccount()
        {
            var bankContent = await _session
                .Query<ContentItem, ContentItemIndex>(index => index.ContentType == "BankAccount" && index.Published && index.Latest)
                .ListAsync();

            if (bankContent != null)
            {
                return Ok(bankContent);
            }
            else
            {
                // If we got this far, something failed, redisplay form
                return Problem(title: S["One or more validation errors occurred."],
                            detail: "There's no user",
                            statusCode: (int)HttpStatusCode.BadRequest);
            }
        }

        [HttpGet]
        [EnableCors("MyPolicy")]
        [ActionName("GetTotalCountNewsContent")]
        [AllowAnonymous]
        public async Task<IActionResult> GetTotalCountNewsContent()
        {
            try
            {
                var countNews = await _session
                .Query<ContentItem, ContentItemIndex>(index => index.ContentType == "News" && index.Published)
                .CountAsync();

                return Ok(countNews);
            }
            catch (Exception)
            {
                // If we got this far, something failed, redisplay form
                return Problem(title: S["One or more validation errors occurred."],
                            detail: "There's no user",
                           statusCode: (int)HttpStatusCode.BadRequest);
            }
        }

        [HttpGet]
        [EnableCors("MyPolicy")]
        [ActionName("SearchUserById")]
        public async Task<IActionResult> SearchUserById(int userId)
        {
            var userContent = await _session
                .Query<ContentItem, ContentItemIndex>(index => index.ContentType == "UserProfileType" && index.Published && index.Latest)
                .With<UserProfilePartIndex>(p => p.UserId == userId)
                .FirstOrDefaultAsync();

            if (userContent != null)
            {
                var content = userContent.Content["UserProfilePart"];

                var resultObject = new
                {
                    content.UserId,
                    content.Balance,
                    content.RateInUsd
                };

                return Ok(resultObject);
            }
            else
            {
                // If we got this far, something failed, redisplay form
                return Problem(title: S["One or more validation errors occurred."],
                            detail: "There's no user",
                            statusCode: (int)HttpStatusCode.BadRequest);
            }
        }

        [HttpGet]
        [EnableCors("MyPolicy")]
        [ActionName("SearchUserByEmail")]
        public async Task<IActionResult> SearchUserByEmail(string email)
        {
            var userContent = await _session
                .Query<ContentItem, ContentItemIndex>(index => index.ContentType == "UserProfileType" && index.Published && index.Latest)
                .With<UserProfilePartIndex>(p => p.Email == email)
                .FirstOrDefaultAsync();

            if (userContent != null)
            {
                var content = userContent.Content["UserProfilePart"];

                var resultObject = new
                {
                    content.UserId,
                    content.Balance,
                    content.RateInUsd
                };

                return Ok(resultObject);
            }

            // If we got this far, something failed, redisplay form
            return Problem(title: S["One or more validation errors occurred."],
                        detail: "There's no user",
                        statusCode: (int)HttpStatusCode.BadRequest);

        }

        [HttpGet]
        [EnableCors("MyPolicy")]
        [ActionName("RefreshTokenApi")]
        public async Task<IActionResult> RefreshTokenApi()
        {
            var user = await _userManager.GetUserAsync(User) as User;

            if (user == null)
            {
                return this.ChallengeOrForbid();
            }

            if (!await _authorizationService.AuthorizeAsync(User, SimApiPermissions.AccessContentApi))
            {
                return this.ChallengeOrForbid();
            }

            var token = GenerateJSONWebToken(user.UserName, user.Email);

            var userContent = await _session
                .Query<ContentItem, ContentItemIndex>(index => index.ContentType == "UserProfileType" && index.Published && index.Latest)
                .With<UserProfilePartIndex>(p => p.UserName == User.Identity.Name)
                .FirstOrDefaultAsync();

            if (userContent != null)
            {
                var userProfilePart = userContent.Content["UserProfilePart"];

                var newUserProfilePart = new UserProfilePart
                {
                    ProfileId = userProfilePart.ProfileId,
                    Email = userProfilePart.Email,
                    UserId = userProfilePart.UserId,
                    UserName = userProfilePart.UserName,
                    Vendor = userProfilePart.Vendor,
                    DefaultForwardingNumber = userProfilePart.DefaultForwardingNumber,
                    Balance = userProfilePart.Balance,
                    OriginalAmount = userProfilePart.OriginalAmount,
                    Amount = -userProfilePart.Price,
                    GmailMsgId = userProfilePart.GmailMsgId,
                    RateInUsd = userProfilePart.RateInUsd,
                    Rating = userProfilePart.Rating,
                    DefaultCoutryName = userProfilePart.DefaultCoutryName,
                    DefaultIso = userProfilePart.DefaultIso,
                    DefaultPrefix = userProfilePart.DefaultPrefix,
                    DefaultOperatorName = userProfilePart.DefaultOperatorName,
                    FrozenBalance = userProfilePart.FrozenBalance,
                    TokenApi = token
                };

                userContent.Apply(newUserProfilePart);


                await _contentManager.UpdateAsync(userContent);

                var resultUserContent = await _contentManager.ValidateAsync(userContent);

                if (resultUserContent.Succeeded)
                {
                    await _contentManager.PublishAsync(userContent);
                }

                return Ok(userContent.Content["UserProfilePart"]);
            }
            else
            {
                // If we got this far, something failed, redisplay form
                return Problem(title: S["One or more validation errors occurred."],
                            detail: "There's no user",
                            statusCode: (int)HttpStatusCode.BadRequest);
            }
        }

        [HttpGet]
        [EnableCors("MyPolicy")]
        [ActionName("GetExpiredAndStatus")]
        public async Task<IActionResult> GetExpiredAndStatus(string id)
        {
            if (!await _authorizationService.AuthorizeAsync(User, SimApiPermissions.AccessContentApi))
            {
                return this.ChallengeOrForbid();
            }

            var url = string.Format("https://5sim.net/v1/user/check/{0}", id);

            var client = new RestClient(url);
            var request = new RestRequest();
            request.AddHeader("Authorization", "Bearer " + fiveSimToken);

            var response = await client.ExecuteGetAsync(request);
            var resObject = JsonConvert.DeserializeObject<OrderDetailPartViewModel>(response.Content);

            return Ok(resObject);

        }

        [HttpPost]
        [EnableCors("MyPolicy")]
        [ActionName("CheckUserRole")]
        [AllowAnonymous]
        public async Task<IActionResult> CheckUserRole(UpdateBalancePaymentMethodModel model)
        {
            var userByEmail = await _userManager.FindByEmailAsync(model.Email) as User;
            var userByName = await _userManager.FindByNameAsync(model.UserName) as User;

            if (userByName != null && userByEmail != null && userByEmail.UserName == userByName.UserName)
            {
                var isValidPassword = await _userManager.CheckPasswordAsync(userByName, model.Password);

                if (isValidPassword)
                {
                    if (await _authorizationService.AuthorizeAsync(User, SimApiPermissions.AccessContentApi) && User.IsInRole("UpdateBalance"))
                    {
                        return Ok(true);
                    }
                }
            }

            // If we got this far, something failed
            return Problem(title: S["One or more validation errors occurred."],
                        detail: "There's no user",
                        statusCode: (int)HttpStatusCode.BadRequest);
        }

        [HttpPost]
        [EnableCors("MyPolicy")]
        [ActionName("UpdateBalanceByAdmin")]
        public async Task<IActionResult> UpdateBalanceByAdmin(UpdateBalancePaymentMethodModel model)
        {
            var userByEmail = await _userManager.FindByEmailAsync(model.Email) as User;
            var userByName = await _userManager.FindByNameAsync(model.UserName) as User;

            if (userByName != null && userByEmail != null && userByEmail.UserName == userByName.UserName)
            {
                var isValidPassword = await _userManager.CheckPasswordAsync(userByName, model.Password);

                if (isValidPassword)
                {
                    if (await _authorizationService.AuthorizeAsync(User, SimApiPermissions.AccessContentApi) && User.IsInRole("UpdateBalance"))
                    {
                        if (model.Method == "PM")
                        {
                            var result = await UpdateSixSimBalanceByPMAsync(model.Amount, model.UserId, model.Method);
                            return Ok(result);
                        }
                        else
                        {
                            var result = await UpdateSixSimBalanceByBankAsync(model.Amount, model.UserId, model.Method);
                            return Ok(result);
                        }
                    }
                }
            }

            // If we got this far, something failed
            return Problem(title: S["One or more validation errors occurred."],
                        detail: "There's no user",
                        statusCode: (int)HttpStatusCode.BadRequest);
        }

        private static bool GetAPIKey(HttpContext httpContext, out StringValues apiKey)
        {
            if (httpContext.Request.Headers.TryGetValue("Authorization", out var value) &&
                value.ToString().StartsWith("token ", StringComparison.InvariantCultureIgnoreCase))
            {
                apiKey = value.ToString().Substring("token ".Length);
                return true;
            }

            apiKey = default;
            return false;
        }

        private async Task<bool> UpdateSixSimBalanceByBankAsync(decimal originalAmount, long userId, string method)
        {
            try
            {
                // Get UserProfile by userId
                var userContent = await _session
                .Query<ContentItem, ContentItemIndex>(index => index.ContentType == "UserProfileType" && index.Published && index.Latest)
                .With<UserProfilePartIndex>(p => p.UserId == userId)
                .FirstOrDefaultAsync();

                var exchangeRateVNDContent = await _session
                .Query<ContentItem, ContentItemIndex>(index => index.ContentType == "ExchangeRate" && index.DisplayText == "VND" && index.Published && index.Latest)
                .FirstOrDefaultAsync();

                string vndRateString = exchangeRateVNDContent.Content["ExchangeRate"]["RateToUsd"]["Text"];
                decimal vndRateDouble = Decimal.Parse(vndRateString);

                var exchangeRateRUBContent = await _session
                    .Query<ContentItem, ContentItemIndex>(index => index.ContentType == "ExchangeRate" && index.DisplayText == "RUB" && index.Published && index.Latest)
                    .FirstOrDefaultAsync();

                string rubRateString = exchangeRateRUBContent.Content["ExchangeRate"]["RateToUsd"]["Text"];
                decimal rubRateDouble = Decimal.Parse(rubRateString);

                if (userContent != null)
                {
                    var amountInUsd = originalAmount * vndRateDouble;

                    var amountInRub = amountInUsd / rubRateDouble;

                    var content = userContent.Content;
                    var userProfilePart = content["UserProfilePart"];
                    decimal currentBalance = userProfilePart.Balance;

                    currentBalance += amountInRub; // All is RUB currency

                    var newUserProfilePart = new UserProfilePart
                    {
                        ProfileId = userProfilePart.ProfileId,
                        Email = userProfilePart.Email,
                        UserId = userProfilePart.UserId,
                        UserName = userProfilePart.UserName,
                        Vendor = userProfilePart.Vendor,
                        DefaultForwardingNumber = userProfilePart.DefaultForwardingNumber,
                        Balance = currentBalance,
                        OriginalAmount = originalAmount,
                        Amount = amountInRub,
                        GmailMsgId = "",
                        RateInUsd = rubRateDouble,
                        Rating = userProfilePart.Rating,
                        DefaultCoutryName = userProfilePart.DefaultCoutryName,
                        DefaultIso = userProfilePart.DefaultIso,
                        DefaultPrefix = userProfilePart.DefaultPrefix,
                        DefaultOperatorName = userProfilePart.DefaultOperatorName,
                        FrozenBalance = userProfilePart.FrozenBalance
                    };

                    userContent.Apply(newUserProfilePart);
                    userContent.Owner = userProfilePart.UserName;
                    userContent.Author = userProfilePart.UserName;

                    // Create new PaymentType
                    var newPaymentContent = await _contentManager.NewAsync("PaymentType");
                    // Set the current user as the owner to check for ownership permissions on creation
                    newPaymentContent.Owner = userProfilePart.UserName;

                    await _contentManager.CreateAsync(newPaymentContent, VersionOptions.Draft);

                    if (newPaymentContent != null)
                    {
                        var newPaymentDetailPart = new PaymentDetailPart
                        {
                            PaymentId = newPaymentContent.Id,
                            TypeName = "charge",
                            ProviderName = method,
                            Amount = amountInRub,
                            Balance = currentBalance,
                            CreatedAt = DateTime.UtcNow,
                            Email = userProfilePart.Email,
                            UserId = userProfilePart.UserId,
                            UserName = userProfilePart.UserName,
                        };

                        newPaymentContent.Apply(newPaymentDetailPart);

                        var resultPayment = await _contentManager.ValidateAsync(newPaymentContent);

                        if (resultPayment.Succeeded)
                        {
                            await _contentManager.UpdateAsync(userContent);

                            await _contentManager.PublishAsync(newPaymentContent);

                            return true;
                        }
                        else
                        {
                            return false;
                        }
                    }

                }
                return true;
            }
            catch (Exception)
            {
                throw;
            }
        }

        private async Task<bool> UpdateSixSimBalanceByPMAsync(decimal amountInUsd, long userId, string method = "PM")
        {
            try
            {
                // Get UserProfile by userId
                var userContent = await _session
                .Query<ContentItem, ContentItemIndex>(index => index.ContentType == "UserProfileType" && index.Published && index.Latest)
                .With<UserProfilePartIndex>(p => p.UserId == userId)
                .FirstOrDefaultAsync();

                var exchangeRateRUBContent = await _session
                    .Query<ContentItem, ContentItemIndex>(index => index.ContentType == "ExchangeRate" && index.DisplayText == "RUB" && index.Published && index.Latest)
                    .FirstOrDefaultAsync();

                string rubRateString = exchangeRateRUBContent.Content["ExchangeRate"]["RateToUsd"]["Text"];
                decimal rubRateDouble = Decimal.Parse(rubRateString);

                if (userContent != null)
                {
                    var amountInRub = amountInUsd / rubRateDouble;

                    var content = userContent.Content;
                    var userProfilePart = content["UserProfilePart"];
                    decimal currentBalance = userProfilePart.Balance;

                    currentBalance += amountInRub; // All is RUB currency

                    var newUserProfilePart = new UserProfilePart
                    {
                        ProfileId = userProfilePart.ProfileId,
                        Email = userProfilePart.Email,
                        UserId = userProfilePart.UserId,
                        UserName = userProfilePart.UserName,
                        Vendor = userProfilePart.Vendor,
                        DefaultForwardingNumber = userProfilePart.DefaultForwardingNumber,
                        Balance = currentBalance,
                        OriginalAmount = amountInUsd,
                        Amount = amountInRub,
                        GmailMsgId = "",
                        RateInUsd = rubRateDouble,
                        Rating = userProfilePart.Rating,
                        DefaultCoutryName = userProfilePart.DefaultCoutryName,
                        DefaultIso = userProfilePart.DefaultIso,
                        DefaultPrefix = userProfilePart.DefaultPrefix,
                        DefaultOperatorName = userProfilePart.DefaultOperatorName,
                        FrozenBalance = userProfilePart.FrozenBalance
                    };

                    userContent.Apply(newUserProfilePart);
                    userContent.Owner = userProfilePart.UserName;
                    userContent.Author = userProfilePart.UserName;

                    // Create new PaymentType
                    var newPaymentContent = await _contentManager.NewAsync("PaymentType");
                    // Set the current user as the owner to check for ownership permissions on creation
                    newPaymentContent.Owner = userProfilePart.UserName;

                    await _contentManager.CreateAsync(newPaymentContent, VersionOptions.Draft);

                    if (newPaymentContent != null)
                    {
                        var newPaymentDetailPart = new PaymentDetailPart
                        {
                            PaymentId = newPaymentContent.Id,
                            TypeName = "charge",
                            ProviderName = method,
                            Amount = amountInRub,
                            Balance = currentBalance,
                            CreatedAt = DateTime.UtcNow,
                            Email = userProfilePart.Email,
                            UserId = userProfilePart.UserId,
                            UserName = userProfilePart.UserName,
                        };

                        newPaymentContent.Apply(newPaymentDetailPart);

                        var resultPayment = await _contentManager.ValidateAsync(newPaymentContent);

                        if (resultPayment.Succeeded)
                        {
                            await _contentManager.UpdateAsync(userContent);

                            await _contentManager.PublishAsync(newPaymentContent);

                            return true;
                        }
                        else
                        {
                            return false;
                        }
                    }

                }
                return true;
            }
            catch (Exception)
            {
                throw;
            }
        }

        private string GenerateJSONWebToken(string userName, string email)
        {
            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["Jwt:Key"]));
            var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

            var claims = new[]
            {
                new Claim(JwtRegisteredClaimNames.Sub, userName),
                new Claim(JwtRegisteredClaimNames.Email, email),
                new Claim("DateOfJoing", DateTime.UtcNow.ToString("yyyy-MM-dd")),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
            };

            var token = new JwtSecurityToken(_config["Jwt:Issuer"],
                _config["Jwt:Issuer"],
                claims,
                expires: DateTime.Now.AddYears(5),
                signingCredentials: credentials);

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}
