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
public class ShowLobbyPartDisplayDriver : ContentPartDisplayDriver<ShowLobbyPart>
{
    public override IDisplayResult Display(ShowLobbyPart part, BuildPartDisplayContext context) =>
        Initialize<ShowLobbyPartViewModel>(GetDisplayShapeType(context), viewModel => PopulateViewModel(part, viewModel))
            .Location("Detail", "Content:1")
            .Location("Summary", "Content:1");

    // This is something that wasn't implemented in the BookDisplayDriver (but could've been). It will generate the
    // editor shape for the PersonPart.
    public override IDisplayResult Edit(ShowLobbyPart part, BuildPartEditorContext context) =>
        Initialize<ShowLobbyPartViewModel>(GetEditorShapeType(context), viewModel => PopulateViewModel(part, viewModel));

    // NEXT STATION: Startup.cs and find the static constructor.

    // So we had an Edit (or EditAsync) method that generates the editor shape. Now it's time to do the content
    // part-specific model binding and validation.
    public override async Task<IDisplayResult> UpdateAsync(ShowLobbyPart part, IUpdateModel updater, UpdatePartEditorContext context)
    {
        var viewModel = new ShowLobbyPartViewModel();

        await updater.TryUpdateModelAsync(viewModel, Prefix);

        part.DealerGameName = viewModel.DealerGameName;
        part.Kind = viewModel.Kind;
        part.DealerName = viewModel.DealerName;
        part.DealerTableId = viewModel.DealerTableId;
        part.GameName = viewModel.GameName;
        part.Id = viewModel.Id;
        part.Status = viewModel.Status;
        part.DisplayStatus = viewModel.DisplayStatus;
        part.Ticks = viewModel.Ticks;
        part.BetAmount = viewModel.BetAmount;
        part.TotalPlayers = viewModel.TotalPlayers;
        part.AttendedPlayers = viewModel.AttendedPlayers;
        part.TableId = viewModel.TableId;
        part.TableNo = viewModel.TableNo;
        part.DateTime = viewModel.DateTime;

        return await EditAsync(part, context);
    }

    private static void PopulateViewModel(ShowLobbyPart part, ShowLobbyPartViewModel viewModel)
    {
        viewModel.ShowLobbyPart = part;

        viewModel.DealerGameName = part.DealerGameName;
        viewModel.Kind = part.Kind;
        viewModel.DealerName = part.DealerName;
        viewModel.DealerTableId = part.DealerTableId;
        viewModel.GameName = part.GameName;
        viewModel.Id = part.Id;
        viewModel.Status = part.Status;
        viewModel.DisplayStatus = part.DisplayStatus;
        viewModel.Ticks = part.Ticks;
        viewModel.BetAmount = part.BetAmount;
        viewModel.TotalPlayers = part.TotalPlayers;
        viewModel.AttendedPlayers = part.AttendedPlayers;
        viewModel.TableId = part.TableId;
        viewModel.TableNo = part.TableNo;
        viewModel.DateTime = part.DateTime;
    }
}
