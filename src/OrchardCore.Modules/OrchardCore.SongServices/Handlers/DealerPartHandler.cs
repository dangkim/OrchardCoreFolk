using OrchardCore.SongServices.ContentParts;
using OrchardCore.ContentManagement.Handlers;
using System.Threading.Tasks;

namespace OrchardCore.SongServices.Handlers;

// Handlers are basically event handlers for content parts and content items. When you ask something like "how can I run
// my code when my content part is published?" most possibly the answer will be to write a handler. This one here is a
// handler for a content part but you could similarly have a handler for whole content items by inheriting from
// ContentHandlerBase.
public class DealerPartHandler : ContentPartHandler<DealerPart>
{
    // Did you notice that when you list Dealer content items on the dashboard the title of the list item also says the
    // Dealer's name? This is because what's displayed there is the content item's DisplayText (you can think of it as a
    // universal title) that we actually set here. Here we implement UpdatedAsync() which runs every time after a
    // content item is updated. Check out all the other events that you can use!
    public override Task UpdatedAsync(UpdateContentContext context, DealerPart instance)
    {
        context.ContentItem.DisplayText = instance.Name;

        return Task.CompletedTask;
    }
}

// END OF TRAINING SECTION: Content Item display management and queries
// NEXT STATION: Fields/ColorField.cs*@