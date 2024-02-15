using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using OrchardCore.ContentManagement.Display.ContentDisplay;
using OrchardCore.ContentManagement.Display.Models;
using OrchardCore.SimService.Models;
using OrchardCore.SimService.ViewModels;
using OrchardCore.DisplayManagement.ModelBinding;
using OrchardCore.DisplayManagement.Views;

namespace OrchardCore.SimService.Drivers
{
    public class UserProfilePartDisplayDriver : ContentPartDisplayDriver<UserProfilePart>
    {
        public override IDisplayResult Display(UserProfilePart part, BuildPartDisplayContext context) =>
            Initialize<UserProfilePartViewModel>(
                GetDisplayShapeType(context),
                viewModel => PopulateViewModel(part, viewModel))
            .Location("Detail", "Content:5")
            .Location("Summary", "Content:5");

        public override IDisplayResult Edit(UserProfilePart part, BuildPartEditorContext context) =>
            Initialize<UserProfilePartViewModel>(
                GetEditorShapeType(context),
                viewModel => PopulateViewModel(part, viewModel))
            .Location("Content:5");

        public override async Task<IDisplayResult> UpdateAsync(UserProfilePart part, IUpdateModel updater, UpdatePartEditorContext context)
        {
            var viewModel = new UserProfilePartViewModel();

            await updater.TryUpdateModelAsync(viewModel, Prefix);

            part.ProfileId = viewModel.ProfileId;
            part.Email = viewModel.Email;
            part.UserId = viewModel.UserId;
            part.UserName = viewModel.UserName;
            part.Vendor = viewModel.Vendor;
            part.DefaultForwardingNumber = viewModel.DefaultForwardingNumber;
            part.Balance = viewModel.Balance;
            part.OriginalAmount = viewModel.OriginalAmount;
            part.Amount = viewModel.Amount;
            part.RateInUsd = viewModel.RateInUsd;
            part.GmailMsgId = viewModel.GmailMsgId;
            part.Rating = viewModel.Rating;
            part.DefaultCoutryName = viewModel.DefaultCoutryName;
            part.DefaultIso = viewModel.DefaultIso;
            part.DefaultPrefix = viewModel.DefaultPrefix;
            part.DefaultOperatorName = viewModel.DefaultOperatorName;
            part.FrozenBalance = viewModel.FrozenBalance;
            part.TokenApi = viewModel.TokenApi;

            return await EditAsync(part, context);
        }


        private static void PopulateViewModel(UserProfilePart part, UserProfilePartViewModel viewModel)
        {
            part.ProfileId = part.ProfileId;
            viewModel.Email = part.Email;
            viewModel.UserId = part.UserId;
            viewModel.UserName = part.UserName;
            viewModel.Vendor = part.Vendor;
            viewModel.DefaultForwardingNumber = part.DefaultForwardingNumber;
            viewModel.Balance = part.Balance;
            viewModel.OriginalAmount = part.OriginalAmount;
            viewModel.Amount = part.Amount;
            viewModel.RateInUsd = part.RateInUsd;
            viewModel.GmailMsgId = part.GmailMsgId;
            viewModel.Rating = part.Rating;
            viewModel.DefaultCoutryName = part.DefaultCoutryName;
            viewModel.DefaultIso = part.DefaultIso;
            viewModel.DefaultPrefix = part.DefaultPrefix;
            viewModel.DefaultOperatorName = part.DefaultOperatorName;
            viewModel.FrozenBalance = part.FrozenBalance;
            viewModel.TokenApi = part.TokenApi;
        }
    }
}
