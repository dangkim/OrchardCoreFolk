using System;
using System.Collections.Generic;
using System.Text;

namespace OrchardCore.SongServices.Models
{
    public class WalletModel
    {
        public string TwoFaCode { get; set; }
        public string TargetAddress { get; set; }
        public decimal Amount { get; set; }
        public string TraderId { get; set; }
    }

    public class WalletForAdminModel
    {
        public string AdminTwoFaCode { get; set; }
        public string SenderEmail { get; set; }
        public string TargetAddress { get; set; }
        public decimal Amount { get; set; }
    }

    //public class TwoFaAuthenticationModel
    //{
    //    public string TwoFaCode { get; set; }
    //    public string ConfirmEmailCode { get; set; }
    //    public string UserId { get; set; }
    //}
}
