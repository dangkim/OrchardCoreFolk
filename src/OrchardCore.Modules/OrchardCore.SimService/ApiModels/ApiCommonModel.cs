using System;
using System.Collections.Generic;
using OrchardCore.ContentManagement;

namespace OrchardCore.SimService.ApiModels
{
    public class BtcPayApiResult
    {
        public string Error { get; set; }
        public string Message { get; set; }
        public string Balance { get; set; }
    }

    public enum FeeTypeEnum
    {
        Seller = 0,
        Share = 1,
        Buyer = 2
    }

    public enum InventoryEnum
    {
        FSim = 1,//5Sim
        SmsHub = 2,
        LSim = 3, // 2ndLine
        USim = 4, //unitedsms
        VSim = 5, //viotp
        USimLongTerm = 6, //unitedsms Long-Term
    }

    public class OfferStatusModel
    {
        public string OfferItemId { get; set; }
        public string Status { get; set; }
        public bool IsDisabled { get; set; }
    }

    public class DetailedInvoice
    {
        public string Crypto { get; set; }
        public string Confirmations { get; set; }
        public string DepositAddress { get; set; }
        public string Amount { get; set; }
        public DateTimeOffset ReceivedTime { get; set; }
        public long? BlockNumber { get; set; }
        public string BalanceLink { get; set; }
        public bool Replaced { get; set; }
        public long Index { get; set; }

    }

    public class UpdateBalanceModel
    {
        public string Email { get; set; }
        public string ContenItemId { get; set; }
        public long UserId { get; set; }
        public int TransactionType { get; set; }
        public string Amount { get; set; }
        public DateTimeOffset CreatedDate { get; set; }
        public string Crypto { get; set; }
        public string BlockChainLink { get; set; }
        public string Rate { get; set; }
        public string CurrencyOfTrade { get; set; }
    }

    public class UpdateBalancePaymentMethodModel
    {
        public int PaymentId { get; set; }
        public string TypeName { get; set; }
        public string ProviderName { get; set; }
        public decimal Amount { get; set; }
        public string Currency { get; set; }
        public DateTime CreatedAt { get; set; }
        public string Email { get; set; }
        public long UserId { get; set; }
        public string UserName { get; set; }
        public string Password { get; set; }
        public string Method { get; set; }
    }

    public class PerfectMoneyModel
    {
        public string PAYEE_ACCOUNT { get; set; }
        public decimal PAYMENT_AMOUNT { get; set; }
        public string PAYMENT_UNITS { get; set; }
        public int PAYMENT_BATCH_NUM { get; set; }
        public string SUGGESTED_MEMO { get; set; }
        public int PAYMENT_ID { get; set; }
    }

    public class SendCoinReturnModel
    {
        public string BlockChainLink { get; set; }
        public string Rate { get; set; }
    }

    public class OfferHomeReturnModel
    {
        public string Owner { get; set; }
        public string Status { get; set; }
        public string Wallet { get; set; }
        public string ContentItemId { get; set; }
        public string Percentage { get; set; }
        public string PreferredCurrency { get; set; }
        public string OfferType { get; set; }
        public string OfferGet { get; set; }
        public string OfferPrice { get; set; }
        public decimal MinAmount { get; set; }
        public decimal MaxAmount { get; set; }
        public decimal BondVndBalance { get; set; }
        public string PaymentMethod { get; set; }
        public string TradeInstructions { get; set; }
        public string OfferTerms { get; set; }
        public string OfferLabel { get; set; }
        public string CreatedUtc { get; set; }

    }

    public class RateFiveSim
    {
        public string Advcash { get; set; }
        public double Advcash_usd_rate { get; set; }
        public string Crypto2 { get; set; }
        public double Crypto2_usd_rate { get; set; }
        public double Cypix_eur_rub { get; set; }
        public double Cypix_eur_usd { get; set; }
        public double Cypix_usd_rub { get; set; }
        public string Free_kassa { get; set; }
        public string Ppayeer { get; set; }
        public double Payssion_usd_rate { get; set; }
        public string Perfect_money { get; set; }
        public double Perfect_money_usd_rate { get; set; }
        public string Pokupo { get; set; }
        public double Rub_usdt_rate { get; set; }
        public double Uah_rub_rate { get; set; }
        public string Unitpay { get; set; }
        public double Usdt_rate { get; set; }
    }

    public class SocialCircle
    {
        public string Email { get; set; }
        public string Facebook { get; set; }
        public string Tiktok { get; set; }
        public string Instagram { get; set; }
    }

}
