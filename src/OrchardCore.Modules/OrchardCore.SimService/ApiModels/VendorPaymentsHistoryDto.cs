using System;
using System.Collections.Generic;
using System.Text;

namespace OrchardCore.SimService.ApiModels
{
    public class VendorPaymentsHistoryDto
    {
        public List<object> Data { get; set; }
        public object PaymentProviders { get; set; }
        public object PaymentStatuses { get; set; }
        public object PaymentTypes { get; set; }
        public decimal Total { get; set; }
    }
}
