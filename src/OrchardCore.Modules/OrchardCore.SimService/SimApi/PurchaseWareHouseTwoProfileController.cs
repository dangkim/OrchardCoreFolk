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
using OrchardCore.SimService.ViewModels;
using OrchardCore.Environment.Cache;
using CommonPermissions = OrchardCore.Contents.CommonPermissions;
using OrchardCore.Users;
using RestSharp;
using YesSql;
using OrchardCore.SimService.Permissions;

namespace OrchardCore.SimService.SimApi
{
    [Route("api/content/[action]/{country}/{operato}/{service}/{price}/{locktime}")]
    [ApiController]
    [Authorize(AuthenticationSchemes = "Api"), IgnoreAntiforgeryToken]
    [OpenApiTag("PurchaseWareHouseTwo", Description = "Get information of purchase.")]
    public class PurchaseWareHouseTwoProfileController : Controller
    {
        private readonly ISession _session;
        private readonly IContentManager _contentManager;
        private readonly UserManager<IUser> _userManager;
        private readonly IAuthorizationService _authorizationService;
        private readonly Microsoft.Extensions.Configuration.IConfiguration _config;
        private readonly IMemoryCache _memoryCache;
        private readonly ISignal _signal;

        public PurchaseWareHouseTwoProfileController(
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
        /// Buy activation number of Ware House Two
        /// </summary>
        /// <remarks>
        /// 
        /// </remarks> 
        [HttpGet]
        [ActionName("buyActivationWareHouseTwo")]
        [ProducesResponseType(typeof(BuyActionNumberWareHouseTwoDto), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(ProblemDetails), (int)HttpStatusCode.Unauthorized)]
        [ProducesResponseType(typeof(ProblemDetails), (int)HttpStatusCode.BadRequest)]
        [ProducesResponseType(typeof(ProblemDetails), (int)HttpStatusCode.InternalServerError)]
        [ReDocCodeSample("php", "$token = 'Your token';" +
            "\n$ch = curl_init();" +
            "\n$country = 'russia'" +
            "\n$operator = 'any'" +
            "\n$product = 'amazon'" +
            "\ncurl_setopt($ch, CURLOPT_URL, 'https://openapi.simforrent.com/api/content/buyActivationWareHouseTwo/'. country . '/' . operator . '/' . service . '/' . price . '/' . lockTime');" +
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
            "\nresponse = requests.get('https://openapi.simforrent.com/api/content/buyActivationWareHouseTwo'+ country + '/' + operator + '/' + service + '/' + price + '/' + lockTime, headers=headers)")]
        [ReDocCodeSample("c#", "var client = new RestClient(\"https://openapi.simforrent.com/api/content/buyActivationWareHouseTwo\");" +
            "\nvar request = new RestRequest();" +
            "\nrequest.AddHeader(\"Authorization\", \"Bearer \" + token);" +
            "\nrequest.AddHeader(\"Accept\", \"application/json\");" +
            "\nvar response = await client.ExecuteGetAsync(request);")]
        public async Task<ActionResult<BuyActionNumberWareHouseTwoDto>> BuyActionNumberWareHouseTwoAsync(string country, string operato, string service, decimal price, int lockTime)
        {
            var twoLineSimToken = await ApiCommon.ReadCache(_session, _memoryCache, _signal, _config, "TwoLineSimToken");

            var url = string.Format("https://2ndline.io/apiv1/order?apikey={0}&serviceId={1}&countryId={2}&operatorId={3}", twoLineSimToken, service, country, operato);
            var user = await _userManager.GetUserAsync(User) as Users.Models.User;

            if (user == null || !user.IsEnabled) return BadRequest();
            if (!await _authorizationService.AuthorizeAsync(User, SimApiPermissions.AccessContentApi))
            {
                return this.ChallengeOrForbid();
            }

            //var fiveSimToken = await ApiCommon.ReadCache(_session, _memoryCache, _signal, _config);
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
                //request.AddHeader("Authorization", "Bearer " + fiveSimToken);

                var response = await client.ExecuteGetAsync(request);
                var resObject = JsonConvert.DeserializeObject<OrderDetailPartViewModel>(response.Content);

                if (resObject.Status == "-1")
                {
                    return Ok(response.Content);
                }

                //Check User's Balance
                var content = userContent.Content;
                var userProfilePart = content["UserProfilePart"];
                decimal currentBalance = userProfilePart.Balance;
                if (currentBalance <= 0 || currentBalance < price)
                {
                    return Ok("You don't have enough money");
                }

                //Save Purchase Profile into Database
                await _contentManager.CreateAsync(newOrderContent, VersionOptions.Draft);

                var newOrderDetailPart = new OrderDetailPart
                {
                    InventoryId = (int)InventoryEnum.LSim,
                    OrderId = long.Parse(((int)InventoryEnum.LSim).ToString() + resObject.Id.ToString()),
                    Phone = resObject.Phone,
                    Operator = operato,
                    Product = service,
                    Price = price,
                    Status = Enum.GetName(typeof(OrderStatusLSimEnum), OrderStatusLSimEnum.WAITING),
                    Expires = DateTime.UtcNow.AddMinutes(20),
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

        #region Buy activation number Viet Nam 
        /// <summary>
        /// Buy activation number of Ware House Two for Viet Nam
        /// </summary>
        /// <remarks>
        /// 
        /// </remarks> 
        [HttpGet]
        [ActionName("buyActivationVietNamWareHouseTwo")]
        [ProducesResponseType(typeof(BuyActionNumberWareHouseTwoDto), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(ProblemDetails), (int)HttpStatusCode.Unauthorized)]
        [ProducesResponseType(typeof(ProblemDetails), (int)HttpStatusCode.BadRequest)]
        [ProducesResponseType(typeof(ProblemDetails), (int)HttpStatusCode.InternalServerError)]
        [ReDocCodeSample("php", "$token = 'Your token';" +
            "\n$ch = curl_init();" +
            "\n$country = 'russia'" +
            "\n$operator = 'any'" +
            "\n$product = 'amazon'" +
            "\ncurl_setopt($ch, CURLOPT_URL, 'https://openapi.simforrent.com/api/content/buyActivationVietNamWareHouseTwo/'. country . '/' . operator . '/' . service . '/' . price . '/' . lockTime');" +
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
            "\nresponse = requests.get('https://openapi.simforrent.com/api/content/buyActivationVietNamWareHouseTwo'+ country + '/' + operator + '/' + service + '/' + price + '/' + lockTime, headers=headers)")]
        [ReDocCodeSample("c#", "var client = new RestClient(\"https://openapi.simforrent.com/api/content/buyActivationVietNamWareHouseTwo\");" +
            "\nvar request = new RestRequest();" +
            "\nrequest.AddHeader(\"Authorization\", \"Bearer \" + token);" +
            "\nrequest.AddHeader(\"Accept\", \"application/json\");" +
            "\nvar response = await client.ExecuteGetAsync(request);")]
        public async Task<ActionResult<BuyActionNumberWareHouseTwoDto>> BuyActionNumberVietNamWareHouseTwoAsync(string country, string operato, string service, decimal price, int lockTime)
        {
            var twoLineSimToken = await ApiCommon.ReadCache(_session, _memoryCache, _signal, _config, "TwoLineSimToken");

            var url = string.Format("https://2ndline.io/apiv1/order?apikey={0}&serviceId={1}&country={2}&operatorId={3}", twoLineSimToken, service, country, operato);

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

            //var fiveSimToken = await ApiCommon.ReadCache(_session, _memoryCache, _signal, _config);

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
                //request.AddHeader("Authorization", "Bearer " + fiveSimToken);

                var response = await client.ExecuteGetAsync(request);
                var resObject = JsonConvert.DeserializeObject<OrderDetailPartViewModel>(response.Content);

                if (resObject.Status == "-1")
                {
                    return Ok(response.Content);
                }

                var urlGetPhoneVN = string.Format("https://2ndline.io/apiv1/ordercheck?apikey={0}&id={1}", twoLineSimToken, resObject.Id.ToString());
                var clientGetPhoneVN = new RestClient(urlGetPhoneVN);
                var requestGetPhoneVN = new RestRequest();

                var responseGetPhoneVN = await clientGetPhoneVN.ExecuteGetAsync(requestGetPhoneVN);
                var resObjectGetPhoneVN = JsonConvert.DeserializeObject<SmsWareHouseTwoDto>(responseGetPhoneVN.Content);

                //Check User's Balance

                var content = userContent.Content;
                var userProfilePart = content["UserProfilePart"];
                decimal currentBalance = userProfilePart.Balance;
                if (currentBalance <= 0 || currentBalance < price)
                {
                    return Ok("You don't have enough money");
                }

                //Save Purchase Profile into Database

                await _contentManager.CreateAsync(newOrderContent, VersionOptions.Draft);

                var newOrderDetailPart = new OrderDetailPart
                {
                    InventoryId = (int)InventoryEnum.LSim,
                    OrderId = long.Parse(((int)InventoryEnum.LSim).ToString() + resObject.Id.ToString()),
                    Phone = resObjectGetPhoneVN.data.phone,
                    Operator = operato,
                    Product = service,
                    Price = price,
                    Status = resObject.Status,
                    Expires = DateTime.UtcNow.AddMinutes(lockTime),
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