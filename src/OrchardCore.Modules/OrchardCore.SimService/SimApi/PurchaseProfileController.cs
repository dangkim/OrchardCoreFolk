using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using Newtonsoft.Json;
using NSwag.Annotations;
using OrchardCore.ContentManagement;
using OrchardCore.ContentManagement.Records;
using OrchardCore.SimService.ApiCommonFunctions;
using OrchardCore.SimService.ApiModels;
using OrchardCore.SimService.Indexes;
using OrchardCore.SimService.Models;
using OrchardCore.SimService.RedocAttributeProcessors;
using OrchardCore.SimService.ViewModels;
using CommonPermissions = OrchardCore.Contents.CommonPermissions;
using OrchardCore.Environment.Cache;
using OrchardCore.Users;
using RestSharp;
using YesSql;
using OrchardCore.SimService.Permissions;
using System.Net.Http;
using System.Net.Http.Headers;
using static OrchardCore.SimService.SimApi.ProductProfileController;
using System.Collections.Generic;
using System.Net.Http.Json;

namespace OrchardCore.SimService.SimApi
{
    [Route("api/content/[action]/{country}/{operato}/{product}")]
    [ApiController]
    [Authorize(AuthenticationSchemes = "Api"), IgnoreAntiforgeryToken]
    [OpenApiTag("Purchase", Description = "Get information of purchase.")]
    public class PurchaseProfileController : Controller
    {
        private readonly ISession _session;
        private readonly IContentManager _contentManager;
        private readonly UserManager<IUser> _userManager;
        private readonly IAuthorizationService _authorizationService;
        private readonly Microsoft.Extensions.Configuration.IConfiguration _config;
        private readonly IMemoryCache _memoryCache;
        private readonly ISignal _signal;
        private readonly IHttpClientFactory _httpClientFactory;

        public PurchaseProfileController(
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

        #region Buy activation number
        /// <summary>
        /// Buy activation number
        /// </summary>
        /// <remarks>
        /// 
        /// </remarks> 
        [HttpGet]
        [ActionName("buyactivation")]
        [ProducesResponseType(typeof(BuyActionNumberDto), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(ProblemDetails), (int)HttpStatusCode.Unauthorized)]
        [ProducesResponseType(typeof(ProblemDetails), (int)HttpStatusCode.BadRequest)]
        [ProducesResponseType(typeof(ProblemDetails), (int)HttpStatusCode.InternalServerError)]
        [ReDocCodeSample("php", "$token = 'Your token';" +
            "\n$ch = curl_init();" +
            "\n$country = 'russia'" +
            "\n$operator = 'any'" +
            "\n$product = 'amazon'" +
            "\ncurl_setopt($ch, CURLOPT_URL, 'https://openapi.simforrent.com/api/content/buyactivation/'. country . '/' . operator . '/' . product');" +
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
            "\ncountry = 'russia'" +
            "\noperator = 'any'" +
            "\nproduct = 'amazon'" +
            "\nheaders = {" +
            "\n'Authorization': 'Bearer ' + token," +
            "\n'Accept': 'application/json'," +
            "\n}" +
            "\nresponse = requests.get('https://openapi.simforrent.com/api/content/buyactivation'+ country + '/' + operator + '/' + product, headers=headers)")]
        [ReDocCodeSample("c#", "var client = new RestClient(\"https://openapi.simforrent.com/api/content/activation\");" +
            "\nvar request = new RestRequest();" +
            "\nrequest.AddHeader(\"Authorization\", \"Bearer \" + token);" +
            "\nrequest.AddHeader(\"Accept\", \"application/json\");" +
            "\nvar response = await client.ExecuteGetAsync(request);")]
        public async Task<ActionResult<BuyActionNumberDto>> BuyActionNumberAsync(string country, string operato, string product)
        {
            var url = string.Format("user/buy/activation/{0}/{1}/{2}", country, operato, product);
            var user = await _userManager.GetUserAsync(User) as Users.Models.User;

            if (user == null || !user.IsEnabled) return BadRequest();
            if (!await _authorizationService.AuthorizeAsync(User, SimApiPermissions.AccessContentApi))
            {
                return this.ChallengeOrForbid();
            }

            var fiveSimToken = await ApiCommon.ReadCache(_session, _memoryCache, _signal, _config);
            var percentStringValue = await ApiCommon.ReadCache(_session, _memoryCache, _signal, _config, "Percentage");
            var percent = string.IsNullOrEmpty(percentStringValue) ? 20 : int.Parse(percentStringValue);

            var userContent = await _session
                    .Query<ContentItem, ContentItemIndex>(index => index.ContentType == "UserProfile" && index.Published && index.Latest)
                    .With<UserProfilePartIndex>(p => p.UserId == user.Id)
                    .FirstOrDefaultAsync();

            if (userContent != null)
            {
                var content = userContent.Content;
                var userProfilePart = content["UserProfilePart"];
                decimal currentBalance = userProfilePart.Balance;

                if (currentBalance <= 0)
                {
                    return Ok(new ErrorModel { Error = "no balance" });
                }

                var newOrderContent = await _contentManager.NewAsync("Orders");

                if (newOrderContent != null)
                {
                    // Set the current user as the owner to check for ownership permissions on creation
                    newOrderContent.Owner = User.Identity.Name;
                    newOrderContent.Author = User.Identity.Name;

                    if (!await _authorizationService.AuthorizeAsync(User, CommonPermissions.EditOwnContent, newOrderContent))
                    {
                        return Forbid();
                    }

                    using var httpClient = _httpClientFactory.CreateClient("fsim");

                    httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", fiveSimToken);

                    using var response = await httpClient.GetAsync(url);

                    var responseData = await response.Content.ReadAsStringAsync();

                    if (responseData == "no free phones")
                    {
                        return Ok(response.Content);
                    }

                    await _contentManager.CreateAsync(newOrderContent, VersionOptions.Draft);

                    var resObject = await response.Content.ReadFromJsonAsync<OrderDetailPartViewModel>();

                    //Check User's Balance
                    var priceProduct = resObject.Price + (resObject.Price * percent / 100);

                    //var content = userContent.Content;
                    //var userProfilePart = content["UserProfilePart"];
                    //decimal currentBalance = userProfilePart.Balance;
                    if (currentBalance <= 0 || currentBalance < priceProduct)
                    {
                        return Ok("You don't have enough money");
                    }

                    //Save Purchase Profile into Database
                    var newOrderDetailPart = new OrderDetailPart
                    {
                        InventoryId = 1,//TODO: Temp
                        OrderId = resObject.Id,
                        Phone = resObject.Phone,
                        Operator = resObject.Operator,
                        Product = resObject.Product,
                        Price = resObject.Price + (resObject.Price * percent / 100),
                        Status = resObject.Status,
                        Expires = resObject.Expires,
                        Created_at = resObject.Created_at,
                        Country = resObject.Country,
                        Category = "activation",
                        Email = user.Email,
                        UserId = user.Id,
                        UserName = user.UserName
                    };

                    newOrderContent.Apply(newOrderDetailPart);

                    var result = await _contentManager.ValidateAsync(newOrderContent);

                    if (result.Succeeded)
                    {
                        newOrderContent.Latest = true;
                        await _contentManager.PublishAsync(newOrderContent);
                    }

                    return Ok(newOrderDetailPart);
                }
            }

            return BadRequest();
        }
        #endregion
    }
}
