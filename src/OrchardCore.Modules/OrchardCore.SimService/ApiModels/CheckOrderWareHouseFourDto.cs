using System.Collections.Generic;

namespace OrchardCore.SimService.ApiModels
{
    public class ResultCheckOrderWareHouseFour
    {
        public string timestamp { get; set; }
        public string date_time { get; set; }
        public string from { get; set; }
        public string to { get; set; }
        public string service { get; set; }
        public string price { get; set; }
        public string reply { get; set; }
        public string pin { get; set; }
    }

    public class CheckOrderWareHouseFourDto
    {
        public string status { get; set; }
        public List<ResultCheckOrderWareHouseFour> message { get; set; }
    }
}
