using System;
using System.Collections.Generic;
using System.Text;

namespace OrchardCore.SongServices.Models
{
    public class CreateConversationModel
    {
        public string Sender { get; set; }
        public string MemberIds { get; set; }
        public bool IsGroup { get; set; }
        public string TradeId { get; set; }
    }
}
