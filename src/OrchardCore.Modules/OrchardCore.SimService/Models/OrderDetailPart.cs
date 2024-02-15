using System;
using System.Collections.Generic;
using System.Text;
using OrchardCore.ContentManagement;

namespace OrchardCore.SimService.Models
{
    public class OrderDetailPart : ContentPart
    {
        public int InventoryId { get; set; }
        public long OrderId { get; set; }
        public string Phone { get; set; }
        public string Operator { get; set; }
        public string Product { get; set; }
        public decimal Price { get; set; }
        public string Status { get; set; }
        public DateTime Expires { get; set; }
        public DateTime Created_at { get; set; }
        public string Country { get; set; }
        public string Email { get; set; }
        public long UserId { get; set; }
        public string UserName { get; set; }
        public string Category { get; set; }
    }

    public class OrderProducts : ContentPart
    {
        public string Operator { get; set; }
        public string Product { get; set; }
        public decimal Price { get; set; }
        public string Status { get; set; }
        public int Visits { get; set; }
    }
}
