using OrchardCore.SongServices.ContentParts;
using OrchardCore.SongServices.ViewModels;
using OrchardCore.ContentManagement.Display.ContentDisplay;
using OrchardCore.ContentManagement.Display.Models;
using OrchardCore.DisplayManagement.ModelBinding;
using OrchardCore.DisplayManagement.Views;
using System.Threading.Tasks;

namespace OrchardCore.SongServices.Drivers;

// Drivers inherited from ContentPartDisplayDrivers have a functionality similar to the one described in
// BookDisplayDriver but these are for ContentParts. Don't forget to register this class with the service provider (see:
// Startup.cs).
public class RoomBetInfoPartDisplayDriver : ContentPartDisplayDriver<RoomBetInfoPart>
{
    public override IDisplayResult Display(RoomBetInfoPart part, BuildPartDisplayContext context) =>
        Initialize<RoomBetInfoPartViewModel>(GetDisplayShapeType(context), viewModel => PopulateViewModel(part, viewModel))
            .Location("Detail", "Content:1")
            .Location("Summary", "Content:1");

    // This is something that wasn't implemented in the BookDisplayDriver (but could've been). It will generate the
    // editor shape for the PersonPart.
    public override IDisplayResult Edit(RoomBetInfoPart part, BuildPartEditorContext context) =>
        Initialize<RoomBetInfoPartViewModel>(GetEditorShapeType(context), viewModel => PopulateViewModel(part, viewModel));

    // NEXT STATION: Startup.cs and find the static constructor.

    // So we had an Edit (or EditAsync) method that generates the editor shape. Now it's time to do the content
    // part-specific model binding and validation.
    public override async Task<IDisplayResult> UpdateAsync(RoomBetInfoPart part, IUpdateModel updater, UpdatePartEditorContext context)
    {
        var viewModel = new RoomBetInfoPartViewModel();

        await updater.TryUpdateModelAsync(viewModel, Prefix);

        part.Amounts = viewModel.Amounts;
        part.Avatars = viewModel.Avatars;
        part.Cats = viewModel.Cats;
        part.Nicknames = viewModel.Nicknames;
        part.Room = viewModel.Room;
        part.Seats = viewModel.Seats;
        part.Table = viewModel.Table;
        part.Winlose = viewModel.Winlose;
        part.DateTime = viewModel.DateTime;

        return await EditAsync(part, context);
    }

    private static void PopulateViewModel(RoomBetInfoPart part, RoomBetInfoPartViewModel viewModel)
    {
        viewModel.RoomBetInfoPart = part;

        viewModel.Amounts = part.Amounts;
        viewModel.Avatars = part.Avatars;
        viewModel.Cats = part.Cats;
        viewModel.Nicknames = part.Nicknames;
        viewModel.Room = part.Room;
        viewModel.Seats = part.Seats;
        viewModel.Table = part.Table;
        viewModel.Winlose = part.Winlose;
        viewModel.DateTime = part.DateTime;
    }
}
