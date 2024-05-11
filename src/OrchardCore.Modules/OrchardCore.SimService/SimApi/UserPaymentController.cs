using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using NSwag.Annotations;
using OrchardCore.ContentManagement;
using OrchardCore.ContentManagement.Records;
using OrchardCore.SimService.ApiModels;
using OrchardCore.SimService.Indexes;
using OrchardCore.SimService.Models;
using OrchardCore.SimService.RedocAttributeProcessors;
using CommonPermissions = OrchardCore.Contents.CommonPermissions;
using OrchardCore.Users;
using YesSql;
using OrchardCore.SimService.Permissions;

namespace OrchardCore.SimService.SimApi
{
    [Route("api/content/[action]")]
    [ApiController]
    [Authorize(AuthenticationSchemes = "Api"), IgnoreAntiforgeryToken]
    [OpenApiTag("User", Description = "Get information of user.")]
    public class UserPaymentController : Controller
    {
        private readonly ISession _session;
        private readonly IContentManager _contentManager;
        private readonly UserManager<IUser> _userManager;
        private readonly IAuthorizationService _authorizationService;
        private readonly Microsoft.Extensions.Configuration.IConfiguration _config;

        public UserPaymentController(
            ISession session,
            IContentManager contentManager,
            UserManager<IUser> userManager,
            IAuthorizationService authorizationService,
            Microsoft.Extensions.Configuration.IConfiguration config)
        {
            _session = session;
            _contentManager = contentManager;
            _userManager = userManager;
            _authorizationService = authorizationService;
            _config = config;
        }

        #region Payments history
        /// <summary>
        /// Payments history
        /// </summary>
        /// <remarks>
        /// Provides payments history.
        /// </remarks> 
        [HttpGet]
        [ActionName("payments")]
        [ProducesResponseType(typeof(PaymentsHistoryDto), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(ProblemDetails), (int)HttpStatusCode.Unauthorized)]
        [ProducesResponseType(typeof(ProblemDetails), (int)HttpStatusCode.BadRequest)]
        [ProducesResponseType(typeof(ProblemDetails), (int)HttpStatusCode.InternalServerError)]
        [ReDocCodeSample("php", "$token = 'Your token';" +
            "\n$ch = curl_init();" +
            "\n$limit = 15;" +
            "\n$offset = 0;" +
            "\n$order = 'id';" +
            "\n$reverse = true;" +
            "\ncurl_setopt($ch, CURLOPT_URL, 'https://openapi.simforrent.com/api/content/payments?limit=' . $limit . '&offset=' . $offset . '&order=' . $order . '&reverse=' . $reverse');" +
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
            "\n$limit = 15;" +
            "\n$offset = 0;" +
            "\n$order = 'id';" +
            "\n$reverse = true;" +
            "\nheaders = {" +
            "\n'Authorization': 'Bearer ' + token," +
            "\n'Accept': 'application/json'," +
            "\n}" +
            "\nparams = (" +
            "\n   ('limit', limit)," +
            "\n   ('offset', offset)," +
            "\n   ('order', order)," +
            "\n   ('reverse', reverse)," +
            "\n)" +
            "\nresponse = requests.get('https://openapi.simforrent.com/api/content/payments', headers=headers, params=params)")]
        [ReDocCodeSample("c#", "var client = new RestClient(\"https://openapi.simforrent.com/api/content/payments\");" +
            "\nvar request = new RestRequest();" +
            "\nrequest.AddHeader(\"Authorization\", \"Bearer \" + token);" +
            "\nrequest.AddHeader(\"Accept\", \"application/json\");" +
            "\nvar response = await client.ExecuteGetAsync(request);")]
        public async Task<ActionResult<PaymentsHistoryDto>> PaymentsRequestAsync(string date, string payment_provider, string payment_type, int limit, int offset, string order, bool reverse)
        {
            var dateFiltered = string.IsNullOrEmpty(date) ? DateTime.MinValue : DateTime.ParseExact(date, "dd-MM-yy", CultureInfo.InvariantCulture);
            // Get from PaymentDetailPart
            var user = await _userManager.GetUserAsync(User) as Users.Models.User;
            if (user == null || !user.IsEnabled) return BadRequest();
            if (!await _authorizationService.AuthorizeAsync(User, SimApiPermissions.AccessContentApi))
            {
                return this.ChallengeOrForbid();
            }

            var paymentTypes = await _session
                .Query<ContentItem, ContentItemIndex>(index => index.ContentType == "Payments" && index.Published)
                .With<PaymentDetailPartIndex>(p => p.UserId == user.Id)
                .OrderBy(x => x.Id)
                .Take(limit).Skip(offset).ListAsync();

            if (paymentTypes == null) return BadRequest();

            if (reverse)
            {
                paymentTypes = await _session
                .Query<ContentItem, ContentItemIndex>(index => index.ContentType == "Payments" && index.Published)
                .With<PaymentDetailPartIndex>(p => p.UserId == user.Id)
                .OrderByDescending(x => x.Id)
                .Take(limit).Skip(offset).ListAsync();
            }
            else if (order.Equals("balance", StringComparison.OrdinalIgnoreCase))
            {
                paymentTypes = await _session
                .Query<ContentItem, ContentItemIndex>(index => index.ContentType == "Payments" && index.Published)
                .With<PaymentDetailPartIndex>(p => p.UserId == user.Id)
                .OrderByDescending(x => x.Balance)
                .Take(limit).Skip(offset).ListAsync();
            }
            else if (order.Equals("amount", StringComparison.OrdinalIgnoreCase))
            {
                paymentTypes = await _session
                .Query<ContentItem, ContentItemIndex>(index => index.ContentType == "Payments" && index.Published)
                .With<PaymentDetailPartIndex>(p => p.UserId == user.Id)
                .OrderByDescending(x => x.Amount)
                .Take(limit).Skip(offset).ListAsync();
            }
            else if (order.Equals("created_at", StringComparison.OrdinalIgnoreCase))
            {
                paymentTypes = await _session
                .Query<ContentItem, ContentItemIndex>(index => index.ContentType == "Payments" && index.Published)
                .With<PaymentDetailPartIndex>(p => p.UserId == user.Id)
                .OrderByDescending(x => x.CreatedAt)
                .Take(limit).Skip(offset).ListAsync();
            }

            var dataPayment = new List<PaymentDetailPart>();
            var data = new List<object>();
            var paymentProviders = new List<object>();
            var types = new List<object>();
            var total = 0;

            foreach (var item in paymentTypes)
            {
                total++;
                var paymentDetailPart = item.Content["PaymentDetailPart"];
                var paymentDetailPartObject = new PaymentDetailPart()
                {
                    PaymentId = paymentDetailPart.PaymentId,
                    CreatedAt = paymentDetailPart.CreatedAt,
                    TypeName = paymentDetailPart.TypeName,
                    ProviderName = paymentDetailPart.ProviderName,
                    Amount = paymentDetailPart.Amount,
                    Balance = paymentDetailPart.Balance
                };

                dataPayment.Add(paymentDetailPartObject);

                var provider = new
                {
                    Name = paymentDetailPart.ProviderName
                };

                paymentProviders.Add(provider);

                var type = new
                {
                    Name = paymentDetailPart.TypeName
                };

                types.Add(type);
            }

            var filterResult = dataPayment.Where(x => (payment_provider == null || x.ProviderName == payment_provider)
                                            && (payment_type == null || x.TypeName == payment_type)
                                            && (DateTime.MinValue == dateFiltered || x.CreatedAt.Date == dateFiltered.Date));

            foreach (var item in filterResult)
            {
                var paymentDetailPartObject = new
                {
                    id = item.PaymentId,
                    createdAt = item.CreatedAt,
                    typeName = item.TypeName,
                    providerName = item.ProviderName,
                    amount = item.Amount,
                    balance = item.Balance
                };

                data.Add(paymentDetailPartObject);
            }

            var resultType = new
            {
                Data = data,
                PaymentProviders = paymentProviders,
                PaymentTypes = types,
                Total = total
            };

            return Ok(resultType);

        }
        #endregion
    }
}
