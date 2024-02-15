using System;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using Newtonsoft.Json.Linq;
using OrchardCore.ContentManagement;
using OrchardCore.ContentManagement.Models;
using OrchardCore.Mvc.Utilities;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using OrchardCore.Email;
using OrchardCore.Users;
using OrchardCore.Users.Models;
using Permissions = OrchardCore.SongServices.Permissions;
using Microsoft.AspNetCore.Cors;
using Newtonsoft.Json;
using System.Text.RegularExpressions;
using System.Web;
using YesSql;
using OrchardCore.ContentManagement.Records;
using System.Text;
using OrchardCore.Users.Services;
using OrchardCore.SongServices.Models;
using SongServices.Core.Models;
using OrchardCore.SongServices.Controllers;
using OrchardCore.SongServices.Indexing;
using Telegram.Bot.Types;

namespace OrchardCore.Content.Controllers
{
    [Route("api/content/[action]")]
    [ApiController]
    [Authorize(AuthenticationSchemes = "Api"), IgnoreAntiforgeryToken, AllowAnonymous]
    public class NotificationApiController : Controller
    {
        private static readonly JsonMergeSettings UpdateJsonMergeSettings = new JsonMergeSettings { MergeArrayHandling = MergeArrayHandling.Replace };
        private readonly Microsoft.Extensions.Configuration.IConfiguration _config;
        private readonly IContentManager _contentManager;
        private readonly IAuthorizationService _authorizationService;
        private readonly IStringLocalizer S;
        private readonly UserManager<IUser> _userManager;
        private readonly IEmailAddressValidator _emailAddressValidator;
        private readonly ILogger _logger;
        private readonly IRabbitMQProducer _rabbitMQProducer;
        private readonly ISession _session;
        private readonly IUserService _userService;
        //private readonly ITeleUpdateService _updateService;

        public string btcPayliteApiUrl;
        public string btcPayliteToken;

        public NotificationApiController(
            IUserService userService,
            Microsoft.Extensions.Configuration.IConfiguration config,
            ISession session,
            IContentManager contentManager,
            IAuthorizationService authorizationService,
            UserManager<IUser> userManager,
            IEmailAddressValidator emailAddressValidator,
            ILogger<ApiController> logger,
            IRabbitMQProducer rabbitMQProducer,
            IStringLocalizer<ApiController> stringLocalizer)
        {
            _userService = userService;
            _config = config;
            _contentManager = contentManager;
            _authorizationService = authorizationService;
            _userManager = userManager;
            _emailAddressValidator = emailAddressValidator ?? throw new ArgumentNullException(nameof(emailAddressValidator));
            _logger = logger;
            S = stringLocalizer;
            _rabbitMQProducer = rabbitMQProducer;
            _session = session;


#if DEBUG
            //btcPayliteApiUrl = "https://localhost:14142";
            //btcPayliteToken = "token 3d3bbe0f8ddba99ce72402fae8114d0a7240bdd3";
            btcPayliteApiUrl = _config["BTCPayLiteUrl"];
            btcPayliteToken = _config["BTCPayLiteToken"];
#else
            btcPayliteApiUrl = _config["BTCPayLiteUrl"];
            btcPayliteToken = _config["BTCPayLiteToken"];
#endif
        }

        [HttpGet]
        [ActionName("JoinToGroup")]
        public async Task<ActionResult> JoinToGroupAsync(string conversationId, string userName)
        {
            var user = await _userManager.FindByNameAsync(User.Identity.Name) as Users.Models.User;

            if (user == null)
            {
                return Ok(false);
            }

            var trader = await _session
                .Query<ContentItem, ContentItemIndex>(index => index.ContentType == "TraderPage" && index.Published && index.Latest)
                .With<TraderForFilteringPartIndex>(p => p.UserId == user.Id)
                .FirstOrDefaultAsync();

            string name = trader.Content["TraderForFilteringPart"].Name;

            if (_rabbitMQProducer.JoinToGroup(conversationId, userName, name))
            {
                return Ok(true);
            }
            return Ok(false);
        }

        [HttpPost]
        [ActionName("CreateNotify")]
        public async Task<ActionResult> CreateNotifyAsync(CreateNotifyModel model)
        {
            var user = await _userManager.FindByNameAsync(User.Identity.Name) as Users.Models.User;

            if (user == null)
            {
                return Ok(false);
            }

            var trader = await _session
                .Query<ContentItem, ContentItemIndex>(index => index.ContentType == "TraderPage" && index.Published && index.Latest)
                .With<TraderForFilteringPartIndex>(p => p.UserId == user.Id)
                .FirstOrDefaultAsync();

            string name = trader.Content["TraderForFilteringPart"].Name;

            model.UserName = name;

            if (_rabbitMQProducer.CreateUserNotify(model))
            {
                return Ok(true);
            }
            return Ok(false);
        }

        [HttpPost]
        [ActionName("CreateMessage")]
        public async Task<ActionResult> CreateMessageAsync(CreateMessageModel model)
        {
            var user = await _userManager.FindByNameAsync(User.Identity.Name) as Users.Models.User;

            if (user == null)
            {
                return Ok(false);
            }

            var trader = await _session
                .Query<ContentItem, ContentItemIndex>(index => index.ContentType == "TraderPage" && index.Published && index.Latest)
                .With<TraderForFilteringPartIndex>(p => p.UserId == user.Id)
                .FirstOrDefaultAsync();

            string name = trader.Content["TraderForFilteringPart"].Name;

            model.Sender = name;

            if (_rabbitMQProducer.CreateMessage(model))
            {
                return Ok(true);
            }
            return Ok(false);
        }

        [HttpPost]
        [ActionName("CreateMessageTele")]
        public async Task<ActionResult> CreateMessageTeleAsync(CreateMessageModel model)
        {
            var query = _session.Query<ContentItem>();

            query.With<ContentItemIndex>(x => x.Published && x.Latest && x.ContentItemId == model.TraderId && x.ContentType == "TraderPage");

            var contentItemTrader = await query.FirstOrDefaultAsync();

            if (contentItemTrader == null)
            {
                return this.ChallengeOrForbid();
            }
            else
            {
                dynamic jsonObjTrader = contentItemTrader.Content;
                var traderObj = jsonObjTrader["TraderForFilteringPart"];

                bool isActivatedTele = traderObj.IsActivatedTele;
                long chatId = traderObj.ChatIdTele;

                //if (isActivatedTele)
                //{
                //await _updateService.SendMessageAsync(model.Content, chatId);

                //return Ok(true);
                //}
            }

            return Ok(false);
        }

        [HttpPost]
        [ActionName("CreateConversation")]
        public async Task<ActionResult> CreateConversationAsync(CreateConversationModel model)
        {
            var user = await _userManager.FindByNameAsync(User.Identity.Name) as Users.Models.User;

            if (user == null)
            {
                return Ok(false);
            }

            var trader = await _session
                .Query<ContentItem, ContentItemIndex>(index => index.ContentType == "TraderPage" && index.Published && index.Latest)
                .With<TraderForFilteringPartIndex>(p => p.UserId == user.Id)
                .FirstOrDefaultAsync();

            string name = trader.Content["TraderForFilteringPart"].Name;

            model.Sender = name;

            if (_rabbitMQProducer.CreateConversation(model))
            {
                return Ok(true);
            }
            return Ok(false);
        }

        [HttpGet]
        [ActionName("ReadNotify")]
        public async Task<ActionResult> ReadNotifyAsync(long NotifyId)
        {
            var user = await _userManager.FindByNameAsync(User.Identity.Name) as Users.Models.User;

            if (user == null)
            {
                return Ok(false);
            }

            var trader = await _session
                .Query<ContentItem, ContentItemIndex>(index => index.ContentType == "TraderPage" && index.Published && index.Latest)
                .With<TraderForFilteringPartIndex>(p => p.UserId == user.Id)
                .FirstOrDefaultAsync();

            string name = trader.Content["TraderForFilteringPart"].Name;

            if (_rabbitMQProducer.ReadUserNotify(name, NotifyId))
            {
                return Ok(true);
            }
            return Ok(false);
        }

        [HttpGet]
        [ActionName("GetUserNotify")]
        public async Task<ActionResult> GetUserNotifyAsync()
        {
            if (User.Identity.Name == null)
            {
                return Ok(false);
            }

            var user = await _userManager.FindByNameAsync(User.Identity.Name) as Users.Models.User;

            if (user == null)
            {
                return Ok(false);
            }
            
            var trader = await _session
                .Query<ContentItem, ContentItemIndex>(index => index.ContentType == "TraderPage" && index.Published && index.Latest)
                .With<TraderForFilteringPartIndex>(p => p.UserId == user.Id)
                .FirstOrDefaultAsync();

            string name = trader.Content["TraderForFilteringPart"].Name;

            if (_rabbitMQProducer.GetUserNotify(name))
            {
                return Ok(true);
            }
            return Ok(false);
        }

        [HttpGet]
        [ActionName("GetUserConversation")]
        public async Task<ActionResult> GetUserConversationAsync(CreateConversationModel model)
        {
            var user = await _userManager.FindByNameAsync(User.Identity.Name) as Users.Models.User;

            if (user == null)
            {
                return Ok(false);
            }

            var trader = await _session
                .Query<ContentItem, ContentItemIndex>(index => index.ContentType == "TraderPage" && index.Published && index.Latest)
                .With<TraderForFilteringPartIndex>(p => p.UserId == user.Id)
                .FirstOrDefaultAsync();

            string name = trader.Content["TraderForFilteringPart"].Name;

            if (_rabbitMQProducer.GetUserConversation(name))
            {
                return Ok(true);
            }
            return Ok(false);
        }

        [HttpGet]
        [ActionName("UserReadMessage")]
        public async Task<ActionResult> UserReadMessageAsync(long conversationId)
        {
            var user = await _userManager.FindByNameAsync(User.Identity.Name) as Users.Models.User;

            if (user == null)
            {
                return Ok(false);
            }

            var trader = await _session
                .Query<ContentItem, ContentItemIndex>(index => index.ContentType == "TraderPage" && index.Published && index.Latest)
                .With<TraderForFilteringPartIndex>(p => p.UserId == user.Id)
                .FirstOrDefaultAsync();

            string name = trader.Content["TraderForFilteringPart"].Name;

            if (_rabbitMQProducer.UserReadMessage(name, conversationId))
            {
                return Ok(true);
            }
            return Ok(false);
        }

    }
}
