using System;
using System.Collections.Generic;
using OrchardCore.ContentManagement;

namespace OrchardCore.SongServices.ApiModels
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
        public int TransactionType { get; set; }
        public string Amount { get; set; }
        public DateTimeOffset CreatedDate { get; set; }
        public string Crypto { get; set; }
        public string Wallet { get; set; }
        public string BlockChainLink { get; set; }
        public string Rate { get; set; }
        public string CurrencyOfTrade { get; set; }
        public int FeeType { get; set; }
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
        public decimal Percentage { get; set; }
        public string PreferredCurrency { get; set; }
        public string OfferType { get; set; }
        public decimal OfferGet { get; set; }
        public decimal OfferPrice { get; set; }
        public decimal MinAmount { get; set; }
        public decimal MaxAmount { get; set; }
        public decimal BondVndBalance { get; set; }
        public string PaymentMethod { get; set; }
        public string TradeInstructions { get; set; }
        public string OfferTerms { get; set; }
        public string OfferLabel { get; set; }
        public string CreatedUtc { get; set; }

    }

    public class TradeReturnModel
    {
        public string Status { get; set; }
        public string AmountCurrencyFromETH { get; set; }
        public string BlockExplorerLink { get; set; }
        public string Buyer { get; set; }
        public string CreateDate { get; set; }
        public string CreatedUtc { get; set; }
        public string CurrencyOfTrade { get; set; }
        public string FeeETHAmount { get; set; }
        public string FeeType { get; set; }
        public decimal PaymentMethod { get; set; }
        public decimal Seller { get; set; }
        public string TradeType { get; set; }
        public string OfferId { get; set; }
        public string ContentItemId { get; set; }
        public string AmountCurrencyFromUSDT20 { get; set; }
        public string AmountCurrencyFromBTC { get; set; }
        public string AmountCurrencyFromVND { get; set; }
        public string FeeUSDT20Amount { get; set; }
        public string FeeVNDAmount { get; set; }
        public string FeeBTCAmount { get; set; }
        public string TradeUSDT20Amount { get; set; }
        public string TradeVNDAmount { get; set; }
        public string TradeBTCAmount { get; set; }
        public string TradeETHAmount { get; set; }
    }

    public class PaymentCategoryReturnModel
    {
        public string DisplayText { get; set; }
        public string IconPath { get; set; }
        public string Color { get; set; }
        public string ContentItemId { get; set; }

        public string[] PaymentMethodNew { get; set; }

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

    //public class PaymentMethodNewReturnModel
    //{
    //    public string Method { get; set; }
    //}

}
