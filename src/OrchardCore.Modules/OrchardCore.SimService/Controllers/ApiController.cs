using System;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using OrchardCore.ContentManagement;
using OrchardCore.SimService.Models;
using CommonPermissions = OrchardCore.Contents.CommonPermissions;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using OrchardCore.Email;
using OrchardCore.Users;
using Microsoft.AspNetCore.Cors;
using System.Text.RegularExpressions;
using System.Web;
using YesSql;
using OrchardCore.ContentManagement.Records;
using OrchardCore.Users.Services;
using OrchardCore.SimService.ApiModels;
using OrchardCore.SimService.ApiCommonFunctions;
using System.Collections.Generic;
using YesSql.Services;
using OrchardCore.Settings;
using OrchardCore.Users.Models;
using System.Security.Claims;
using Microsoft.AspNetCore.DataProtection;
using OrchardCore.Entities;
using OrchardCore.Modules;
using SignInResult = Microsoft.AspNetCore.Identity.SignInResult;
using OrchardCore.Users.Events;
using OrchardCore.Users.Handlers;
using Newtonsoft.Json.Serialization;
using OrchardCore.Scripting;
using OrchardCore.OpenId.Services;
using OrchardCore.OpenId.Settings;
using System.ComponentModel.DataAnnotations;
using NSwag.Annotations;
using OrchardCore.SimService.Permissions;
using OrchardCore.Users.ViewModels;
using System.Text.Json.Settings;

namespace OrchardCore.SimService.Controllers
{
    [Route("api/content/[action]")]
    [ApiController]
    [Authorize(AuthenticationSchemes = "Api"), IgnoreAntiforgeryToken, AllowAnonymous]
    [OpenApiIgnore]
    public class ApiController : Controller
    {
        private static readonly JsonMergeSettings _updateJsonMergeSettings = new JsonMergeSettings { MergeArrayHandling = MergeArrayHandling.Replace };
        private readonly Microsoft.Extensions.Configuration.IConfiguration _config;
        private readonly IContentManager _contentManager;
        private readonly IAuthorizationService _authorizationService;
        private readonly IStringLocalizer S;
        private readonly UserManager<IUser> _userManager;
        private readonly IEmailAddressValidator _emailAddressValidator;
        private readonly ILogger _logger;
        private readonly YesSql.ISession _session;
        private readonly IUserService _userService;
        private readonly ISiteService _siteService;
        private readonly SignInManager<IUser> _signInManager;
        private readonly IEnumerable<ILoginFormEvent> _accountEvents;
        private readonly IScriptingManager _scriptingManager;
        private readonly IEnumerable<IExternalLoginEventHandler> _externalLoginHandlers;
        private readonly IDataProtectionProvider _dataProtectionProvider;
        private readonly IClock _clock;
        private readonly IOpenIdClientService _clientService;


        public string btcPayliteApiUrl;
        public string btcPayliteToken;


        public ApiController(
            IUserService userService,
            Microsoft.Extensions.Configuration.IConfiguration config,
            YesSql.ISession session,
            IContentManager contentManager,
            IAuthorizationService authorizationService,
            UserManager<IUser> userManager,
            IEmailAddressValidator emailAddressValidator,
            ILogger<ApiController> logger,
            ISiteService siteService,
            SignInManager<IUser> signInManager,
            IEnumerable<ILoginFormEvent> accountEvents,
            IScriptingManager scriptingManager,
            IEnumerable<IExternalLoginEventHandler> externalLoginHandlers,
            IDataProtectionProvider dataProtectionProvider,
            IClock clock,
            IOpenIdClientService clientService,
            IStringLocalizer<ApiController> stringLocalizer)
        {
            _userService = userService;
            _config = config;
            _contentManager = contentManager;
            _authorizationService = authorizationService;
            _userManager = userManager;
            _emailAddressValidator = emailAddressValidator ?? throw new ArgumentNullException(nameof(emailAddressValidator));
            _logger = logger;
            _session = session;
            _signInManager = signInManager;
            _siteService = siteService;
            _accountEvents = accountEvents;
            _scriptingManager = scriptingManager;
            _externalLoginHandlers = externalLoginHandlers;
            _dataProtectionProvider = dataProtectionProvider;
            _clock = clock;
            _clientService = clientService;
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
            if (!await _authorizationService.AuthorizeAsync(User, SimApiPermissions.AccessContentApi))
            {
                return this.ChallengeOrForbid();
            }

            var contentItem = await _contentManager.GetAsync(contentItemId);

            if (contentItem == null)
            {
                return NotFound();
            }

            if (!await _authorizationService.AuthorizeAsync(User, CommonPermissions.ViewContent, contentItem))
            {
                return this.ChallengeOrForbid();
            }

            return Ok(contentItem);
        }

        [HttpPost]
        public async Task<IActionResult> Post(ContentItem model, bool draft = false)
        {
            if (!await _authorizationService.AuthorizeAsync(User, SimApiPermissions.AccessContentApi))
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

                contentItem.Merge(model, _updateJsonMergeSettings);

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
        [EnableCors("MyPolicy")]
        [ActionName("Register")]
        [AllowAnonymous]
        public async Task<IActionResult> Register(RegisterModel model)
        {
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
                else
                {
                    model.UserName = model.Email;
                }
            }

            if (TryValidateModel(model) && ModelState.IsValid)
            {
                var redirectUrl = _config["VxFulFrontendUrl"];
                var iuser = await this.RegisterUser(model, S["Confirm your account"], _logger);
                var user = iuser as Users.Models.User;
                // If we get a user, redirect to returnUrl
                if (user != null)
                {
                    var time = BitConverter.GetBytes(DateTime.UtcNow.ToBinary());
                    var key = Guid.NewGuid().ToByteArray();
                    var token = Convert.ToBase64String(time.Concat(key).ToArray());

                    var newContentItem = await _contentManager.NewAsync("UserProfileType");

                    newContentItem.Owner = user.UserName;
                    newContentItem.Author = user.UserName;

                    var userProfilePart = new UserProfilePart()
                    {
                        Email = model.Email,
                        UserId = (user as Users.Models.User).Id,
                        UserName = user.UserName,
                        Vendor = "demo",
                        DefaultForwardingNumber = "",
                        Balance = 0m,
                        Rating = 96,
                        DefaultCoutryName = "vietnam",
                        DefaultIso = "vn",
                        DefaultPrefix = "+84",
                        DefaultOperatorName = "virtual16",
                        FrozenBalance = 0m,
                        TokenApi = token,
                    };

                    newContentItem.Apply(userProfilePart);

                    var createdResult = await _contentManager.UpdateValidateAndCreateAsync(newContentItem, VersionOptions.Published);

                    if (createdResult.Succeeded)
                    {
                        return Ok(userProfilePart);
                    }
                    else
                    {
                        return Problem(
                            title: S["One or more validation errors occurred."],
                            detail: String.Join(',', createdResult.Errors),
                            statusCode: (int)HttpStatusCode.BadRequest);
                    }
                }

            }

            // If we got this far, something failed, redisplay form
            return Problem(title: S["One or more validation errors occurred."],
                        detail: String.Join(',', ModelState.Values.FirstOrDefault().Errors.Select(e => e.ErrorMessage)),
                        statusCode: (int)HttpStatusCode.BadRequest);

        }

        [HttpPost]
        [EnableCors("MyPolicy")]
        [ActionName("ForgetPassword")]
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
                var userWithEmail = await _userManager.FindByEmailAsync(model.Email) as Users.Models.User;

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
        [EnableCors("MyPolicy")]
        [ActionName("ResetPassword")]
        public async Task<IActionResult> ResetPassword(ResetPasswordModel model)
        {            
            var user = await _userManager.FindByIdAsync(model.UserId) as Users.Models.User;

            if (user == null)
            {
                return NotFound();
            }

            //if (user.Email != model.Email)
            //{
            //    ModelState.AddModelError("Wrong User", S["Cannot change password of a wrong user"]);
            //}

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
        [EnableCors("MyPolicy")]
        [ActionName("GetUserInfo")]
        public async Task<IActionResult> GetUserInfo()
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
                decimal btcBalanceValue = 0;
                decimal ethBalanceValue = 0;
                decimal usdtBalanceValue = 0;
                decimal vndBalanceValue = 0;

                using var session = _session.Store.CreateSession();
                var traders = await session.Query<ContentItem, ContentItemIndex>(x => x.Published && x.Latest && x.DisplayText.Contains(user.Id.ToString()) && x.ContentType == "Trader").ListAsync();
                var currentTrader = traders.FirstOrDefault();

                var btcBalanceString = Convert.ToString(currentTrader.Content["TraderForFilteringPart"].BTCBalance);
                var ethBalanceString = Convert.ToString(currentTrader.Content["TraderForFilteringPart"].ETHBalance);
                var usdtBalanceString = Convert.ToString(currentTrader.Content["TraderForFilteringPart"].USDT20Balance);
                var vndBalanceString = Convert.ToString(currentTrader.Content["TraderForFilteringPart"].VndBalance);
                var userName = Convert.ToString(currentTrader.Content["TraderForFilteringPart"].Name);

                btcBalanceValue = btcBalanceString == null ? 0 : Decimal.Parse(btcBalanceString);
                ethBalanceValue = ethBalanceString == null ? 0 : Decimal.Parse(ethBalanceString);
                usdtBalanceValue = usdtBalanceString == null ? 0 : Decimal.Parse(usdtBalanceString);
                vndBalanceValue = vndBalanceString == null ? 0 : Decimal.Parse(vndBalanceString);

                var traderId = currentTrader.ContentItemId;

                var dicBalance = await ApiCommon.CalculateBalanceAsync(btcBalanceValue, ethBalanceValue, usdtBalanceValue, vndBalanceValue, user.UserName, _session);

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
                user.IsEnabled,
            };

            return Ok(userInfo);
        }

        [HttpPost]
        [ActionName("ExternalLogin")]
        [EnableCors("MyPolicy")]
        [AllowAnonymous]
        public IActionResult ExternalLogin(string provider, string email, string password, string returnUrl = null)
        {
            // Request a redirect to the external login provider.
            _logger.LogError("Go to ExternalLogin");
            var redirectUrl = _config["PaxHubUrl"] + "/" + nameof(ExternalLoginCallback) + "?returnUrl=" + returnUrl;
            var properties = _signInManager.ConfigureExternalAuthenticationProperties(provider, redirectUrl);
            return Challenge(properties, provider);
        }

        [HttpPost]
        [ActionName("EmailLogin")]
        [AllowAnonymous]
        public async Task<IActionResult> EmailLoginAsync(string email)
        {
            var user = await _userManager.FindByEmailAsync(email) as Users.Models.User;

            return Ok(user);
        }

        [HttpGet]
        [ActionName("ExternalLoginCallback")]
        [AllowAnonymous]
        public async Task<IActionResult> ExternalLoginCallback(string returnUrl = null, string remoteError = null)
        {
            _logger.LogError("ExternalLoginCallback");
            if (remoteError != null)
            {
                _logger.LogError("Error from external provider: {Error}", remoteError);
                return RedirectToLocal(returnUrl);
            }

            var info = await _signInManager.GetExternalLoginInfoAsync();
            if (info == null)
            {
                _logger.LogError("Could not get external login info.");
                return RedirectToLocal(returnUrl);
            }

            var registrationSettings = (await _siteService.GetSiteSettingsAsync()).As<RegistrationSettings>();
            var user = await _userManager.FindByLoginAsync(info.LoginProvider, info.ProviderKey);

            if (user != null)
            {
                await _accountEvents.InvokeAsync((e, user, modelState) => e.LoggingInAsync(user.UserName, (key, message) => modelState.AddModelError(key, message)), user, ModelState, _logger);
                var localEmail = info.Principal.FindFirstValue(ClaimTypes.Email) ?? info.Principal.FindFirstValue("email");

                var identityResult = await _signInManager.UpdateExternalAuthenticationTokensAsync(info);
                if (!identityResult.Succeeded)
                {
                    _logger.LogError("Error updating the external authentication tokens.");
                }
                else
                {
                    return RedirectToLocal(returnUrl, user.UserName, localEmail, true);

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

                    // Should go to the home page of Sixsim
                    return RedirectToLocal(returnUrl, user.UserName, email, true);
                }
                else
                {
                    // no user could be matched, check if a new user can register
                    if (registrationSettings.UsersCanRegister == UserRegistrationType.NoRegistration)
                    {
                        string message = S["Site does not allow user registration."];
                        _logger.LogWarning(message);
                        ModelState.AddModelError("", message);
                    }
                    else
                    {
                        var externalLoginViewModel = new RegisterExternalLoginViewModel();

                        externalLoginViewModel.NoPassword = registrationSettings.NoPasswordForExternalUsers;
                        externalLoginViewModel.NoEmail = registrationSettings.NoEmailForExternalUsers;
                        externalLoginViewModel.NoUsername = registrationSettings.NoUsernameForExternalUsers;

                        // If registrationSettings.NoUsernameForExternalUsers is true, this username will not be used
                        externalLoginViewModel.UserName = await GenerateUsernameAsync(info);
                        externalLoginViewModel.Email = email;

                        user = await this.RegisterUser(new RegisterViewModel()
                        {
                            UserName = externalLoginViewModel.UserName,
                            Email = externalLoginViewModel.Email,
                            Password = externalLoginViewModel.UserName + "A@",
                            ConfirmPassword = externalLoginViewModel.UserName + "A@",
                        }, S["Confirm your account"], _logger);

                        // If the registration was successful we can link the external provider and redirect the user
                        if (user != null)
                        {
                            var time = BitConverter.GetBytes(DateTime.UtcNow.ToBinary());
                            var key = Guid.NewGuid().ToByteArray();
                            var token = Convert.ToBase64String(time.Concat(key).ToArray());

                            // Create UserProfileType/Part
                            var newContentItem = await _contentManager.NewAsync("UserProfileType");

                            newContentItem.Owner = user.UserName;
                            newContentItem.Author = user.UserName;

                            var userProfilePart = new UserProfilePart()
                            {
                                Email = email,
                                UserId = (user as Users.Models.User).Id,
                                UserName = user.UserName,
                                Vendor = "demo",
                                DefaultForwardingNumber = "",
                                Balance = 0m,
                                Rating = 96,
                                DefaultCoutryName = "vietnam",
                                DefaultIso = "vn",
                                DefaultPrefix = "+84",
                                DefaultOperatorName = "virtual16",
                                FrozenBalance = 0m,
                                TokenApi = token
                            };

                            newContentItem.Apply(userProfilePart);
                            var createdResult = await _contentManager.UpdateValidateAndCreateAsync(newContentItem, VersionOptions.Published);

                            if (createdResult.Succeeded)
                            {
                                return RedirectToLocal(returnUrl, externalLoginViewModel.UserName, email, true);
                            }

                            return RedirectToLocal(returnUrl);
                        }

                        return View("RegisterExternalLogin", externalLoginViewModel);
                    }
                }
            }

            return RedirectToLocal(returnUrl);
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
            var loginResultModel = new LoginResultModel();

            var claims = info.Principal.GetSerializableClaims();
            var userRoles = await _userManager.GetRolesAsync(user);
            var context = new UpdateRolesContext(user, info.LoginProvider, claims, userRoles);

            var rolesToAdd = new List<string>();
            var rolesToRemove = new List<string>();

            var loginSettings = (await _siteService.GetSiteSettingsAsync()).As<LoginSettings>();

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

        private async Task<string> GenerateUsernameAsync(ExternalLoginInfo info)
        {
            var ret = string.Concat("u", IdGenerator.GenerateId());
            var externalClaims = info?.Principal.GetSerializableClaims();
            var userNames = new Dictionary<Type, string>();

            foreach (var item in _externalLoginHandlers)
            {
                try
                {
                    var userName = await item.GenerateUserName(info.LoginProvider, externalClaims.ToArray());
                    if (!string.IsNullOrWhiteSpace(userName))
                    {
                        // Set the return value to the username generated by the first IExternalLoginHandler.
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

            return ret;
        }

        private void AddIdentityErrors(IdentityResult result)
        {
            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }
        }

        private IActionResult RedirectToLocal(string returnUrl, string userName = "", string email = "", bool isFromGoogle = false)
        {
            if (isFromGoogle)
            {
                return Redirect(_config["VxFulFrontendUrl"] + "/login?username=" + userName + "&isgoogle=true&email=" + email);
            }

            var url = _config["VxFulFrontendUrl"] + returnUrl;

            return Redirect(url);
        }

        private async Task<OpenIdClientSettings> GetClientSettingsAsync()
        {
            var settings = await _clientService.GetSettingsAsync();
            if ((await _clientService.ValidateSettingsAsync(settings)).Any(result => result != ValidationResult.Success))
            {
                _logger.LogWarning("The OpenID Connect module is not correctly configured.");

                return null;
            }

            return settings;
        }

    }
}
