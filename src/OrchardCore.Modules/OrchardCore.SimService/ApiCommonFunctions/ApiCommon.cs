using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using OrchardCore.ContentManagement;
using OrchardCore.ContentManagement.Records;
using OrchardCore.Data;
using OrchardCore.Environment.Cache;
using OrchardCore.SimService.ApiModels;
using OrchardCore.SimService.ContentParts;
using OrchardCore.SimService.Indexes;
using OrchardCore.SimService.ViewModels;
using OrchardCore.Users.Models;
using YesSql;
using AppSettingConfig = Microsoft.Extensions.Configuration.IConfiguration;

namespace OrchardCore.SimService.ApiCommonFunctions
{
    public static class ApiCommon
    {
        const string CHARS = "ACDEFGHIJKLMNOPQRUVWXYZ";

        public static async Task<bool> UpdateSixSimBalanceByCoin(IContentManager _contentManager, ISession session, UpdateBalanceModel updateBalanceModel, User user, ILogger logger)
        {
            try
            {
                // Get UserProfile by userId
                var userContent = await session
                .Query<ContentItem, ContentItemIndex>(index => index.ContentType == "UserProfile" && index.Published && index.Latest)
                .With<UserProfilePartIndex>(p => p.UserName == user.UserName)
                .FirstOrDefaultAsync();

                //var newUserContent = await _contentManager.GetAsync(userContent.ContentItemId, VersionOptions.DraftRequired);

                if (userContent != null)
                {
                    var content = userContent.Content;
                    var userProfilePart = content["UserProfilePart"];
                    decimal currentBalance = content["UserProfilePart"].Balance;

                    logger.LogError("UpdateSixSimBalanceByCoin: {Amount} Rate: {Rate}", updateBalanceModel.Amount, updateBalanceModel.Rate);

                    var amount = Decimal.Parse(updateBalanceModel.Amount) * Decimal.Parse(updateBalanceModel.Rate); // All is RUB currency
                    currentBalance += amount;

                    var newUserProfilePart = new UserProfilePart
                    {
                        ProfileId = userProfilePart.ProfileId,
                        Email = userProfilePart.Email,
                        UserId = userProfilePart.UserId,
                        UserName = userProfilePart.UserName,
                        Vendor = userProfilePart.Vendor,
                        DefaultForwardingNumber = userProfilePart.DefaultForwardingNumber,
                        Balance = currentBalance,
                        Amount = amount,
                        Rating = userProfilePart.Rating,
                        DefaultCountryName = userProfilePart.DefaultCoutryName,
                        DefaultIso = userProfilePart.DefaultIso,
                        DefaultPrefix = userProfilePart.DefaultPrefix,
                        DefaultOperatorName = userProfilePart.DefaultOperatorName,
                        FrozenBalance = userProfilePart.FrozenBalance
                    };

                    userContent.Apply(newUserProfilePart);
                    userContent.Owner = user.UserName;
                    userContent.Author = user.UserName;

                    var result = await _contentManager.ValidateAsync(userContent);

                    // Create new Payments
                    var newPaymentContent = await _contentManager.NewAsync("Payments");
                    // Set the current user as the owner to check for ownership permissions on creation
                    newPaymentContent.Owner = user.UserName;
                    newPaymentContent.Author = user.UserName;

                    await _contentManager.CreateAsync(newPaymentContent, VersionOptions.Draft);

                    if (result.Succeeded && newPaymentContent != null)
                    {
                        await _contentManager.UpdateAsync(userContent);

                        return true;
                    }

                }
                return false;
            }
            catch (Exception)
            {
                throw;
            }
        }
        public static async Task<bool> UpdateSixSimBalanceByBank(IContentManager _contentManager, ISession session, UpdateBalanceModel updateBalanceModel, User user, ILogger logger)
        {
            try
            {
                // Get UserProfile by userId
                var userContent = await session
                .Query<ContentItem, ContentItemIndex>(index => index.ContentType == "UserProfile" && index.Published && index.Latest)
                .With<UserProfilePartIndex>(p => p.UserId == updateBalanceModel.UserId)
                .FirstOrDefaultAsync();

                if (userContent != null)
                {
                    var content = userContent.Content;
                    var userProfilePart = content["UserProfilePart"];
                    decimal currentBalance = content["UserProfilePart"].Balance;

                    logger.LogError("UpdateSixSimBalanceByCoin: {Amount} Rate: {Rate}", updateBalanceModel.Amount, updateBalanceModel.Rate);

                    var amount = decimal.Parse(updateBalanceModel.Amount) * decimal.Parse(updateBalanceModel.Rate); // All is RUB currency
                    currentBalance += amount;

                    var newUserProfilePart = new UserProfilePart
                    {
                        ProfileId = userProfilePart.ProfileId,
                        Email = userProfilePart.Email,
                        UserId = userProfilePart.UserId,
                        UserName = userProfilePart.UserName,
                        Vendor = userProfilePart.Vendor,
                        DefaultForwardingNumber = userProfilePart.DefaultForwardingNumber,
                        Balance = currentBalance,
                        Amount = amount,
                        Rating = userProfilePart.Rating,
                        DefaultCountryName = userProfilePart.DefaultCoutryName,
                        DefaultIso = userProfilePart.DefaultIso,
                        DefaultPrefix = userProfilePart.DefaultPrefix,
                        DefaultOperatorName = userProfilePart.DefaultOperatorName,
                        FrozenBalance = userProfilePart.FrozenBalance
                    };

                    userContent.Apply(newUserProfilePart);
                    userContent.Owner = user.UserName;
                    userContent.Author = user.UserName;

                    var result = await _contentManager.ValidateAsync(userContent);

                    // Create new Payments
                    var newPaymentContent = await _contentManager.NewAsync("Payments");
                    // Set the current user as the owner to check for ownership permissions on creation
                    newPaymentContent.Owner = user.UserName;
                    newPaymentContent.Author = user.UserName;

                    await _contentManager.CreateAsync(newPaymentContent, VersionOptions.Draft);

                    if (result.Succeeded && newPaymentContent != null)
                    {
                        await _contentManager.UpdateAsync(userContent);

                        return true;
                    }

                }
                return false;
            }
            catch (Exception)
            {
                throw;
            }
        }
        public static async Task<string> ReadCache(ISession session, IMemoryCache _memoryCache, ISignal _signal, AppSettingConfig _config, string contentType = "FiveSimToken", string userName = "", string currency = "")
        {
            if (!string.IsNullOrEmpty(userName))
            {
                if (contentType == "BlockedUsers")
                {
                    bool isBlocked;

                    if (!_memoryCache.TryGetValue(userName, out isBlocked))
                    {
                        var blockedUser = await session.Query<ContentItem, ContentItemIndex>(x => x.Published && x.Latest && x.ContentType == "BlockedUsers" && x.DisplayText == userName).FirstOrDefaultAsync();

                        if (blockedUser != null)
                        {
                            var blockedUserName = blockedUser.DisplayText;

                            isBlocked = _memoryCache.Set(userName, true, _signal.GetToken(userName));
                        }
                    }

                    return isBlocked.ToString().ToLower();
                }
            }

            return contentType switch
            {
                "BtcpayToken" => await GetValueFromCacheOrDb(session, _memoryCache, _signal, _config["BtcpayCacheKey"], _config["BtcpaySignalCacheKey"], contentType, "BtcpayToken", "Token"),
                "BtcpayStoreKey" => await GetValueFromCacheOrDb(session, _memoryCache, _signal, _config["BtcpayStoreIdKey"], _config["BtcpayStoreIdSignalKey"], contentType, "BtcpayStoreKey", "StoreId"),
                "FiveSimToken" => await GetValueFromCacheOrDb(session, _memoryCache, _signal, _config["FiveSimCacheKey"], _config["FiveSimCacheSignalKey"], contentType, "FiveSimToken", "Token"),
                "SmsHubToken" => await GetValueFromCacheOrDb(session, _memoryCache, _signal, _config["SmsHubCacheKey"], _config["FiveSimCacheSignalKey"], contentType, "SmsHubToken", "Token"),
                "TwoLineSimToken" => await GetValueFromCacheOrDb(session, _memoryCache, _signal, _config["TwoLineSimCacheKey"], _config["TwoLineSimCacheSignalKey"], contentType, "TwoLineSimToken", "Token"),
                "SmsHubProducts" => await GetValueFromCacheOrDb(session, _memoryCache, _signal, _config["SmsHubProductCacheKey"], _config["SmsHubProductCacheSignalKey"], contentType, "SmsHubProducts", "Product"),
                "SmsHubCountries" => await GetValueFromCacheOrDb(session, _memoryCache, _signal, _config["SmsHubCountryCacheKey"], _config["SmsHubCountryCacheSignalKey"], contentType, "SmsHubCountries", "Country"),
                "USimToken" => await GetValueFromCacheOrDb(session, _memoryCache, _signal, _config["USimCacheKey"], _config["USimCacheSignalKey"], contentType, "USimToken", "Token"),
                "VSimToken" => await GetValueFromCacheOrDb(session, _memoryCache, _signal, _config["VSimCacheKey"], _config["VSimCacheSignalKey"], contentType, "VSimToken", "Token"),
                "Percentage" => await GetValueFromCacheOrDb(session, _memoryCache, _signal, _config["PercentKey"], _config["PercentSignalKey"], contentType, "Percentage", "Percent"),
                "LSimPercentage" => await GetValueFromCacheOrDb(session, _memoryCache, _signal, _config["LSimPercentKey"], _config["LSimPercentSignalKey"], contentType, "LSimPercentage", "Percent"),
                "USimPercentage" => await GetValueFromCacheOrDb(session, _memoryCache, _signal, _config["USimPercentKey"], _config["USimPercentSignalKey"], contentType, "USimPercentage", "Percent"),
                "VSimPercentage" => await GetValueFromCacheOrDb(session, _memoryCache, _signal, _config["VSimPercentKey"], _config["VSimPercentSignalKey"], contentType, "VSimPercentage", "Percent"),
                _ => string.Empty,
            };
        }

        public static async Task<string> ReadExchangeRateCache(ISession session, IMemoryCache _memoryCache, ISignal _signal, AppSettingConfig _config, string currency = "USD")
        {
            if (currency == "usd")
            {
                var cacheKey = _config["ExchangeRateUSDKey"];
                var signalKey = _config["ExchangeRateSignalUSDKey"];

                var exchangeRateUSDcache = "";
                if (!_memoryCache.TryGetValue(cacheKey, out exchangeRateUSDcache))
                {
                    var rateContent = await session.Query<ContentItem, ContentItemIndex>(x => x.Published && x.Latest && x.ContentType == "ExchangeRate" && x.DisplayText == "USD").FirstOrDefaultAsync();

                    if (rateContent == null)
                    {
                        return string.Empty;
                    }

                    var rateString = rateContent.Content["ExchangeRate"]["RateFromRub"]["Text"];

                    if (rateString == null)
                    {
                        return string.Empty;
                    }

                    string rateUSD = Convert.ToString(rateString);

                    var cacheEntryOptions = new MemoryCacheEntryOptions().SetSlidingExpiration(TimeSpan.FromMinutes(30));

                    exchangeRateUSDcache = _memoryCache.Set(cacheKey, rateUSD, _signal.GetToken(signalKey));
                }

                return exchangeRateUSDcache;
            }
            else if (currency == "cny")
            {
                var cacheKey = _config["ExchangeRateCNYKey"];
                var signalKey = _config["ExchangeRateSignalCNYKey"];

                var exchangeRateCNYcache = "";
                if (!_memoryCache.TryGetValue(cacheKey, out exchangeRateCNYcache))
                {
                    var rateContent = await session.Query<ContentItem, ContentItemIndex>(x => x.Published && x.Latest && x.ContentType == "ExchangeRate" && x.DisplayText == "CNY").FirstOrDefaultAsync();

                    if (rateContent == null)
                    {
                        return string.Empty;
                    }

                    var rateString = rateContent.Content["ExchangeRate"]["RateFromRub"]["Text"];

                    if (rateString == null)
                    {
                        return string.Empty;
                    }

                    string rateCNY = Convert.ToString(rateString);

                    var cacheEntryOptions = new MemoryCacheEntryOptions().SetSlidingExpiration(TimeSpan.FromMinutes(30));

                    exchangeRateCNYcache = _memoryCache.Set(cacheKey, rateCNY, _signal.GetToken(signalKey));
                }

                return exchangeRateCNYcache;
            }
            else if (currency == "vnd")
            {
                var cacheKey = _config["ExchangeRateVNDKey"];
                var signalKey = _config["ExchangeRateSignalVNDKey"];

                var exchangeRateVNDcache = "";
                if (!_memoryCache.TryGetValue(cacheKey, out exchangeRateVNDcache))
                {
                    var rateContent = await session.Query<ContentItem, ContentItemIndex>(x => x.Published && x.Latest && x.ContentType == "ExchangeRate" && x.DisplayText == "VND").FirstOrDefaultAsync();

                    if (rateContent == null)
                    {
                        return string.Empty;
                    }

                    var rateString = rateContent.Content["ExchangeRate"]["RateFromRub"]["Text"];

                    if (rateString == null)
                    {
                        return string.Empty;
                    }

                    string rateVND = Convert.ToString(rateString);

                    var cacheEntryOptions = new MemoryCacheEntryOptions().SetSlidingExpiration(TimeSpan.FromMinutes(30));

                    exchangeRateVNDcache = _memoryCache.Set(cacheKey, rateVND, _signal.GetToken(signalKey));
                }

                return exchangeRateVNDcache;
            }
            else
            {
                return string.Empty;
            }
        }

        public static string EncodeUserId(long userId)
        {
            if (userId == 0)
            {
                return CHARS[0].ToString();
            }

            var result = string.Empty;

            while (userId > 0)
            {
                var remainder = (int)(userId % CHARS.Length);
                result = CHARS[remainder] + result;
                userId /= CHARS.Length;
            }

            return result;
        }

        public static long DecodeUserId(string encodedUserId)
        {
            long result = 0;

            for (var i = 0; i < encodedUserId.Length; i++)
            {
                result *= CHARS.Length;
                result += CHARS.IndexOf(encodedUserId[i]);
            }

            return result;
        }

        #region Private Function
        public static bool IsUrlValid(string stringCheck)
        {
            return Uri.TryCreate(stringCheck, UriKind.Absolute, out Uri uriResult)
                && (uriResult.Scheme == Uri.UriSchemeHttp || uriResult.Scheme == Uri.UriSchemeHttps);
        }

        public static bool CheckExpriesDay(string dateCheck)
        {
            DateTime inputDateTime = DateTime.ParseExact(dateCheck, "yyyy-MM-dd HH:mm:ss EDT", null);
            DateTime currentDate = DateTime.Now;
            DateTime futureDateTime = inputDateTime.AddDays(30);

            int comparisonResult = DateTime.Compare(futureDateTime, currentDate);

            if (comparisonResult > 0)
            {
                return true;
            }
            return false;
        }

        private static async Task<string> GetValueFromCacheOrDb(ISession session, IMemoryCache memoryCache, ISignal signal, string cacheKey, string signalKey, string contentType, string contentField, string valueObject)
        {
            if (!memoryCache.TryGetValue(cacheKey, out string valueCached))
            {
                var valueContent = await session.Query<ContentItem, ContentItemIndex>(x => x.Published && x.Latest && x.ContentType == contentType).FirstOrDefaultAsync();

                var valueObj = valueContent.Content[contentField];

                string valueText = Convert.ToString(valueObj[valueObject]["Text"]);

                valueCached = memoryCache.Set(cacheKey, valueText, signal.GetToken(signalKey));
            }
            return valueCached;
        }
        #endregion
    }
}
