using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;
using System.Text.Json.Serialization;

namespace OrchardCore.SimService.ViewModels
{
    public class OrderDetailPartViewModel
    {
        public int InventoryId { get; set; }
        public long Id { get; set; }
        public string Phone { get; set; }
        public string Operator { get; set; }
        public string Product { get; set; }
        public decimal Price { get; set; }
        public string Status { get; set; }
        public DateTime Expires { get; set; }
        public DateTime Created_at { get; set; }
        public string Country { get; set; }

        [JsonIgnore]
        public string Email { get; set; }
        public long UserId { get; set; }
        public List<SmsPartViewModel> Sms { get; set; }

        [JsonIgnore]
        public string UserName { get; set; }
        public string Category { get; set; }
    }
}
