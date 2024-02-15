using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;

namespace OrchardCore.SimService.ApiModels
{
    public class Iso
    {
        public decimal af { get; set; }
    }

    public class Prefix
    {
        [JsonProperty("+93")]
        public decimal _93 { get; set; }
    }

    public class Virtual18CountriesList
    {
        public decimal activation { get; set; }
    }

    public class Virtual21CountriesList
    {
        public decimal activation { get; set; }
    }

    public class Virtual23CountriesList
    {
        public decimal activation { get; set; }
    }

    public class Virtual4CountriesList
    {
        public decimal activation { get; set; }
    }

    public class AfghanistanCountriesList
    {
        public Iso iso { get; set; }
        public Prefix prefix { get; set; }
        public string text_en { get; set; }
        public string text_ru { get; set; }
        public Virtual18CountriesList virtual18 { get; set; }
        public Virtual21CountriesList virtual21 { get; set; }
        public Virtual23CountriesList virtual23 { get; set; }
        public Virtual4CountriesList virtual4 { get; set; }
    }

    public class GetCountriesListDto
    {
        public AfghanistanCountriesList afghanistan { get; set; }
    }
}
