using System;
using System.Collections.Generic;
using System.Text;

namespace OrchardCore.SongServices.Models
{
    public class CreateTradeModel
    {
        public string TradeId { get; set; }
        public string Seller { get; set; }
        public string Buyer { get; set; }
        public float Amount { get; set; }
        public int Duration { get; set; }
        public string Status { get; set; }
        public DateTime CreateDate { get; set; }
    }
}
