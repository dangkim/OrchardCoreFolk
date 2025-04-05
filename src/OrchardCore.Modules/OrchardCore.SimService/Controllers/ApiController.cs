using System;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using OrchardCore.ContentManagement;
using OrchardCore.SimService.Models;
using OrchardCore.SimService.ContentParts;
using CommonPermissions = OrchardCore.Contents.CommonPermissions;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using OrchardCore.Email;
using OrchardCore.Users;
using Microsoft.AspNetCore.Cors;
using System.Text.RegularExpressions;
using System.Web;
using YesSql;
using OrchardCore.ContentManagement.Records;
using OrchardCore.Users.Services;
using OrchardCore.SimService.ApiModels;
using OrchardCore.SimService.ApiCommonFunctions;
using System.Collections.Generic;
using YesSql.Services;
using OrchardCore.Settings;
using OrchardCore.Users.Models;
using System.Security.Claims;
using Microsoft.AspNetCore.DataProtection;
using OrchardCore.Entities;
using OrchardCore.Modules;
using SignInResult = Microsoft.AspNetCore.Identity.SignInResult;
using OrchardCore.Users.Events;
using OrchardCore.Users.Handlers;
using OrchardCore.Scripting;
using OrchardCore.OpenId.Services;
using OrchardCore.OpenId.Settings;
using System.ComponentModel.DataAnnotations;
using OrchardCore.SimService.Permissions;
using OrchardCore.Users.ViewModels;
using System.Text.Json.Settings;
using System.Text.Json;
using System.Text;
using static GraphQL.Validation.Rules.OverlappingFieldsCanBeMerged;

namespace OrchardCore.SimService.Controllers
{
    [Route("api/content/[action]")]
    [ApiController]
    [Authorize(AuthenticationSchemes = "Api"), IgnoreAntiforgeryToken, AllowAnonymous]
    public class ApiController : Controller
    {
        private static readonly JsonMergeSettings _updateJsonMergeSettings = new JsonMergeSettings { MergeArrayHandling = MergeArrayHandling.Replace };
        private readonly Microsoft.Extensions.Configuration.IConfiguration _config;
        private readonly IContentManager _contentManager;
        private readonly IAuthorizationService _authorizationService;
        private readonly IStringLocalizer S;


        public string btcPayliteApiUrl;
        public string btcPayliteToken;


        public ApiController(
            IUserService userService,
            Microsoft.Extensions.Configuration.IConfiguration config,
            YesSql.ISession session,
            IContentManager contentManager,
            IAuthorizationService authorizationService,
            UserManager<IUser> userManager,
            IEmailAddressValidator emailAddressValidator,
            ILogger<ApiController> logger,
            ISiteService siteService,
            SignInManager<IUser> signInManager,
            IEnumerable<ILoginFormEvent> accountEvents,
            IScriptingManager scriptingManager,
            IEnumerable<IExternalLoginEventHandler> externalLoginHandlers,
            IDataProtectionProvider dataProtectionProvider,
            IClock clock,
            IOpenIdClientService clientService,
            IStringLocalizer<ApiController> stringLocalizer)
        {
            _config = config;
            _contentManager = contentManager;
            _authorizationService = authorizationService;
            S = stringLocalizer;
        }

        [Route("{contentItemId}"), HttpGet]
        public async Task<IActionResult> Get(string contentItemId)
        {
            if (!await _authorizationService.AuthorizeAsync(User, SimApiPermissions.AccessContentApi))
            {
                return this.ChallengeOrForbid();
            }

            var contentItem = await _contentManager.GetAsync(contentItemId);

            if (contentItem == null)
            {
                return NotFound();
            }

            if (!await _authorizationService.AuthorizeAsync(User, CommonPermissions.ViewContent, contentItem))
            {
                return this.ChallengeOrForbid();
            }

            return Ok(contentItem);
        }

        [HttpPost]
        public async Task<IActionResult> Post(ContentItem model, bool draft = false)
        {
            if (!await _authorizationService.AuthorizeAsync(User, SimApiPermissions.AccessContentApi))
            {
                return this.ChallengeOrForbid();
            }

            //dynamic jsonObj = contentItem.Content;

            //string followerChart = Convert.ToString(jsonObj["Influencer"]["FollowerChart"]["Text"]);
            //string chartDate = Convert.ToString(jsonObj["Influencer"]["ChartCategoryDate"]["Text"]);
            //followerChart = followerChart + ";" + followerAndPhotoModel.NumberOfFollowers;
            //chartDate = chartDate + ";" + followerAndPhotoModel.ChartDate;

            //jsonObj["Influencer"]["NumberOfFollowers"]["Value"] = followerAndPhotoModel.NumberOfFollowers;
            //jsonObj["Influencer"]["FollowerChart"]["Text"] = followerChart.Trim(charsToTrim);
            //jsonObj["Influencer"]["ChartCategoryDate"]["Text"] = chartDate.Trim(charsToTrim);

            //var photos = jsonObj["Influencer"]["Photo"]["Paths"];

            //foreach (var item in followerAndPhotoModel.PhotoPaths)
            //{
            //    photos.Add(item);
            //}
            // It is really important to keep the proper method calls order with the ContentManager
            // so that all event handlers gets triggered in the right sequence.

            var contentItem = await _contentManager.GetAsync(model.ContentItemId, VersionOptions.DraftRequired);

            if (contentItem == null)
            {
                if (!await _authorizationService.AuthorizeAsync(User, CommonPermissions.PublishContent))
                {
                    return this.ChallengeOrForbid();
                }

                var newContentItem = await _contentManager.NewAsync(model.ContentType);
                newContentItem.Merge(model);

                var result = await _contentManager.UpdateValidateAndCreateAsync(newContentItem, draft ? VersionOptions.DraftRequired : VersionOptions.Published);
                if (result.Succeeded)
                {
                    contentItem = newContentItem;
                }
                else
                {
                    return Problem(
                        title: S["One or more validation errors occurred."],
                        detail: string.Join(',', result.Errors),
                        statusCode: (int)HttpStatusCode.BadRequest);
                }
            }
            else
            {
                if (!await _authorizationService.AuthorizeAsync(User, CommonPermissions.EditContent, contentItem))
                {
                    return this.ChallengeOrForbid();
                }

                contentItem.Merge(model, _updateJsonMergeSettings);

                await _contentManager.UpdateAsync(contentItem);
                var result = await _contentManager.ValidateAsync(contentItem);

                if (result.Succeeded)
                {
                    if (!draft)
                    {
                        await _contentManager.PublishAsync(contentItem);
                    }
                }
                else
                {
                    return Problem(
                        title: S["One or more validation errors occurred."],
                        detail: string.Join(',', result.Errors),
                        statusCode: (int)HttpStatusCode.BadRequest);
                }
            }

            return Ok(contentItem);
        }
       
        private static bool IsEmail(string str)
        {
            if (String.IsNullOrWhiteSpace(str))
                return false;

            var _Email = new Regex("^((([a-z]|\\d|[!#\\$%&'\\*\\+\\-\\/=\\?\\^_`{\\|}~]|[\\u00A0-\\uD7FF\\uF900-\\uFDCF\\uFDF0-\\uFFEF])+(\\.([a-z]|\\d|[!#\\$%&'\\*\\+\\-\\/=\\?\\^_`{\\|}~]|[\\u00A0-\\uD7FF\\uF900-\\uFDCF\\uFDF0-\\uFFEF])+)*)|((\\x22)((((\\x20|\\x09)*(\\x0d\\x0a))?(\\x20|\\x09)+)?(([\\x01-\\x08\\x0b\\x0c\\x0e-\\x1f\\x7f]|\\x21|[\\x23-\\x5b]|[\\x5d-\\x7e]|[\\u00A0-\\uD7FF\\uF900-\\uFDCF\\uFDF0-\\uFFEF])|(\\\\([\\x01-\\x09\\x0b\\x0c\\x0d-\\x7f]|[\\u00A0-\\uD7FF\\uF900-\\uFDCF\\uFDF0-\\uFFEF]))))*(((\\x20|\\x09)*(\\x0d\\x0a))?(\\x20|\\x09)+)?(\\x22)))@((([a-z]|\\d|[\\u00A0-\\uD7FF\\uF900-\\uFDCF\\uFDF0-\\uFFEF])|(([a-z]|\\d|[\\u00A0-\\uD7FF\\uF900-\\uFDCF\\uFDF0-\\uFFEF])([a-z]|\\d|-|\\.|_|~|[\\u00A0-\\uD7FF\\uF900-\\uFDCF\\uFDF0-\\uFFEF])*([a-z]|\\d|[\\u00A0-\\uD7FF\\uF900-\\uFDCF\\uFDF0-\\uFFEF])))\\.)+(([a-z]|[\\u00A0-\\uD7FF\\uF900-\\uFDCF\\uFDF0-\\uFFEF])|(([a-z]|[\\u00A0-\\uD7FF\\uF900-\\uFDCF\\uFDF0-\\uFFEF])([a-z]|\\d|-|\\.|_|~|[\\u00A0-\\uD7FF\\uF900-\\uFDCF\\uFDF0-\\uFFEF])*([a-z]|[\\u00A0-\\uD7FF\\uF900-\\uFDCF\\uFDF0-\\uFFEF])))\\.?$", RegexOptions.IgnoreCase | RegexOptions.ExplicitCapture | RegexOptions.Compiled, TimeSpan.FromSeconds(2.0));
            return _Email.IsMatch(str);
        }
    }
}
