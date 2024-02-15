using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NSwag.Annotations;
using OrchardCore.ContentManagement;
using OrchardCore.SimService.ApiModels;
using RestSharp;
using Microsoft.AspNetCore.Identity;
using OrchardCore.Users;
using Microsoft.AspNetCore.Authentication;
using OpenIddict.Server.AspNetCore;

namespace OrchardCore.SimService.SimApi
{
    [Route("api/content/[action]")]
    [ApiController]
    [Authorize(AuthenticationSchemes = "Api"), IgnoreAntiforgeryToken]
    [OpenApiTag("Countries list", Description = "Get information of countries list.")]
    public class CountriesProfileController : Controller
    {
        public string fiveSimToken;
        private readonly SignInManager<IUser> _signInManager;
        private readonly UserManager<IUser> _userManager;
        private readonly IContentManager _contentManager;
        private readonly IAuthorizationService _authorizationService;
        
        public CountriesProfileController(
            SignInManager<IUser> signInManager,
            UserManager<IUser> userManager,
            IContentManager contentManager,
            IAuthorizationService authorizationService)
        {
            _signInManager = signInManager;
            _userManager = userManager;
            _contentManager = contentManager;
            _authorizationService = authorizationService;
        }

        #region API Countries
        /// <summary>
        /// Get countries list
        /// </summary>
        /// <remarks>
        /// Returns a list of countries with available operators for purchase.
        /// </remarks> 
        [HttpGet]
        [ActionName("getcountries")]
        [ProducesResponseType(typeof(GetCountriesListDto), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(ProblemDetails), (int)HttpStatusCode.Unauthorized)]
        [ProducesResponseType(typeof(ProblemDetails), (int)HttpStatusCode.BadRequest)]
        [ProducesResponseType(typeof(ProblemDetails), (int)HttpStatusCode.InternalServerError)]
        [AllowAnonymous]
        public async Task<IActionResult> GetCountriesListAsync()
        {

            string url = string.Format("https://5sim.net/v1/guest/countries");
            var client = new RestClient(url);
            var request = new RestRequest();

            var response = await client.ExecuteGetAsync(request);
            return Ok(response.Content);
        }

        /// <summary>
        /// Get countries list of Ware House Two
        /// </summary>
        /// <remarks>
        /// Returns a list of countries with available operators for purchase.
        /// </remarks> 
        [HttpGet]
        [ActionName("getCountriesWareHouseTwo")]
        [ProducesResponseType(typeof(GetCountriesWareHouseTwoListDto), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(ProblemDetails), (int)HttpStatusCode.Unauthorized)]
        [ProducesResponseType(typeof(ProblemDetails), (int)HttpStatusCode.BadRequest)]
        [ProducesResponseType(typeof(ProblemDetails), (int)HttpStatusCode.InternalServerError)]
        [AllowAnonymous]
        public async Task<IActionResult> GetCountriesWareHouse2ListAsync()
        {
            string url = string.Format("https://2ndline.io/apiv1/availablecountry");
            var client = new RestClient(url);
            var request = new RestRequest();

            var response = await client.ExecuteGetAsync(request);
            return Ok(response.Content);
        }
        #endregion

        /// <summary>
        /// Logoff
        /// </summary>
        /// <remarks>
        /// Log off account.
        /// </remarks> 
        [HttpPost]
        [ActionName("Logoff")]

        public async Task<IActionResult> Logout()
        {
            await this.HttpContext.SignOutAsync();

            return SignOut(OpenIddictServerAspNetCoreDefaults.AuthenticationScheme);
        }
    }
}


