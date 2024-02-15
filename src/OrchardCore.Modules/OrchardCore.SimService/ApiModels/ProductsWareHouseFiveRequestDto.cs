using System;
using System.Collections.Generic;
using System.Text;

namespace OrchardCore.SimService.ApiModels
{
    public class DataProduct
    {
        public int id { get; set; }
        public string name { get; set; }
        public decimal price { get; set; }
    }

    public class ProductsWareHouseFiveRequestDto
    {
        public int status_code { get; set; }
        public string message { get; set; }
        public bool success { get; set; }
        public List<DataProduct> data { get; set; }
    }
}
