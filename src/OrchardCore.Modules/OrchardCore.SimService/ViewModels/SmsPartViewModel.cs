using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace OrchardCore.SimService.ViewModels
{
    public class SmsPartViewModel
    {
        public DateTime Created_at { get; set; }
        public string Sender { get; set; }
        public string Text { get; set; }
        public string Code { get; set; }
        public string Email { get; set; }
        public long UserId { get; set; }
        public string UserName { get; set; }
        public DateTime Date { get; set; }
        public long OrderId { get; set; }
    }
}
