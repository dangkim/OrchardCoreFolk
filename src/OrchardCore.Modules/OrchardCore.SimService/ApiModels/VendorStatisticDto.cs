using System;
using System.Collections.Generic;
using System.Text;

namespace OrchardCore.SimService.ApiModels
{
    public class DefaultCountryVendorStatistic
    {
        public string name { get; set; }
        public string iso { get; set; }
        public string prefix { get; set; }
    }

    public class DefaultOperatorVendorStatistic
    {
        public string name { get; set; }
    }

    public class VendorStatisticDto
    {
        public int id { get; set; }
        public string email { get; set; }
        public string vendor { get; set; }
        public string default_forwarding_number { get; set; }
        public decimal balance { get; set; }
        public decimal rating { get; set; }
        public DefaultCountryVendorStatistic default_country { get; set; }
        public DefaultOperatorVendorStatistic default_operator { get; set; }
        public decimal frozen_balance { get; set; }
    }
}
