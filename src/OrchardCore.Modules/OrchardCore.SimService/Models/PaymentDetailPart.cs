using System;
using System.Collections.Generic;
using System.Text;
using OrchardCore.ContentManagement;

namespace OrchardCore.SimService.Models
{
    public class PaymentDetailPart : ContentPart
    {
        public long PaymentId { get; set; }
        public string TypeName { get; set; }
        public string ProviderName { get; set; }
        public decimal Amount { get; set; }
        public decimal Balance { get; set; }
        public DateTime CreatedAt { get; set; }
        public string Email { get; set; }
        public long UserId { get; set; }
        public string UserName { get; set; }
        public long OrderId { get; set; }
    }
}
