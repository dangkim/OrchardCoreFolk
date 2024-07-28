using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using OrchardCore.ContentManagement;
using OrchardCore.ContentManagement.Records;
using OrchardCore.SimService.ApiModels;
using OrchardCore.SimService.Indexes;
using OrchardCore.SimService.Models;
using OrchardCore.SimService.ViewModels;
using OrchardCore.Environment.Cache;
using OrchardCore.Users.Models;
using RestSharp;
using YesSql;
using OrchardCore.Data;

namespace OrchardCore.SimService.ApiCommonFunctions
{
    public static class ApiCommon
    {
        const string CHARS = "ACDEFGHIJKLMNOPQRUVWXYZ";

        public static async Task<Dictionary<string, decimal>> CalculateBalanceAsync(decimal userBalanceBTC, decimal userBalanceEth, decimal userBalanceUsdt20, decimal userBalanceVND, string userName, ISession session)
        {
            var document = await session.Query<ContentItem, ContentItemIndex>(x => x.Published && x.Latest && x.DisplayText.Contains("pending") && x.ContentType == "Trade" && x.Owner == userName).ListAsync();
            var dictionaryBalance = new Dictionary<string, decimal>();

            foreach (var item in document)
            {
                dynamic jsonObj = item.Content;
                var tradeObj = jsonObj["Trade"];

                TradeContent trade = JsonConvert.DeserializeObject<TradeContent>(tradeObj.ToString());

                if ((trade.Status.Text != "cancel" && trade.Status.Text != "completed") || trade.OfferType.Text == "withdraw")
                {
                    var tradeAmountBtc = (trade.TradeBTCAmount == null || trade.TradeBTCAmount.Text == null) ? 0.0m : Decimal.Parse(trade.TradeBTCAmount.Text);
                    var feeAmountBtc = (trade.FeeBTCAmount == null || trade.FeeBTCAmount.Text == null) ? 0.0m : Decimal.Parse(trade.FeeBTCAmount.Text);

                    var tradeAmountEth = (trade.TradeETHAmount == null || trade.TradeETHAmount.Text == null) ? 0.0m : Decimal.Parse(trade.TradeETHAmount.Text);
                    var feeAmountEth = (trade.FeeETHAmount == null || trade.FeeETHAmount.Text == null) ? 0.0m : Decimal.Parse(trade.FeeETHAmount.Text);

                    var tradeAmountUsdt20 = (trade.TradeUSDT20Amount == null || trade.TradeUSDT20Amount.Text == null) ? 0.0m : Decimal.Parse(trade.TradeUSDT20Amount.Text);
                    var feeAmountUsdt20 = (trade.FeeUSDT20Amount == null || trade.FeeUSDT20Amount.Text == null) ? 0.0m : Decimal.Parse(trade.FeeUSDT20Amount.Text);

                    var tradeAmountVnd = (trade.TradeVNDAmount == null || trade.TradeVNDAmount.Text == null) ? 0.0m : Decimal.Parse(trade.TradeVNDAmount.Text);
                    var feeAmountVnd = (trade.FeeVNDAmount == null || trade.FeeVNDAmount.Text == null) ? 0.0m : Decimal.Parse(trade.FeeVNDAmount.Text);

                    userBalanceBTC -= tradeAmountBtc + feeAmountBtc;
                    userBalanceEth -= tradeAmountEth + feeAmountEth;
                    userBalanceUsdt20 -= tradeAmountUsdt20 + feeAmountUsdt20;
                    userBalanceVND -= tradeAmountVnd + feeAmountVnd;
                }
            }

            dictionaryBalance.Add("btc", userBalanceBTC);
            dictionaryBalance.Add("eth", userBalanceEth);
            dictionaryBalance.Add("usdt20", userBalanceUsdt20);
            dictionaryBalance.Add("vnd", userBalanceVND);

            return dictionaryBalance;
        }
        public static async Task<bool> UpdateSixSimBalanceByCoin(IContentManager _contentManager, IReadOnlySession session, UpdateBalanceModel updateBalanceModel, User user, ILogger logger)
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
                        DefaultCoutryName = userProfilePart.DefaultCoutryName,
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
                        var newPaymentDetailPart = new PaymentDetailPart
                        {
                            PaymentId = newPaymentContent.Id,
                            TypeName = "charge",
                            ProviderName = updateBalanceModel.Crypto,
                            Amount = amount,
                            Balance = currentBalance,
                            CreatedAt = DateTime.UtcNow,
                            UserId = user.Id,
                            UserName = user.UserName,
                            Email = user.Email
                        };

                        newPaymentContent.Apply(newPaymentDetailPart);

                        var resultPayment = await _contentManager.ValidateAsync(newPaymentContent);

                        if (resultPayment.Succeeded)
                        {
                            await _contentManager.UpdateAsync(userContent);

                            await _contentManager.PublishAsync(newPaymentContent);

                            return true;
                        }
                        else
                        {
                            return false;
                        }
                    }

                }
                return false;
            }
            catch (Exception)
            {
                throw;
            }
        }
        public static async Task<bool> UpdateSixSimBalanceByBank(IContentManager _contentManager, IReadOnlySession session, UpdateBalanceModel updateBalanceModel, User user, ILogger logger)
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
                        DefaultCoutryName = userProfilePart.DefaultCoutryName,
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
                        var newPaymentDetailPart = new PaymentDetailPart
                        {
                            PaymentId = newPaymentContent.Id,
                            TypeName = "charge",
                            ProviderName = updateBalanceModel.Crypto,
                            Amount = amount,
                            Balance = currentBalance,
                            CreatedAt = DateTime.UtcNow,
                            UserId = user.Id,
                            UserName = user.UserName,
                            Email = user.Email
                        };

                        newPaymentContent.Apply(newPaymentDetailPart);

                        var resultPayment = await _contentManager.ValidateAsync(newPaymentContent);

                        if (resultPayment.Succeeded)
                        {
                            await _contentManager.UpdateAsync(userContent);

                            await _contentManager.PublishAsync(newPaymentContent);

                            return true;
                        }
                        else
                        {
                            return false;
                        }
                    }

                }
                return false;
            }
            catch (Exception)
            {
                throw;
            }
        }
        public static async Task<string> ReadCache(IReadOnlySession session, IMemoryCache _memoryCache, ISignal _signal, Microsoft.Extensions.Configuration.IConfiguration _config, string contentType = "FiveSimToken")
        {
            return contentType switch
            {
                "BtcpayToken" => await GetValueFromCacheOrDb(session, _memoryCache, _signal, _config["BtcpayCacheKey"], _config["BtcpaySignalCacheKey"], contentType, "BtcpayToken", "Token"),
                "BtcpayStoreKey" => await GetValueFromCacheOrDb(session, _memoryCache, _signal, _config["BtcpayStoreIdKey"], _config["BtcpayStoreIdSignalKey"], contentType, "BtcpayStoreKey", "Token"),
                "FiveSimToken" => await GetValueFromCacheOrDb(session, _memoryCache, _signal, _config["FiveSimCacheKey"], _config["FiveSimCacheSignalKey"], contentType, "FiveSimToken", "Token"),
                "TwoLineSimToken" => await GetValueFromCacheOrDb(session, _memoryCache, _signal, _config["TwoLineSimCacheKey"], _config["TwoLineSimCacheSignalKey"], contentType, "TwoLineSimToken", "Token"),
                "CSimToken" => await GetValueFromCacheOrDb(session, _memoryCache, _signal, _config["CSimCacheKey"], _config["CSimCacheSignalKey"], contentType, "CSimToken", "Token"),
                "USimToken" => await GetValueFromCacheOrDb(session, _memoryCache, _signal, _config["USimCacheKey"], _config["USimCacheSignalKey"], contentType, "USimToken", "Token"),
                "VSimToken" => await GetValueFromCacheOrDb(session, _memoryCache, _signal, _config["VSimCacheKey"], _config["VSimCacheSignalKey"], contentType, "VSimToken", "Token"),
                "Percentage" => await GetValueFromCacheOrDb(session, _memoryCache, _signal, _config["PercentKey"], _config["PercentSignalKey"], contentType, "Percentage", "Percent"),
                "LSimPercentage" => await GetValueFromCacheOrDb(session, _memoryCache, _signal, _config["LSimPercentKey"], _config["LSimPercentSignalKey"], contentType, "LSimPercentage", "Percent"),
                "CSimPercentage" => await GetValueFromCacheOrDb(session, _memoryCache, _signal, _config["CSimPercentKey"], _config["CSimPercentSignalKey"], contentType, "CSimPercentage", "Percent"),
                "USimPercentage" => await GetValueFromCacheOrDb(session, _memoryCache, _signal, _config["USimPercentKey"], _config["USimPercentSignalKey"], contentType, "USimPercentage", "Percent"),
                "VSimPercentage" => await GetValueFromCacheOrDb(session, _memoryCache, _signal, _config["VSimPercentKey"], _config["VSimPercentSignalKey"], contentType, "VSimPercentage", "Percent"),
                _ => string.Empty,
            };
        }

        public static async Task<string> ReadExchangeRateCache(IReadOnlySession session, IMemoryCache _memoryCache, ISignal _signal, Microsoft.Extensions.Configuration.IConfiguration _config, string currency = "VND")
        {
            if (currency == "VND")
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

                    var rateString = rateContent.Content["ExchangeRate"]["RateToUsd"]["Text"];

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
            else if (currency == "RUB")
            {
                var cacheKey = _config["ExchangeRateRUBKey"];
                var signalKey = _config["ExchangeRateSignalRUBKey"];

                var exchangeRateRUBcache = "";
                if (!_memoryCache.TryGetValue(cacheKey, out exchangeRateRUBcache))
                {
                    var rateContent = await session.Query<ContentItem, ContentItemIndex>(x => x.Published && x.Latest && x.ContentType == "ExchangeRate" && x.DisplayText == "RUB").FirstOrDefaultAsync();

                    if (rateContent == null)
                    {
                        return string.Empty;
                    }

                    var rateString = rateContent.Content["ExchangeRate"]["RateToUsd"]["Text"];

                    if (rateString == null)
                    {
                        return string.Empty;
                    }

                    string rateRUB = Convert.ToString(rateString);

                    var cacheEntryOptions = new MemoryCacheEntryOptions().SetSlidingExpiration(TimeSpan.FromMinutes(30));

                    exchangeRateRUBcache = _memoryCache.Set(cacheKey, rateRUB, _signal.GetToken(signalKey));
                }

                return exchangeRateRUBcache;
            }
            else
            {
                return string.Empty;
            }
        }

        public static async Task<BuyHostingNumberDto> BuyHostingNumber(string simToken, string country, string operato, string product, int inventory = (int)InventoryEnum.FSim)
        {
            if (inventory == (int)InventoryEnum.FSim)
            {
                string url = string.Format("https://5sim.net/v1/user/buy/hosting/{0}/{1}/{2}", country, operato, product);

                var client = new RestClient(url);
                var request = new RestRequest();
                request.AddHeader("Authorization", "Bearer " + simToken);

                var response = await client.ExecuteGetAsync(request);
                var resObject = JsonConvert.DeserializeObject<BuyHostingNumberDto>(response.Content);

                return resObject;
            }
            else
            {
                return new BuyHostingNumberDto();
            }
        }

        public static async Task<OrderDetailPartViewModel> CheckOrderWareHouseTwoAsync(string simToken, string orderId, int inventory = (int)InventoryEnum.LSim)
        {
            if (inventory == (int)InventoryEnum.LSim)
            {
                orderId = orderId.Remove(0, 1);
                var url = string.Format("https://2ndline.io/apiv1/ordercheck?apikey={0}&id={1}", simToken, orderId);

                var client = new RestClient(url);
                var request = new RestRequest();

                var response = await client.ExecuteGetAsync(request);

                if (response.StatusCode == HttpStatusCode.InternalServerError)
                {
                    var resErrorOrderDetailPart = new OrderDetailPartViewModel
                    {

                        Status = Enum.GetName(typeof(OrderStatusLSimEnum), OrderStatusLSimEnum.CANCELED)
                    };

                    return resErrorOrderDetailPart;
                }

                var resObject = JsonConvert.DeserializeObject<SmsWareHouseTwoDto>(response.Content);
                var smsLSim = new SmsPartViewModel { Code = resObject.data.code, Text = IsUrlValid(resObject.data.message) ? "" : resObject.data.message, Created_at = DateTime.UtcNow, Date = DateTime.UtcNow };

                string LSimStatusOrder = "";
                if (resObject.data.statusOrder.ToString() == "-1")
                {
                    LSimStatusOrder = Enum.GetName(typeof(OrderStatusLSimEnum), OrderStatusLSimEnum.CANCELED);
                }
                else if (resObject.data.statusOrder.ToString() == "0")
                {
                    LSimStatusOrder = Enum.GetName(typeof(OrderStatusLSimEnum), OrderStatusLSimEnum.WAITING);
                }
                else if (resObject.data.statusOrder.ToString() == "1")
                {
                    LSimStatusOrder = Enum.GetName(typeof(OrderStatusLSimEnum), OrderStatusLSimEnum.SUCCESS);
                }

                var resOrderDetailPart = new OrderDetailPartViewModel
                {
                    InventoryId = (int)InventoryEnum.LSim,
                    Id = long.Parse(((int)InventoryEnum.LSim).ToString() + resObject.data.id.ToString()),
                    Phone = resObject.data.phone,
                    Operator = "",
                    Product = "",
                    Price = 0,
                    Status = LSimStatusOrder,
                    Expires = DateTime.UtcNow.AddMinutes(20),
                    Created_at = DateTime.UtcNow,
                    Country = "",
                    Category = "activation",
                    Email = "",
                    UserId = 0,
                    UserName = "",
                    Sms = new List<SmsPartViewModel> { smsLSim }

                };

                return resOrderDetailPart;
            }
            else
            {
                return new OrderDetailPartViewModel();
            }
        }

        public static async Task<OrderDetailPartViewModel> CheckOrderWareHouseThreeAsync(string simToken, string orderId, int inventory = (int)InventoryEnum.CSim)
        {
            if (inventory == (int)InventoryEnum.CSim)
            {
                orderId = orderId.Remove(0, 1);
                var url = string.Format("https://chothuesimcode.com/api?act=code&apik={0}&id={1}", simToken, orderId);

                var client = new RestClient(url);
                var request = new RestRequest();

                var response = await client.ExecuteGetAsync(request);

                var resObject = JsonConvert.DeserializeObject<CheckOrderWareHouseThreeDto>(response.Content);
                if (resObject.ResponseCode == 3)
                {
                    throw new Exception(resObject.Msg);
                }

                var smsCSim = new SmsPartViewModel();

                string CSimStatusOrder = "";
                if (resObject.ResponseCode == 2)
                {
                    CSimStatusOrder = Enum.GetName(typeof(OrderStatusLSimEnum), OrderStatusLSimEnum.CANCELED);
                }
                else if (resObject.ResponseCode == 1)
                {
                    CSimStatusOrder = Enum.GetName(typeof(OrderStatusLSimEnum), OrderStatusLSimEnum.WAITING);
                }
                else if (resObject.ResponseCode == 0)
                {
                    CSimStatusOrder = Enum.GetName(typeof(OrderStatusLSimEnum), OrderStatusLSimEnum.SUCCESS);
                    smsCSim.Code = resObject.Result.Code;
                    smsCSim.Text = IsUrlValid(resObject.Result.SMS) ? "" : resObject.Result.SMS;
                    smsCSim.Created_at = DateTime.UtcNow;
                    smsCSim.Date = DateTime.UtcNow;
                }

                var resOrderDetailPart = new OrderDetailPartViewModel
                {
                    InventoryId = (int)InventoryEnum.CSim,
                    Id = long.Parse(orderId),
                    Phone = "",
                    Operator = "",
                    Product = "",
                    Price = 0,
                    Status = CSimStatusOrder,
                    Expires = DateTime.UtcNow.AddMinutes(5),
                    Created_at = DateTime.UtcNow,
                    Country = "VietNam",
                    Category = "activation",
                    Email = "",
                    UserId = 0,
                    UserName = "",
                    Sms = new List<SmsPartViewModel> { smsCSim }

                };

                return resOrderDetailPart;
            }
            else
            {
                return new OrderDetailPartViewModel();
            }
        }

        public static async Task<OrderDetailPartViewModel> CheckOrderWareHouseFourAsync(string simToken, string orderId, int inventory = (int)InventoryEnum.USim)
        {
            if (inventory == (int)InventoryEnum.USim)
            {
                orderId = orderId.Remove(0, 1);
                //Check status
                var urlStatus = string.Format("https://www.unitedsms.net/api_command.php?cmd=request_status&{0}&id={1}", simToken, orderId);

                //Check sms
                var urlSms = string.Format("https://www.unitedsms.net/api_command.php?cmd=read_sms&{0}&id={1}", simToken, orderId);


                var clientStatus = new RestClient(urlStatus);
                var requestStatus = new RestRequest();

                var responseStatus = await clientStatus.ExecuteGetAsync(requestStatus);

                var resObjectStatus = JsonConvert.DeserializeObject<CheckStatusOrderWareHouseFourDto>(responseStatus.Content);
                if (!resObjectStatus.status.Equals("ok", StringComparison.Ordinal))
                {
                    throw new Exception(resObjectStatus.message.ToString());
                }

                var smsUSim = new SmsPartViewModel();

                string USimStatusOrder = "";
                if (resObjectStatus.message.status.Equals("Rejected", StringComparison.Ordinal))
                {
                    USimStatusOrder = Enum.GetName(typeof(OrderStatusLSimEnum), OrderStatusLSimEnum.CANCELED);
                }
                else if (resObjectStatus.message.status.Equals("Reserved", StringComparison.Ordinal) || resObjectStatus.message.status.Equals("Awaiting MDN", StringComparison.Ordinal))
                {
                    USimStatusOrder = Enum.GetName(typeof(OrderStatusLSimEnum), OrderStatusLSimEnum.WAITING);
                }
                else if (resObjectStatus.message.status.Equals("Completed", StringComparison.Ordinal))
                {
                    USimStatusOrder = Enum.GetName(typeof(OrderStatusLSimEnum), OrderStatusLSimEnum.SUCCESS);

                    var clientSms = new RestClient(urlSms);
                    var requestSms = new RestRequest();

                    var responseSms = await clientSms.ExecuteGetAsync(requestSms);

                    var resObjectSms = JsonConvert.DeserializeObject<CheckOrderWareHouseFourDto>(responseSms.Content);
                    if (!resObjectSms.status.Equals("ok", StringComparison.Ordinal))
                    {
                        throw new Exception(resObjectSms.message.ToString());
                    }

                    var resultSms = resObjectSms.message.First();

                    smsUSim.Code = resultSms.pin;
                    smsUSim.Text = IsUrlValid(resultSms.reply) ? "" : resultSms.reply;
                    smsUSim.Created_at = DateTime.UtcNow;
                    smsUSim.Date = DateTime.UtcNow;
                }

                var resOrderDetailPart = new OrderDetailPartViewModel
                {
                    InventoryId = (int)InventoryEnum.USim,
                    Id = long.Parse(orderId),
                    Phone = resObjectStatus.message.mdn,
                    Operator = "",
                    Product = resObjectStatus.message.service,
                    Price = 0,
                    Status = USimStatusOrder,
                    Expires = DateTime.UtcNow.AddSeconds(resObjectStatus.message.till_expiration),
                    Created_at = DateTime.UtcNow,
                    Country = "USA",
                    Category = "activation",
                    Email = "",
                    UserId = 0,
                    UserName = "",
                    Sms = new List<SmsPartViewModel> { smsUSim }

                };

                return resOrderDetailPart;
            }
            else
            {
                return new OrderDetailPartViewModel();
            }
        }

        public static async Task<OrderDetailPartViewModel> CheckOrderWareHouseFourLongTermAsync(string simToken, string orderId, int inventory = (int)InventoryEnum.USimLongTerm)
        {
            if (inventory == (int)InventoryEnum.USimLongTerm)
            {
                orderId = orderId.Remove(0, 1);

                CheckStatusOrderWareHouseFourLongTermDto resObjectStatus = new CheckStatusOrderWareHouseFourLongTermDto();
                CheckStatusOrderWareHouseFourLongTermDto resObjectStatusActive = new CheckStatusOrderWareHouseFourLongTermDto();
                // CheckStatusErrorOrderWareHouseFourLongTermDto resObjectStatusError;
                CheckOrderWareHouseFourDto resObjectSms = new CheckOrderWareHouseFourDto();

                var smsUSim = new SmsPartViewModel();
                string USimStatusOrder = String.Empty;

                //Check status
                var urlStatus = string.Format("https://www.unitedsms.net/api_command.php?cmd=ltr_status&{0}&id={1}", simToken, orderId);

                var clientStatus = new RestClient(urlStatus);
                var requestStatus = new RestRequest();

                var responseStatus = await clientStatus.ExecuteGetAsync(requestStatus);

                try
                {
                    resObjectStatus = JsonConvert.DeserializeObject<CheckStatusOrderWareHouseFourLongTermDto>(responseStatus.Content);

                    if (!resObjectStatus.status.Equals("ok", StringComparison.Ordinal))
                    {
                        throw new Exception(resObjectStatus.message.ToString());
                    }

                    //Call Api Active because somtime date_time is null
                    string phoneDateNull = resObjectStatus.message.mdn;
                    var urlActiveDateNull = string.Format("https://www.unitedsms.net/api_command.php?cmd=ltr_activate&{0}&mdn={1}", simToken, phoneDateNull);

                    var clientActiveDateNull = new RestClient(urlActiveDateNull);
                    var requestActive = new RestRequest();

                    var responseActive = await clientStatus.ExecuteGetAsync(requestActive);
                    resObjectStatusActive = JsonConvert.DeserializeObject<CheckStatusOrderWareHouseFourLongTermDto>(responseActive.Content);

                    if (resObjectStatusActive.message.ltr_status.Equals("online", StringComparison.Ordinal))
                    {
                        if (!CheckExpriesDay(resObjectStatusActive.message.date_time))
                        {
                            USimStatusOrder = Enum.GetName(typeof(OrderStatusLSimEnum), OrderStatusLSimEnum.SUCCESS);
                        }
                        else
                        {
                            USimStatusOrder = Enum.GetName(typeof(OrderStatusLSimEnum), OrderStatusLSimEnum.WAITING);
                        }
                    }

                    else
                    {
                        USimStatusOrder = Enum.GetName(typeof(OrderStatusLSimEnum), OrderStatusLSimEnum.WAITING);
                    }

                    //Check sms
                    var urlSms = string.Format("https://www.unitedsms.net/api_command.php?cmd=read_sms&{0}&ltr_id={1}", simToken, orderId);

                    var clientSms = new RestClient(urlSms);
                    var requestSms = new RestRequest();

                    var responseSms = await clientSms.ExecuteGetAsync(requestSms);

                    resObjectSms = JsonConvert.DeserializeObject<CheckOrderWareHouseFourDto>(responseSms.Content);
                    if (!resObjectSms.status.Equals("ok", StringComparison.Ordinal))
                    {
                        throw new Exception(resObjectSms.message.ToString());
                    }

                }
                catch
                {
                    // resObjectStatusError = JsonConvert.DeserializeObject<CheckStatusErrorOrderWareHouseFourLongTermDto>(responseStatus.Content);
                    USimStatusOrder = Enum.GetName(typeof(OrderStatusLSimEnum), OrderStatusLSimEnum.CANCELED);
                }

                List<ResultCheckOrderWareHouseFour> resultCheckOrders = resObjectSms.message;

                List<SmsPartViewModel> smsParts = resultCheckOrders.Select(result =>
                    new SmsPartViewModel
                    {
                        Code = result.pin,
                        Text = IsUrlValid(result.reply) ? "" : result.reply,
                        Created_at = DateTime.UtcNow,
                        Date = DateTime.UtcNow
                    }).ToList();

                var resOrderDetailPart = new OrderDetailPartViewModel
                {
                    InventoryId = (int)InventoryEnum.USimLongTerm,
                    Id = long.Parse(orderId),
                    Phone = resObjectStatus.message.mdn,
                    Operator = "",
                    Product = "",
                    Price = 0,
                    Status = USimStatusOrder,
                    Expires = DateTime.UtcNow,
                    Created_at = DateTime.UtcNow,
                    Country = "USA",
                    Category = "activation",
                    Email = "",
                    UserId = 0,
                    UserName = "",
                    Sms = smsParts

                };

                return resOrderDetailPart;

            }
            else
            {
                return new OrderDetailPartViewModel();
            }
        }

        public static async Task<OrderDetailPartViewModel> CheckOrderWareHouseFiveAsync(string simToken, string orderId, int inventory = (int)InventoryEnum.VSim)
        {
            if (inventory == (int)InventoryEnum.VSim)
            {
                orderId = orderId.Remove(0, 1);
                var url = string.Format("https://api.viotp.com/session/getv2?requestId={0}&token={1}", orderId, simToken);

                var client = new RestClient(url);
                var request = new RestRequest();

                var response = await client.ExecuteGetAsync(request);

                var resObject = JsonConvert.DeserializeObject<CheckOrderWareHouseFiveDto>(response.Content);
                if (resObject.status_code != 200)
                {
                    throw new Exception(resObject.message);
                }

                var smsVSim = new SmsPartViewModel();

                string VSimStatusOrder = "";
                if (resObject.data.Status == 2)
                {
                    VSimStatusOrder = Enum.GetName(typeof(OrderStatusLSimEnum), OrderStatusLSimEnum.CANCELED);
                }
                else if (resObject.data.Status == 0)
                {
                    VSimStatusOrder = Enum.GetName(typeof(OrderStatusLSimEnum), OrderStatusLSimEnum.WAITING);
                }
                else if (resObject.data.Status == 1)
                {
                    VSimStatusOrder = Enum.GetName(typeof(OrderStatusLSimEnum), OrderStatusLSimEnum.SUCCESS);
                    smsVSim.Code = resObject.data.Code;
                    smsVSim.Text = IsUrlValid(resObject.data.SmsContent) ? "" : resObject.data.SmsContent;
                    smsVSim.Created_at = DateTime.UtcNow;
                    smsVSim.Date = DateTime.UtcNow;
                }

                var resOrderDetailPart = new OrderDetailPartViewModel
                {
                    InventoryId = (int)InventoryEnum.CSim,
                    Id = long.Parse(orderId),
                    Phone = "",
                    Operator = "",
                    Product = resObject.data.ServiceName,
                    Price = 0,
                    Status = VSimStatusOrder,
                    Expires = DateTime.UtcNow.AddMinutes(5),
                    Created_at = resObject.data.CreatedTime,
                    Country = resObject.data.CountryISO,
                    Category = "activation",
                    Email = "",
                    UserId = 0,
                    UserName = "",
                    Sms = new List<SmsPartViewModel> { smsVSim }

                };

                return resOrderDetailPart;
            }
            else
            {
                return new OrderDetailPartViewModel();
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
        private static bool IsUrlValid(string stringCheck)
        {
            return Uri.TryCreate(stringCheck, UriKind.Absolute, out Uri uriResult)
                && (uriResult.Scheme == Uri.UriSchemeHttp || uriResult.Scheme == Uri.UriSchemeHttps);
        }

        private static bool CheckExpriesDay(string dateCheck)
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
            if (!memoryCache.TryGetValue(cacheKey, out string cachedToken))
            {
                var tokenContent = await session.Query<ContentItem, ContentItemIndex>(x => x.Published && x.Latest && x.ContentType == contentType).FirstOrDefaultAsync();

                var tokenObj = tokenContent.Content[contentField];

                string token = Convert.ToString(tokenObj[valueObject]["Text"]);

                cachedToken = memoryCache.Set(cacheKey, token, signal.GetToken(signalKey));
            }
            return cachedToken;
        }

        #endregion
    }
}
