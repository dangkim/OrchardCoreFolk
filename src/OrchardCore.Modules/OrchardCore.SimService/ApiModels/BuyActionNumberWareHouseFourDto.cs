using System;
using System.Collections.Generic;
using System.Text;

namespace OrchardCore.SimService.ApiModels
{
    public class MessageBuyNumberWareHouseFour
    {
        public string id { get; set; }
        public string mdn { get; set; }
        public string service { get; set; }
        public string status { get; set; }
        public string state { get; set; }
        public decimal markup { get; set; }
        public decimal price { get; set; }
        public int till_expiration { get; set; }
    }

    public class BuyActionNumberWareHouseFourDto
    {
        public string status { get; set; }
        public List<MessageBuyNumberWareHouseFour> message { get; set; }
    }
}
