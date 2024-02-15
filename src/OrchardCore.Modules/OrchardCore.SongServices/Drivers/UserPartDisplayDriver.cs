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
public class UserPartDisplayDriver : ContentPartDisplayDriver<UserPart>
{
    public override IDisplayResult Display(UserPart part, BuildPartDisplayContext context) =>
        Initialize<UserPartViewModel>(GetDisplayShapeType(context), viewModel => PopulateViewModel(part, viewModel))
            .Location("Detail", "Content:1")
            .Location("Summary", "Content:1");

    // This is something that wasn't implemented in the BookDisplayDriver (but could've been). It will generate the
    // editor shape for the PersonPart.
    public override IDisplayResult Edit(UserPart part, BuildPartEditorContext context) =>
        Initialize<UserPartViewModel>(GetEditorShapeType(context), viewModel => PopulateViewModel(part, viewModel));

    // NEXT STATION: Startup.cs and find the static constructor.

    // So we had an Edit (or EditAsync) method that generates the editor shape. Now it's time to do the content
    // part-specific model binding and validation.
    public override async Task<IDisplayResult> UpdateAsync(UserPart part, IUpdateModel updater, UpdatePartEditorContext context)
    {
        var viewModel = new UserPartViewModel();

        await updater.TryUpdateModelAsync(viewModel, Prefix);

        part.Name = viewModel.Name;
        part.AvatarId = viewModel.AvatarId;
        part.BookmarkedReportContentItemIds = viewModel.BookmarkedReportContentItemIds;
        part.Balance = viewModel.Balance;
        part.Currency = viewModel.Currency;
        part.CustomNickname = viewModel.CustomNickname;
        part.Id = viewModel.Id;
        part.UserName = viewModel.UserName;
        part.Nickname = viewModel.Nickname;
        part.Op = viewModel.Op;
        part.RiskId = viewModel.RiskId;
        part.TestUser = viewModel.TestUser;

        part.DateTime = viewModel.DateTime;

        return await EditAsync(part, context);
    }

    private static void PopulateViewModel(UserPart part, UserPartViewModel viewModel)
    {
        viewModel.UserPart = part;

        viewModel.Name = part.Name;
        viewModel.AvatarId = part.AvatarId;
        viewModel.BookmarkedReportContentItemIds = part.BookmarkedReportContentItemIds;
        viewModel.Balance = part.Balance;
        viewModel.Currency = part.Currency;
        viewModel.CustomNickname = part.CustomNickname;
        viewModel.Id = part.Id;
        viewModel.UserName = part.UserName;
        viewModel.Nickname = part.Nickname;
        viewModel.Op = part.Op;
        viewModel.RiskId = part.RiskId;
        viewModel.TestUser = part.TestUser;
    }
}

// NEXT STATION: Controllers/PersonListController and go back to the OlderThan30 method where we left.
