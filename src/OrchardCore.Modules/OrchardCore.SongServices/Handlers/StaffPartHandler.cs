using OrchardCore.SongServices.ContentParts;
using OrchardCore.ContentManagement.Handlers;
using System.Threading.Tasks;

namespace OrchardCore.SongServices.Handlers;

public class StaffPartHandler : ContentPartHandler<StaffPart>
{
    public override Task UpdatedAsync(UpdateContentContext context, StaffPart instance)
    {
        context.ContentItem.DisplayText = instance.FullName + ";" + instance.Nickname + ";" + instance.CustomNickname + ";" + instance.Team + ";" + instance.Operator + ";" + instance.Balance + ";" + instance.Currency + ";" + instance.DateTime;

        return Task.CompletedTask;
    }
}
