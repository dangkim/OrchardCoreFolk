using System;
using System.Collections.Generic;
using System.Text;

namespace OrchardCore.SimService.ApiModels
{

    public class ResultBuyNumberWareHouseThree
    {
        public int Id { get; set; }
        public string Number { get; set; }
        public string App { get; set; }
        public decimal Cost { get; set; }
        public decimal Balance { get; set; }
    }

    public class BuyActionNumberWareHouseThreeDto
    {
        public int ResponseCode { get; set; }
        public string Msg { get; set; }
        public ResultBuyNumberWareHouseThree Result { get; set; }
    }
}
