using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using OrchardCore.ContentManagement;
using OrchardCore.ContentManagement.Records;
using OrchardCore.Users.Models;
using OrchardCore.SongServices.ApiModels;
using YesSql;
using OrchardCore.SongServices.ContentParts;
using OrchardCore.Environment.Cache;
using Microsoft.Extensions.Caching.Memory;
using OrchardCore.SongServices.Indexing;


namespace OrchardCore.SongServices.ApiCommonFunctions
{
    public static class ApiCommon
    {
        public static async Task<Dictionary<string, decimal>> CalculateBalanceAsync(decimal userBalanceBTC, decimal userBalanceEth, decimal userBalanceUsdt20, decimal userBalanceVND, string userName, YesSql.ISession session)
        {
            var document = await session.Query<ContentItem, ContentItemIndex>(x => x.Published && x.Latest && x.ContentType == "TradePage" && x.Owner == userName)
                .With<TradeFilteringPartIndex>(p => p.Status == "pending")
                .ListAsync();

            var dictionaryBalance = new Dictionary<string, decimal>();

            foreach (var item in document)
            {
                var trade = item.As<TradeFilteringPart>();

                if ((trade.TradeStatus.ToLower() != "cancel" && trade.TradeStatus.ToLower() != "completed") || trade.OfferType.ToLower() == "withdraw")
                {
                    var tradeAmountBtc = trade.TradeBTCAmount;
                    var feeAmountBtc = trade.FeeBTCAmount;

                    var tradeAmountEth = trade.TradeETHAmount;
                    var feeAmountEth = trade.FeeETHAmount;

                    var tradeAmountUsdt20 = trade.TradeUSDT20Amount;
                    var feeAmountUsdt20 = trade.FeeUSDT20Amount;

                    var tradeAmountVnd = trade.TradeVNDAmount;
                    var feeAmountVnd = trade.FeeVNDAmount;

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

        public static async Task<bool> UpdateBalance(IContentManager _contentManager, YesSql.ISession session, UpdateBalanceModel updateBalanceModel, User user)
        {
            var btcBalanceValue = 0m;
            var ethBalanceValue = 0m;
            var usdtBalanceValue = 0m;
            var vndBalanceValue = 0m;

            try
            {
                // Create Trade: This trade is a trade of update balance (transfer money, withdraw, deposit)
                var newContentItemTrade = await _contentManager.NewAsync("TradePage");
                var tradeType = updateBalanceModel.TransactionType == 1 ? "deposit" : "withdraw";
                newContentItemTrade.Owner = user.UserName;
                newContentItemTrade.Author = user.UserName;

                var tradePart = new TradeFilteringPart()
                {
                    Buyer = user.UserName,
                    BuyerContentId = "NA",
                    OfferId = "NA",
                    TradeStatus = "completed",
                    CurrencyOfTrade = updateBalanceModel.CurrencyOfTrade,
                    Duration = 60,
                    FeeBTCAmount = 0m,
                    FeeETHAmount = 0m,
                    FeeType = -1, //0: seller; 1: 50/50; 2: buyer; -1: just deposit/withdraw, no fee type
                    FeeUSDT20Amount = 0m,
                    FeeVNDAmount = 0m,
                    PaymentMethod = "NA",
                    OfferType = "NA",
                    OfferWallet = "NA",
                    Seller = "NA",
                    SellerContentId = "NA",
                    TradeBTCAmount = updateBalanceModel.Crypto.ToLower() == "btc" ? Decimal.Parse(updateBalanceModel.Amount) : 0m,
                    TradeETHAmount = updateBalanceModel.Crypto.ToLower() == "eth" ? Decimal.Parse(updateBalanceModel.Amount) : 0m,
                    TradeUSDT20Amount = (updateBalanceModel.Crypto.ToLower() == "usdt20") ? Decimal.Parse(updateBalanceModel.Amount) : 0m,
                    TradeVNDAmount = updateBalanceModel.Wallet.ToLower() == "vnd" ? Decimal.Parse(updateBalanceModel.Amount) : 0m,
                    TradeType = tradeType,
                    DateTime = DateTime.Now
                };

                newContentItemTrade.Apply(tradePart);

                var resultTrade = await _contentManager.UpdateValidateAndCreateAsync(newContentItemTrade, VersionOptions.Published);

                if (resultTrade.Succeeded)
                {
                    var contentItemTrader = await session
                        .Query<ContentItem, ContentItemIndex>(index => index.ContentType == "TraderPage" && index.ContentItemId == updateBalanceModel.ContenItemId)
                        //.With<TraderForFilteringPartIndex>(p => p.Email == updateBalanceModel.Email)
                        .FirstOrDefaultAsync();

                    if (contentItemTrader != null)
                    {
                        var traderPart = contentItemTrader.As<TraderForFilteringPart>();

                        if (updateBalanceModel.Crypto.ToLower() == "eth")
                        {
                            ethBalanceValue += Decimal.Parse(updateBalanceModel.Amount);

                            var currentETHBalance = traderPart.ETHBalance;

                            if (updateBalanceModel.TransactionType == 1)
                            {
                                currentETHBalance += ethBalanceValue;
                            }
                            else if (updateBalanceModel.TransactionType == 0)
                            {
                                currentETHBalance -= ethBalanceValue;
                            }

                            traderPart.ETHBalance = currentETHBalance;
                        }
                        else if (updateBalanceModel.Crypto.ToLower() == "btc")
                        {
                            btcBalanceValue += Decimal.Parse(updateBalanceModel.Amount);

                            var currentBTCBalance = traderPart.BTCBalance;

                            if (updateBalanceModel.TransactionType == 1)
                            {
                                currentBTCBalance += btcBalanceValue;
                            }
                            else if (updateBalanceModel.TransactionType == 0)
                            {
                                currentBTCBalance -= btcBalanceValue;
                            }

                            traderPart.BTCBalance = currentBTCBalance;
                        }
                        else if (updateBalanceModel.Crypto.ToLower() == "usdt20")
                        {
                            usdtBalanceValue += Decimal.Parse(updateBalanceModel.Amount);

                            var currentUSDTBalance = traderPart.USDT20Balance;

                            if (updateBalanceModel.TransactionType == 1)
                            {
                                currentUSDTBalance += usdtBalanceValue;
                            }
                            else if (updateBalanceModel.TransactionType == 0)
                            {
                                currentUSDTBalance -= usdtBalanceValue;
                            }

                            traderPart.USDT20Balance = currentUSDTBalance;
                        }
                        else if (updateBalanceModel.Wallet.ToLower() == "vnd")
                        {
                            vndBalanceValue += Decimal.Parse(updateBalanceModel.Amount);

                            var currentVNDBalance = traderPart.VndBalance;

                            if (updateBalanceModel.TransactionType == 1)
                            {
                                currentVNDBalance += vndBalanceValue;
                            }
                            else if (updateBalanceModel.TransactionType == 0)
                            {
                                currentVNDBalance -= vndBalanceValue;
                            }

                            traderPart.VndBalance = currentVNDBalance;
                        }

                        contentItemTrader.Apply(traderPart);

                        contentItemTrader.Latest = true;

                        await _contentManager.UpdateAsync(contentItemTrader);

                        var result = await _contentManager.ValidateAsync(contentItemTrader);

                        if (result.Succeeded)
                        {
                            await _contentManager.PublishAsync(contentItemTrader);
                        }
                        else
                        {
                            return false;
                        }
                    }

                    return true;
                }
                else
                {
                    return false;
                }
            }
            catch (Exception)
            {
                throw;
            }
        }

        public static async Task<string> ReadCache(YesSql.ISession session, IMemoryCache _memoryCache, ISignal _signal, Microsoft.Extensions.Configuration.IConfiguration _config, string contentType = "FiveSimToken")
        {
            var fiveSimTokenCache = string.Empty;
            var fiveSimToken = string.Empty;

            if (contentType == "FiveSimToken")
            {
                var cacheKey = _config["FiveSimCacheKey"];
                var signalKey = _config["FiveSimCacheSignalKey"];

                if (!_memoryCache.TryGetValue(cacheKey, out fiveSimTokenCache))
                {
                    var tokenContent = await session.Query<ContentItem, ContentItemIndex>(x => x.Published && x.Latest && x.ContentType == contentType).FirstOrDefaultAsync();

                    var simtokenObj = tokenContent.Content["FiveSimToken"];
                    fiveSimToken = Convert.ToString(simtokenObj["Token"]["Text"]);

                    fiveSimTokenCache = _memoryCache.Set(cacheKey, fiveSimToken, _signal.GetToken(signalKey));
                }
            }

            return fiveSimTokenCache;
        }
    }
}
