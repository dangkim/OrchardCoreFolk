using System;
using System.Collections.Generic;
using System.Text;

namespace OrchardCore.SimService.ApiModels
{
    public class BeelineCaP
    {
        public decimal cost { get; set; }
        public decimal count { get; set; }
    }

    public class MatrixCaP
    {
        public decimal cost { get; set; }
        public decimal count { get; set; }
    }

    public class MegafonCaP
    {
        public decimal cost { get; set; }
        public decimal count { get; set; }
    }

    public class MtsCaP
    {
        public decimal cost { get; set; }
        public decimal count { get; set; }
    }

    public class RostelecomCaP
    {
        public decimal cost { get; set; }
        public decimal count { get; set; }
    }

    public class Tele2CaP
    {
        public decimal cost { get; set; }
        public decimal count { get; set; }
    }

    public class Virtual15CaP
    {
        public decimal cost { get; set; }
        public decimal count { get; set; }
    }

    public class YotaCaP
    {
        public decimal cost { get; set; }
        public decimal count { get; set; }
    }

    public class TelegramCaP
    {
        public BeelineCaP beeline { get; set; }
        public MatrixCaP matrix { get; set; }
        public MegafonCaP megafon { get; set; }
        public MtsCaP mts { get; set; }
        public RostelecomCaP rostelecom { get; set; }
        public Tele2CaP tele2 { get; set; }
        public Virtual15CaP virtual15 { get; set; }
        public YotaCaP yota { get; set; }
    }

    public class RussiaCaP
    {
        public TelegramCaP telegram { get; set; }
    }

    public class PricesByCountryandProduct
    {
        public RussiaCaP russia { get; set; }
    }
}
