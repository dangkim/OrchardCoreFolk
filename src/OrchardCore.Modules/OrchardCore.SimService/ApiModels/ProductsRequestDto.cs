using System;
using System.Collections.Generic;
using System.Text;

namespace OrchardCore.SimService.ApiModels
{
    public class Product
    {
        public string Category { get; set; }
        public decimal Qty { get; set; }
        public decimal Price { get; set; }
    }

    public class ProductsRequestDto
    {
        public Product Oneday { get; set; }
    }
}
