using System;
using System.Collections.Generic;
using System.Text;

namespace OrchardCore.SimService.ApiModels
{
    public class SmCheckOrder
    {
        public int id { get; set; }
        public string created_at { get; set; }
        public DateTime date { get; set; }
        public string sender { get; set; }
        public string text { get; set; }
        public string code { get; set; }
    }

    public class CheckOrderDto
    {
        public int id { get; set; }
        public string created_at { get; set; }
        public string phone { get; set; }
        public string product { get; set; }
        public decimal price { get; set; }
        public string status { get; set; }
        public string expires { get; set; }
        public string Operator {get; set; }
        public List<SmCheckOrder> sms { get; set; }
        public bool forwarding { get; set; }
        public string forwarding_number { get; set; }
        public string country { get; set; }
    }
}
