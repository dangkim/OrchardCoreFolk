using System;
using System.Collections.Generic;
using System.Text;

namespace OrchardCore.SimService.ApiModels
{
    public class BuyActionNumberDto
    {
        public int id { get; set; }
        public string phone { get; set; }
        public string @operator { get; set; }
        public string product { get; set; }
        public decimal price { get; set; }
        public string status { get; set; }
        public string expires { get; set; }
        public object sms { get; set; }
        public string created_at { get; set; }
        public bool forwarding { get; set; }
        public string forwarding_number { get; set; }
        public string country { get; set; }
    }
}
