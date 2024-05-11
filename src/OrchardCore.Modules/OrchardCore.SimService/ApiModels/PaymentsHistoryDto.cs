using System;
using System.Collections.Generic;
using System.Text;

namespace OrchardCore.SimService.ApiModels
{
    public class Datum_Payments
    {
        public int ID { get; set; }
        public string TypeName { get; set; }
        public string ProviderName { get; set; }
        public decimal Amount { get; set; }
        public decimal Balance { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    public class Payments
    {
        public string Name { get; set; }
    }

    public class PaymentProvider
    {
        public string Name { get; set; }
    }

    public class PaymentsHistoryDto
    {
        public List<Object> Data { get; set; }
        public List<Payments> PaymentTypes { get; set; }
        public List<PaymentProvider> PaymentProviders { get; set; }
        public decimal Total { get; set; }
    }
}
