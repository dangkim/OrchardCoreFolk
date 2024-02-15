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
public class ShowLobbyPartViewModel : IValidatableObject
{
    public string DealerGameName { get; set; }
    public string Kind { get; set; }
    public string DealerName { get; set; }
    public string DealerTableId { get; set; }
    public string GameName { get; set; }
    public string Id { get; set; }
    public string Status { get; set; }
    public string DisplayStatus { get; set; }
    public string Ticks { get; set; }
    public decimal BetAmount { get; set; }
    public int TotalPlayers { get; set; }
    public int AttendedPlayers { get; set; }
    public int TableId { get; set; }
    public int TableNo { get; set; }

    public DateTime? DateTime { get; set; }

    [BindNever]
    public ShowLobbyPart ShowLobbyPart { get; set; }

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        // To use GetService overload you need to add the Microsoft.Extensions.DependencyInjection NuGet package to your
        // module. This way you can get any service you want just as you've injected them in a constructor.
        var localizer = validationContext.GetService<IStringLocalizer<ShowLobbyPartViewModel>>();
        var clock = validationContext.GetService<IClock>();

        if (DateTime.HasValue && clock.UtcNow > DateTime.Value.AddMilliseconds(200))
        {
            yield return new ValidationResult(localizer["The date must be 200ms or older."], new[] { nameof(DateTime) });
        }
    }
}
