using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using NSwag.Annotations;
using OrchardCore.ContentManagement;
using OrchardCore.SimService.ApiCommonFunctions;
using OrchardCore.SimService.ApiModels;
using OrchardCore.Environment.Cache;
using CommonPermissions = OrchardCore.Contents.CommonPermissions;
using OrchardCore.Users;
using RestSharp;
using YesSql;
using IConfiguration = Microsoft.Extensions.Configuration.IConfiguration;
using OrchardCore.SimService.Permissions;

namespace OrchardCore.SimService.SimApi
{
    [Route("api/content/[action]")]
    [ApiController]
    [Authorize(AuthenticationSchemes = "Api"), IgnoreAntiforgeryToken]
    [OpenApiTag("Rates", Description = "Get information of Rates.")]
    public class RateProfileController : Controller
    {
        private readonly UserManager<IUser> _userManager;
        private readonly IContentManager _contentManager;
        private readonly IAuthorizationService _authorizationService;
        private readonly IConfiguration _config;
        private readonly ISession _session;
        private readonly IMemoryCache _memoryCache;
        private readonly ISignal _signal;

        public RateProfileController(
            UserManager<IUser> userManager,
            ISession session,
            IMemoryCache memoryCache,
            ISignal signal,
            IContentManager contentManager,
            IAuthorizationService authorizationService,
            Microsoft.Extensions.Configuration.IConfiguration config)
        {
            _userManager = userManager;
            _session = session;
            _memoryCache = memoryCache;
            _signal = signal;
            _contentManager = contentManager;
            _authorizationService = authorizationService;
            _config = config;
        }

        #region Rates
        /// <summary>
        /// Get rates
        /// </summary>
        /// <remarks>
        /// Returns rates.
        /// </remarks> 
        [HttpGet]
        [ActionName("ratesrefill")]
        //[ProducesResponseType(typeof(GetCountriesListDto), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(ProblemDetails), (int)HttpStatusCode.Unauthorized)]
        [ProducesResponseType(typeof(ProblemDetails), (int)HttpStatusCode.BadRequest)]
        [ProducesResponseType(typeof(ProblemDetails), (int)HttpStatusCode.InternalServerError)]
        [AllowAnonymous]
        public async Task<IActionResult> GetOperatorsListAsync()
        {
            //var user = await _userManager.GetUserAsync(User) as Users.Models.User;
            //if (user == null) return BadRequest();

            var fiveSimToken = await ApiCommon.ReadCache(_session, _memoryCache, _signal, _config);

            string url = string.Format("https://5sim.net/v1/user/payment/settings");

            var client = new RestClient(url);
            var request = new RestRequest();
            request.AddHeader("Authorization", "Bearer " + fiveSimToken);

            var response = await client.ExecuteGetAsync(request);
            if (response.ResponseStatus == ResponseStatus.Error)
            {
                var rate = new RateRefillDto() { perfect_money_usd_rate = 79.6229 };
                return Ok(rate);
            }
            return Ok(response.Content);
        }
        #endregion

        #region RatesUSD
        /// <summary>
        /// Get rates USD
        /// </summary>
        /// <remarks>
        /// Returns rates.
        /// </remarks> 
        [HttpGet]
        [ActionName("ratesusd")]
        [ProducesResponseType(typeof(ProblemDetails), (int)HttpStatusCode.Unauthorized)]
        [ProducesResponseType(typeof(ProblemDetails), (int)HttpStatusCode.BadRequest)]
        [ProducesResponseType(typeof(ProblemDetails), (int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> GetRateUsdListAsync()
        {
            var user = await _userManager.GetUserAsync(User) as Users.Models.User;
            if (user == null || !user.IsEnabled || !user.EmailConfirmed) return BadRequest();
            if (!await _authorizationService.AuthorizeAsync(User, SimApiPermissions.AccessContentApi))
            {
                return this.ChallengeOrForbid();
            }

            var fiveSimToken = await ApiCommon.ReadCache(_session, _memoryCache, _signal, _config);

            string url = string.Format("https://api.exchangerate-api.com/v4/latest/USD");

            var client = new RestClient(url);
            var request = new RestRequest();
            //request.AddHeader("Authorization", "Bearer " + fiveSimToken);

            var response = await client.ExecuteGetAsync(request);

            JObject responseConvert = JObject.Parse(response.Content);
            responseConvert.Property("provider").Remove();
            responseConvert.Property("WARNING_UPGRADE_TO_V6").Remove();
            responseConvert.Property("terms").Remove();

            return Ok(responseConvert);
        }
        #endregion

    }
}

