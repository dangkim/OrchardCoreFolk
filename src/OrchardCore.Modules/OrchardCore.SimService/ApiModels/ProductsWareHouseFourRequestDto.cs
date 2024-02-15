using System;
using System.Collections.Generic;
using System.Text;

namespace OrchardCore.SimService.ApiModels
{
    public class Message
    {
        public string name { get; set; }
        public string price { get; set; }
        public string landline_price { get; set; }
        public string ltr_price { get; set; }
        public string available { get; set; }
        public string landline_available { get; set; }
        public string ltr_available { get; set; }
    }

    public class ProductsWareHouseFourRequestDto
    {
        public string status { get; set; }
        public List<Message> message { get; set; }
    }
}
