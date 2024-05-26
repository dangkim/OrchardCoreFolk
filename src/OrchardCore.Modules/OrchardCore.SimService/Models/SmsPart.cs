using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json.Serialization;
using OrchardCore.ContentManagement;

namespace OrchardCore.SimService.Models
{
    public class SmsPart : ContentPart
    {
        public DateTime Created_at { get; set; }

        [JsonIgnore]
        public string Sender { get; set; }
        public string Text { get; set; }
        public string Code { get; set; }

        [JsonIgnore]
        public string Email { get; set; }

        [JsonIgnore]
        public long UserId { get; set; }

        [JsonIgnore]
        public string UserName { get; set; }
        public DateTime Date { get; set; }
        public long OrderId { get; set; }
    }
}
