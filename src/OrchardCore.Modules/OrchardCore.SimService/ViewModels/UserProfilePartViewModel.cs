using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace OrchardCore.SimService.ViewModels
{
    public class UserProfilePartViewModel
    {
        public int ProfileId { get; set; }
        public string Email { get; set; }
        public long UserId { get; set; }
        public string UserName { get; set; }
        public string Vendor { get; set; }
        public string DefaultForwardingNumber { get; set; }
        public decimal Balance { get; set; }
        public decimal OriginalAmount { get; set; }
        public decimal Amount { get; set; }
        public decimal RateInUsd { get; set; }
        public string GmailMsgId { get; set; }
        public int Rating { get; set; }
        public string DefaultCoutryName { get; set; }
        public string DefaultIso { get; set; }
        public string DefaultPrefix { get; set; }
        public string DefaultOperatorName { get; set; }
        public decimal FrozenBalance { get; set; }
        public string TokenApi { get; set; }
    }
}
