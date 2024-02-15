using System;
using System.Collections.Generic;
using System.Text;

namespace OrchardCore.SimService.ApiModels
{
    public class DataOrderWareHouseFive
    {
        public int ID { get; set; }
        public int ServiceID { get; set; }
        public string ServiceName { get; set; }
        public int Status { get; set; }
        public int Price { get; set; }
        public string Phone { get; set; }
        public string SmsContent { get; set; }
        public string IsSound { get; set; }
        public DateTime CreatedTime { get; set; }
        public string Code { get; set; }
        public string PhoneOriginal { get; set; }
        public string CountryISO { get; set; }
        public string CountryCode { get; set; }
    }
    public class CheckOrderWareHouseFiveDto
    {
        public int status_code { get; set; }
        public bool success { get; set; }
        public string message { get; set; }
        public DataOrderWareHouseFive data { get; set; }
    }
}
