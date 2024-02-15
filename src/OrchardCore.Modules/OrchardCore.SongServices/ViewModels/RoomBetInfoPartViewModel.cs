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
public class RoomBetInfoPartViewModel : IValidatableObject
{
    public string Amounts { get; set; }
    public string Avatars { get; set; }
    public string Cats { get; set; }
    public string Ids { get; set; }
    public string Nicknames { get; set; }
    public string Room { get; set; }
    public string Seats { get; set; }
    public string Table { get; set; }
    public string Winlose { get; set; }

    public DateTime? DateTime { get; set; }

    [BindNever]
    public RoomBetInfoPart RoomBetInfoPart { get; set; }

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        // To use GetService overload you need to add the Microsoft.Extensions.DependencyInjection NuGet package to your
        // module. This way you can get any service you want just as you've injected them in a constructor.
        var localizer = validationContext.GetService<IStringLocalizer<RoomBetInfoPartViewModel>>();
        var clock = validationContext.GetService<IClock>();

        if (DateTime.HasValue && clock.UtcNow > DateTime.Value.AddMilliseconds(200))
        {
            //yield return new ValidationResult(localizer["The person must be 18 or older."], new[] { nameof(DateTime) });
            yield return new ValidationResult(localizer["The person must be 18 or older."], new[] { nameof(DateTime) });
        }

        // Now go back to the CardsPartDisplayDrvier.
    }
}
