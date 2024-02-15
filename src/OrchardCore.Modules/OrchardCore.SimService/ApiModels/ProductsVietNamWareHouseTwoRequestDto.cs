using System;
using System.Collections.Generic;
using System.Text;

namespace OrchardCore.SimService.ApiModels
{
    public class ProductsVietNamWareHouseTwoRequestDto
    {
        public int serviceId { get; set; }
        public string name { get; set; }
        public int lockTime { get; set; }
        public decimal price { get; set; }
        public decimal priceCall { get; set; }
    }
}
