using System;
using System.Collections.Generic;
using System.Text;

namespace OrchardCore.SimService.ApiModels
{
    public class DefaultCountry
    {
        public string Name { get; set; }
        public string Iso { get; set; }
        public string Prefix { get; set; }
    }

    public class DefaultOperator
    {
        public string Name { get; set; }
    }

    public class ProfileDto
    {
        public int Id { get; set; }
        public string Email { get; set; }
        public string Vendor { get; set; }
        public string Default_forwarding_number { get; set; }
        public decimal Balance { get; set; }
        public int Rating { get; set; }
        public DefaultCountry Default_country { get; set; }
        public DefaultOperator Default_operator { get; set; }
        public decimal Frozen_balance { get; set; }
    }
}
