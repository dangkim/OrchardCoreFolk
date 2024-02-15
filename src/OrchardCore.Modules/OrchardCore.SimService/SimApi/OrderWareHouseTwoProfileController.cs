using System;
using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using Newtonsoft.Json.Linq;
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
using YesSql;
using OrchardCore.SimService.Permissions;

namespace OrchardCore.SimService.SimApi
{
    [Route("api/content/[action]/{id}")]
    [ApiController]
    [Authorize(AuthenticationSchemes = "Api"), IgnoreAntiforgeryToken, AllowAnonymous]
    [OpenApiTag("Order management", Description = "Get information of order.")]
    public class OrderWareHouseTwoProfileController : Controller
    {
        //public string fiveSimToken;
        private readonly ISession _session;
        private readonly IContentManager _contentManager;
        private readonly UserManager<IUser> _userManager;
        private readonly IAuthorizationService _authorizationService;
        private readonly Microsoft.Extensions.Configuration.IConfiguration _config;
        private readonly IMemoryCache _memoryCache;
        private readonly ISignal _signal;

        public OrderWareHouseTwoProfileController(
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
            //fiveSimToken = _config["FiveSimToken"];
        }

        #region Check order
        /// <summary>
        /// Check order of Ware House Two (Get SMS)
        /// </summary>
        /// <remarks>
        /// 
        /// </remarks> 
        [HttpGet]
        [ActionName("checkOrderWareHouseTwo")]
        [ProducesResponseType(typeof(CheckOrderWareHouseTwoDto), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(ProblemDetails), (int)HttpStatusCode.Unauthorized)]
        [ProducesResponseType(typeof(ProblemDetails), (int)HttpStatusCode.BadRequest)]
        [ProducesResponseType(typeof(ProblemDetails), (int)HttpStatusCode.InternalServerError)]
        [ReDocCodeSample("php", "$token = 'Your token';" +
            "\n$ch = curl_init();" +
            "\n$id = 1;" +
            "\ncurl_setopt($ch, CURLOPT_URL, 'https://openapi.simforrent.com/api/content/checkOrderWareHouseTwo/'. $id);" +
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
            "\nresponse = requests.get('https://openapi.simforrent.com/api/content/checkOrderWareHouseTwo/' + id, headers=headers)")]
        [ReDocCodeSample("c#", "var client = new RestClient(\"https://openapi.simforrent.com/api/content/checkOrderWareHouseTwo\");" +
            "\nvar request = new RestRequest();" +
            "\nrequest.AddHeader(\"Authorization\", \"Bearer \" + token);" +
            "\nrequest.AddHeader(\"Accept\", \"application/json\");" +
            "\nvar response = await client.ExecuteGetAsync(request);")]
        public async Task<ActionResult<CheckOrderWareHouseTwoDto>> CheckOrderWareHouseTwoAsync(string id, int inventory = (int)InventoryEnum.LSim)
        {
            // check orderId from 5sim with OrderDetailPart
            var user = await _userManager.GetUserAsync(User) as Users.Models.User;
            if (user == null || !user.IsEnabled) return BadRequest();
            if (!await _authorizationService.AuthorizeAsync(User, SimApiPermissions.AccessContentApi))
            {
                return this.ChallengeOrForbid();
            }

            var simToken = await ApiCommon.ReadCache(_session, _memoryCache, _signal, _config, "TwoLineSimToken");

            var orderContent = await _session
                .Query<ContentItem, ContentItemIndex>(index => index.ContentType == "OrderType" && index.Published && index.Latest)
                .With<OrderDetailPartIndex>(p => p.UserId == user.Id && p.OrderId == Int64.Parse(id))
                .FirstOrDefaultAsync();

            if (orderContent == null) return BadRequest();

            var orderDetailPart = orderContent.Content["OrderDetailPart"];

            if (orderDetailPart == null) return BadRequest();

            var resObject = await ApiCommon.CheckOrderWareHouseTwoAsync(simToken, id, inventory);

            if (orderDetailPart.Status.ToString().ToLower() != resObject.Status.ToLower())
            {
                var newOrderDetailPart = new OrderDetailPart
                {
                    InventoryId = orderDetailPart.InventoryId,
                    OrderId = orderDetailPart.OrderId,
                    Phone = orderDetailPart.Phone,
                    Operator = orderDetailPart.Operator,
                    Product = orderDetailPart.Product,
                    Price = orderDetailPart.Price,
                    Status = resObject.Status,
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

            if (resObject.Status.ToLower() == OrderStatusLSimEnum.SUCCESS.ToString().ToLower())
            {
                // Create List of SmsType and insert this list to OrderType
                foreach (var itemSms in resObject.Sms)
                {
                    var smsContent = await _session
                       .Query<ContentItem, ContentItemIndex>(index => index.ContentType == "SmsType" && index.Published && index.Latest)
                       .With<SmsPartIndex>(p => p.UserId == user.Id && p.OrderId == Int64.Parse(id) && p.Code == itemSms.Code)
                       .FirstOrDefaultAsync();

                    if (smsContent == null)
                    {
                        //Get payment of this order
                        var paymentContent = await _session
                        .Query<ContentItem, ContentItemIndex>(index => index.ContentType == "PaymentType" && index.Published && index.Latest)
                        .With<PaymentDetailPartIndex>(p => p.UserId == user.Id && p.OrderId == Int64.Parse(id))
                        .FirstOrDefaultAsync();

                        if (paymentContent == null && resObject.Sms.Count == 1)
                        {
                            var userContent = await _session
                                .Query<ContentItem, ContentItemIndex>(index => index.ContentType == "UserProfileType" && index.Published && index.Latest)
                                .With<UserProfilePartIndex>(p => p.UserId == user.Id)
                                .FirstOrDefaultAsync();

                            if (userContent == null) return BadRequest();

                            var userProfilePart = userContent.Content["UserProfilePart"];

                            if (userProfilePart == null) return Forbid();

                            decimal currentBalance = userProfilePart.Balance;
                            //decimal frozenBalance = resObject.Price + (resObject.Price * percent / 100);
                            decimal frozenBalance = orderDetailPart.Price;

                            //currentBalance -= (resObject.Price + (resObject.Price * percent / 100)); // All is RUB currency
                            currentBalance -= frozenBalance;

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

                            // Create new PaymentType
                            var newPaymentContent = await _contentManager.NewAsync("PaymentType");
                            // Set the current user as the owner to check for ownership permissions on creation
                            newPaymentContent.Owner = user.UserName;
                            newPaymentContent.Author = user.UserName;

                            await _contentManager.CreateAsync(newPaymentContent, VersionOptions.DraftRequired);

                            if (newPaymentContent != null)
                            {
                                var newPaymentDetailPart = new PaymentDetailPart
                                {
                                    PaymentId = newPaymentContent.Id,
                                    TypeName = "Buying",
                                    ProviderName = orderDetailPart.Product,
                                    Amount = -frozenBalance,
                                    Balance = currentBalance,
                                    CreatedAt = DateTime.UtcNow,
                                    Email = userProfilePart.Email,
                                    UserId = userProfilePart.UserId,
                                    UserName = userProfilePart.UserName,
                                    OrderId = Int64.Parse(id)
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
                                OrderId = Int64.Parse(id)
                            };

                            newSmsContentItem.Apply(newSmsPart);

                            var resultSms = await _contentManager.ValidateAsync(newSmsContentItem);

                            if (resultSms.Succeeded)
                            {
                                var smsContainedPart = new { ListContentItemId = orderContent.ContentItemId, Order = 0 };
                                var smsObjContainedPart = JObject.FromObject(smsContainedPart);
                                newSmsContentItem.Content["ContainedPart"] = smsObjContainedPart;
                                await _contentManager.PublishAsync(newSmsContentItem);
                            }
                        }
                    }
                }
            }

            // Update Price just for displaying from 5sim
            //resObject.Price += (resObject.Price * percent / 100);
            decimal priceRevert = orderDetailPart.Price;

            resObject.Price += priceRevert;
            resObject.Country = orderDetailPart.Country;
            resObject.Email = orderDetailPart.Email;
            resObject.Operator = orderDetailPart.Operator;
            resObject.Product = orderDetailPart.Product;
            resObject.Created_at = orderDetailPart.Created_at;
            resObject.Expires = orderDetailPart.Expires;
            resObject.Id = orderDetailPart.OrderId;

            return Ok(resObject);
        }
        #endregion
    }
}
