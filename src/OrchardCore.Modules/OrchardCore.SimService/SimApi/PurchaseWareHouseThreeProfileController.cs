using System;
using System.Net;
using System.Text.RegularExpressions;
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
using OrchardCore.Data;

namespace OrchardCore.SimService.SimApi
{
    [Route("api/content/[action]/{product}/{operato?}/{number?}")]
    [ApiController]
    [Authorize(AuthenticationSchemes = "Api"), IgnoreAntiforgeryToken]
    [OpenApiTag("PurchaseWareHouseThree", Description = "Get information of purchase of Ware House Three.")]
    public class PurchaseWareHouseThreeProfileController : Controller
    {
        private readonly IReadOnlySession _session;
        private readonly IContentManager _contentManager;
        private readonly UserManager<IUser> _userManager;
        private readonly IAuthorizationService _authorizationService;
        private readonly Microsoft.Extensions.Configuration.IConfiguration _config;
        private readonly IMemoryCache _memoryCache;
        private readonly ISignal _signal;

        public PurchaseWareHouseThreeProfileController(
          IReadOnlySession session,
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
        /// Buy activation number of  Ware House Three
        /// </summary>
        /// <remarks>
        /// 
        /// </remarks> 
        [HttpGet]
        [ActionName("buyActivationWareHouseThree")]
        [ProducesResponseType(typeof(BuyActionNumberWareHouseThreeDto), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(ProblemDetails), (int)HttpStatusCode.Unauthorized)]
        [ProducesResponseType(typeof(ProblemDetails), (int)HttpStatusCode.BadRequest)]
        [ProducesResponseType(typeof(ProblemDetails), (int)HttpStatusCode.InternalServerError)]
        [ReDocCodeSample("php", "$token = 'Your token';" +
            "\n$ch = curl_init();" +
            "\n$operator = 'any'" +
            "\n$product = '1001'" +
            "\ncurl_setopt($ch, CURLOPT_URL, 'https://openapi.simforrent.com/api/content/buyActivationWareHouseThree/' . operator . '/' . product');" +
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
            "\nresponse = requests.get('https://openapi.simforrent.com/api/content/buyActivationWareHouseThree' + '/' + operator + '/' + product, headers=headers)")]
        [ReDocCodeSample("c#", "var client = new RestClient(\"https://openapi.simforrent.com/api/content/buyActivationWareHouseThree\");" +
            "\nvar request = new RestRequest();" +
            "\nrequest.AddHeader(\"Authorization\", \"Bearer \" + token);" +
            "\nrequest.AddHeader(\"Accept\", \"application/json\");" +
            "\nvar response = await client.ExecuteGetAsync(request);")]
        public async Task<ActionResult<BuyActionNumberWareHouseThreeDto>> BuyActionNumberWareHouseThreeAsync(string operato, int product)
        {
            var user = await _userManager.GetUserAsync(User) as Users.Models.User;
            
            if (user == null || !user.IsEnabled || !user.EmailConfirmed) return BadRequest();

            if (!await _authorizationService.AuthorizeAsync(User, SimApiPermissions.AccessContentApi))
            {
                return this.ChallengeOrForbid();
            }

            var cSimToken = await ApiCommon.ReadCache(_session, _memoryCache, _signal, _config, "CSimToken");

            var percentStringValue = await ApiCommon.ReadCache(_session, _memoryCache, _signal, _config, "CSimPercentage");
            var percent = string.IsNullOrEmpty(percentStringValue) ? 50 : int.Parse(percentStringValue);

            var exchangeRateRUBContent = await _session
                    .Query<ContentItem, ContentItemIndex>(index => index.ContentType == "ExchangeRate" && index.DisplayText == "RUB" && index.Published && index.Latest)
                    .FirstOrDefaultAsync();

            string rubRateString = exchangeRateRUBContent.Content["ExchangeRate"]["RateToUsd"]["Text"];
            decimal rubRateDouble = Decimal.Parse(rubRateString);

            var url = "";
            if (string.IsNullOrEmpty(operato))
            {
                url = string.Format("https://chothuesimcode.com/api?act=number&apik={0}&appId={1}", cSimToken, product);
            }
            else
            {
                url = string.Format("https://chothuesimcode.com/api?act=number&apik={0}&appId={1}&carrier={2}", cSimToken, product, operato);
            }

            if (user == null || !user.IsEnabled || !user.EmailConfirmed) return BadRequest();
            if (!await _authorizationService.AuthorizeAsync(User, SimApiPermissions.AccessContentApi))
            {
                return this.ChallengeOrForbid();
            }

            var userContent = await _session
                .Query<ContentItem, ContentItemIndex>(index => index.ContentType == "UserProfile" && index.Published && index.Latest)
                .With<UserProfilePartIndex>(p => p.UserId == user.Id)
                .FirstOrDefaultAsync();

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

                var client = new RestClient(url);
                var request = new RestRequest();

                var response = await client.ExecuteGetAsync(request);

                var resObject = JsonConvert.DeserializeObject<BuyActionNumberWareHouseThreeDto>(response.Content);

                if (resObject.ResponseCode == 2 || resObject.ResponseCode == 3 || resObject.ResponseCode == 500 || resObject.ResponseCode == 404)
                {
                    //throw new Exception(resObject.Msg);
                    return Ok("Error.");
                }

                //Check User's Balance
                var priceProductWareHouseThree = Math.Round((((decimal)resObject.Result.Cost + ((decimal)resObject.Result.Cost * percent / 100)) / 24) / rubRateDouble, 2);

                var content = userContent.Content;
                var userProfilePart = content["UserProfilePart"];
                decimal currentBalance = userProfilePart.Balance;
                if (currentBalance <= 0 || currentBalance < priceProductWareHouseThree)
                {
                    return Ok("You don't have enough money");
                }

                //Save Purchase Profile into Database
                if (string.IsNullOrEmpty(operato))
                {
                    operato = "any";
                }

                await _contentManager.CreateAsync(newOrderContent, VersionOptions.Draft);

                var newOrderDetailPart = new OrderDetailPart
                {
                    InventoryId = (int)InventoryEnum.CSim,
                    OrderId = long.Parse(((int)InventoryEnum.CSim).ToString() + resObject.Result.Id.ToString()),
                    Phone = resObject.Result.Number,
                    Operator = operato + (" (" + product.ToString() + ")"),
                    Product = resObject.Result.App,
                    Price = Math.Round((((decimal)resObject.Result.Cost + ((decimal)resObject.Result.Cost * percent / 100)) / 24) / rubRateDouble, 2),
                    Status = Enum.GetName(typeof(OrderStatusLSimEnum), OrderStatusLSimEnum.WAITING),
                    Expires = DateTime.UtcNow.AddMinutes(5),
                    Created_at = DateTime.UtcNow,
                    Country = "VietNam",
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
        /// Buy activation number again for Ware House Three
        /// </summary>
        /// <remarks>
        /// 
        /// </remarks> 
        [HttpGet]
        [ActionName("buyAgainActivationWareHouseThree")]
        [ProducesResponseType(typeof(BuyActionNumberWareHouseThreeDto), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(ProblemDetails), (int)HttpStatusCode.Unauthorized)]
        [ProducesResponseType(typeof(ProblemDetails), (int)HttpStatusCode.BadRequest)]
        [ProducesResponseType(typeof(ProblemDetails), (int)HttpStatusCode.InternalServerError)]
        [ReDocCodeSample("php", "$token = 'Your token';" +
            "\n$ch = curl_init();" +
            "\n$operator = 'any'" +
            "\n$product = '1001'" +
            "\ncurl_setopt($ch, CURLOPT_URL, 'https://openapi.simforrent.com/api/content/buyAgainActivationWareHouseThree/' . operator . '/' . product');" +
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
            "\nresponse = requests.get('https://openapi.simforrent.com/api/content/buyAgainActivationWareHouseThree' + operator + '/' + product, headers=headers)")]
        [ReDocCodeSample("c#", "var client = new RestClient(\"https://openapi.simforrent.com/api/content/buyAgainActivationWareHouseThree\");" +
            "\nvar request = new RestRequest();" +
            "\nrequest.AddHeader(\"Authorization\", \"Bearer \" + token);" +
            "\nrequest.AddHeader(\"Accept\", \"application/json\");" +
            "\nvar response = await client.ExecuteGetAsync(request);")]
        public async Task<ActionResult<BuyActionNumberWareHouseThreeDto>> BuyAgainActionNumberWareHouseThreeAsync(string operato, int product, string number)
        {
            var cSimToken = await ApiCommon.ReadCache(_session, _memoryCache, _signal, _config, "CSimToken");

            var percentStringValue = await ApiCommon.ReadCache(_session, _memoryCache, _signal, _config, "CSimPercentage");
            var percent = string.IsNullOrEmpty(percentStringValue) ? 50 : int.Parse(percentStringValue);

            var exchangeRateRUBContent = await _session
                    .Query<ContentItem, ContentItemIndex>(index => index.ContentType == "ExchangeRate" && index.DisplayText == "RUB" && index.Published && index.Latest)
                    .FirstOrDefaultAsync();

            string rubRateString = exchangeRateRUBContent.Content["ExchangeRate"]["RateToUsd"]["Text"];
            decimal rubRateDouble = Decimal.Parse(rubRateString);

            string checkNumber = new Regex(@"(?<=\().+?(?=\))").Match(operato).Value;
            if (string.IsNullOrEmpty(checkNumber) || !checkNumber.Equals(product.ToString(), StringComparison.Ordinal))
            {
                throw new Exception("Product Id is incorrect!");
            }

            var url = string.Format("https://chothuesimcode.com/api?act=number&apik={0}&appId={1}&number={2}", cSimToken, product, number);

            var user = await _userManager.GetUserAsync(User) as Users.Models.User;

            if (user == null || !user.IsEnabled || !user.EmailConfirmed) return BadRequest();
            if (!await _authorizationService.AuthorizeAsync(User, SimApiPermissions.AccessContentApi))
            {
                return this.ChallengeOrForbid();
            }

            var userContent = await _session
                .Query<ContentItem, ContentItemIndex>(index => index.ContentType == "UserProfile" && index.Published && index.Latest)
                .With<UserProfilePartIndex>(p => p.UserId == user.Id)
                .FirstOrDefaultAsync();

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

                var client = new RestClient(url);
                var request = new RestRequest();

                var response = await client.ExecuteGetAsync(request);

                var resObject = JsonConvert.DeserializeObject<BuyActionNumberWareHouseThreeDto>(response.Content);

                if (resObject.ResponseCode == 2 || resObject.ResponseCode == 3 || resObject.ResponseCode == 500 || resObject.ResponseCode == 404)
                {
                    //throw new Exception(resObject.Msg);
                    return Ok("Error.");
                }

                //Check User's Balance
                var priceProductWareHouseThree = Math.Round((((decimal)resObject.Result.Cost + ((decimal)resObject.Result.Cost * percent / 100)) / 24) / rubRateDouble, 2);

                var content = userContent.Content;
                var userProfilePart = content["UserProfilePart"];
                decimal currentBalance = userProfilePart.Balance;
                if (currentBalance <= 0 || currentBalance < priceProductWareHouseThree)
                {
                    return Ok("You don't have enough money");
                }

                //Save Purchase Profile into Database
                if (string.IsNullOrEmpty(operato))
                {
                    operato = "any";
                }

                await _contentManager.CreateAsync(newOrderContent, VersionOptions.Draft);

                var newOrderDetailPart = new OrderDetailPart
                {
                    InventoryId = (int)InventoryEnum.CSim,
                    OrderId = long.Parse(((int)InventoryEnum.CSim).ToString() + resObject.Result.Id.ToString()),
                    Phone = resObject.Result.Number,
                    Operator = operato,
                    Product = resObject.Result.App,
                    Price = Math.Round((((decimal)resObject.Result.Cost + ((decimal)resObject.Result.Cost * percent / 100)) / 24) / rubRateDouble, 2),
                    Status = Enum.GetName(typeof(OrderStatusLSimEnum), OrderStatusLSimEnum.WAITING),
                    Expires = DateTime.UtcNow.AddMinutes(5),
                    Created_at = DateTime.UtcNow,
                    Country = "VietNam",
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
