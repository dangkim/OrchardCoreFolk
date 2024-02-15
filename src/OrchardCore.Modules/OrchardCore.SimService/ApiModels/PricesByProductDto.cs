using System;
using System.Collections.Generic;
using System.Text;

namespace OrchardCore.SimService.ApiModels
{
    public class Virtual18
    {
        public decimal cost { get; set; }
        public decimal count { get; set; }
    }

    public class Virtual23
    {
        public decimal cost { get; set; }
        public decimal count { get; set; }
    }

    public class Virtual4
    {
        public decimal cost { get; set; }
        public decimal count { get; set; }
    }

    public class Afghanistan
    {
        public Virtual18 virtual18 { get; set; }
        public Virtual23 virtual23 { get; set; }
        public Virtual4 virtual4 { get; set; }
    }

    public class Telegram
    {
        public Afghanistan afghanistan { get; set; }
    }

    public class PricesByProductDto
    {
        public Telegram telegram { get; set; }
    }
}
