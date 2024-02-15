using System;
using System.Collections.Generic;
using System.Text;

namespace OrchardCore.SimService.ApiModels
{
    public class Sm
    {
        public int id { get; set; }
        public DateTime created_at { get; set; }
        public DateTime date { get; set; }
        public string sender { get; set; }
        public string text { get; set; }
        public string code { get; set; }
    }

    public class BuyHostingNumberDto
    {
        public int id { get; set; }
        public string phone { get; set; }
        public string product { get; set; }
        public int price { get; set; }
        public string status { get; set; }
        public DateTime expires { get; set; }
        public List<Sm> sms { get; set; }
        public DateTime created_at { get; set; }
    }
}
