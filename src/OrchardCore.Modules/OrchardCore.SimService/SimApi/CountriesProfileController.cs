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
using System.Net.Http;
using System.Collections.Generic;
using System.Net.Http.Json;
using System.Threading;
using System.Linq;

namespace OrchardCore.SimService.SimApi
{
    [Route("api/content/[action]")]
    [ApiController]
    [Authorize(AuthenticationSchemes = "Api"), IgnoreAntiforgeryToken]
    [OpenApiTag("Countries list", Description = "Get information of countries list.")]
    public class CountriesProfileController : Controller
    {
        public string fiveSimToken;
        private readonly IHttpClientFactory _httpClientFactory;

        public CountriesProfileController(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }

        #region API Countries
        /// <summary>
        /// Get countries list
        /// </summary>
        /// <remarks>
        /// Returns a list of countries with available operators for purchase.
        /// </remarks> 
        [HttpGet]
        [ActionName("getstablecountries")]
        [ProducesResponseType(typeof(GetCountriesListDto), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(ProblemDetails), (int)HttpStatusCode.Unauthorized)]
        [ProducesResponseType(typeof(ProblemDetails), (int)HttpStatusCode.BadRequest)]
        [ProducesResponseType(typeof(ProblemDetails), (int)HttpStatusCode.InternalServerError)]
        [AllowAnonymous]
        public async Task<IActionResult> GetCountriesListAsync()
        {
            var url = string.Format("https://5sim.net/v1/guest/countries");

            using var httpClient = _httpClientFactory.CreateClient("fsim");

            using var response = await httpClient.GetAsync(url);

            var responseData = await response.Content.ReadAsStringAsync();            

            return Ok(responseData);
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


    public class CountryInfo
    {
        public Dictionary<string, int> Iso { get; set; }

        public string Text_En { get; set; }
    }

    public class CountryObject
    {
        public string Country { get; set; }
        public string Iso { get; set; }
        public string TextEn { get; set; }
    }
}

