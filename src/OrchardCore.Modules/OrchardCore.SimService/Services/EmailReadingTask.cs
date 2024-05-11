using System;
using Microsoft.Extensions.DependencyInjection;
using OrchardCore.BackgroundTasks;
using System.Threading;
using System.Threading.Tasks;
using OrchardCore.ContentManagement;
using YesSql;
using OrchardCore.ContentManagement.Records;
using System.Collections.Generic;
using System.Text;
using System.Globalization;
using System.Linq;
using System.Web;
using System.Text.RegularExpressions;
using OrchardCore.Contents.Indexing;
using OrchardCore.SimService.Models;
using Microsoft.Extensions.Logging;
using MailKit.Net.Imap;
using MailKit.Security;
using MailKit;
using MailKit.Search;
using System.Threading.Channels;
using System.Security.Cryptography;
using OrchardCore.Users.Models;
using OrchardCore.SimService.Indexes;
using OrchardCore.SimService.ApiCommonFunctions;
using Microsoft.Extensions.Caching.Memory;
using OrchardCore.Environment.Cache;
using Microsoft.Extensions.Configuration;

namespace OrchardCore.SimService.Services
{
    /// <summary>
    /// This background task will read email.
    /// </summary>
    [BackgroundTask(Schedule = "* * 0 * * *", Description = "read gmail.", IsIncludedSeconds = true)]
    public class EmailReadingTask : IBackgroundTask
    {
        private readonly ILogger _logger;
        private readonly Microsoft.Extensions.Configuration.IConfiguration _config;

        public EmailReadingTask(ILogger<EmailReadingTask> logger, Microsoft.Extensions.Configuration.IConfiguration config)
        {
            _logger = logger;
            _config = config;
        }

        public async Task DoWorkAsync(IServiceProvider serviceProvider, CancellationToken cancellationToken)
        {
            var session = serviceProvider.GetService<ISession>();
            var memoryCache = serviceProvider.GetService<IMemoryCache>();
            var signal = serviceProvider.GetService<ISignal>();
            var contentManager = serviceProvider.GetRequiredService<IContentManager>();

            using var client = new ImapClient();

            // get rate from cache
            var vndRateString = await ApiCommon.ReadExchangeRateCache(session, memoryCache, signal, _config, "VND");
            var rubRateString = await ApiCommon.ReadExchangeRateCache(session, memoryCache, signal, _config, "RUB");

            var vndRateDouble = Decimal.Parse(vndRateString);
            var rubRateDouble = Decimal.Parse(rubRateString);

            try
            {
                if (!string.IsNullOrEmpty(vndRateString) && !string.IsNullOrEmpty(rubRateString))
                {
                    // Connect to the IMAP server
                    await client.ConnectAsync("imap.gmail.com", 993, SecureSocketOptions.SslOnConnect, cancellationToken);

                    // Authenticate with the server
                    await client.AuthenticateAsync("noidiahanquoc78@gmail.com", "qmrtjgqmojterhac", cancellationToken);

                    var inbox = client.Inbox;

                    // Open the Inbox folder                    
                    await inbox.OpenAsync(FolderAccess.ReadWrite, cancellationToken);

                    var uids = await inbox.SearchAsync(SearchQuery.NotSeen, cancellationToken);

                    var messages = inbox.Fetch(uids, MessageSummaryItems.UniqueId | MessageSummaryItems.Envelope | MessageSummaryItems.Flags, cancellationToken: cancellationToken)
                               .OrderByDescending(summary => summary.Envelope.Date)
                               .Take(10);

                    foreach (var message in messages)
                    {
                        var email = await inbox.GetMessageAsync(message.UniqueId, cancellationToken);
                        var subject = email.Subject;
                        var mailBody = email.Body;

                        var readableText = email.TextBody;

                        var userId = -1;

                        _logger.LogError("-----------------check isVCB----------------------");
                        var isVCB = subject.Contains("[SMSForwarder] New message from Vietcombank");
                        //var isTCB = subject.Contains("[SMSForwarder] New message from Techcombank");

                        //if (isVCB || isTCB)
                        if (isVCB)
                        {
                            //READ MAIL BODY-------------------------------------------------------------------------------------
                            _logger.LogError("-----------------READ MAIL BODY----------------------");


                            if (!string.IsNullOrEmpty(readableText))
                            {
                                _logger.LogError("-----------------READ CTS----------------------");
                                var firstIndex = readableText.IndexOf("CTS");
                                var lastIndex = readableText.LastIndexOf("CTS");
                                if (firstIndex != -1 && lastIndex != -1)
                                {
                                    //MESSAGE MARKS AS READ AFTER READING MESSAGE
                                    await inbox.AddFlagsAsync(message.UniqueId, MessageFlags.Seen, true, cancellationToken);
                                    var firstIndexOfZ = firstIndex + 2;
                                    userId = Int32.Parse(readableText.Substring(firstIndexOfZ + 1, lastIndex - (firstIndexOfZ + 1)));
                                }

                                if (userId != -1)
                                {
                                    var firstIndexOfPlus = readableText.IndexOf('+');
                                    StringBuilder stringBuilder = new StringBuilder();

                                    for (int i = firstIndexOfPlus + 1; i < readableText.Length - (firstIndexOfPlus + 1); i++)
                                    {
                                        if (Char.IsNumber(readableText[i]))
                                        {
                                            stringBuilder.Append(readableText[i]);
                                        }
                                        else if (readableText[i] != ',' && !Char.IsNumber(readableText[i]))
                                        {
                                            break;
                                        }
                                    }

                                    if (stringBuilder.Length > 0)
                                    {
                                        // Update Balance should come along with an id of email
                                        var amount = Decimal.Parse(stringBuilder.ToString());

                                        var amountInUsd = amount * vndRateDouble;

                                        var amountInRub = amountInUsd / rubRateDouble;

                                        await UpdateSixSimBalanceByBankAsync(contentManager, session, amount, amountInRub, "VND", rubRateDouble, userId, message.EmailId, isVCB);
                                    }
                                }
                            }
                        }

                    }

                    // Disconnect from the server
                    await client.DisconnectAsync(true, cancellationToken);
                }
            }
            catch (Exception)
            {
                // Disconnect from the server
                await client.DisconnectAsync(true, cancellationToken);
                throw;
            }

            return;
        }


        private async Task<bool> UpdateSixSimBalanceByBankAsync(IContentManager _contentManager, ISession session, decimal originalAmount, decimal amount, string currency, decimal rateInUsd, int userId, string gmailMsgId, bool isVCB = false)
        {
            _logger.LogError("-----------------UpdateSixSimBalanceByBankAsync----------------------");
            try
            {
                // Get UserProfile by userId
                var userContent = await session
                .Query<ContentItem, ContentItemIndex>(index => index.ContentType == "UserProfile" && index.Published && index.Latest)
                .With<UserProfilePartIndex>(p => p.UserId == userId) // TODO: temporarily uncheck gmailMsgId,  Should check gmailMsgId
                .FirstOrDefaultAsync();

                if (userContent != null)
                {
                    _logger.LogError("-----------------userContent----------------------");
                    var newUserContent = await _contentManager.GetAsync(userContent.ContentItemId, VersionOptions.DraftRequired);

                    var content = userContent.Content;
                    var userProfilePart = content["UserProfilePart"];
                    decimal currentBalance = userProfilePart.Balance;

                    currentBalance += amount; // All is RUB currency

                    var newUserProfilePart = new UserProfilePart
                    {
                        ProfileId = userProfilePart.ProfileId,
                        Email = userProfilePart.Email,
                        UserId = userProfilePart.UserId,
                        UserName = userProfilePart.UserName,
                        Vendor = userProfilePart.Vendor,
                        DefaultForwardingNumber = userProfilePart.DefaultForwardingNumber,
                        Balance = currentBalance,
                        OriginalAmount = originalAmount,
                        Amount = amount,
                        GmailMsgId = gmailMsgId,
                        RateInUsd = rateInUsd,
                        Rating = userProfilePart.Rating,
                        DefaultCoutryName = userProfilePart.DefaultCoutryName,
                        DefaultIso = userProfilePart.DefaultIso,
                        DefaultPrefix = userProfilePart.DefaultPrefix,
                        DefaultOperatorName = userProfilePart.DefaultOperatorName,
                        FrozenBalance = userProfilePart.FrozenBalance
                    };

                    newUserContent.Apply(newUserProfilePart);
                    newUserContent.Owner = userProfilePart.UserName;
                    newUserContent.Author = userProfilePart.UserName;

                    // Create new Payments
                    var newPaymentContent = await _contentManager.NewAsync("Payments");
                    // Set the current user as the owner to check for ownership permissions on creation
                    newPaymentContent.Owner = userProfilePart.UserName;
                    newPaymentContent.Author = userProfilePart.UserName;

                    await _contentManager.CreateAsync(newPaymentContent, VersionOptions.Draft);

                    if (newPaymentContent != null)
                    {
                        var newPaymentDetailPart = new PaymentDetailPart
                        {
                            PaymentId = newPaymentContent.Id,
                            TypeName = "charge",
                            ProviderName = isVCB ? "VCB" : "N/A",
                            Amount = amount,
                            Balance = currentBalance,
                            CreatedAt = DateTime.UtcNow,
                            Email = userProfilePart.Email,
                            UserId = userProfilePart.UserId,
                            UserName = userProfilePart.UserName,
                        };

                        newPaymentContent.Apply(newPaymentDetailPart);

                        var resultPayment = await _contentManager.ValidateAsync(newPaymentContent);
                        var resultUser = await _contentManager.ValidateAsync(newUserContent);

                        if (resultPayment.Succeeded && resultUser.Succeeded)
                        {
                            //await _contentManager.UpdateAsync(userContent);
                            await _contentManager.PublishAsync(newUserContent);

                            await _contentManager.PublishAsync(newPaymentContent);

                            return true;
                        }
                        else
                        {
                            return false;
                        }
                    }

                }
                return true;
            }
            catch (Exception)
            {
                throw;
            }
        }
    }
}
