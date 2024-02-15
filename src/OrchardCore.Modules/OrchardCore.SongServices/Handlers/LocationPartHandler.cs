using OrchardCore.SongServices.ContentParts;
using OrchardCore.ContentManagement.Handlers;
using System.Threading.Tasks;

namespace OrchardCore.SongServices.Handlers;

public class LocationPartHandler : ContentPartHandler<LocationPart>
{
    public override Task UpdatedAsync(UpdateContentContext context, LocationPart instance)
    {
        context.ContentItem.DisplayText = instance.Building + ";" + instance.City + ";" + instance.Country + ";" + instance.Street + ";" + instance.Site + ";" + instance.Building + ";" + instance.Zone + ";" + instance.Room + ";" + instance.DateTime;

        return Task.CompletedTask;
    }
}
