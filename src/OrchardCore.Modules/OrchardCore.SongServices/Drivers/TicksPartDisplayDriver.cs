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
public class TicksPartDisplayDriver : ContentPartDisplayDriver<TicksPart>
{
    public override IDisplayResult Display(TicksPart part, BuildPartDisplayContext context) =>
        Initialize<TicksPartViewModel>(GetDisplayShapeType(context), viewModel => PopulateViewModel(part, viewModel))
            .Location("Detail", "Content:1")
            .Location("Summary", "Content:1");

    // This is something that wasn't implemented in the BookDisplayDriver (but could've been). It will generate the
    // editor shape for the PersonPart.
    public override IDisplayResult Edit(TicksPart part, BuildPartEditorContext context) =>
        Initialize<TicksPartViewModel>(GetEditorShapeType(context), viewModel => PopulateViewModel(part, viewModel));

    // NEXT STATION: Startup.cs and find the static constructor.

    // So we had an Edit (or EditAsync) method that generates the editor shape. Now it's time to do the content
    // part-specific model binding and validation.
    public override async Task<IDisplayResult> UpdateAsync(TicksPart part, IUpdateModel updater, UpdatePartEditorContext context)
    {
        var viewModel = new TicksPartViewModel();

        await updater.TryUpdateModelAsync(viewModel, Prefix);

        part.Table = viewModel.Table;
        part.Value = viewModel.Value;
        part.DateTime = viewModel.DateTime;

        return await EditAsync(part, context);
    }

    private static void PopulateViewModel(TicksPart part, TicksPartViewModel viewModel)
    {
        viewModel.TicksPart = part;

        viewModel.Table = part.Table;
        viewModel.Value = part.Value;
        viewModel.DateTime = part.DateTime;
    }
}

// NEXT STATION: Controllers/PersonListController and go back to the OlderThan30 method where we left.
