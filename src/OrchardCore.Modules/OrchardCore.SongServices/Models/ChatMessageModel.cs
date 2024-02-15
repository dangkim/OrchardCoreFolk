using System;
using System.Collections.Generic;
using System.Text;

namespace OrchardCore.SongServices.Models
{
    public class OfferMessageModel
    {
        public string UserName { get; set; }
        public string OfferItemId { get; set; }
        public string DisplayText { get; set; }
        public string Message { get; set; }
        public string Link { get; set; }
        public string TraderContentId { get; set; }
        public string ConnectionId { get; set; }
    }

    public class VmMessage
    {
        //public string UserName { get; set; }
        public string OfferItemId { get; set; }
        public string DisplayText { get; set; }
        public List<string> Message { get; set; }
        public string Link { get; set; }
        public string TraderContentId { get; set; }
        public string ConnectionId { get; set; }
        public string GroupId { get; set; }
        public List<string> MemberIds { get; set; }
        public string SenderId { get; set; }
        public string ReceiverId { get; set; }
        public string Messagestatus { get; set; }
        public DateTime Messagedate { get; set; } = DateTime.Now;
        public bool IsGroup { get; set; } = false;
        public bool IsMultiple { get; set; } = false;
        public bool IsPrivate { get; set; } = false;
    }

    public class GroupOffer
    {
        public string GroupId { get; set; }
        public List<string> MemberIds { get; set; }
    }


    public class OfferPrivateMessageModel
    {
        public string UserName { get; set; }
        public string OfferItemId { get; set; }
        public string DisplayText { get; set; }
        public string Message { get; set; }
        public string Link { get; set; }
        public string TraderContentId { get; set; }
        public string ConnectionId { get; set; }
    }

    public class GettingOfflineModel
    {
        public string Senderid { get; set; }
        public string ReceiverId { get; set; }
    }
}
