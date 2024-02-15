using System;
using System.Collections.Generic;
using System.Text;

namespace OrchardCore.SimService.ApiModels
{
    public class ResultCheckOrderWareHouseThree
    {
        public string SMS { get; set; }
        public string Code { get; set; }
        public decimal Cost { get; set; }
        public bool IsCall { get; set; }
        public object CallFile { get; set; }
    }

    public class CheckOrderWareHouseThreeDto
    {
        public int ResponseCode { get; set; }
        public string Msg { get; set; }
        public ResultCheckOrderWareHouseThree Result { get; set; }
    }
}
