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
public class BetsPartDisplayDriver : ContentPartDisplayDriver<BetsPart>
{
    public override IDisplayResult Display(BetsPart part, BuildPartDisplayContext context) =>
        Initialize<BetsPartViewModel>(GetDisplayShapeType(context), viewModel => PopulateViewModel(part, viewModel))
            .Location("Detail", "Content:1")
            .Location("Summary", "Content:1");

    // This is something that wasn't implemented in the BookDisplayDriver (but could've been). It will generate the
    // editor shape for the PersonPart.
    public override IDisplayResult Edit(BetsPart part, BuildPartEditorContext context) =>
        Initialize<BetsPartViewModel>(GetEditorShapeType(context), viewModel => PopulateViewModel(part, viewModel));

    // NEXT STATION: Startup.cs and find the static constructor.

    // So we had an Edit (or EditAsync) method that generates the editor shape. Now it's time to do the content
    // part-specific model binding and validation.
    public override async Task<IDisplayResult> UpdateAsync(BetsPart part, IUpdateModel updater, UpdatePartEditorContext context)
    {
        var viewModel = new BetsPartViewModel();

        await updater.TryUpdateModelAsync(viewModel, Prefix);

        part.Kind = viewModel.Kind;
        part.Code = viewModel.Code;
        part.Name = viewModel.Name;
        part.Chips = viewModel.Chips;
        part.Id = viewModel.Id;
        part.Max = viewModel.Max;
        part.Min = viewModel.Min;
        part.Allowed = viewModel.Allowed;
        part.Table = viewModel.Table;

        part.DateTime = viewModel.DateTime;

        return await EditAsync(part, context);
    }

    private static void PopulateViewModel(BetsPart part, BetsPartViewModel viewModel)
    {
        viewModel.BetsPart = part;

        viewModel.Kind = part.Kind;
        viewModel.Code = part.Code;
        viewModel.Name = part.Name;
        viewModel.Chips = part.Chips;
        viewModel.Id = part.Id;
        viewModel.Max = part.Max;
        viewModel.Min = part.Min;
        viewModel.Allowed = part.Allowed;
        viewModel.Table = part.Table;
        viewModel.DateTime = part.DateTime;
    }
}

// NEXT STATION: Controllers/PersonListController and go back to the OlderThan30 method where we left.
