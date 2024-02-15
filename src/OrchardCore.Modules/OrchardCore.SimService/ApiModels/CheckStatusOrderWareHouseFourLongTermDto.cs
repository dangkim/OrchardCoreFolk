using System;
using System.Collections.Generic;
using System.Text;

namespace OrchardCore.SimService.ApiModels
{
    public class MessageCheckStatusOrderWareHouseFourLongTermDto
    {
        public string mdn { get; set; }
        public string ltr_status { get; set; }
        public int till_change { get; set; }
        public int next_online { get; set; }
        public int timestamp { get; set; }
        public string date_time { get; set; }
    }

    public class CheckStatusOrderWareHouseFourLongTermDto
    {
        public string status { get; set; }
        public MessageCheckStatusOrderWareHouseFourLongTermDto message { get; set; }
    }
}
