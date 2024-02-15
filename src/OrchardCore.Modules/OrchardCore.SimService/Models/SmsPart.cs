using System;
using System.Collections.Generic;
using System.Text;
using OrchardCore.ContentManagement;

namespace OrchardCore.SimService.Models
{
    public class SmsPart : ContentPart
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
