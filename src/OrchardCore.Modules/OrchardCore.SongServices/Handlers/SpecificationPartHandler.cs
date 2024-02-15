using OrchardCore.SongServices.ContentParts;
using OrchardCore.ContentManagement.Handlers;
using System.Threading.Tasks;

namespace OrchardCore.SongServices.Handlers;

public class SpecificationPartHandler : ContentPartHandler<SpecificationPart>
{
    public override Task UpdatedAsync(UpdateContentContext context, SpecificationPart instance)
    {
        context.ContentItem.DisplayText = instance.ReportStatus + ";" + instance.RootCause + ";" + instance.Event + ";" + instance.Sender + ";" + instance.Writer + ";" + instance.AssignerContentItemId + ";" + instance.AssigneeContentItemId + ";" + instance.LocationContentItemId + ";" + instance.Behavior + ";" + instance.DateTime;

        return Task.CompletedTask;
    }
}
