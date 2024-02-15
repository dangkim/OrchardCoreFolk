using OrchardCore.SongServices.ContentParts;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Localization;
using OrchardCore.Modules;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace OrchardCore.SongServices.ViewModels;

// IValidateObject is an ASP.NET Core feature to use on view models where the model binder will automatically execute
// the Validate method which will return any validation error.
public class BetsPartViewModel : IValidatableObject
{
    public string Kind { get; set; }
    public string Code { get; set; }
    public string Name { get; set; }
    public string Chips { get; set; }
    public string Id { get; set; }
    public decimal Max { get; set; }
    public decimal Min { get; set; }
    public bool Allowed { get; set; }
    public string Table { get; set; }

    public DateTime? DateTime { get; set; }

    [BindNever]
    public BetsPart BetsPart { get; set; }

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        // To use GetService overload you need to add the Microsoft.Extensions.DependencyInjection NuGet package to your
        // module. This way you can get any service you want just as you've injected them in a constructor.
        var localizer = validationContext.GetService<IStringLocalizer<BetsPartViewModel>>();
        var clock = validationContext.GetService<IClock>();

        if (DateTime.HasValue && clock.UtcNow > DateTime.Value.AddMilliseconds(200))
        {
            //yield return new ValidationResult(localizer["The person must be 18 or older."], new[] { nameof(DateTime) });
            yield return new ValidationResult(localizer["The person must be 18 or older."], new[] { nameof(DateTime) });
        }

        // Now go back to the CardsPartDisplayDrvier.
    }
}
