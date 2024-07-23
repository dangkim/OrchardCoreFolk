using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using NSwag.Annotations;
using OrchardCore.ContentManagement;
using OrchardCore.SimService.ApiModels;
using OrchardCore.SimService.RedocAttributeProcessors;
using OrchardCore.Users;
using RestSharp;

namespace OrchardCore.SimService.SimApi
{
    [Route("api/content/[action]")]
    [ApiController]
    [Authorize(AuthenticationSchemes = "Api"), IgnoreAntiforgeryToken]
    [OpenApiTag("Operators list", Description = "Get information of operators list.")]
    public class OperatorProfileController : Controller
    {
        public string fiveSimToken;
        private readonly SignInManager<IUser> _signInManager;
        private readonly UserManager<IUser> _userManager;
        private readonly IContentManager _contentManager;
        private readonly IAuthorizationService _authorizationService;

        public OperatorProfileController(
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

        #region API Operators

        #region Operators by country
        /// <summary>
        /// Operators by country of Ware House Two
        /// </summary>
        /// <remarks>
        /// Returns operators by country.
        /// </remarks> 
        [HttpGet]
        [ActionName("operatorsWareHouseTwo")]
        [ProducesResponseType(typeof(OperatorsWareHouseTwoDto), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(ProblemDetails), (int)HttpStatusCode.Unauthorized)]
        [ProducesResponseType(typeof(ProblemDetails), (int)HttpStatusCode.BadRequest)]
        [ProducesResponseType(typeof(ProblemDetails), (int)HttpStatusCode.InternalServerError)]
        [ReDocCodeSample("php", "$ch = curl_init();" +
            "\n$country = 'russia';" +
            "\ncurl_setopt($ch, CURLOPT_URL, 'https://openapi.simforrent.com/api/user/operatorsWareHouseTwo?countryId=' . $country');" +
            "\ncurl_setopt($ch, CURLOPT_RETURNTRANSFER, 1);" +
            "\ncurl_setopt($ch, CURLOPT_CUSTOMREQUEST, 'GET');" +
            "\n$headers = array();" +
            "\n$headers[] = 'Accept: application/json';" +
            "\ncurl_setopt($ch, CURLOPT_HTTPHEADER, $headers);" +
            "\n$result = curl_exec($ch);" +
            "\nif (curl_errno($ch)) {" +
            "\necho 'Error:' . curl_error($ch);" +
            "\n}" +
            "curl_close($ch);")]
        [ReDocCodeSample("python", "import requests" +
            "\ncountry = 'russia'" +
            "\nheaders = {" +
            "\n'Accept': 'application/json'," +
            "\n}" +
            "\nparams = (" +
            "\n    ('country', country)," +
            "\n)" +
            "\nresponse = requests.get('https://openapi.simforrent.com/api/user/operatorsWareHouseTwo', headers=headers, params=params)")]
        [ReDocCodeSample("c#", "var client = new RestClient(\"https://openapi.simforrent.com/api/user/operatorsWareHouseTwo\");" +
            "\nvar request = new RestRequest();" +
            "\nrequest.AddHeader(\"Accept\", \"application/json\");" +
            "\nvar response = await client.ExecuteGetAsync(request);")]
        public async Task<ActionResult<OperatorsWareHouseTwoDto>> PricesByCountryRequestAsync(string countryId)
        {
            string url = string.Format("https://2ndline.io/apiv1/availableoperator?countryId={0}", countryId);

            var client = new RestClient(url);
            var request = new RestRequest();

            var response = await client.ExecuteGetAsync(request);

            return Ok(response.Content);
        }
        #endregion

        #endregion
    }
}
