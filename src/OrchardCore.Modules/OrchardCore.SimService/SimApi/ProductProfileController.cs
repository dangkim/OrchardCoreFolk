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
using System.Collections;
using System.Linq;
using System.Dynamic;
using Microsoft.Extensions.Caching.Memory;
using OrchardCore.Environment.Cache;
using YesSql;
using IConfiguration = Microsoft.Extensions.Configuration.IConfiguration;
using OrchardCore.SimService.ApiCommonFunctions;
using Microsoft.AspNetCore.Identity;
using OrchardCore.Users;
using System.Net.Http;
using System.Threading;
using System.Net.Http.Json;
using System.IO;
using System.Text.Json;
using System.IO.Compression;
using Org.BouncyCastle.Asn1.Ocsp;

namespace OrchardCore.SimService.SimApi
{
    [Route("api/content/[action]/{country?}/{sixsimoperator?}/{product?}")]
    [ApiController]
    [Authorize(AuthenticationSchemes = "Api"), IgnoreAntiforgeryToken, AllowAnonymous]
    [OpenApiTag("Products and prices", Description = "Get information of products.")]
    public class ProductProfileController : Controller
    {
        private readonly ISession _session;
        private readonly UserManager<IUser> _userManager;
        private readonly IContentManager _contentManager;
        private readonly IAuthorizationService _authorizationService;
        private readonly IConfiguration _config;
        private readonly IMemoryCache _memoryCache;
        private readonly ISignal _signal;
        private readonly IHttpClientFactory _httpClientFactory;

        public ProductProfileController(
            ISession session,
            IMemoryCache memoryCache,
            ISignal signal,
            IContentManager contentManager,
            UserManager<IUser> userManager,
            IAuthorizationService authorizationService,
            Microsoft.Extensions.Configuration.IConfiguration config,
            IHttpClientFactory httpClientFactory)
        {
            _session = session;
            _memoryCache = memoryCache;
            _signal = signal;
            _contentManager = contentManager;
            _userManager = userManager;
            _authorizationService = authorizationService;
            _config = config;
            _httpClientFactory = httpClientFactory;
        }

        #region API Product

        #region Products request
        /// <summary>
        /// Products request
        /// </summary>
        /// <remarks>
        /// Provides payments history.
        /// </remarks>
        [HttpGet]
        [ActionName("products")]
        [ProducesResponseType(typeof(ProductsRequestDto), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(ProblemDetails), (int)HttpStatusCode.Unauthorized)]
        [ProducesResponseType(typeof(ProblemDetails), (int)HttpStatusCode.BadRequest)]
        [ProducesResponseType(typeof(ProblemDetails), (int)HttpStatusCode.InternalServerError)]
        [ReDocCodeSample("php", "$token = 'Your token';" +
            "\n$ch = curl_init();" +
            "\n$country = 'russia';" +
            "\n$operator= 'any';" +
            "\ncurl_setopt($ch, CURLOPT_URL, 'https://openapi.simforrent.com/api/content/products/' . $country . '/' . $operator');" +
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
            "\noperator = 'any'" +
            "\nheaders = {" +
            "\n'Accept': 'application/json'," +
            "\n}" +
            "\nresponse = requests.get('https://openapi.simforrent.com/api/content/products/' + country + '/' + operator, headers=headers)")]
        [ReDocCodeSample("c#", "var client = new RestClient(\"https://openapi.simforrent.com/api/user/products\");" +
            "\nvar request = new RestRequest();" +
            "\nrequest.AddHeader(\"Accept\", \"application/json\");" +
            "\nvar response = await client.ExecuteGetAsync(request);")]
        [AllowAnonymous]
        public async Task<IActionResult> ProductsRequestAsync(string country, string sixsimoperator, string product = "")
        {
            var percentStringValue = await ApiCommon.ReadCache(_session, _memoryCache, _signal, _config, "Percentage");
            var percent = string.IsNullOrEmpty(percentStringValue) ? 20 : int.Parse(percentStringValue);

            var url = string.Format("https://5sim.net/v1/guest/products/{0}/{1}", country, sixsimoperator);

            if (!string.IsNullOrEmpty(product))
            {
                url = string.Format("https://5sim.net/v1/guest/products/{0}/{1}/{2}", country, sixsimoperator, product);
            }

            using var httpClient = _httpClientFactory.CreateClient("fsim");

            using var response = await httpClient.GetAsync(url);

            var responseData = string.IsNullOrEmpty(product) ? await response.Content.ReadFromJsonAsync<Dictionary<string, FSProduct>>() :
                                                                new Dictionary<string, FSProduct> { { "default", await response.Content.ReadFromJsonAsync<FSProduct>() } };

            var productObjects = new Dictionary<string, FSProduct>(responseData);

            foreach (var item in productObjects)
            {
                var price = item.Value.Price + (item.Value.Price * percent / 100);
                item.Value.Price = price;
            }

            return Ok(productObjects);

        }
        #endregion

        #region Prices request
        /// <summary>
        /// Prices request
        /// </summary>
        /// <remarks>
        /// Provides payments history.
        /// </remarks> 
        [HttpGet]
        [ActionName("prices")]
        [ProducesResponseType(typeof(PricesRequestDto), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(ProblemDetails), (int)HttpStatusCode.Unauthorized)]
        [ProducesResponseType(typeof(ProblemDetails), (int)HttpStatusCode.BadRequest)]
        [ProducesResponseType(typeof(ProblemDetails), (int)HttpStatusCode.InternalServerError)]
        [ReDocCodeSample("php", "$ch = curl_init();" +
            "\ncurl_setopt($ch, CURLOPT_URL, 'https://openapi.simforrent.com/api/user/prices');" +
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
            "\nheaders = {" +
            "\n'Accept': 'application/json'," +
            "\n}" +
            "\nresponse = requests.get('https://openapi.simforrent.com/api/user/prices', headers=headers)")]
        [ReDocCodeSample("c#", "var client = new RestClient(\"https://openapi.simforrent.com/api/user/prices\");" +
            "\nvar request = new RestRequest();" +
            "\nrequest.AddHeader(\"Accept\", \"application/json\");" +
            "\nvar response = await client.ExecuteGetAsync(request);")]
        [AllowAnonymous]
        public async Task<ActionResult<object>> PricesRequestAsync()
        {
            var url = string.Format("https://5sim.net/v1/guest/prices");

            var httpClient = _httpClientFactory.CreateClient("fsim");

            using var response = await httpClient.GetAsync(url);

            // Check if response is compressed with gzip
            //if (response.Content.Headers.ContentEncoding.Contains("gzip"))
            //{
            //    // Read compressed response content as stream
            //    using var compressedStream = await response.Content.ReadAsStreamAsync();
            //    using var gzipStream = new GZipStream(compressedStream, CompressionMode.Decompress);
            //    using var reader = new StreamReader(gzipStream);
            //    // Read and handle uncompressed response content
            //    var responseBody = await reader.ReadToEndAsync();
            //    return Ok(responseBody);
            //}
            //else
            //{
            // Read uncompressed response content as string
            var responseBody = await response.Content.ReadFromJsonAsync<object>();
            return Ok(responseBody);
            //}

            //var responseData = await response.Content.ReadFromJsonAsync<object>();
            //var responseStream = await response.Content.ReadAsStreamAsync();

            //var buffer = new byte[16384];
            //int bytesRead;
            //var responseBody = new StringBuilder();

            //while ((bytesRead = await responseStream.ReadAsync(buffer)) > 0)
            //{
            //    // Append the chunk of data to the StringBuilder
            //    responseBody.Append(Encoding.UTF8.GetString(buffer, 0, bytesRead));
            //}

            //var responseData = JsonSerializer.Deserialize<object>(responseBody.ToString());

            //return Ok(responseData);
        }
        #endregion

        #region Prices by country
        /// <summary>
        /// Prices by country
        /// </summary>
        /// <remarks>
        /// Returns product prices by country.
        /// </remarks> 
        [HttpGet]
        [ActionName("pricesbycountry")]
        [ProducesResponseType(typeof(PricesRequestDto), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(ProblemDetails), (int)HttpStatusCode.Unauthorized)]
        [ProducesResponseType(typeof(ProblemDetails), (int)HttpStatusCode.BadRequest)]
        [ProducesResponseType(typeof(ProblemDetails), (int)HttpStatusCode.InternalServerError)]
        [ReDocCodeSample("php", "$ch = curl_init();" +
            "\n$country = 'russia';" +
            "\ncurl_setopt($ch, CURLOPT_URL, 'https://openapi.simforrent.com/api/user/pricesbycountry?country=' . $country');" +
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
            "\nresponse = requests.get('https://openapi.simforrent.com/api/user/pricesbycountry', headers=headers, params=params)")]
        [ReDocCodeSample("c#", "var client = new RestClient(\"https://openapi.simforrent.com/api/user/pricesbycountry\");" +
            "\nvar request = new RestRequest();" +
            "\nrequest.AddHeader(\"Accept\", \"application/json\");" +
            "\nvar response = await client.ExecuteGetAsync(request);")]
        public async Task<ActionResult<PricesRequestDto>> PricesByCountryRequestAsync(string country)
        {
            string url = string.Format("https://5sim.net/v1/guest/prices?country={0}", country);

            var client = new RestClient(url);
            var request = new RestRequest();

            var response = await client.ExecuteGetAsync(request);
            var resObject = JsonSerializer.Deserialize<PricesRequestDto>(response.Content);
            return Ok(resObject);
        }
        #endregion

        #region Prices by product
        /// <summary>
        /// Prices by product
        /// </summary>
        /// <remarks>
        /// Returns product prices by country.
        /// </remarks> 
        [HttpGet]
        [ActionName("pricesbyproduct")]
        [ProducesResponseType(typeof(PricesByProductDto), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(ProblemDetails), (int)HttpStatusCode.Unauthorized)]
        [ProducesResponseType(typeof(ProblemDetails), (int)HttpStatusCode.BadRequest)]
        [ProducesResponseType(typeof(ProblemDetails), (int)HttpStatusCode.InternalServerError)]
        [ReDocCodeSample("php", "$ch = curl_init();" +
            "\n$product = 'telegram';" +
            "\ncurl_setopt($ch, CURLOPT_URL, 'https://openapi.simforrent.com/api/user/pricesbyproduct?product=' . $product');" +
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
            "\nproduct = 'telegram'" +
            "\nheaders = {" +
            "\n'Accept': 'application/json'," +
            "\n}" +
            "\nparams = (" +
            "\n    ('product', product)," +
            "\n)" +
            "\nresponse = requests.get('https://openapi.simforrent.com/api/user/pricesbyproduct', headers=headers, params=params)")]
        [ReDocCodeSample("c#", "var client = new RestClient(\"https://openapi.simforrent.com/api/user/pricesbyproduct\");" +
            "\nvar request = new RestRequest();" +
            "\nrequest.AddHeader(\"Accept\", \"application/json\");" +
            "\nvar response = await client.ExecuteGetAsync(request);")]
        public async Task<ActionResult<PricesByProductDto>> PricesByProductRequestAsync(string product)
        {
            string url = string.Format("https://5sim.net/v1/guest/prices?product={0}", product);

            var client = new RestClient(url);
            var request = new RestRequest();

            var response = await client.ExecuteGetAsync(request);
            var resObject = JsonSerializer.Deserialize<PricesByProductDto>(response.Content);
            return Ok(resObject);
        }
        #endregion

        #region Prices by country and product
        /// <summary>
        /// Prices by country and product
        /// </summary>
        /// <remarks>
        /// Returns product prices by country and specific product.
        /// </remarks> 
        [HttpGet]
        [ActionName("pricesbycountryandproduct")]
        [ProducesResponseType(typeof(PricesByCountryandProduct), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(ProblemDetails), (int)HttpStatusCode.Unauthorized)]
        [ProducesResponseType(typeof(ProblemDetails), (int)HttpStatusCode.BadRequest)]
        [ProducesResponseType(typeof(ProblemDetails), (int)HttpStatusCode.InternalServerError)]
        [AllowAnonymous]
        public async Task<ActionResult<PricesByCountryandProduct>> PricesByCountryandProductRequestAsync(string country, string product)
        {
            string url = string.Format("https://5sim.net/v1/guest/prices?country={0}&product={1}", country, product);

            var client = new RestClient(url);
            var request = new RestRequest();
            //request.AddHeader("Authorization", "Bearer " + fiveSimToken);
            //request.AddHeader("Accept", "application/json");
            //var response = await client.ExecuteGetAsync<PaymentsHistoryDto>(request);
            //return Ok(response);

            var response = await client.ExecuteGetAsync(request);
            var resObject = JsonSerializer.Deserialize<PricesByCountryandProduct>(response.Content);
            return Ok(resObject);
        }
        #endregion

        #endregion

        private static void AddProperty(ExpandoObject expando, string propertyName, object propertyValue)
        {
            // ExpandoObject supports IDictionary so we can extend it like this
            var expandoDict = expando as IDictionary<string, object>;
            if (expandoDict.ContainsKey(propertyName))
                expandoDict[propertyName] = propertyValue;
            else
                expandoDict.Add(propertyName, propertyValue);
        }

        public class FSProduct
        {
            public string Category { get; set; }
            public int Qty { get; set; }
            public decimal Price { get; set; }
        }
    }
}
