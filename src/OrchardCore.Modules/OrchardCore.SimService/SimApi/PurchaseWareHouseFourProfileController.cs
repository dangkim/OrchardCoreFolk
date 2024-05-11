using System;
using System.Globalization;
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
    [Route("api/content/[action]/{service}/{carrier}/{price}")]
    [ApiController]
    [Authorize(AuthenticationSchemes = "Api"), IgnoreAntiforgeryToken]
    [OpenApiTag("PurchaseWareHouseFour", Description = "Get information of purchase of Ware House Four.")]
    public class PurchaseWareHouseFourProfileController : Controller
    {
        private readonly ISession _session;
        private readonly IContentManager _contentManager;
        private readonly UserManager<IUser> _userManager;
        private readonly IAuthorizationService _authorizationService;
        private readonly Microsoft.Extensions.Configuration.IConfiguration _config;
        private readonly IMemoryCache _memoryCache;
        private readonly ISignal _signal;

        public PurchaseWareHouseFourProfileController(
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
        /// Buy activation number of  Ware House Four
        /// </summary>
        /// <remarks>
        /// 
        /// </remarks> 
        [HttpGet]
        [ActionName("buyActivationWareHouseFour")]
        [ProducesResponseType(typeof(BuyActionNumberWareHouseFourDto), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(ProblemDetails), (int)HttpStatusCode.Unauthorized)]
        [ProducesResponseType(typeof(ProblemDetails), (int)HttpStatusCode.BadRequest)]
        [ProducesResponseType(typeof(ProblemDetails), (int)HttpStatusCode.InternalServerError)]
        [ReDocCodeSample("php", "$token = 'Your token';" +
            "\n$ch = curl_init();" +
            "\n$operator = 'any'" +
            "\n$product = '1001'" +
            "\ncurl_setopt($ch, CURLOPT_URL, 'https://openapi.simforrent.com/api/content/buyActivationWareHouseFour/' . service . '/' . carrier . '/' . price');" +
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
            "\nresponse = requests.get('https://openapi.simforrent.com/api/content/buyActivationWareHouseFour' + '/' + service + '/' + carrier + '/' + price, headers=headers)")]
        [ReDocCodeSample("c#", "var client = new RestClient(\"https://openapi.simforrent.com/api/content/buyActivationWareHouseFour\");" +
            "\nvar request = new RestRequest();" +
            "\nrequest.AddHeader(\"Authorization\", \"Bearer \" + token);" +
            "\nrequest.AddHeader(\"Accept\", \"application/json\");" +
            "\nvar response = await client.ExecuteGetAsync(request);")]
        public async Task<ActionResult<BuyActionNumberWareHouseFourDto>> BuyActionNumberWareHouseFourAsync(string service, string carrier, decimal price)
        {
            var uSimToken = await ApiCommon.ReadCache(_session, _memoryCache, _signal, _config, "USimToken");
            var percentStringValue = await ApiCommon.ReadCache(_session, _memoryCache, _signal, _config, "USimPercentage");
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
                .Query<ContentItem, ContentItemIndex>(index => index.ContentType == "UserProfile" && index.Published && index.Latest)
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

            var newOrderContent = await _contentManager.NewAsync("Orders");

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
                if (string.IsNullOrEmpty(carrier) || carrier.Equals("us", StringComparison.Ordinal))
                {
                    url = string.Format("https://www.unitedsms.net/api_command.php?cmd=request&{0}&service={1}", uSimToken, service);
                }
                else
                {
                    url = string.Format("https://www.unitedsms.net/api_command.php?cmd=request&{0}&service={1}&carrier={2}", uSimToken, service, carrier);
                }

                var client = new RestClient(url);
                var request = new RestRequest();

                var response = await client.ExecuteGetAsync(request);

                var resObject = JsonConvert.DeserializeObject<BuyActionNumberWareHouseFourDto>(response.Content);

                if (!resObject.status.Equals("ok", StringComparison.Ordinal))
                {
                    return Ok("Error.");
                }

                await _contentManager.CreateAsync(newOrderContent, VersionOptions.Draft);

                var resultObject = resObject.message[0];

                var newOrderDetailPart = new OrderDetailPart
                {
                    InventoryId = (int)InventoryEnum.USim,
                    OrderId = long.Parse(((int)InventoryEnum.USim).ToString() + resultObject.id),
                    Phone = resultObject.mdn,
                    Operator = carrier,
                    Product = resultObject.service,
                    Price = Math.Round(((decimal)resultObject.price + ((decimal)resultObject.price * percent / 100)) / rubRateDouble, 2),
                    Status = Enum.GetName(typeof(OrderStatusLSimEnum), OrderStatusLSimEnum.WAITING),
                    Expires = DateTime.UtcNow.AddSeconds(resultObject.till_expiration),
                    Created_at = DateTime.UtcNow,
                    Country = "USA",
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

        #region Buy activation number Long Term
        /// <summary>
        /// Buy activation number of  Ware House Four Long Term
        /// </summary>
        /// <remarks>
        /// 
        /// </remarks> 
        [HttpGet]
        [ActionName("buyActivationWareHouseFourLongTerm")]
        [ProducesResponseType(typeof(BuyActionNumberWareHouseFourLongTermDto), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(ProblemDetails), (int)HttpStatusCode.Unauthorized)]
        [ProducesResponseType(typeof(ProblemDetails), (int)HttpStatusCode.BadRequest)]
        [ProducesResponseType(typeof(ProblemDetails), (int)HttpStatusCode.InternalServerError)]
        [ReDocCodeSample("php", "$token = 'Your token';" +
            "\n$ch = curl_init();" +
            "\n$operator = 'any'" +
            "\n$product = '1001'" +
            "\ncurl_setopt($ch, CURLOPT_URL, 'https://openapi.simforrent.com/api/content/buyActivationWareHouseFourLongTerm/' . service . '/' . carrier . '/' . price');" +
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
            "\nresponse = requests.get('https://openapi.simforrent.com/api/content/buyActivationWareHouseFourLongTerm' + '/' + service + '/' + carrier + '/' + price, headers=headers)")]
        [ReDocCodeSample("c#", "var client = new RestClient(\"https://openapi.simforrent.com/api/content/buyActivationWareHouseFourLongTerm\");" +
            "\nvar request = new RestRequest();" +
            "\nrequest.AddHeader(\"Authorization\", \"Bearer \" + token);" +
            "\nrequest.AddHeader(\"Accept\", \"application/json\");" +
            "\nvar response = await client.ExecuteGetAsync(request);")]
        public async Task<ActionResult<BuyActionNumberWareHouseFourLongTermDto>> BuyActionNumberWareHouseFourLongTermAsync(string service, string carrier, decimal price)
        {
            var uSimToken = await ApiCommon.ReadCache(_session, _memoryCache, _signal, _config, "USimToken");
            var percentStringValue = await ApiCommon.ReadCache(_session, _memoryCache, _signal, _config, "USimPercentage");
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
                .Query<ContentItem, ContentItemIndex>(index => index.ContentType == "UserProfile" && index.Published && index.Latest)
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

            var newOrderContent = await _contentManager.NewAsync("Orders");

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
                if (string.IsNullOrEmpty(carrier) || carrier.Equals("us", StringComparison.Ordinal))
                {
                    url = string.Format("https://www.unitedsms.net/api_command.php?cmd=ltr_rent&{0}&service={1}", uSimToken, service);
                }
                else
                {
                    url = string.Format("https://www.unitedsms.net/api_command.php?cmd=ltr_rent&{0}&service={1}&carrier={2}", uSimToken, service, carrier);
                }

                var client = new RestClient(url);
                var request = new RestRequest();

                var response = await client.ExecuteGetAsync(request);

                var resObject = JsonConvert.DeserializeObject<BuyActionNumberWareHouseFourLongTermDto>(response.Content);

                if (!resObject.status.Equals("ok", StringComparison.Ordinal))
                {
                    return Ok("Error.");
                }

                await _contentManager.CreateAsync(newOrderContent, VersionOptions.Draft);

                var resultObject = resObject.message;

                var newOrderDetailPart = new OrderDetailPart
                {
                    InventoryId = (int)InventoryEnum.USimLongTerm,
                    OrderId = long.Parse(((int)InventoryEnum.USimLongTerm).ToString() + resultObject.id),
                    Phone = resultObject.mdn,
                    Operator = carrier,
                    Product = resultObject.service,
                    Price = Math.Round(((decimal)resultObject.price + ((decimal)resultObject.price * percent / 100)) / rubRateDouble, 2),
                    Status = Enum.GetName(typeof(OrderStatusLSimEnum), OrderStatusLSimEnum.WAITING),
                    Expires = DateTime.ParseExact(resultObject.expires, "yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture),
                    Created_at = DateTime.UtcNow,
                    Country = "USA",
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
