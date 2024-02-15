using System;
using System.Collections.Generic;
using System.Text;

namespace OrchardCore.SimService.ApiModels
{
    public class DataBuyWareHouseFive
    {
        public string phone_number { get; set; }
        public int balance { get; set; }
        public string request_id { get; set; }
        public string re_phone_number { get; set; }
        public string countryISO { get; set; }
        public string countryCode { get; set; }
    }

    public class BuyActionNumberWareHouseFiveDto
    {
        public int status_code { get; set; }
        public bool success { get; set; }
        public string message { get; set; }
        public DataBuyWareHouseFive data { get; set; }
    }
}
