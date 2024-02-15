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
public class DealerPartDisplayDriver : ContentPartDisplayDriver<DealerPart>
{
    public override IDisplayResult Display(DealerPart part, BuildPartDisplayContext context) =>
        Initialize<DealerPartViewModel>(GetDisplayShapeType(context), viewModel => PopulateViewModel(part, viewModel))
            .Location("Detail", "Content:1")
            .Location("Summary", "Content:1");

    // This is something that wasn't implemented in the BookDisplayDriver (but could've been). It will generate the
    // editor shape for the PersonPart.
    public override IDisplayResult Edit(DealerPart part, BuildPartEditorContext context) =>
        Initialize<DealerPartViewModel>(GetEditorShapeType(context), viewModel => PopulateViewModel(part, viewModel));

    // NEXT STATION: Startup.cs and find the static constructor.

    // So we had an Edit (or EditAsync) method that generates the editor shape. Now it's time to do the content
    // part-specific model binding and validation.
    public override async Task<IDisplayResult> UpdateAsync(DealerPart part, IUpdateModel updater, UpdatePartEditorContext context)
    {
        var viewModel = new DealerPartViewModel();

        await updater.TryUpdateModelAsync(viewModel, Prefix);

        part.Name = viewModel.Name;
        part.Table = viewModel.Table;
        part.Value = viewModel.Value;
        part.DateTime = viewModel.DateTime;

        return await EditAsync(part, context);
    }

    private static void PopulateViewModel(DealerPart part, DealerPartViewModel viewModel)
    {
        viewModel.DealerPart = part;
        
        viewModel.Name = part.Name;
        viewModel.Table = part.Table;
        viewModel.Value = part.Value;
        viewModel.DateTime = part.DateTime;
    }
}

// NEXT STATION: Controllers/PersonListController and go back to the OlderThan30 method where we left.
