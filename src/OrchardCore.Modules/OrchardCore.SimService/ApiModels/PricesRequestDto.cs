using System;
using System.Collections.Generic;
using System.Text;

namespace OrchardCore.SimService.ApiModels
{
    public class Beeline
    {
        public decimal cost { get; set; }
        public decimal count { get; set; }
    }

    public class Lycamobile
    {
        public decimal cost { get; set; }
        public decimal count { get; set; }
    }

    public class Matrix
    {
        public decimal cost { get; set; }
        public decimal count { get; set; }
    }

    public class _1688
    {
        public Beeline beeline { get; set; }
        public Lycamobile lycamobile { get; set; }
        public Matrix matrix { get; set; }
    }

    public class Russia
    {
        public _1688 _1688 { get; set; }
    }

    public class PricesRequestDto
    {
        public Russia russia { get; set; }
    }
}
