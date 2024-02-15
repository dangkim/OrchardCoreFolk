using System;
using System.Collections.Generic;
using System.Text;

namespace OrchardCore.SongServices.Models
{
    public class CreateNotifyModel
    {
        public List<string> UserNames { get; set; }
        public string UserName { get; set; }
        public string Content { get; set; }
        public string TradeId { get; set; }
        public bool IsAll { get; set; }
    }
}
