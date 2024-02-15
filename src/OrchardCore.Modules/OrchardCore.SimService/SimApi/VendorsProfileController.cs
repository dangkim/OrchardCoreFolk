using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using NSwag.Annotations;
using OrchardCore.ContentManagement;
using OrchardCore.SimService.ApiModels;
using OrchardCore.SimService.RedocAttributeProcessors;
using RestSharp;
using Newtonsoft.Json;


namespace OrchardCore.SimService.SimApi
{
    [Route("api/content/[action]")]
    [ApiController]
    [Authorize(AuthenticationSchemes = "Api"), IgnoreAntiforgeryToken, AllowAnonymous]
    [OpenApiTag("Vendors", Description = "Get information of vendors.")]
    public class VendorsProfileController : Controller
    {
        public string fiveSimToken;
        private readonly IContentManager _contentManager;
        private readonly IAuthorizationService _authorizationService;
        private readonly IConfiguration _config;

        public VendorsProfileController(
            IContentManager contentManager,
            IAuthorizationService authorizationService,
            Microsoft.Extensions.Configuration.IConfiguration config)
        {
            _contentManager = contentManager;
            _authorizationService = authorizationService;
            _config = config;
            fiveSimToken = _config["FiveSimToken"];
        }

        #region API Vendors

        #region Vendor statistic
        /// <summary>
        /// Vendor statistic
        /// </summary>
        /// <remarks>
        /// 
        /// </remarks> 
        [HttpGet]
        [ActionName("vendorstatistic")]
        [ProducesResponseType(typeof(VendorStatisticDto), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(ProblemDetails), (int)HttpStatusCode.Unauthorized)]
        [ProducesResponseType(typeof(ProblemDetails), (int)HttpStatusCode.BadRequest)]
        [ProducesResponseType(typeof(ProblemDetails), (int)HttpStatusCode.InternalServerError)]
        [ReDocCodeSample("php", "$token = 'Your token';" +
            "\n$ch = curl_init();" +
            "\ncurl_setopt($ch, CURLOPT_URL, 'https://openapi.simforrent.com/api/user/vendorstatistic');" +
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
            "\nheaders = {" +
            "\n'Authorization': 'Bearer ' + token," +
            "\n'Accept': 'application/json'," +
            "\n}" +
            "\nresponse = requests.get('https://openapi.simforrent.com/api/user/vendorstatistic', headers=headers)")]
        [ReDocCodeSample("c#", "var client = new RestClient(\"https://openapi.simforrent.com/api/user/vendorstatistic\");" +
            "\nvar request = new RestRequest();" +
            "\nrequest.AddHeader(\"Authorization\", \"Bearer \" + token);" +
            "\nrequest.AddHeader(\"Accept\", \"application/json\");" +
            "\nvar response = await client.ExecuteGetAsync(request);")]
        [AllowAnonymous]
        public async Task<ActionResult<VendorStatisticDto>> VendorStatisticAsync()
        {
            string url = string.Format("https://5sim.net/v1/user/vendor");

            var client = new RestClient(url);
            var request = new RestRequest();
            request.AddHeader("Authorization", "Bearer " + fiveSimToken);
            //request.AddHeader("Accept", "application/json");
            //var response = await client.ExecuteGetAsync<PaymentsHistoryDto>(request);
            //return Ok(response);

            var response = await client.ExecuteGetAsync(request);
            var resObject = JsonConvert.DeserializeObject<VendorStatisticDto>(response.Content);
            return Ok(resObject);
        }
        #endregion

        #region Wallets reserve

        /// <summary>
        /// Wallets reserve
        /// </summary>
        /// <remarks>
        /// Available reserves currency for partner.
        /// </remarks> 
        [HttpGet]
        [ActionName("walletsreserve")]
        [ProducesResponseType(typeof(WalletsreserveDto), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(ProblemDetails), (int)HttpStatusCode.Unauthorized)]
        [ProducesResponseType(typeof(ProblemDetails), (int)HttpStatusCode.BadRequest)]
        [ProducesResponseType(typeof(ProblemDetails), (int)HttpStatusCode.InternalServerError)]
        [ReDocCodeSample("php", "$token = 'Your token';" +
            "\n$ch = curl_init();" +
            "\ncurl_setopt($ch, CURLOPT_URL, 'https://openapi.simforrent.com/api/user/walletsreserve');" +
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
            "\nheaders = {" +
            "\n'Authorization': 'Bearer ' + token," +
            "\n'Accept': 'application/json'," +
            "\n}" +
            "\nresponse = requests.get('https://openapi.simforrent.com/api/user/walletsreserve', headers=headers)")]
        [ReDocCodeSample("c#", "var client = new RestClient(\"https://openapi.simforrent.com/api/user/walletsreserve\");" +
            "\nvar request = new RestRequest();" +
            "\nrequest.AddHeader(\"Authorization\", \"Bearer \" + token);" +
            "\nrequest.AddHeader(\"Accept\", \"application/json\");" +
            "\nvar response = await client.ExecuteGetAsync(request);")]
        [AllowAnonymous]
        public async Task<ActionResult<WalletsreserveDto>> WalletsReserveAsync()
        {
            string url = string.Format("https://5sim.net/v1/vendor/wallets");

            var client = new RestClient(url);
            var request = new RestRequest();
            request.AddHeader("Authorization", "Bearer " + fiveSimToken);
            //request.AddHeader("Accept", "application/json");
            //var response = await client.ExecuteGetAsync<PaymentsHistoryDto>(request);
            //return Ok(response);

            var response = await client.ExecuteGetAsync(request);
            var resObject = JsonConvert.DeserializeObject<WalletsreserveDto>(response.Content);
            return Ok(resObject);
        }
        #endregion

        #region Vendor orders history

        /// <summary>
        /// Vendor orders history
        /// </summary>
        /// <remarks>
        /// Provides vendor's orders history by chosen category.
        /// </remarks> 
        [HttpGet]
        [ActionName("vendorordershistory")]
        [ProducesResponseType(typeof(VendorOrdersHistoryDto), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(ProblemDetails), (int)HttpStatusCode.Unauthorized)]
        [ProducesResponseType(typeof(ProblemDetails), (int)HttpStatusCode.BadRequest)]
        [ProducesResponseType(typeof(ProblemDetails), (int)HttpStatusCode.InternalServerError)]
        [ReDocCodeSample("php", "$token = 'Your token';" +
            "\n$ch = curl_init();" +
            "\n$category = 'activation';" +
            "\n$limit = 15;" +
            "\n$offset = 0;" +
            "\n$order = 'id';" +
            "\n$reverse = true;" +
            "\ncurl_setopt($ch, CURLOPT_URL, 'https://openapi.simforrent.com/api/user/vendorordershistory?category=' . $category . '&limit=' . $limit . '&offset=' . $offset . '&order=' . $id . '&reverse=' . $reverse');" +
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
            "\ncategory = 'activation';" +
            "\nlimit = 15;" +
            "\noffset = 0;" +
            "\norder = 'id';" +
            "\nreverse = true;" +
            "\nheaders = {" +
            "\n'Authorization': 'Bearer ' + token," +
            "\n'Accept': 'application/json'," +
            "\n}" +
            "\nparams = (" +
            "\n    ('category', category)," +
            "\n    ('limit', limit)," +
            "\n    ('offset', offset)," +
            "\n    ('order', order)," +
            "\n    ('reverse', reverse)," +
            "\n)" +
            "\nresponse = requests.get('https://openapi.simforrent.com/api/user/vendorordershistory', headers=headers, params=params)")]
        [ReDocCodeSample("c#", "var client = new RestClient(\"https://openapi.simforrent.com/api/user/vendorordershistory\");" +
            "\nvar request = new RestRequest();" +
            "\nrequest.AddHeader(\"Authorization\", \"Bearer \" + token);" +
            "\nrequest.AddHeader(\"Accept\", \"application/json\");" +
            "\nvar response = await client.ExecuteGetAsync(request);")]
        [AllowAnonymous]
        public async Task<ActionResult<VendorOrdersHistoryDto>> VendorordershistoryAsync(string category, int limit, int offset, string order, bool reverse)
        {
            string url = string.Format("https://5sim.net/v1/vendor/orders?category={0}&limit={1}&offset={2}&order={3}&reverse={4}", category, limit, offset, order, reverse);

            var client = new RestClient(url);
            var request = new RestRequest();
            request.AddHeader("Authorization", "Bearer " + fiveSimToken);
            //request.AddHeader("Accept", "application/json");
            //var response = await client.ExecuteGetAsync<PaymentsHistoryDto>(request);
            //return Ok(response);

            var response = await client.ExecuteGetAsync(request);
            var resObject = JsonConvert.DeserializeObject<VendorOrdersHistoryDto>(response.Content);
            return Ok(resObject);
        }
        #endregion

        #region Vendor payments history

        /// <summary>
        /// Vendor payments history
        /// </summary>
        /// <remarks>
        /// Provides vendor's payments history.
        /// </remarks> 
        [HttpGet]
        [ActionName("vendorpaymentshistory")]
        [ProducesResponseType(typeof(VendorPaymentsHistoryDto), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(ProblemDetails), (int)HttpStatusCode.Unauthorized)]
        [ProducesResponseType(typeof(ProblemDetails), (int)HttpStatusCode.BadRequest)]
        [ProducesResponseType(typeof(ProblemDetails), (int)HttpStatusCode.InternalServerError)]
        [ReDocCodeSample("php", "$token = 'Your token';" +
            "\n$ch = curl_init();" +
            "\n$limit = 15;" +
            "\n$offset = 0;" +
            "\n$order = 'id';" +
            "\n$reverse = true;" +
            "\ncurl_setopt($ch, CURLOPT_URL, 'https://openapi.simforrent.com/api/user/vendorpaymentshistory?limit=' . $limit . '&offset=' . $offset . '&order=' . $id . '&reverse=' . $reverse');" +
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
            "\ncategory = 'activation';" +
            "\nlimit = 15;" +
            "\noffset = 0;" +
            "\norder = 'id';" +
            "\nreverse = true;" +
            "\nheaders = {" +
            "\n'Authorization': 'Bearer ' + token," +
            "\n'Accept': 'application/json'," +
            "\n}" +
            "\nparams = (" +
            "\n    ('limit', limit)," +
            "\n    ('offset', offset)," +
            "\n    ('order', order)," +
            "\n    ('reverse', reverse)," +
            "\n)" +
            "\nresponse = requests.get('https://openapi.simforrent.com/api/user/vendorpaymentshistory', headers=headers, params=params)")]
        [ReDocCodeSample("c#", "var client = new RestClient(\"https://openapi.simforrent.com/api/user/vendorpaymentshistory\");" +
            "\nvar request = new RestRequest();" +
            "\nrequest.AddHeader(\"Authorization\", \"Bearer \" + token);" +
            "\nrequest.AddHeader(\"Accept\", \"application/json\");" +
            "\nvar response = await client.ExecuteGetAsync(request);")]
        [AllowAnonymous]
        public async Task<ActionResult<VendorPaymentsHistoryDto>> VendorpaymentshistoryAsync(int limit, int offset, string order, bool reverse)
        {
            string url = string.Format("https://5sim.net/v1/vendor/payments?limit={0}&offset={1}&order={2}&reverse={3}", limit, offset, order, reverse);

            var client = new RestClient(url);
            var request = new RestRequest();
            request.AddHeader("Authorization", "Bearer " + fiveSimToken);
            //request.AddHeader("Accept", "application/json");
            //var response = await client.ExecuteGetAsync<PaymentsHistoryDto>(request);
            //return Ok(response);

            var response = await client.ExecuteGetAsync(request);
            var resObject = JsonConvert.DeserializeObject<VendorPaymentsHistoryDto>(response.Content);
            return Ok(resObject);
        }
        #endregion

        #endregion

    }
}


