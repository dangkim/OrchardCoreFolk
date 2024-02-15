using System;
using System.Collections.Generic;
using System.Text;

namespace OrchardCore.SimService.ApiModels
{
    public class VendorOrdersHistoryDto
    {
        public List<object> Data { get; set; }
        public List<object> ProductNames { get; set; }
        public List<object> Statuses { get; set; }
        public decimal Total { get; set; }
    }
}
