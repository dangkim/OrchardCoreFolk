using System;
using System.Collections.Generic;
using System.Text;

namespace OrchardCore.SimService.ApiModels
{
    public class ProductsWareHouseTwoRequestDto
    {
        public string serviceId { get; set; }
        public string name { get; set; }
        public decimal price { get; set; }
        public int lockTime { get; set; }
        public decimal priceCall { get; set; }
        public int quantity { get; set; }
    }
}
