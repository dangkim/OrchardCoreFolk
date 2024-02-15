using System;
using System.Collections.Generic;
using System.Text;
using OrchardCore.ContentManagement;

namespace OrchardCore.SongServices.ContentParts
{
    public class TraderForFilteringPart : ContentPart
    {
        public string Name { get; set; }
        public bool IsActivatedTele { get; set; }
        public decimal BondVndBalance { get; set; }
        public decimal VndBalance { get; set; }
        public decimal BTCBalance { get; set; }
        public decimal ETHBalance { get; set; }
        public decimal USDT20Balance { get; set; }
        public string WithdrawVNDStatus { get; set; }
        public int ReferenceCode { get; set; }
        public string DateSend { get; set; }
        public long UserId { get; set; }
        public string Email { get; set; }
        public string PhoneNumber { get; set; }
        public string BankAccounts { get; set; }
        public string ChatIdTele { get; set; }
        public string DeviceId { get; set; }
        public string MoneyStatus { get; set; }
        public decimal Amount { get; set; }
        public decimal TotalFeeBTC { get; set; }
        public decimal TotalFeeETH { get; set; }
        public decimal TotalFeeUSDT { get; set; }
        public decimal TotalFeeVND { get; set; }
        public string BookmarkOffers { get; set; }
        public DateTime? DateTime { get; set; }
    }
}
