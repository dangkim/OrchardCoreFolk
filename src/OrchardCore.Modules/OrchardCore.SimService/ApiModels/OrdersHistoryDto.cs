using System;
using System.Collections.Generic;
using System.Text;

namespace OrchardCore.SimService.ApiModels
{
    public class Datum_Orders
    {
        public int Id { get; set; }
        public string Phone { get; set; }
        public string Operator { get; set; }
        public string Product { get; set; }
        public decimal Price { get; set; }
        public string Status { get; set; }
        public DateTime Expires { get; set; }
        public List<object> Sms { get; set; }
        public DateTime Created_at { get; set; }
        public string Country { get; set; }
    }

    public class OrdersHistoryDto
    {
        public List<Datum_Orders> Data { get; set; }
        public List<object> ProductNames { get; set; }
        public List<object> Statuses { get; set; }
        public decimal Total { get; set; }
    }
}
