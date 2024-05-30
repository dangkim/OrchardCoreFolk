using System;
using System.Collections.Generic;
using System.Globalization;
using System.Net;
using System.Threading.Tasks;
using Dapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using Newtonsoft.Json.Linq;
using NSwag.Annotations;
using OrchardCore.ContentManagement;
using OrchardCore.ContentManagement.Records;
using OrchardCore.SimService.ApiModels;
using OrchardCore.SimService.Indexes;
using OrchardCore.SimService.Models;
using OrchardCore.SimService.RedocAttributeProcessors;
using OrchardCore.Data;
using OrchardCore.Environment.Cache;
using CommonPermissions = OrchardCore.Contents.CommonPermissions;
using OrchardCore.Users;
using YesSql;
using OrchardCore.SimService.Permissions;
using System.Linq.Expressions;
using System.Linq;
using YesSql.Services;
using System.Net.Http;
using OrchardCore.Lists.Models;

namespace OrchardCore.SimService.SimApi
{
    [Route("api/content/[action]")]
    [ApiController]
    [Authorize(AuthenticationSchemes = "Api"), IgnoreAntiforgeryToken]
    [OpenApiTag("User", Description = "Get information of user.")]
    public class UserProfileController : Controller
    {
        private readonly ISession _session;
        private readonly IReadOnlySession _readOnlySession;
        private readonly IContentManager _contentManager;
        private readonly UserManager<IUser> _userManager;
        private readonly IAuthorizationService _authorizationService;
        private readonly Microsoft.Extensions.Configuration.IConfiguration _config;
        private readonly IDbConnectionAccessor _dbConnectionAccessor;
        private readonly IMemoryCache _memoryCache;
        private readonly ISignal _signal;
        private readonly IHttpClientFactory _httpClientFactory;

        public UserProfileController(
            ISession session,
            IReadOnlySession readOnlySession,
            IMemoryCache memoryCache,
            ISignal signal,
            IDbConnectionAccessor dbConnectionAccessor,
            IContentManager contentManager,
            UserManager<IUser> userManager,
            IAuthorizationService authorizationService,
            Microsoft.Extensions.Configuration.IConfiguration config,
            IHttpClientFactory httpClientFactory)
        {
            _session = session;
            _readOnlySession = readOnlySession;
            _memoryCache = memoryCache;
            _signal = signal;
            _dbConnectionAccessor = dbConnectionAccessor;
            _contentManager = contentManager;
            _userManager = userManager;
            _authorizationService = authorizationService;
            _config = config;
            _httpClientFactory = httpClientFactory;
        }

        #region Balance Request
        /// <summary>
        /// Balance Request
        /// </summary>
        /// <remarks>
        /// Provides profile data: email, balance and rating.
        /// </remarks> 
        [HttpGet]
        [ActionName("profile")]
        [ProducesResponseType(typeof(ProfileDto), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(ProblemDetails), (int)HttpStatusCode.Unauthorized)]
        [ProducesResponseType(typeof(ProblemDetails), (int)HttpStatusCode.BadRequest)]
        [ProducesResponseType(typeof(ProblemDetails), (int)HttpStatusCode.InternalServerError)]
        [ReDocCodeSample("php", "$token = 'Your token';" +
            "\n$ch = curl_init();" +
            "\ncurl_setopt($ch, CURLOPT_URL, 'https://openapi.simforrent.com/api/user/profile');" +
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
            "\nresponse = requests.get('https://openapi.simforrent.com/api/user/profile', headers=headers)")]
        [ReDocCodeSample("c#", "var client = new RestClient(\"https://openapi.simforrent.com/api/user/profile\");" +
            "\nvar request = new RestRequest();" +
            "\nrequest.AddHeader(\"Authorization\", \"Bearer \" + token);" +
            "\nrequest.AddHeader(\"Accept\", \"application/json\");" +
            "\nvar response = await client.ExecuteGetAsync(request);")]
        public async Task<ActionResult<ProfileDto>> BalanceRequestAsync()
        {
            //var user = await _userManager.GetUserAsync(User) as Users.Models.User;
            //if (user == null) return BadRequest();

            //var fiveSimToken = await ApiCommon.ReadCache(_session, _memoryCache, _signal, _config);

            //var client = new RestClient("https://5sim.net/v1/user/profile");
            //var request = new RestRequest();
            //request.AddHeader("Authorization", "Bearer " + fiveSimToken);
            //request.AddHeader("Accept", "application/json");
            //var response = await client.ExecuteGetAsync<ProfileDto>(request);
            //return Ok(response);
            var user = await _userManager.GetUserAsync(User) as Users.Models.User;

            if (user == null)
            {
                return Forbid();
            }

            if (!await _authorizationService.AuthorizeAsync(User, SimApiPermissions.AccessContentApi))
            {
                return Forbid();
            }

            var userContent = await _session
                .Query<ContentItem, ContentItemIndex>(index => index.ContentType == "UserProfile" && index.Published && index.Latest)
                .With<UserProfilePartIndex>(p => p.UserName == User.Identity.Name)
                .FirstOrDefaultAsync();

            if (userContent != null)
            {
                var totalPrice = 0m;
                // Get OrderDetail to calculate balance
                var orderContents = await _session
                .Query<ContentItem, ContentItemIndex>(index => index.ContentType == "Orders" && index.Published && index.Latest)
                .With<OrderDetailPartIndex>(p => p.UserId == user.Id && (p.Status == "RECEIVED" || p.Status == "PENDING" || p.Status == OrderStatusLSimEnum.WAITING.ToString()) && p.Expires > DateTime.UtcNow)
                .ListAsync();

                foreach (var item in orderContents)
                {
                    var detailPart = item.Content["OrderDetailPart"];

                    var price = (decimal)detailPart.Price;

                    totalPrice += price;
                }

                var content = userContent.Content;
                var userPart = content["UserProfilePart"];
                userPart.Balance -= totalPrice;

                var returnResult = new
                {
                    userPart.Balance,
                    userPart.FrozenBalance,
                    userPart.Email,
                    UserId = user.Id,
                    userPart.Rating,
                    userPart.DefaultCoutryName,
                    userPart.DefaultOperatorName,
                };

                return Ok(returnResult);
            }
            else
            {
                // If we got this far, something failed, redisplay form
                return Problem(title: "One or more validation errors occurred.",
                            detail: "There's no user",
                            statusCode: (int)HttpStatusCode.BadRequest);
            }
        }
        #endregion

        #region Orders history
        /// <summary>
        /// Orders history
        /// </summary>
        /// <remarks>
        /// Provides orders history by choosen category.
        /// </remarks> 
        [HttpGet]
        [ActionName("stableordershistory")]
        [ProducesResponseType(typeof(OrdersHistoryDto), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(ProblemDetails), (int)HttpStatusCode.Unauthorized)]
        [ProducesResponseType(typeof(ProblemDetails), (int)HttpStatusCode.BadRequest)]
        [ProducesResponseType(typeof(ProblemDetails), (int)HttpStatusCode.InternalServerError)]
        [ReDocCodeSample("php", "$token = 'Your token';" +
            "\n$ch = curl_init();" +
            "\n$category = 'hosting';" +
            "\n$limit = 15;" +
            "\n$offset = 0;" +
            "\n$order = 'id';" +
            "\n$reverse = true;" +
            "\ncurl_setopt($ch, CURLOPT_URL, 'https://openapi.simforrent.com/api/content/orders?category=' . $category . '&limit=' . $limit . '&offset=' . $offset . '&order=' . $order . '&reverse=' . $reverse');" +
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
            "\n$category = 'hosting';" +
            "\n$limit = 15;" +
            "\n$offset = 0;" +
            "\n$order = 'id';" +
            "\n$reverse = true;" +
            "\nheaders = {" +
            "\n'Authorization': 'Bearer ' + token," +
            "\n'Accept': 'application/json'," +
            "\n}" +
            "\nparams = (" +
            "\n   ('category', category)," +
            "\n   ('limit', limit)," +
            "\n   ('offset', offset)," +
            "\n   ('order', order)," +
            "\n   ('reverse', reverse)," +
            "\n)" +
            "\nresponse = requests.get('https://openapi.simforrent.com/api/content/orders', headers=headers, params=params)")]
        [ReDocCodeSample("c#", "var client = new RestClient(\"https://openapi.simforrent.com/api/user/orders\");" +
            "\nvar request = new RestRequest();" +
            "\nrequest.AddHeader(\"Authorization\", \"Bearer \" + token);" +
            "\nrequest.AddHeader(\"Accept\", \"application/json\");" +
            "\nvar response = await client.ExecuteGetAsync(request);")]
        public async Task<ActionResult<OrdersHistoryDto>> OrdersRequestAsync(string category, string date, int limit, int offset, string order, string phone, bool reverse, string status, string product)
        {
            var user = await _userManager.GetUserAsync(User) as Users.Models.User;

            if (user == null || !user.IsEnabled) return BadRequest();
            if (!await _authorizationService.AuthorizeAsync(User, SimApiPermissions.AccessContentApi))
            {
                return this.ChallengeOrForbid();
            }

            IEnumerable<ContentItem> orderTypes = null;

            orderTypes = await FilterOrderHistory(user.Id, category, date, limit, offset, order, phone, reverse, status, product);

            if (orderTypes == null)
            {
                return Forbid();
            }

            var orderValues = orderTypes.Select(i => i.As<OrderDetailPart>());

            var returnedResult = new
            {
                data = orderValues.Select(orderPart => new
                {
                    orderPart.InventoryId,
                    Id = orderPart.OrderId,
                    orderPart.Phone,
                    orderPart.Operator,
                    orderPart.Product,
                    orderPart.Price,
                    orderPart.Status,
                    orderPart.Expires,
                    orderPart.Created_at,
                    orderPart.Country,
                    orderPart.Email,
                    orderPart.UserId,
                    orderPart.UserName,
                    orderPart.Category
                }),
                total = orderValues.Count()
            };

            return Ok(returnedResult);
        }
        #endregion

        [HttpGet]
        [ActionName("GroupByProducts")]
        public async Task<ActionResult<object>> OrdersGroupByProductsAsync(string status)
        {
            var user = await _userManager.GetUserAsync(User) as Users.Models.User;

            if (user == null || !user.IsEnabled) return BadRequest();
            if (!await _authorizationService.AuthorizeAsync(User, SimApiPermissions.AccessContentApi))
            {
                return this.ChallengeOrForbid();
            }

            var results = await OrdersGroupByProducts(status);

            return Ok(results);
        }

        [HttpGet]
        [ActionName("GroupByUsers")]
        public async Task<ActionResult<object>> OrdersGroupByUsersAsync(string status)
        {
            var user = await _userManager.GetUserAsync(User) as Users.Models.User;

            if (user == null || !user.IsEnabled) return BadRequest();
            if (!await _authorizationService.AuthorizeAsync(User, SimApiPermissions.AccessContentApi))
            {
                return this.ChallengeOrForbid();
            }

            var results = await OrdersGroupByUsers(status);

            return Ok(results);
        }

        private async Task<IEnumerable<ContentItem>> FilterOrder(long userId, string category, string date, int limit, int offset, string order, string phone, bool reverse, string status)
        {
            var orderTypes = _session
                   .Query<ContentItem, ContentItemIndex>(index => index.ContentType == "Orders" && index.Published)
                   .With<OrderDetailPartIndex>(p => p.UserId == userId);

            if (!string.IsNullOrEmpty(date))
            {
                var parsedDate = DateTime.ParseExact(date, "dd-MM-yy", CultureInfo.InvariantCulture);
                orderTypes = orderTypes
                   .With<OrderDetailPartIndex>(p => p.Created_at >= parsedDate && p.Created_at <= parsedDate.AddDays(1));
            }

            if (!string.IsNullOrEmpty(phone))
            {
                orderTypes = orderTypes
                   .With<OrderDetailPartIndex>(p => p.Phone.Contains(phone));
            }

            if (!string.IsNullOrEmpty(category))
            {
                orderTypes = orderTypes
                   .With<OrderDetailPartIndex>(p => p.Category.Contains(category));
            }

            if (!string.IsNullOrEmpty(status))
            {
                if (status == "RECEIVED" || status == "PENDING")
                {
                    orderTypes = orderTypes.With<OrderDetailPartIndex>(p => p.Status.Contains(status, StringComparison.CurrentCultureIgnoreCase) && p.Expires > DateTime.UtcNow);
                }
                else
                {
                    orderTypes = orderTypes.With<OrderDetailPartIndex>(p => p.Status.Contains(status, StringComparison.CurrentCultureIgnoreCase));
                }
            }

            if (order.Equals("id", StringComparison.OrdinalIgnoreCase))
            {
                if (reverse)
                {
                    var sortedOrderTypes = await orderTypes.OrderByDescending(p => p.Id).Take(limit).Skip(offset).ListAsync();
                    return sortedOrderTypes;
                }
                else
                {
                    var sortedOrderTypes = await orderTypes.OrderBy(p => p.Id).Take(limit).Skip(offset).ListAsync();
                    return sortedOrderTypes;
                }

            }
            else if (order.Equals("product_name", StringComparison.OrdinalIgnoreCase))
            {
                if (reverse)
                {
                    var sortedOrderTypes = await orderTypes.OrderByDescending(p => p.Product).Take(limit).Skip(offset).ListAsync();
                    return sortedOrderTypes;
                }
                else
                {
                    var sortedOrderTypes = await orderTypes.OrderBy(p => p.Product).Take(limit).Skip(offset).ListAsync();
                    return sortedOrderTypes;
                }
            }
            else if (order.Equals("created_at", StringComparison.OrdinalIgnoreCase))
            {
                if (reverse)
                {
                    var sortedOrderTypes = await orderTypes.OrderByDescending(p => p.Created_at).Take(limit).Skip(offset).ListAsync();
                    return sortedOrderTypes;
                }
                else
                {
                    var sortedOrderTypes = await orderTypes.OrderBy(p => p.Created_at).Take(limit).Skip(offset).ListAsync();
                    return sortedOrderTypes;
                }
            }
            else if (order.Equals("country", StringComparison.OrdinalIgnoreCase))
            {
                if (reverse)
                {
                    var sortedOrderTypes = await orderTypes.OrderByDescending(p => p.Country).Take(limit).Skip(offset).ListAsync();
                    return sortedOrderTypes;
                }
                else
                {
                    var sortedOrderTypes = await orderTypes.OrderBy(p => p.Country).Take(limit).Skip(offset).ListAsync();
                    return sortedOrderTypes;
                }
            }
            else if (order.Equals("phone_phone", StringComparison.OrdinalIgnoreCase))
            {
                if (reverse)
                {
                    var sortedOrderTypes = await orderTypes.OrderByDescending(p => p.Phone).Take(limit).Skip(offset).ListAsync();
                    return sortedOrderTypes;
                }
                else
                {
                    var sortedOrderTypes = await orderTypes.OrderBy(p => p.Phone).Take(limit).Skip(offset).ListAsync();
                    return sortedOrderTypes;
                }
            }
            else if (order.Equals("status", StringComparison.OrdinalIgnoreCase))
            {
                if (reverse)
                {
                    var sortedOrderTypes = await orderTypes.OrderByDescending(p => p.Status).Take(limit).Skip(offset).ListAsync();
                    return sortedOrderTypes;
                }
                else
                {
                    var sortedOrderTypes = await orderTypes.OrderBy(p => p.Status).Take(limit).Skip(offset).ListAsync();
                    return sortedOrderTypes;
                }
            }
            else
            {
                var sortedOrderTypes = await orderTypes.OrderByDescending(p => p.Id).Take(limit).Skip(offset).ListAsync();
                return sortedOrderTypes;
            }
        }

        private async Task<IEnumerable<ContentItem>> FilterOrderHistory(long userId, string category, string date, int limit, int offset, string order, string phone, bool reverse, string status, string product)
        {
            var normalizedStatus = status?.ToLower() ?? "";

            var orderTypes = _readOnlySession
                   .Query<ContentItem, ContentItemIndex>(index => index.ContentType == "Orders" && index.Published)
                   .With<OrderDetailPartIndex>(p => p.UserId == userId)
                   .With<OrderDetailPartIndex>(p => p.Phone.Contains(phone ?? ""))
                   .With<OrderDetailPartIndex>(p => p.Category.Contains(category ?? ""))
                   .With<OrderDetailPartIndex>(p => p.Product.Contains(product ?? ""))
                   .With<OrderDetailPartIndex>(p => p.Status.Contains(normalizedStatus ?? ""));


            if (!string.IsNullOrEmpty(date))
            {
                var parsedDate = DateTime.ParseExact(date, "dd/MM/yyyy HH:mm:ss", CultureInfo.InvariantCulture);

                // Specify that the DateTime is in local time
                var localDateTimeWithKind = DateTime.SpecifyKind(parsedDate, DateTimeKind.Local);

                // Convert to UTC
                var utcDateTime = localDateTimeWithKind.ToUniversalTime();

                orderTypes = orderTypes
                   .With<OrderDetailPartIndex>(p => p.Created_at >= utcDateTime && p.Created_at <= utcDateTime.AddDays(1));
            }

            var sortOptions = new Dictionary<string, Expression<Func<OrderDetailPartIndex, object>>>
                                        {
                                            { "order_id", p => p.OrderId },
                                            { "product_name", p => p.Product },
                                            { "created_at", p => p.Created_at },
                                            { "country", p => p.Country },
                                            { "phone_phone", p => p.Phone },
                                            { "status", p => p.Status }
                                        };

            // Default sort option
            var sortExpression = sortOptions.ContainsKey(order.ToLower()) ? sortOptions[order.ToLower()] : sortOptions["id"];

            // Apply sorting
            var sortedOrderTypes = reverse ? await orderTypes.OrderByDescending(sortExpression).Skip(offset).Take(limit).ListAsync() : await orderTypes.OrderBy(sortExpression).Skip(offset).Take(limit).ListAsync();

            // Apply pagination
            //sortedOrderTypes = sortedOrderTypes.Skip(offset).Take(limit);

            return sortedOrderTypes;
        }

        private async Task<int> TotalFilterOrder(long userId, string date, string phone, string status)
        {
            var totalSearch = 0;

            if (!string.IsNullOrEmpty(status) && !string.IsNullOrEmpty(phone) && !string.IsNullOrEmpty(date))
            {
                var parsedDate = DateTime.ParseExact(date, "dd-MM-yy", CultureInfo.InvariantCulture);
                totalSearch = await _session
                    .Query<ContentItem, ContentItemIndex>(index => index.ContentType == "Orders" && index.Published)
                    .With<OrderDetailPartIndex>(p => p.UserId == userId
                            && p.Status.Contains(status, StringComparison.CurrentCultureIgnoreCase)
                            && p.Phone.Contains(phone)
                            && p.Created_at >= parsedDate && p.Created_at <= parsedDate.AddDays(1)).CountAsync();
            }

            else if (!string.IsNullOrEmpty(status) && !string.IsNullOrEmpty(phone))
            {
                totalSearch = await _session
                    .Query<ContentItem, ContentItemIndex>(index => index.ContentType == "Orders" && index.Published)
                    .With<OrderDetailPartIndex>(p => p.UserId == userId
                            && p.Status.Contains(status, StringComparison.CurrentCultureIgnoreCase)
                            && p.Phone.Contains(phone)).CountAsync();
            }

            else if (!string.IsNullOrEmpty(status) && !string.IsNullOrEmpty(date))
            {
                var parsedDate = DateTime.ParseExact(date, "dd-MM-yy", CultureInfo.InvariantCulture);
                totalSearch = await _session
                    .Query<ContentItem, ContentItemIndex>(index => index.ContentType == "Orders" && index.Published)
                    .With<OrderDetailPartIndex>(p => p.UserId == userId
                            && p.Status.Contains(status, StringComparison.CurrentCultureIgnoreCase)
                            && p.Created_at >= parsedDate && p.Created_at <= parsedDate.AddDays(1)).CountAsync();
            }

            else if (!string.IsNullOrEmpty(phone) && !string.IsNullOrEmpty(date))
            {
                var parsedDate = DateTime.ParseExact(date, "dd-MM-yy", CultureInfo.InvariantCulture);
                totalSearch = await _session
                    .Query<ContentItem, ContentItemIndex>(index => index.ContentType == "Orders" && index.Published)
                    .With<OrderDetailPartIndex>(p => p.UserId == userId
                            && p.Phone.Contains(phone)
                            && p.Created_at >= parsedDate && p.Created_at <= parsedDate.AddDays(1)).CountAsync();
            }

            else if (!string.IsNullOrEmpty(phone))
            {
                totalSearch = await _session
                    .Query<ContentItem, ContentItemIndex>(index => index.ContentType == "Orders" && index.Published)
                    .With<OrderDetailPartIndex>(p => p.UserId == userId && p.Phone.Contains(phone)).CountAsync();
            }

            else if (!string.IsNullOrEmpty(status))
            {
                totalSearch = await _session
                    .Query<ContentItem, ContentItemIndex>(index => index.ContentType == "Orders" && index.Published)
                    .With<OrderDetailPartIndex>(p => p.UserId == userId && p.Status.Contains(status, StringComparison.CurrentCultureIgnoreCase)).CountAsync();
            }

            else if (!string.IsNullOrEmpty(date))
            {
                var parsedDate = DateTime.ParseExact(date, "dd-MM-yy", CultureInfo.InvariantCulture);
                totalSearch = await _session
                    .Query<ContentItem, ContentItemIndex>(index => index.ContentType == "Orders" && index.Published)
                    .With<OrderDetailPartIndex>(p => p.Created_at >= parsedDate && p.Created_at <= parsedDate.AddDays(1)).CountAsync();
            }

            return totalSearch;
        }

        private async Task<List<JObject>> OrdersGroupByProducts(string status)
        {
            var connection = _dbConnectionAccessor.CreateConnection();

            IEnumerable<dynamic> queryResults;

            using (connection)
            {
                await connection.OpenAsync();

                using var transaction = connection.BeginTransaction(_session.Store.Configuration.IsolationLevel);
                queryResults = await connection.QueryAsync("Select Product, Country, Operator, Status, COUNT(Product) as Visits from OrderDetailPartIndex group by Product, Country, Operator, Status having Status = @Status", new { Status = status }, transaction);
            }

            var results = new List<JObject>();

            foreach (var document in queryResults)
            {
                results.Add(JObject.FromObject(document));
            }

            return results;
        }

        private async Task<List<JObject>> OrdersGroupByUsers(string status)
        {
            var connection = _dbConnectionAccessor.CreateConnection();

            IEnumerable<dynamic> queryResults;

            using (connection)
            {
                await connection.OpenAsync();

                using var transaction = connection.BeginTransaction(_session.Store.Configuration.IsolationLevel);
                queryResults = await connection.QueryAsync("Select UserName, Product, Status, COUNT(Product) as Visits from OrderDetailPartIndex group by Product, UserName, Status having Status = @Status", new { Status = status }, transaction);
            }

            var results = new List<JObject>();

            foreach (var document in queryResults)
            {
                results.Add(JObject.FromObject(document));
            }

            return results;
        }
    }
}
