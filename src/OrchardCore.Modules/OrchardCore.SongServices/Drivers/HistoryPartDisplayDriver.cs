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
public class HistoryPartDisplayDriver : ContentPartDisplayDriver<HistoryPart>
{
    public override IDisplayResult Display(HistoryPart part, BuildPartDisplayContext context) =>
        Initialize<HistoryPartViewModel>(GetDisplayShapeType(context), viewModel => PopulateViewModel(part, viewModel))
            .Location("Detail", "Content:1")
            .Location("Summary", "Content:1");

    // This is something that wasn't implemented in the BookDisplayDriver (but could've been). It will generate the
    // editor shape for the PersonPart.
    public override IDisplayResult Edit(HistoryPart part, BuildPartEditorContext context) =>
        Initialize<HistoryPartViewModel>(GetEditorShapeType(context), viewModel => PopulateViewModel(part, viewModel));

    // NEXT STATION: Startup.cs and find the static constructor.

    // So we had an Edit (or EditAsync) method that generates the editor shape. Now it's time to do the content
    // part-specific model binding and validation.
    public override async Task<IDisplayResult> UpdateAsync(HistoryPart part, IUpdateModel updater, UpdatePartEditorContext context)
    {
        var viewModel = new HistoryPartViewModel();

        await updater.TryUpdateModelAsync(viewModel, Prefix);

        part.Cards = viewModel.Cards;
        part.Powerup = viewModel.Powerup;
        part.Status = viewModel.Status;
        part.Kind = viewModel.Kind;
        part.Result = viewModel.Result;
        part.Round = viewModel.Round;
        part.Kind = viewModel.Kind;
        part.RoundId = viewModel.RoundId;
        part.Shoe = viewModel.Shoe;
        part.Table = viewModel.Table;
        part.DateTime = viewModel.DateTime;

        return await EditAsync(part, context);
    }

    private static void PopulateViewModel(HistoryPart part, HistoryPartViewModel viewModel)
    {
        viewModel.HistoryPart = part;

        viewModel.Cards = part.Cards;
        viewModel.Powerup = part.Powerup;
        viewModel.Status = part.Status;
        viewModel.Kind = part.Kind;
        viewModel.Result = part.Result;
        viewModel.Round = part.Round;
        viewModel.Kind = part.Kind;
        viewModel.RoundId = part.RoundId;
        viewModel.Shoe = part.Shoe;
        viewModel.Table = part.Table;
        viewModel.DateTime = part.DateTime;
    }
}

// NEXT STATION: Controllers/PersonListController and go back to the OlderThan30 method where we left.
