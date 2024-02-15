using System;
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
using OrchardCore.Environment.Cache;
using CommonPermissions = OrchardCore.Contents.CommonPermissions;
using OrchardCore.Users;
using RestSharp;
using YesSql;
using OrchardCore.SimService.Permissions;

namespace OrchardCore.SimService.SimApi
{
    [Route("api/content/[action]/{serviceId}/{country}/{network}/{price}/{number?}")]
    [ApiController]
    [Authorize(AuthenticationSchemes = "Api"), IgnoreAntiforgeryToken]
    [OpenApiTag("PurchaseWareHouseFive", Description = "Get information of purchase of Ware House Five.")]
    public class PurchaseWareHouseFiveProfileController : Controller
    {
        private readonly ISession _session;
        private readonly IContentManager _contentManager;
        private readonly UserManager<IUser> _userManager;
        private readonly IAuthorizationService _authorizationService;
        private readonly Microsoft.Extensions.Configuration.IConfiguration _config;
        private readonly IMemoryCache _memoryCache;
        private readonly ISignal _signal;

        public PurchaseWareHouseFiveProfileController(
         ISession session,
         IMemoryCache memoryCache,
         ISignal signal,
         IContentManager contentManager,
         UserManager<IUser> userManager,
         IAuthorizationService authorizationService,
         Microsoft.Extensions.Configuration.IConfiguration config)
        {
            _session = session;
            _memoryCache = memoryCache;
            _signal = signal;
            _contentManager = contentManager;
            _userManager = userManager;
            _authorizationService = authorizationService;
            _config = config;
        }

        #region Buy activation number
        /// <summary>
        /// Buy activation number of  Ware House Five
        /// </summary>
        /// <remarks>
        /// 
        /// </remarks> 
        [HttpGet]
        [ActionName("buyActivationWareHouseFive")]
        [ProducesResponseType(typeof(BuyActionNumberWareHouseFiveDto), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(ProblemDetails), (int)HttpStatusCode.Unauthorized)]
        [ProducesResponseType(typeof(ProblemDetails), (int)HttpStatusCode.BadRequest)]
        [ProducesResponseType(typeof(ProblemDetails), (int)HttpStatusCode.InternalServerError)]
        [ReDocCodeSample("php", "$token = 'Your token';" +
            "\n$ch = curl_init();" +
            "\n$operator = 'any'" +
            "\n$product = '1001'" +
            "\ncurl_setopt($ch, CURLOPT_URL, 'https://openapi.simforrent.com/api/content/buyActivationWareHouseFive/' . serviceId . '/'  . country . '/' . network . '/' . price');" +
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
            "\noperator = 'any'" +
            "\nproduct = '1001'" +
            "\nheaders = {" +
            "\n'Authorization': 'Bearer ' + token," +
            "\n'Accept': 'application/json'," +
            "\n}" +
            "\nresponse = requests.get('https://openapi.simforrent.com/api/content/buyActivationWareHouseFive' + '/' + serviceId  + '/' + country + '/' + network + '/' + price, headers=headers)")]
        [ReDocCodeSample("c#", "var client = new RestClient(\"https://openapi.simforrent.com/api/content/buyActivationWareHouseFive\");" +
            "\nvar request = new RestRequest();" +
            "\nrequest.AddHeader(\"Authorization\", \"Bearer \" + token);" +
            "\nrequest.AddHeader(\"Accept\", \"application/json\");" +
            "\nvar response = await client.ExecuteGetAsync(request);")]
        public async Task<ActionResult<BuyActionNumberWareHouseFiveDto>> BuyActionNumberWareHouseFiveAsync(int serviceId, string country, string network, decimal price)
        {
            var vSimToken = await ApiCommon.ReadCache(_session, _memoryCache, _signal, _config, "VSimToken");
            var percentStringValue = await ApiCommon.ReadCache(_session, _memoryCache, _signal, _config, "VSimPercentage");
            var percent = string.IsNullOrEmpty(percentStringValue) ? 50 : int.Parse(percentStringValue);

            var exchangeRateRUBContent = await _session
                    .Query<ContentItem, ContentItemIndex>(index => index.ContentType == "ExchangeRate" && index.DisplayText == "RUB" && index.Published && index.Latest)
                    .FirstOrDefaultAsync();

            string rubRateString = exchangeRateRUBContent.Content["ExchangeRate"]["RateToUsd"]["Text"];
            decimal rubRateDouble = Decimal.Parse(rubRateString);

            var user = await _userManager.GetUserAsync(User) as Users.Models.User;

            if (user == null || !user.IsEnabled) return BadRequest();
            if (!await _authorizationService.AuthorizeAsync(User, SimApiPermissions.AccessContentApi))
            {
                return this.ChallengeOrForbid();
            }

            var userContent = await _session
                .Query<ContentItem, ContentItemIndex>(index => index.ContentType == "UserProfileType" && index.Published && index.Latest)
                .With<UserProfilePartIndex>(p => p.UserId == user.Id)
                .FirstOrDefaultAsync();

            //Check User's Balance
            var content = userContent.Content;
            var userProfilePart = content["UserProfilePart"];
            decimal currentBalance = userProfilePart.Balance;
            if (currentBalance <= 0 || currentBalance < price)
            {
                return Ok("You don't have enough money");
            }

            var newOrderContent = await _contentManager.NewAsync("OrderType");

            if (newOrderContent != null)
            {
                // Set the current user as the owner to check for ownership permissions on creation
                newOrderContent.Owner = User.Identity.Name;
                newOrderContent.Author = User.Identity.Name;

                if (!await _authorizationService.AuthorizeAsync(User, CommonPermissions.EditContent, newOrderContent))
                {
                    return Forbid();
                }

                var url = "";
                if (string.IsNullOrEmpty(network) || network.Equals("null"))
                {
                    switch (country)
                    {
                        case "ro":
                            network = "VODAFONE";
                            break;
                        case "kh":
                            network = "METFONE";
                            break;
                        default:
                            network = "MOBIFONE";
                            break;
                    }
                    url = string.Format("https://api.viotp.com/request/getv2?token={0}&serviceId={1}&network={2}", vSimToken, serviceId, network);
                }
                else
                {
                    network = network.ToUpper();
                    url = string.Format("https://api.viotp.com/request/getv2?token={0}&serviceId={1}&network={2}", vSimToken, serviceId, network);
                }

                var client = new RestClient(url);
                var request = new RestRequest();

                var response = await client.ExecuteGetAsync(request);

                var resObject = JsonConvert.DeserializeObject<BuyActionNumberWareHouseFiveDto>(response.Content);

                if (resObject.status_code != 200)
                {
                    return Ok("Error.");
                }

                await _contentManager.CreateAsync(newOrderContent, VersionOptions.Draft);

                var resultObject = resObject.message[0];

                if (string.IsNullOrEmpty(network))
                {
                    network = "any";
                }

                var urlProduct = string.Format("https://api.viotp.com/service/getv2?token={0}&country={1}", vSimToken, country);

                var clientProduct = new RestClient(urlProduct);
                var requestProduct = new RestRequest();

                var responseProduct = await clientProduct.ExecuteGetAsync(requestProduct);
                var resObjectProduct = JsonConvert.DeserializeObject<ProductsWareHouseFiveRequestDto>(responseProduct.Content);

                if (resObjectProduct.status_code != 200)
                {
                    return BadRequest();
                }

                var productObjectsProduct = resObjectProduct.data;
                DataProduct dataProduct = productObjectsProduct.Find(product => product.id == serviceId);

                var newOrderDetailPart = new OrderDetailPart
                {
                    InventoryId = (int)InventoryEnum.VSim,
                    OrderId = long.Parse(((int)InventoryEnum.VSim).ToString() + resObject.data.request_id),
                    Phone = resObject.data.re_phone_number,
                    Operator = network,
                    Product = dataProduct.name,
                    Price = price,
                    Status = Enum.GetName(typeof(OrderStatusLSimEnum), OrderStatusLSimEnum.WAITING),
                    Expires = DateTime.UtcNow.AddMinutes(5),
                    Created_at = DateTime.UtcNow,
                    Country = country,
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
            return BadRequest();
        }
        #endregion

        #region Buy activation number again
        /// <summary>
        /// Buy again activation number of  Ware House Five
        /// </summary>
        /// <remarks>
        /// 
        /// </remarks> 
        [HttpGet]
        [ActionName("buyActivationWareHouseFiveAgain")]
        [ProducesResponseType(typeof(BuyActionNumberWareHouseFiveDto), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(ProblemDetails), (int)HttpStatusCode.Unauthorized)]
        [ProducesResponseType(typeof(ProblemDetails), (int)HttpStatusCode.BadRequest)]
        [ProducesResponseType(typeof(ProblemDetails), (int)HttpStatusCode.InternalServerError)]
        [ReDocCodeSample("php", "$token = 'Your token';" +
            "\n$ch = curl_init();" +
            "\n$operator = 'any'" +
            "\n$product = '1001'" +
            "\ncurl_setopt($ch, CURLOPT_URL, 'https://openapi.simforrent.com/api/content/buyActivationWareHouseFiveAgain/' . serviceId . '/'  . number . '/' . network . '/' . country . '/' . price');" +
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
            "\noperator = 'any'" +
            "\nproduct = '1001'" +
            "\nheaders = {" +
            "\n'Authorization': 'Bearer ' + token," +
            "\n'Accept': 'application/json'," +
            "\n}" +
            "\nresponse = requests.get('https://openapi.simforrent.com/api/content/buyActivationWareHouseFiveAgain' + '/' + serviceId +  '/' + number + '/' + network + '/' + country + '/' + price, headers=headers)")]
        [ReDocCodeSample("c#", "var client = new RestClient(\"https://openapi.simforrent.com/api/content/buyActivationWareHouseFiveAgain\");" +
            "\nvar request = new RestRequest();" +
            "\nrequest.AddHeader(\"Authorization\", \"Bearer \" + token);" +
            "\nrequest.AddHeader(\"Accept\", \"application/json\");" +
            "\nvar response = await client.ExecuteGetAsync(request);")]
        public async Task<ActionResult<BuyActionNumberWareHouseFiveDto>> BuyActionNumberWareHouseFiveAgainAsync(int serviceId, string number, string network, string country, decimal price)
        {
            var vSimToken = await ApiCommon.ReadCache(_session, _memoryCache, _signal, _config, "VSimToken");
            var percentStringValue = await ApiCommon.ReadCache(_session, _memoryCache, _signal, _config, "VSimPercentage");
            var percent = string.IsNullOrEmpty(percentStringValue) ? 50 : int.Parse(percentStringValue);

            var exchangeRateRUBContent = await _session
                    .Query<ContentItem, ContentItemIndex>(index => index.ContentType == "ExchangeRate" && index.DisplayText == "RUB" && index.Published && index.Latest)
                    .FirstOrDefaultAsync();

            string rubRateString = exchangeRateRUBContent.Content["ExchangeRate"]["RateToUsd"]["Text"];
            decimal rubRateDouble = Decimal.Parse(rubRateString);

            var user = await _userManager.GetUserAsync(User) as Users.Models.User;

            if (user == null || !user.IsEnabled) return BadRequest();
            if (!await _authorizationService.AuthorizeAsync(User, SimApiPermissions.AccessContentApi))
            {
                return this.ChallengeOrForbid();
            }

            var userContent = await _session
                .Query<ContentItem, ContentItemIndex>(index => index.ContentType == "UserProfileType" && index.Published && index.Latest)
                .With<UserProfilePartIndex>(p => p.UserId == user.Id)
                .FirstOrDefaultAsync();

            //Check User's Balance
            var content = userContent.Content;
            var userProfilePart = content["UserProfilePart"];
            decimal currentBalance = userProfilePart.Balance;
            if (currentBalance <= 0 || Math.Round((currentBalance * rubRateDouble), 2) < price)
            {
                return Ok("You don't have enough money");
            }

            var newOrderContent = await _contentManager.NewAsync("OrderType");

            if (newOrderContent != null)
            {
                // Set the current user as the owner to check for ownership permissions on creation
                newOrderContent.Owner = User.Identity.Name;
                newOrderContent.Author = User.Identity.Name;

                if (!await _authorizationService.AuthorizeAsync(User, CommonPermissions.EditContent, newOrderContent))
                {
                    return Forbid();
                }

                var url = string.Format("https://api.viotp.com/request/getv2?token={0}&serviceId={2}&number={3}", vSimToken, serviceId, number);

                var client = new RestClient(url);
                var request = new RestRequest();

                var response = await client.ExecuteGetAsync(request);

                var resObject = JsonConvert.DeserializeObject<BuyActionNumberWareHouseFiveDto>(response.Content);

                if (resObject.status_code != 200)
                {
                    return Ok("Error.");
                }

                await _contentManager.CreateAsync(newOrderContent, VersionOptions.Draft);

                var resultObject = resObject.message[0];

                if (string.IsNullOrEmpty(network))
                {
                    network = "any";
                }

                var urlProduct = string.Format("https://api.viotp.com/service/getv2?token={0}&country={1}", vSimToken, country);

                var clientProduct = new RestClient(urlProduct);
                var requestProduct = new RestRequest();

                var responseProduct = await client.ExecuteGetAsync(requestProduct);
                var resObjectProduct = JsonConvert.DeserializeObject<ProductsWareHouseFiveRequestDto>(responseProduct.Content);

                if (resObjectProduct.status_code != 200)
                {
                    return BadRequest();
                }

                var productObjectsProduct = resObjectProduct.data;
                DataProduct dataProduct = productObjectsProduct.Find(product => product.id == serviceId);

                var newOrderDetailPart = new OrderDetailPart
                {
                    InventoryId = (int)InventoryEnum.VSim,
                    OrderId = long.Parse(((int)InventoryEnum.VSim).ToString() + resObject.data.request_id),
                    Phone = resObject.data.re_phone_number,
                    Operator = network,
                    Product = dataProduct.name,
                    Price = price,
                    Status = Enum.GetName(typeof(OrderStatusLSimEnum), OrderStatusLSimEnum.WAITING),
                    Expires = DateTime.UtcNow.AddMinutes(5),
                    Created_at = DateTime.UtcNow,
                    Country = country,
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
            return BadRequest();
        }
        #endregion
    }
}
