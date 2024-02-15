using OrchardCore.SongServices.ContentParts;
using OrchardCore.SongServices.ViewModels;
using OrchardCore.ContentManagement.Display.ContentDisplay;
using OrchardCore.ContentManagement.Display.Models;
using OrchardCore.DisplayManagement.ModelBinding;
using OrchardCore.DisplayManagement.Views;
using System.Threading.Tasks;

namespace OrchardCore.SongServices.Drivers;

public class LocationPartDisplayDriver : ContentPartDisplayDriver<LocationPart>
{
    public override IDisplayResult Display(LocationPart part, BuildPartDisplayContext context) =>
        Initialize<LocationPartViewModel>(GetDisplayShapeType(context), viewModel => PopulateViewModel(part, viewModel))
            .Location("Detail", "Content:1")
            .Location("Summary", "Content:1");

    public override IDisplayResult Edit(LocationPart part, BuildPartEditorContext context) =>
        Initialize<LocationPartViewModel>(GetEditorShapeType(context), viewModel => PopulateViewModel(part, viewModel));

    public override async Task<IDisplayResult> UpdateAsync(LocationPart part, IUpdateModel updater, UpdatePartEditorContext context)
    {
        var viewModel = new LocationPartViewModel();

        await updater.TryUpdateModelAsync(viewModel, Prefix);

        part.Country = viewModel.Country;
        part.City = viewModel.City;
        part.Street = viewModel.Street;
        part.Site = viewModel.Site;
        part.Building = viewModel.Building;
        part.Floor = viewModel.Floor;
        part.Zone = viewModel.Zone;
        part.Room = viewModel.Room;

        part.DateTime = viewModel.DateTime;

        return await EditAsync(part, context);
    }

    private static void PopulateViewModel(LocationPart part, LocationPartViewModel viewModel)
    {
        viewModel.LocationPart = part;

        viewModel.Country = part.Country;
        viewModel.City = part.City;
        viewModel.Street = part.Street;
        viewModel.Site = part.Site;
        viewModel.Building = part.Building;
        viewModel.Floor = part.Floor;
        viewModel.Zone = part.Zone;
        viewModel.Room = part.Room;

        viewModel.DateTime = part.DateTime;
    }
}
