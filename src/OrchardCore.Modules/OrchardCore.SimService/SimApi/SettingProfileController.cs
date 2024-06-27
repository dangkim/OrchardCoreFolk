using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using NSwag.Annotations;
using OrchardCore.ContentManagement;
using OrchardCore.SimService.ApiCommonFunctions;
using OrchardCore.SimService.ApiModels;
using OrchardCore.Environment.Cache;
using CommonPermissions = OrchardCore.Contents.CommonPermissions;
using OrchardCore.Users;
using RestSharp;
using YesSql;
using OrchardCore.SimService.Permissions;

namespace OrchardCore.SimService.SimApi
{
    [Route("api/content/[action]")]
    [ApiController]
    [Authorize(AuthenticationSchemes = "Api"), IgnoreAntiforgeryToken]
    [OpenApiTag("Settings", Description = "Get information of settings.")]
    public class SettingProfileController : Controller
    {
        private readonly IContentManager _contentManager;
        private readonly IAuthorizationService _authorizationService;
        private readonly Microsoft.Extensions.Configuration.IConfiguration _config;
        private readonly UserManager<IUser> _userManager;
        private readonly ISession _session;
        private readonly IMemoryCache _memoryCache;
        private readonly ISignal _signal;

        public SettingProfileController(
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

        #region Setting countries
        /// <summary>
        /// Get setting countries
        /// </summary>
        /// <remarks>
        /// Returns setting countries.
        /// </remarks> 
        [HttpGet]
        [ActionName("settingcountries")]
        [ProducesResponseType(typeof(GetCountriesListDto), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(ProblemDetails), (int)HttpStatusCode.Unauthorized)]
        [ProducesResponseType(typeof(ProblemDetails), (int)HttpStatusCode.BadRequest)]
        [ProducesResponseType(typeof(ProblemDetails), (int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> GetCountriesListAsync()
        {
            var user = await _userManager.GetUserAsync(User) as Users.Models.User;
            if (user == null || !user.IsEnabled || !user.EmailConfirmed) return BadRequest();
            if (!await _authorizationService.AuthorizeAsync(User, SimApiPermissions.AccessContentApi))
            {
                return this.ChallengeOrForbid();
            }

            var fiveSimToken = await ApiCommon.ReadCache(_session, _memoryCache, _signal, _config);

            string url = string.Format("https://5sim.net/v1/user/settings/countries");

            var client = new RestClient(url);
            var request = new RestRequest();
            request.AddHeader("Authorization", "Bearer " + fiveSimToken);
            var response = await client.ExecuteGetAsync(request);
            return Ok(response.Content);
        }
        #endregion

        #region Setting operators
        /// <summary>
        /// Get settings operators
        /// </summary>
        /// <remarks>
        /// Returns settings operators.
        /// </remarks> 
        [HttpGet]
        [ActionName("settingoperators")]
        [ProducesResponseType(typeof(GetCountriesListDto), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(ProblemDetails), (int)HttpStatusCode.Unauthorized)]
        [ProducesResponseType(typeof(ProblemDetails), (int)HttpStatusCode.BadRequest)]
        [ProducesResponseType(typeof(ProblemDetails), (int)HttpStatusCode.InternalServerError)]
        [AllowAnonymous]
        public async Task<IActionResult> GetOperatorsListAsync()
        {
            var user = await _userManager.GetUserAsync(User) as Users.Models.User;
            if (user == null || !user.IsEnabled || !user.EmailConfirmed) return BadRequest();
            if (!await _authorizationService.AuthorizeAsync(User, SimApiPermissions.AccessContentApi))
            {
                return this.ChallengeOrForbid();
            }

            var fiveSimToken = await ApiCommon.ReadCache(_session, _memoryCache, _signal, _config);

            string url = string.Format("https://5sim.net/v1/user/settings/operators");

            var client = new RestClient(url);
            var request = new RestRequest();
            request.AddHeader("Authorization", "Bearer " + fiveSimToken);
            var response = await client.ExecuteGetAsync(request);
            return Ok(response.Content);
        }
        #endregion

        #region Setting purge ban phones
        /// <summary>
        /// Get settings purge ban phones
        /// </summary>
        /// <remarks>
        /// Returns settings purge ban phones.
        /// </remarks> 
        [HttpGet]
        [ActionName("purgeban")]
        [ProducesResponseType(typeof(GetCountriesListDto), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(ProblemDetails), (int)HttpStatusCode.Unauthorized)]
        [ProducesResponseType(typeof(ProblemDetails), (int)HttpStatusCode.BadRequest)]
        [ProducesResponseType(typeof(ProblemDetails), (int)HttpStatusCode.InternalServerError)]
        [AllowAnonymous]
        public async Task<IActionResult> GetPurgeBanListAsync()
        {
            var user = await _userManager.GetUserAsync(User) as Users.Models.User;
            if (user == null || !user.IsEnabled || !user.EmailConfirmed) return BadRequest();
            if (!await _authorizationService.AuthorizeAsync(User, SimApiPermissions.AccessContentApi))
            {
                return this.ChallengeOrForbid();
            }

            var fiveSimToken = await ApiCommon.ReadCache(_session, _memoryCache, _signal, _config);

            string url = string.Format("https://5sim.net/v1/user/settings/purge-ban-phones");

            var client = new RestClient(url);
            var request = new RestRequest();
            request.AddHeader("Authorization", "Bearer " + fiveSimToken);
            var response = await client.ExecuteGetAsync(request);
            return Ok(response.Content);
        }
        #endregion

    }
}
