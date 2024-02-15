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
public class JoinRoomSuccessPartDisplayDriver : ContentPartDisplayDriver<JoinRoomSuccessPart>
{
    public override IDisplayResult Display(JoinRoomSuccessPart part, BuildPartDisplayContext context) =>
        Initialize<JoinRoomSuccessPartViewModel>(GetDisplayShapeType(context), viewModel => PopulateViewModel(part, viewModel))
            .Location("Detail", "Content:1")
            .Location("Summary", "Content:1");

    // This is something that wasn't implemented in the BookDisplayDriver (but could've been). It will generate the
    // editor shape for the PersonPart.
    public override IDisplayResult Edit(JoinRoomSuccessPart part, BuildPartEditorContext context) =>
        Initialize<JoinRoomSuccessPartViewModel>(GetEditorShapeType(context), viewModel => PopulateViewModel(part, viewModel));

    // NEXT STATION: Startup.cs and find the static constructor.

    // So we had an Edit (or EditAsync) method that generates the editor shape. Now it's time to do the content
    // part-specific model binding and validation.
    public override async Task<IDisplayResult> UpdateAsync(JoinRoomSuccessPart part, IUpdateModel updater, UpdatePartEditorContext context)
    {
        var viewModel = new JoinRoomSuccessPartViewModel();

        await updater.TryUpdateModelAsync(viewModel, Prefix);

        part.Room = viewModel.Room;
        part.Seat = viewModel.Seat;
        part.Table = viewModel.Table;
        part.DateTime = viewModel.DateTime;

        return await EditAsync(part, context);
    }

    private static void PopulateViewModel(JoinRoomSuccessPart part, JoinRoomSuccessPartViewModel viewModel)
    {
        viewModel.JoinRoomSuccessPart = part;

        viewModel.Room = part.Room;
        viewModel.Seat = part.Seat;
        viewModel.Table = part.Table;
        viewModel.DateTime = part.DateTime;
    }
}
