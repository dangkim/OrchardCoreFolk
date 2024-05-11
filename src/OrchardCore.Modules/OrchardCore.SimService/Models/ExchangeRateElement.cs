using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OrchardCore.ContentFields.Fields;
using OrchardCore.ContentManagement;

namespace OrchardCore.SimService.Models;

public class ExchangeRateElement : ContentElement
{
    public TextField FromCurrency { get; set; }

    public TextField RateToUsd { get; set; }
}
