using System;
using System.Collections.Generic;
using System.Text;

namespace OrchardCore.SongServices.Models
{
    public class UpdateTraderModel
    {
        public string UserId { get; set; }
        public decimal VndBalance { get; set; }
        public decimal BondVndBalance { get; set; }
        public string MoneyStatus { get; set; }
        public string WithdrawVNDStatus { get; set; }
        public string BankAccounts { get; set; }
        public string TraderContentId { get; set; }
        public decimal Amount { get; set; }
        public string ReferenceCode { get; set; }
        public string TwoFaCode { get; set; }
    }
}
