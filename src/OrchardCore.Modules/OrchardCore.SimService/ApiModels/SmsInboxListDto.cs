using System;
using System.Collections.Generic;
using System.Text;

namespace OrchardCore.SimService.ApiModels
{
    public class DatumSms
    {
        public int ID { get; set; }
        public DateTime created_at { get; set; }
        public DateTime date { get; set; }
        public string sender { get; set; }
        public string text { get; set; }
        public string code { get; set; }
        public bool is_wave { get; set; }
        public string wave_uuid { get; set; }
    }

    public class SmsInboxListDto
    {
        public List<DatumSms> Data { get; set; }
        public decimal Total { get; set; }
    }
}
