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
using System.Web;
using YesSql;
using OrchardCore.Users.Services;
using OrchardCore.SongServices.Models;
using OrchardCore.SongServices.Controllers;
using RestSharp;
using AngleSharp.Dom;
using OrchardCore.SongServices.ViewModels;

namespace OrchardCore.SongServices.Controllers
{
    [Route("api/content/[action]")]
    [ApiController]
    [Authorize(AuthenticationSchemes = "Api"), IgnoreAntiforgeryToken, AllowAnonymous]
    public class TwoFAController : Controller
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

        public TwoFAController(
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

        //[HttpPost]
        //[ActionName("Check2FACode")]
        //public async Task<IActionResult> Check2FACode(TwoFaCodeModel request)
        //{
        //    var user = await _userManager.FindByNameAsync(User.Identity.Name) as User;
        //    var btcApiCheck2FACodeUrl = $"{btcPayliteApiUrl}/api/v1/users/check2facode/{request.TwoFaCode}/{user.Email}";

        //    using var client = new RestClient(btcApiCheck2FACodeUrl);
        //    var restRequest = new RestRequest();

        //    restRequest.AddHeader("Authorization", btcPayliteToken);

        //    try
        //    {
        //        var response = await client.ExecuteGetAsync(restRequest);
        //        return Ok(response.Content);
        //    }
        //    catch
        //    {
        //        return Problem(
        //                        title: S["One or more validation errors occurred."],
        //                        detail: "BTC userName is not found.",
        //                        statusCode: (int)HttpStatusCode.BadRequest);
        //    }
        //}


        //[HttpPost]
        //[ActionName("Verify2FACode")]
        //[AllowAnonymous]
        //public async Task<IActionResult> Verify2FACode(VerifyTwoFaModel request)
        //{
        //    var user = await _userManager.FindByIdAsync(request.UserId) as User;
        //    var btcApiVerify2FACodeUrl = $"{btcPayliteApiUrl}/api/v1/users/verify2facode/{request.TwoFaCode}/{user.Email}";

        //    using var client = new RestClient(btcApiVerify2FACodeUrl);
        //    var restRequest = new RestRequest();

        //    restRequest.AddHeader("Authorization", btcPayliteToken);

        //    try
        //    {
        //        var response = await client.ExecuteGetAsync(restRequest);
                
        //        if (Convert.ToBoolean(response.Content))
        //        {
        //            var result = await _userManager.ConfirmEmailAsync(user, HttpUtility.UrlDecode(request.ConfirmEmailCode));
        //            if (result.Succeeded)
        //            {
        //                return Ok(true);
        //            }
        //            else
        //            {
        //                return Problem(
        //                        title: S["One or more validation errors occurred."],
        //                        detail: "Invalid token , this token was expired. Try resend another confirmation email",
        //                        statusCode: (int)HttpStatusCode.BadRequest);
        //            }
        //        }
        //        else
        //        {
        //            return Problem(
        //                        title: S["One or more validation errors occurred."],
        //                        detail: "Verification code is not valid, try another code",
        //                        statusCode: (int)HttpStatusCode.BadRequest);
        //        }
        //    }
        //    catch
        //    {
        //        return Problem(
        //                        title: S["One or more validation errors occurred."],
        //                        detail: "BTC userName is not found.",
        //                        statusCode: (int)HttpStatusCode.BadRequest);
        //    }
        //}

        //[HttpGet]
        //[ActionName("GetUser2FAQrCode")]
        //[AllowAnonymous]
        //public async Task<IActionResult> GetUser2FAQrCode(string userId)
        //{
        //    var user = await _userManager.FindByIdAsync(userId) as User;
        //    var btcApiGetUser2FAQrCodeUrl = $"{btcPayliteApiUrl}/api/v1/users/getuser2faqrcode/{user.Email}";

        //    using var client = new RestClient(btcApiGetUser2FAQrCodeUrl);
        //    var restRequest = new RestRequest();

        //    restRequest.AddHeader("Authorization", btcPayliteToken);

        //    try
        //    {
        //        var response = await client.ExecuteGetAsync(restRequest);
        //        return Ok(response.Content);
        //    }
        //    catch
        //    {
        //        ModelState.AddModelError("userName", S["BTC userName is not found."]);
        //    }

        //    return Problem(
        //        title: S["One or more validation errors occurred."],
        //        detail: String.Join(',', ModelState.Values.FirstOrDefault().Errors.Select(e => e.ErrorMessage)),
        //        statusCode: (int)HttpStatusCode.BadRequest);
        //}

    }
}
