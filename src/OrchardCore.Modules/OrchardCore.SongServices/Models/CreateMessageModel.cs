using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace OrchardCore.SongServices.Models
{
    public class CreateMessageModel
    {
        public string Sender { get; set; }
        public string Content { get; set; }
        public string Link { get; set; }
        public long ConversationId { get; set; }
        public string TraderId { get; set; }
        public string MessageRequestId { get; set; }
        public bool IsNote { get; set; }
    }
}
