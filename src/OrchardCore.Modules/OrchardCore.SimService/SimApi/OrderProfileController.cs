using System;
using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
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
using OrchardCore.Users;
using RestSharp;
using YesSql;
using OrchardCore.SimService.Permissions;
using System.Collections.Generic;
using Dapper;
using System.Linq;
using YesSql.Services;
using OrchardCore.Lists.Models;
using static System.Net.Mime.MediaTypeNames;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using OrchardCore.Data;

namespace OrchardCore.SimService.SimApi
{
    [Route("api/content/[action]/{id?}/{product?}/{country?}")]
    [ApiController]
    [Authorize(AuthenticationSchemes = "Api"), IgnoreAntiforgeryToken, AllowAnonymous]
    [OpenApiTag("Order management", Description = "Get information of order.")]
    public class OrderProfileController : Controller
    {
        //public string fiveSimToken;
        private readonly ISession _session;
        private readonly IReadOnlySession _sessionReadOnly;
        private readonly IContentManager _contentManager;
        private readonly UserManager<IUser> _userManager;
        private readonly IAuthorizationService _authorizationService;
        private readonly Microsoft.Extensions.Configuration.IConfiguration _config;
        private readonly IMemoryCache _memoryCache;
        private readonly ISignal _signal;
        private readonly IHttpClientFactory _httpClientFactory;
        private const string PENDING = "pending";
        private const string CANCELED = "canceled";
        private const string RECEIVED = "received";
        private const string BANNED = "banned";
        private const string TIMEOUT = "timeout";
        private const string FINISHED = "finished";

        public OrderProfileController(
            ISession session,
            IReadOnlySession sessionReadOnly,
            IMemoryCache memoryCache,
            ISignal signal,
            IContentManager contentManager,
            UserManager<IUser> userManager,
            IAuthorizationService authorizationService,
            Microsoft.Extensions.Configuration.IConfiguration config,
            IHttpClientFactory httpClientFactory)
        {
            _session = session;
            _sessionReadOnly = sessionReadOnly;
            _memoryCache = memoryCache;
            _signal = signal;
            _contentManager = contentManager;
            _userManager = userManager;
            _authorizationService = authorizationService;
            _config = config;
            _httpClientFactory = httpClientFactory;
        }

        #region Check order
        /// <summary>
        /// Check order (Get SMS)
        /// </summary>
        /// <remarks>
        /// 
        /// </remarks> 
        [HttpGet]
        [ActionName("checkorder")]
        [ProducesResponseType(typeof(CheckOrderDto), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(ProblemDetails), (int)HttpStatusCode.Unauthorized)]
        [ProducesResponseType(typeof(ProblemDetails), (int)HttpStatusCode.BadRequest)]
        [ProducesResponseType(typeof(ProblemDetails), (int)HttpStatusCode.InternalServerError)]
        [ReDocCodeSample("php", "$token = 'Your token';" +
            "\n$ch = curl_init();" +
            "\n$id = 1;" +
            "\ncurl_setopt($ch, CURLOPT_URL, 'https://openapi.simforrent.com/api/content/checkorder/'. $id);" +
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
            "\nid = '1'" +
            "\nheaders = {" +
            "\n'Authorization': 'Bearer ' + token," +
            "\n'Accept': 'application/json'," +
            "\n}" +
            "\nresponse = requests.get('https://openapi.simforrent.com/api/content/checkorder/' + id, headers=headers)")]
        [ReDocCodeSample("c#", "var client = new RestClient(\"https://openapi.simforrent.com/api/content/checkorder\");" +
            "\nvar request = new RestRequest();" +
            "\nrequest.AddHeader(\"Authorization\", \"Bearer \" + token);" +
            "\nrequest.AddHeader(\"Accept\", \"application/json\");" +
            "\nvar response = await client.ExecuteGetAsync(request);")]
        public async Task<ActionResult<CheckOrderDto>> CheckOrderAsync(string id)
        {
            var user = await _userManager.GetUserAsync(User) as Users.Models.User;
            if (user == null || !user.IsEnabled || !user.EmailConfirmed) return BadRequest();

            if (!await _authorizationService.AuthorizeAsync(User, SimApiPermissions.AccessContentApi))
            {
                return this.ChallengeOrForbid();
            }

            var orderId = long.Parse(id);

            var simToken = await ApiCommon.ReadCache(_sessionReadOnly, _memoryCache, _signal, _config);
            var percentStringValue = await ApiCommon.ReadCache(_sessionReadOnly, _memoryCache, _signal, _config, "Percentage");
            var percent = string.IsNullOrEmpty(percentStringValue) ? 20 : int.Parse(percentStringValue);

            var orderContent = await _sessionReadOnly
                .Query<ContentItem, ContentItemIndex>(index => index.ContentType == "Orders" && index.Published && index.Latest)
                .With<OrderDetailPartIndex>(p => p.UserId == user.Id && p.OrderId == orderId)
                .FirstOrDefaultAsync();

            if (orderContent == null) return BadRequest();

            var orderDetailPart = orderContent.As<OrderDetailPart>();

            if (orderDetailPart == null) return BadRequest();

            using var httpClient = _httpClientFactory.CreateClient("fsim");

            var url = string.Format("https://5sim.net/v1/user/check/{0}", orderId);

            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", simToken);

            using var response = await httpClient.GetAsync(url);

            var resObject = await response.Content.ReadFromJsonAsync<OrderDetailPartViewModel>();

            if (!orderDetailPart.Status.Equals(resObject.Status, StringComparison.OrdinalIgnoreCase))
            {
                orderDetailPart.Status = resObject.Status.ToLower();
                orderContent.Apply(orderDetailPart);

                await _contentManager.UpdateAsync(orderContent);

                var resultOrder = await _contentManager.ValidateAsync(orderContent);

                if (resultOrder.Succeeded)
                {
                    await _contentManager.PublishAsync(orderContent);
                }
            }

            // Temp
            //var sms = new SmsPartViewModel()
            //{
            //    Code = "404943",
            //    Created_at = DateTime.Now,
            //    Date = DateTime.Now,
            //    Sender = "Google",
            //    Text = "G-404943 is your Google verification code."
            //};

            //resObject.Sms.Add(sms);
            //resObject.Status = "pending";
            // End Temp

            if (resObject.Sms.Count > 0)
            {
                if (!resObject.Status.Equals("finished", StringComparison.OrdinalIgnoreCase))
                {
                    // Create List of SmsType and insert this list to Orders
                    foreach (var itemSms in resObject.Sms)
                    {
                        var smsContent = await _sessionReadOnly
                           .Query<ContentItem, ContentItemIndex>(index => index.ContentType == "SmsType" && index.Published && index.Latest)
                           .With<SmsPartIndex>(p => p.UserId == user.Id && p.OrderId == Int64.Parse(id) && p.Code == itemSms.Code)
                           .FirstOrDefaultAsync();

                        if (smsContent == null)
                        {
                            //Get payment of this order
                            var paymentContent = await _sessionReadOnly
                            .Query<ContentItem, ContentItemIndex>(index => index.ContentType == "Payments" && index.Published && index.Latest)
                            .With<PaymentDetailPartIndex>(p => p.UserId == user.Id && p.OrderId == Int64.Parse(id))
                            .FirstOrDefaultAsync();

                            if (paymentContent == null && resObject.Sms.Count == 1)
                            {
                                var userContent = await _sessionReadOnly
                                    .Query<ContentItem, ContentItemIndex>(index => index.ContentType == "UserProfile" && index.Published && index.Latest)
                                    .With<UserProfilePartIndex>(p => p.UserId == user.Id)
                                    .FirstOrDefaultAsync();

                                if (userContent == null) return BadRequest();

                                var userProfilePart = userContent.As<UserProfilePart>();

                                if (userProfilePart == null) return Forbid();

                                var currentBalance = userProfilePart.Balance;
                                var frozenBalance = resObject.Price + (resObject.Price * percent / 100);

                                currentBalance -= (resObject.Price + (resObject.Price * percent / 100)); // All is RUB currency

                                var newUserProfilePart = new UserProfilePart
                                {
                                    ProfileId = userProfilePart.ProfileId,
                                    Email = userProfilePart.Email,
                                    UserId = userProfilePart.UserId,
                                    UserName = userProfilePart.UserName,
                                    Vendor = userProfilePart.Vendor,
                                    DefaultForwardingNumber = userProfilePart.DefaultForwardingNumber,
                                    Balance = currentBalance,
                                    OriginalAmount = userProfilePart.OriginalAmount,
                                    Amount = -frozenBalance,
                                    GmailMsgId = userProfilePart.GmailMsgId,
                                    RateInUsd = userProfilePart.RateInUsd,
                                    Rating = userProfilePart.Rating,
                                    DefaultCoutryName = userProfilePart.DefaultCoutryName,
                                    DefaultIso = userProfilePart.DefaultIso,
                                    DefaultPrefix = userProfilePart.DefaultPrefix,
                                    DefaultOperatorName = userProfilePart.DefaultOperatorName,
                                    FrozenBalance = frozenBalance
                                };

                                userContent.Apply(newUserProfilePart);
                                userContent.Owner = user.UserName;
                                userContent.Author = user.UserName;

                                var resultUserContent = await _contentManager.ValidateAsync(userContent);

                                // Create new Payments
                                var newPaymentContent = await _contentManager.NewAsync("Payments");
                                // Set the current user as the owner to check for ownership permissions on creation
                                newPaymentContent.Owner = user.UserName;
                                newPaymentContent.Author = user.UserName;

                                await _contentManager.CreateAsync(newPaymentContent, VersionOptions.DraftRequired);

                                if (newPaymentContent != null)
                                {
                                    var newPaymentDetailPart = new PaymentDetailPart
                                    {
                                        PaymentId = newPaymentContent.Id,
                                        TypeName = "buying",
                                        ProviderName = resObject.Product,
                                        Amount = -frozenBalance,
                                        Balance = currentBalance,
                                        CreatedAt = DateTime.UtcNow,
                                        Email = userProfilePart.Email,
                                        UserId = userProfilePart.UserId,
                                        UserName = userProfilePart.UserName,
                                        OrderId = orderId
                                    };

                                    newPaymentContent.Apply(newPaymentDetailPart);

                                    var resultPayment = await _contentManager.ValidateAsync(newPaymentContent);

                                    if (resultPayment.Succeeded && resultUserContent.Succeeded)
                                    {
                                        await _contentManager.PublishAsync(newPaymentContent);

                                        await _contentManager.UpdateAsync(userContent);
                                    }
                                }
                            }

                            var newSmsContentItem = await _contentManager.NewAsync("SmsType");
                            newSmsContentItem.Author = user.UserName;
                            newSmsContentItem.Owner = user.UserName;

                            await _contentManager.CreateAsync(newSmsContentItem, VersionOptions.DraftRequired);

                            if (newSmsContentItem != null)
                            {
                                var newSmsPart = new SmsPart
                                {
                                    Code = itemSms.Code,
                                    Sender = itemSms.Sender,
                                    Created_at = itemSms.Created_at,
                                    Date = itemSms.Date,
                                    Text = itemSms.Text,
                                    Email = user.Email,
                                    UserId = user.Id,
                                    UserName = user.UserName,
                                    OrderId = long.Parse(id)
                                };

                                newSmsContentItem.Apply(newSmsPart);

                                var resultSms = await _contentManager.ValidateAsync(newSmsContentItem);

                                if (resultSms.Succeeded)
                                {
                                    newSmsContentItem.Alter<ContainedPart>(part =>
                                    {
                                        part.ListContentItemId = orderContent.ContentItemId;
                                        part.Order = 0;
                                    });

                                    await _contentManager.PublishAsync(newSmsContentItem);
                                }
                            }
                        }
                    }
                }
            }

            // Update Price just for displaying from 5sim
            resObject.Price += (resObject.Price * percent / 100);

            return Ok(resObject);
        }
        #endregion

        #region Finish order
        /// <summary>
        /// Finish order
        /// </summary>
        /// <remarks>
        /// 
        /// </remarks> 
        [HttpGet]
        [ActionName("finishorder")]
        [ProducesResponseType(typeof(CheckOrderDto), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(ProblemDetails), (int)HttpStatusCode.Unauthorized)]
        [ProducesResponseType(typeof(ProblemDetails), (int)HttpStatusCode.BadRequest)]
        [ProducesResponseType(typeof(ProblemDetails), (int)HttpStatusCode.InternalServerError)]
        [ReDocCodeSample("php", "$token = 'Your token';" +
            "\n$ch = curl_init();" +
            "\n$id = 1;" +
            "\ncurl_setopt($ch, CURLOPT_URL, 'https://openapi.simforrent.com/api/user/finishorder/'. $id);" +
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
            "\nid = '1'" +
            "\nheaders = {" +
            "\n'Authorization': 'Bearer ' + token," +
            "\n'Accept': 'application/json'," +
            "\n}" +
            "\nresponse = requests.get('https://openapi.simforrent.com/api/user/finishorder/' + id, headers=headers)")]
        [ReDocCodeSample("c#", "var client = new RestClient(\"https://openapi.simforrent.com/api/user/finishorder\");" +
            "\nvar request = new RestRequest();" +
            "\nrequest.AddHeader(\"Authorization\", \"Bearer \" + token);" +
            "\nrequest.AddHeader(\"Accept\", \"application/json\");" +
            "\nvar response = await client.ExecuteGetAsync(request);")]
        public async Task<ActionResult<CheckOrderDto>> FinishOrderAsync(string id)
        {
            var user = await _userManager.GetUserAsync(User) as Users.Models.User;
            if (user == null || !user.IsEnabled || !user.EmailConfirmed || !user.EmailConfirmed) return BadRequest();
            if (!await _authorizationService.AuthorizeAsync(User, SimApiPermissions.AccessContentApi))
            {
                return this.ChallengeOrForbid();
            }

            var orderId = long.Parse(id);
            var simToken = await ApiCommon.ReadCache(_session, _memoryCache, _signal, _config);

            var orderContent = await _sessionReadOnly
                .Query<ContentItem, ContentItemIndex>(index => index.ContentType == "Orders" && index.Published && index.Latest)
                .With<OrderDetailPartIndex>(p => p.UserId == user.Id && p.OrderId == orderId)
                .FirstOrDefaultAsync();

            if (orderContent == null) return BadRequest();

            using var httpClient = _httpClientFactory.CreateClient("fsim");

            var url = string.Format("https://5sim.net/v1/user/finish/{0}", orderId);

            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", simToken);

            using var response = await httpClient.GetAsync(url);

            var resObject = await response.Content.ReadFromJsonAsync<OrderDetailPartViewModel>();

            var orderDetailPart = orderContent.As<OrderDetailPart>();

            if (!orderDetailPart.Status.Equals(FINISHED, StringComparison.OrdinalIgnoreCase))
            {
                var newOrderDetailPart = new OrderDetailPart
                {
                    InventoryId = orderDetailPart.InventoryId,
                    OrderId = orderDetailPart.OrderId,
                    Phone = orderDetailPart.Phone,
                    Operator = orderDetailPart.Operator,
                    Product = orderDetailPart.Product,
                    Price = orderDetailPart.Price,
                    Status = resObject.Status.ToLower(),
                    Expires = orderDetailPart.Expires,
                    Created_at = orderDetailPart.Created_at,
                    Country = orderDetailPart.Country,
                    Category = orderDetailPart.Category,
                    Email = orderDetailPart.Email,
                    UserId = orderDetailPart.UserId,
                    UserName = orderDetailPart.UserName
                };

                orderContent.Apply(newOrderDetailPart);

                var resultOrder = await _contentManager.ValidateAsync(orderContent);

                if (resultOrder.Succeeded)
                {
                    await _contentManager.UpdateAsync(orderContent);
                }
            }

            return Ok(resObject);
        }

        #endregion

        #region Cancel order
        /// <summary>
        /// Cancel order
        /// </summary>
        /// <remarks>
        /// 
        /// </remarks> 
        [HttpGet]
        [ActionName("cancelorder")]
        [ProducesResponseType(typeof(CheckOrderDto), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(ProblemDetails), (int)HttpStatusCode.Unauthorized)]
        [ProducesResponseType(typeof(ProblemDetails), (int)HttpStatusCode.BadRequest)]
        [ProducesResponseType(typeof(ProblemDetails), (int)HttpStatusCode.InternalServerError)]
        [ReDocCodeSample("php", "$token = 'Your token';" +
            "\n$ch = curl_init();" +
            "\n$id = 1;" +
            "\ncurl_setopt($ch, CURLOPT_URL, 'https://openapi.simforrent.com/api/user/cancelorder/'. $id);" +
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
            "\nid = '1'" +
            "\nheaders = {" +
            "\n'Authorization': 'Bearer ' + token," +
            "\n'Accept': 'application/json'," +
            "\n}" +
            "\nresponse = requests.get('https://openapi.simforrent.com/api/user/cancelorder/' + id, headers=headers)")]
        [ReDocCodeSample("c#", "var client = new RestClient(\"https://openapi.simforrent.com/api/user/cancelorder\");" +
            "\nvar request = new RestRequest();" +
            "\nrequest.AddHeader(\"Authorization\", \"Bearer \" + token);" +
            "\nrequest.AddHeader(\"Accept\", \"application/json\");" +
            "\nvar response = await client.ExecuteGetAsync(request);")]
        public async Task<ActionResult<CheckOrderDto>> CancelOrderAsync(string id)
        {
            var user = await _userManager.GetUserAsync(User) as Users.Models.User;
            if (user == null || !user.IsEnabled || !user.EmailConfirmed) return BadRequest();
            if (!await _authorizationService.AuthorizeAsync(User, SimApiPermissions.AccessContentApi))
            {
                return this.ChallengeOrForbid();
            }

            var orderId = long.Parse(id);

            var simToken = await ApiCommon.ReadCache(_session, _memoryCache, _signal, _config);

            var orderContent = await _sessionReadOnly
                .Query<ContentItem, ContentItemIndex>(index => index.ContentType == "Orders" && index.Published && index.Latest)
                .With<OrderDetailPartIndex>(p => p.UserId == user.Id && p.OrderId == orderId)
                .FirstOrDefaultAsync();

            if (orderContent == null) return BadRequest();

            using var httpClient = _httpClientFactory.CreateClient("fsim");

            var url = string.Format("https://5sim.net/v1/user/cancel/{0}", orderId);

            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", simToken);

            using var response = await httpClient.GetAsync(url);

            var resObject = await response.Content.ReadFromJsonAsync<OrderDetailPartViewModel>();

            var orderDetailPart = orderContent.As<OrderDetailPart>();
            
            if (!orderDetailPart.Status.Equals(CANCELED, StringComparison.OrdinalIgnoreCase))
            {
                var newOrderDetailPart = new OrderDetailPart
                {
                    InventoryId = orderDetailPart.InventoryId,
                    OrderId = orderDetailPart.OrderId,
                    Phone = orderDetailPart.Phone,
                    Operator = orderDetailPart.Operator,
                    Product = orderDetailPart.Product,
                    Price = orderDetailPart.Price,
                    Status = resObject.Status.ToLower(),
                    Expires = orderDetailPart.Expires,
                    Created_at = orderDetailPart.Created_at,
                    Country = orderDetailPart.Country,
                    Category = orderDetailPart.Category,
                    Email = orderDetailPart.Email,
                    UserId = orderDetailPart.UserId,
                    UserName = orderDetailPart.UserName
                };

                orderContent.Apply(newOrderDetailPart);

                var resultOrder = await _contentManager.ValidateAsync(orderContent);

                if (resultOrder.Succeeded)
                {
                    await _contentManager.UpdateAsync(orderContent);
                }
            }

            return Ok(resObject);
        }
        #endregion

        #region Ban order
        /// <summary>
        /// Ban order
        /// </summary>
        /// <remarks>
        /// 
        /// </remarks> 
        [HttpGet]
        [ActionName("banorder")]
        [ProducesResponseType(typeof(CheckOrderDto), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(ProblemDetails), (int)HttpStatusCode.Unauthorized)]
        [ProducesResponseType(typeof(ProblemDetails), (int)HttpStatusCode.BadRequest)]
        [ProducesResponseType(typeof(ProblemDetails), (int)HttpStatusCode.InternalServerError)]
        [ReDocCodeSample("php", "$token = 'Your token';" +
            "\n$ch = curl_init();" +
            "\n$id = 1;" +
            "\ncurl_setopt($ch, CURLOPT_URL, 'https://openapi.simforrent.com/api/user/banorder/'. $id);" +
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
            "\nid = '1'" +
            "\nheaders = {" +
            "\n'Authorization': 'Bearer ' + token," +
            "\n'Accept': 'application/json'," +
            "\n}" +
            "\nresponse = requests.get('https://openapi.simforrent.com/api/user/banorder/' + id, headers=headers)")]
        [ReDocCodeSample("c#", "var client = new RestClient(\"https://openapi.simforrent.com/api/user/banorder\");" +
            "\nvar request = new RestRequest();" +
            "\nrequest.AddHeader(\"Authorization\", \"Bearer \" + token);" +
            "\nrequest.AddHeader(\"Accept\", \"application/json\");" +
            "\nvar response = await client.ExecuteGetAsync(request);")]
        public async Task<ActionResult<CheckOrderDto>> BanOrderAsync(string id)
        {
            var user = await _userManager.GetUserAsync(User) as Users.Models.User;
            if (user == null || !user.IsEnabled || !user.EmailConfirmed) return BadRequest();
            if (!await _authorizationService.AuthorizeAsync(User, SimApiPermissions.AccessContentApi))
            {
                return this.ChallengeOrForbid();
            }

            var orderId = long.Parse(id);

            var simToken = await ApiCommon.ReadCache(_session, _memoryCache, _signal, _config);

            var orderContent = await _sessionReadOnly
                .Query<ContentItem, ContentItemIndex>(index => index.ContentType == "Orders" && index.Published && index.Latest)
                .With<OrderDetailPartIndex>(p => p.UserId == user.Id && p.OrderId == orderId)
                .FirstOrDefaultAsync();

            if (orderContent == null) return BadRequest();

            using var httpClient = _httpClientFactory.CreateClient("fsim");

            var url = string.Format("https://5sim.net/v1/user/ban/{0}", orderId);

            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", simToken);

            using var response = await httpClient.GetAsync(url);

            var resObject = await response.Content.ReadFromJsonAsync<OrderDetailPartViewModel>();

            var orderDetailPart = orderContent.As<OrderDetailPart>();

            if (!orderDetailPart.Status.Equals(BANNED, StringComparison.OrdinalIgnoreCase))
            {
                var newOrderDetailPart = new OrderDetailPart
                {
                    InventoryId = orderDetailPart.InventoryId,
                    OrderId = orderDetailPart.OrderId,
                    Phone = orderDetailPart.Phone,
                    Operator = orderDetailPart.Operator,
                    Product = orderDetailPart.Product,
                    Price = orderDetailPart.Price,
                    Status = resObject.Status.ToLower(),
                    Expires = orderDetailPart.Expires,
                    Created_at = orderDetailPart.Created_at,
                    Country = orderDetailPart.Country,
                    Category = orderDetailPart.Category,
                    Email = orderDetailPart.Email,
                    UserId = orderDetailPart.UserId,
                    UserName = orderDetailPart.UserName
                };

                orderContent.Apply(newOrderDetailPart);

                var resultOrder = await _contentManager.ValidateAsync(orderContent);

                if (resultOrder.Succeeded)
                {
                    await _contentManager.UpdateAsync(orderContent);
                }
            }

            return Ok(resObject);
        }
        #endregion

        #region SMS inbox list
        /// <summary>
        /// SMS inbox list
        /// </summary>
        /// <remarks>
        /// Get SMS inbox list by order's id.
        /// </remarks> 
        [HttpGet]
        [ActionName("smsinboxlist")]
        [ProducesResponseType(typeof(SmsInboxListDto), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(ProblemDetails), (int)HttpStatusCode.Unauthorized)]
        [ProducesResponseType(typeof(ProblemDetails), (int)HttpStatusCode.BadRequest)]
        [ProducesResponseType(typeof(ProblemDetails), (int)HttpStatusCode.InternalServerError)]
        [ReDocCodeSample("php", "$token = 'Your token';" +
            "\n$ch = curl_init();" +
            "\n$id = 1;" +
            "\ncurl_setopt($ch, CURLOPT_URL, 'https://openapi.simforrent.com/api/user/smsinboxlist/'. $id);" +
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
            "\nid = '1'" +
            "\nheaders = {" +
            "\n'Authorization': 'Bearer ' + token," +
            "\n'Accept': 'application/json'," +
            "\n}" +
            "\nresponse = requests.get('https://openapi.simforrent.com/api/user/smsinboxlist/' + id, headers=headers)")]
        [ReDocCodeSample("c#", "var client = new RestClient(\"https://openapi.simforrent.com/api/user/smsinboxlist\");" +
            "\nvar request = new RestRequest();" +
            "\nrequest.AddHeader(\"Authorization\", \"Bearer \" + token);" +
            "\nrequest.AddHeader(\"Accept\", \"application/json\");" +
            "\nvar response = await client.ExecuteGetAsync(request);")]
        public async Task<ActionResult<SmsInboxListDto>> SMSInboxListAsync(string id)
        {
            var user = await _userManager.GetUserAsync(User) as Users.Models.User;
            if (user == null || !user.IsEnabled || !user.EmailConfirmed) return BadRequest();
            if (!await _authorizationService.AuthorizeAsync(User, SimApiPermissions.AccessContentApi))
            {
                return this.ChallengeOrForbid();
            }

            var fiveSimToken = await ApiCommon.ReadCache(_session, _memoryCache, _signal, _config);

            string url = string.Format("https://5sim.net/v1/user/sms/inbox/{0}", id);

            var client = new RestClient(url);
            var request = new RestRequest();
            request.AddHeader("Authorization", "Bearer " + fiveSimToken);

            var response = await client.ExecuteGetAsync(request);
            var resObject = JsonConvert.DeserializeObject<SmsInboxListDto>(response.Content);
            return Ok(resObject);
        }
        #endregion

        #region getOrderByProductAndCountry

        [HttpGet]
        [ActionName("stableorderproductcountry")]
        public async Task<ActionResult<List<CheckOrderDto>>> GetStableOrderByProductAndCountryAsync(string id, string product, string country)
        {
            var user = await _userManager.GetUserAsync(User) as Users.Models.User;
            if (user == null || !user.IsEnabled || !user.EmailConfirmed) return BadRequest();

            if (!await _authorizationService.AuthorizeAsync(User, SimApiPermissions.AccessContentApi))
            {
                return this.ChallengeOrForbid();
            }

            var simToken = await ApiCommon.ReadCache(_sessionReadOnly, _memoryCache, _signal, _config);
            var percentStringValue = await ApiCommon.ReadCache(_sessionReadOnly, _memoryCache, _signal, _config, "Percentage");
            var percent = string.IsNullOrEmpty(percentStringValue) ? 20 : int.Parse(percentStringValue);

            var orderContent = await _sessionReadOnly
                .Query<ContentItem, ContentItemIndex>(index => index.ContentType == "Orders" && index.Published && index.Latest)
                .With<OrderDetailPartIndex>(p => p.UserId == user.Id
                                            && p.Product == product
                                            && p.Country == country
                                            && p.Expires > DateTime.UtcNow
                                            && (p.Status == PENDING || p.Status == RECEIVED))
                .OrderByDescending(o => o.OrderId)
                .Take(3).ListAsync();

            if (orderContent == null) return BadRequest();

            var orderDetailParts = orderContent.Select(ord => ord.As<OrderDetailPart>()).ToList();

            var ordersReturn = orderDetailParts.Select(ord =>
                                                        new
                                                        {
                                                            ord.InventoryId,
                                                            Id = ord.OrderId,
                                                            ord.Phone,
                                                            ord.Operator,
                                                            ord.Product,
                                                            ord.Price,
                                                            ord.Status,
                                                            ord.Expires,
                                                            ord.Created_at,
                                                            ord.Country,
                                                            ord.Category
                                                        }
                                                    ).ToList();

            return Ok(ordersReturn);
        }

        #endregion

    }
}
