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
using OrchardCore.SimService.Indexes;
using Microsoft.AspNetCore.Http.HttpResults;

namespace OrchardCore.SimService.Controllers
{
    [Route("api/content/[action]")]
    [ApiController]
    [Authorize(AuthenticationSchemes = "Api"), IgnoreAntiforgeryToken, AllowAnonymous]
    public class UserProfileController : Controller
    {
        private readonly Microsoft.Extensions.Configuration.IConfiguration _config;
        private readonly IContentManager _contentManager;
        private readonly IAuthorizationService _authorizationService;
        private readonly IStringLocalizer S;


        public string btcPayliteApiUrl;
        public string btcPayliteToken;
        private readonly ISession _session;

        public UserProfileController(
            Microsoft.Extensions.Configuration.IConfiguration config,
            ISession session,
            IContentManager contentManager,
            IAuthorizationService authorizationService,
            IStringLocalizer<ApiController> stringLocalizer)
        {
            _config = config;
            _contentManager = contentManager;
            _authorizationService = authorizationService;
            _session = session;
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

        [Route("{userId}"), HttpGet]
        public async Task<IActionResult> GetUserBalance(long userId)
        {
            if (!await _authorizationService.AuthorizeAsync(User, SimApiPermissions.AccessContentApi))
            {
                return this.ChallengeOrForbid();
            }

            try
            {
                var userContent = await _session
                .Query<ContentItem, ContentItemIndex>(index => index.ContentType == "UserProfile" && index.Published && index.Latest)
                .With<UserProfilePartIndex>(p => p.UserId == userId)
                .FirstOrDefaultAsync();

                if (userContent == null)
                {
                    var userProfilePart = userContent.As<UserProfilePart>();
                    var currentBalance = userProfilePart.Balance; // All is RUB currency
                    return Ok(currentBalance);
                }
                else
                {
                    return NotFound(null);
                }
            }
            catch (Exception ex)
            {
                return NotFound(ex.Message);
            }
        }

        [HttpPost]
        public async Task<IActionResult> DepositInRub([FromForm] long userId, [FromForm] decimal amountInRub)
        {
            if (!await _authorizationService.AuthorizeAsync(User, SimApiPermissions.AccessContentApi))
            {
                return this.ChallengeOrForbid();
            }

            try
            {
                var userContent = await _session
                .Query<ContentItem, ContentItemIndex>(index => index.ContentType == "UserProfile" && index.Published && index.Latest)
                .With<UserProfilePartIndex>(p => p.UserId == userId)
                .FirstOrDefaultAsync();

                if (userContent != null)
                {
                    var userProfilePart = userContent.As<UserProfilePart>();
                    var currentBalance = userProfilePart.Balance;

                    currentBalance += amountInRub; // All is RUB currency

                    userProfilePart.Balance = currentBalance;

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
                        Amount = amountInRub,
                        GmailMsgId = userProfilePart.GmailMsgId,
                        RateFromRub = userProfilePart.RateFromRub,
                        Rating = userProfilePart.Rating,
                        DefaultCountryName = userProfilePart.DefaultCountryName,
                        DefaultIso = userProfilePart.DefaultIso,
                        DefaultPrefix = userProfilePart.DefaultPrefix,
                        DefaultOperatorName = userProfilePart.DefaultOperatorName,
                        FrozenBalance = userProfilePart.FrozenBalance
                    };

                    userContent.Apply(newUserProfilePart);
                }
                else
                {
                    return NotFound(null);
                }

                return Ok(userContent);
            }
            catch (Exception ex)
            {
                return NotFound(ex.Message);
            }
        }

        [HttpPost]
        public async Task<IActionResult> DeductInRub([FromForm] long userId, [FromForm] decimal amountInRub)
        {
            if (!await _authorizationService.AuthorizeAsync(User, SimApiPermissions.AccessContentApi))
            {
                return this.ChallengeOrForbid();
            }

            try
            {
                var userContent = await _session
                .Query<ContentItem, ContentItemIndex>(index => index.ContentType == "UserProfile" && index.Published && index.Latest)
                .With<UserProfilePartIndex>(p => p.UserId == userId)
                .FirstOrDefaultAsync();

                if (userContent != null)
                {
                    var userProfilePart = userContent.As<UserProfilePart>();
                    var currentBalance = userProfilePart.Balance;

                    currentBalance -= amountInRub; // All is RUB currency

                    userProfilePart.Balance = currentBalance;

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
                        Amount = amountInRub,
                        GmailMsgId = userProfilePart.GmailMsgId,
                        RateFromRub = userProfilePart.RateFromRub,
                        Rating = userProfilePart.Rating,
                        DefaultCountryName = userProfilePart.DefaultCountryName,
                        DefaultIso = userProfilePart.DefaultIso,
                        DefaultPrefix = userProfilePart.DefaultPrefix,
                        DefaultOperatorName = userProfilePart.DefaultOperatorName,
                        FrozenBalance = userProfilePart.FrozenBalance
                    };

                    userContent.Apply(newUserProfilePart);
                }
                else
                {
                    return NotFound(null);
                }

                return Ok(userContent);
            }
            catch (Exception ex)
            {
                return NotFound(ex.Message);
            }
        }
    }
}
