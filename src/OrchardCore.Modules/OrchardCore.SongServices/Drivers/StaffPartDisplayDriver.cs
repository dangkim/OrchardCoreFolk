using OrchardCore.SongServices.ContentParts;
using OrchardCore.SongServices.ViewModels;
using OrchardCore.ContentManagement.Display.ContentDisplay;
using OrchardCore.ContentManagement.Display.Models;
using OrchardCore.DisplayManagement.ModelBinding;
using OrchardCore.DisplayManagement.Views;
using System.Threading.Tasks;
using System;

namespace OrchardCore.SongServices.Drivers;

public class StaffPartDisplayDriver : ContentPartDisplayDriver<StaffPart>
{
    public override IDisplayResult Display(StaffPart part, BuildPartDisplayContext context) =>
        Initialize<StaffPartViewModel>(GetDisplayShapeType(context), viewModel => PopulateViewModel(part, viewModel))
            .Location("Detail", "Content:1")
            .Location("Summary", "Content:1");

    public override IDisplayResult Edit(StaffPart part, BuildPartEditorContext context) =>
        Initialize<StaffPartViewModel>(GetEditorShapeType(context), viewModel => PopulateViewModel(part, viewModel));

    public override async Task<IDisplayResult> UpdateAsync(StaffPart part, IUpdateModel updater, UpdatePartEditorContext context)
    {
        var viewModel = new StaffPartViewModel();

        await updater.TryUpdateModelAsync(viewModel, Prefix);

        part.Nickname = viewModel.Nickname;
        part.AvatarId = viewModel.AvatarId;
        part.Operator = viewModel.Operator;
        part.Team = viewModel.Team;
        part.FullName = viewModel.FullName;
        part.UserName = viewModel.UserName;
        part.BookmarkedReportContentItemIds = viewModel.BookmarkedReportContentItemIds;
        part.Balance = viewModel.Balance;
        part.Currency = viewModel.Currency;
        part.CustomNickname = viewModel.CustomNickname;
        part.StaffId = viewModel.StaffId;
        part.Birthday = viewModel.Birthday;

        part.DateTime = DateTime.UtcNow;

        return await EditAsync(part, context);
    }

    private static void PopulateViewModel(StaffPart part, StaffPartViewModel viewModel)
    {
        viewModel.StaffPart = part;

        viewModel.Nickname = part.Nickname;
        viewModel.AvatarId = part.AvatarId;
        viewModel.Operator = part.Operator;
        viewModel.Team = part.Team;
        viewModel.FullName = part.FullName;
        viewModel.UserName = part.UserName;
        viewModel.BookmarkedReportContentItemIds = part.BookmarkedReportContentItemIds;
        viewModel.Balance = part.Balance;
        viewModel.Currency = part.Currency;
        viewModel.CustomNickname = part.CustomNickname;
        viewModel.StaffId = part.StaffId;
        viewModel.Birthday = part.Birthday;

        viewModel.DateTime = part.DateTime;
    }
}
