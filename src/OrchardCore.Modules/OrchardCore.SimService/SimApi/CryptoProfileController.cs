using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using Newtonsoft.Json;
using NSwag.Annotations;
using OrchardCore.ContentManagement;
using OrchardCore.SimService.ApiCommonFunctions;
using OrchardCore.SimService.ApiModels;
using OrchardCore.SimService.Permissions;
using OrchardCore.Environment.Cache;
using OrchardCore.Users;
using RestSharp;
using OrchardCore.SimService.RedocAttributeProcessors;

namespace OrchardCore.SimService.SimApi
{
    [Route("api/content/[action]")]
    [ApiController]
    [Authorize(AuthenticationSchemes = "Api"), IgnoreAntiforgeryToken]
    [OpenApiTag("Crypto", Description = "Get information of cryto.")]
    public class CryptoProfileController : Controller
    {
        public string fiveSimToken;
        private readonly IContentManager _contentManager;
        private readonly IAuthorizationService _authorizationService;
        private readonly Microsoft.Extensions.Configuration.IConfiguration _config;
        private readonly YesSql.ISession _session;
        private readonly IMemoryCache _memoryCache;
        private readonly ISignal _signal;
        private readonly UserManager<IUser> _userManager;

        public CryptoProfileController(
            IContentManager contentManager,
            UserManager<IUser> userManager,
            IAuthorizationService authorizationService,
            YesSql.ISession session,
            IMemoryCache memoryCache,
            ISignal signal,
            Microsoft.Extensions.Configuration.IConfiguration config)
        {
            _contentManager = contentManager;
            _userManager = userManager;
            _authorizationService = authorizationService;
            _config = config;
            _session = session;
            _memoryCache = memoryCache;
            _signal = signal;
        }

        #region BTC and LTC rates
        /// <summary>
        /// BTC and LTC rates
        /// </summary>
        /// <remarks>
        /// Return rates of crypto by pointed currency.
        /// </remarks> 
        [HttpGet]
        [ActionName("cryptorates")]
        [ProducesResponseType(typeof(CryptoRateDto), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(ProblemDetails), (int)HttpStatusCode.Unauthorized)]
        [ProducesResponseType(typeof(ProblemDetails), (int)HttpStatusCode.BadRequest)]
        [ProducesResponseType(typeof(ProblemDetails), (int)HttpStatusCode.InternalServerError)]
        [ReDocCodeSample("php", "$token = 'Your token';" +
            "\n$ch = curl_init();" +
            "\n$currency = 'RUB';" +
            "\ncurl_setopt($ch, CURLOPT_URL, 'https://openapi.simforrent.com/api/user/cryptorates?currency=' . $currency);" +
            "\ncurl_setopt($ch, CURLOPT_RETURNTRANSFER, 1);" +
            "\ncurl_setopt($ch, CURLOPT_CUSTOMREQUEST, 'GET');" +
            "\n$headers = array();" +
            "\n$headers[] = 'Authorization: Bearer ' . $token;" +
            "\n$headers[] = 'Accept: application/json';" +
            "\ncurl_setopt($ch, CURLOPT_HTTPHEADER, $headers);" +
            "\n$result = curl_exec($ch);" +
            "\nif (curl_errno($ch)) {" +
            "\necho 'Error:' . curl_error($ch);" +
            "\n}" +
            "curl_close($ch);")]
        [ReDocCodeSample("python", "import requests" +
            "\ntoken = 'Your token'" +
            "\ncurrency = 'RUB';" +
            "\nheaders = {" +
            "\n'Authorization': 'Bearer ' + token," +
            "\n'Accept': 'application/json'," +
            "\n}" +
            "\nparams = (" +
            "\n    ('currency', currency)," +
            "\n)" +
            "\nresponse = requests.get('https://openapi.simforrent.com/api/user/cryptorates', headers=headers, params=params)")]
        [ReDocCodeSample("c#", "var client = new RestClient(\"https://openapi.simforrent.com/api/user/cryptorates\");" +
            "\nvar request = new RestRequest();" +
            "\nrequest.AddHeader(\"Authorization\", \"Bearer \" + token);" +
            "\nrequest.AddHeader(\"Accept\", \"application/json\");" +
            "\nvar response = await client.ExecuteGetAsync(request);")]
        public async Task<ActionResult<CryptoRateDto>> CryptoRatesAsync(string currency)
        {
            var user = await _userManager.GetUserAsync(User) as Users.Models.User;

            if (user == null || !user.IsEnabled) return BadRequest();

            if (!await _authorizationService.AuthorizeAsync(User, SimApiPermissions.AccessContentApi))
            {
                return this.ChallengeOrForbid();
            }

            var fiveSimToken = await ApiCommon.ReadCache(_session, _memoryCache, _signal, _config);

            var url = string.Format("https://5sim.net/v1/user/payment/crypto/rates?currency={0}", currency);

            var client = new RestClient(url);
            var request = new RestRequest();
            request.AddHeader("Authorization", "Bearer " + fiveSimToken);
            var response = await client.ExecuteGetAsync(request);
            var resObject = JsonConvert.DeserializeObject<CryptoRateDto>(response.Content);
            return Ok(resObject);
        }
        #endregion

        #region Address payment
        /// <summary>
        /// Address payment
        /// </summary>
        /// <remarks>
        /// Return address to crypto payment.
        /// </remarks> 
        [HttpGet]
        [ActionName("addresspayment")]
        [ProducesResponseType(typeof(AddressPaymentDto), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(ProblemDetails), (int)HttpStatusCode.Unauthorized)]
        [ProducesResponseType(typeof(ProblemDetails), (int)HttpStatusCode.BadRequest)]
        [ProducesResponseType(typeof(ProblemDetails), (int)HttpStatusCode.InternalServerError)]
        [ReDocCodeSample("php", "$token = 'Your token';" +
            "\n$ch = curl_init();" +
            "\n$currency = 'BTC';" +
            "\ncurl_setopt($ch, CURLOPT_URL, 'https://openapi.simforrent.com/api/user/addresspayment?currency=' . $currency);" +
            "\ncurl_setopt($ch, CURLOPT_RETURNTRANSFER, 1);" +
            "\ncurl_setopt($ch, CURLOPT_CUSTOMREQUEST, 'GET');" +
            "\n$headers = array();" +
            "\n$headers[] = 'Authorization: Bearer ' . $token;" +
            "\n$headers[] = 'Accept: application/json';" +
            "\ncurl_setopt($ch, CURLOPT_HTTPHEADER, $headers);" +
            "\n$result = curl_exec($ch);" +
            "\nif (curl_errno($ch)) {" +
            "\necho 'Error:' . curl_error($ch);" +
            "\n}" +
            "curl_close($ch);")]
        [ReDocCodeSample("python", "import requests" +
            "\ntoken = 'Your token'" +
            "\ncurrency = 'BTC';" +
            "\nheaders = {" +
            "\n'Authorization': 'Bearer ' + token," +
            "\n'Accept': 'application/json'," +
            "\n}" +
            "\nparams = (" +
            "\n    ('currency', currency)," +
            "\n)" +
            "\nresponse = requests.get('https://openapi.simforrent.com/api/user/addresspayment', headers=headers, params=params)")]
        [ReDocCodeSample("c#", "var client = new RestClient(\"https://openapi.simforrent.com/api/user/addresspayment\");" +
            "\nvar request = new RestRequest();" +
            "\nrequest.AddHeader(\"Authorization\", \"Bearer \" + token);" +
            "\nrequest.AddHeader(\"Accept\", \"application/json\");" +
            "\nvar response = await client.ExecuteGetAsync(request);")]
        public async Task<ActionResult<AddressPaymentDto>> AddressPaymentAsync(string currency)
        {
            var user = await _userManager.GetUserAsync(User) as Users.Models.User;

            if (user == null || !user.IsEnabled) return BadRequest();

            if (!await _authorizationService.AuthorizeAsync(User, SimApiPermissions.AccessContentApi))
            {
                return this.ChallengeOrForbid();
            }

            var fiveSimToken = await ApiCommon.ReadCache(_session, _memoryCache, _signal, _config);

            string url = string.Format("https://5sim.net/v1/user/payment/crypto/getaddress?currency={0}", currency);

            var client = new RestClient(url);
            var request = new RestRequest();
            request.AddHeader("Authorization", "Bearer " + fiveSimToken);

            var response = await client.ExecuteGetAsync(request);
            var resObject = JsonConvert.DeserializeObject<AddressPaymentDto>(response.Content);
            return Ok(resObject);
        }
        #endregion

    }
}

