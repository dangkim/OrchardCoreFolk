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
    public class SmsPartDisplayDriver : ContentPartDisplayDriver<SmsPart>
    {
        public override IDisplayResult Display(SmsPart part, BuildPartDisplayContext context) =>
            Initialize<SmsPartViewModel>(
                GetDisplayShapeType(context),
                viewModel => PopulateViewModel(part, viewModel))
            .Location("Detail", "Content:5")
            .Location("Summary", "Content:5");

        public override IDisplayResult Edit(SmsPart part, BuildPartEditorContext context) =>
            Initialize<SmsPartViewModel>(
                GetEditorShapeType(context),
                viewModel => PopulateViewModel(part, viewModel))
            .Location("Content:5");

        public override async Task<IDisplayResult> UpdateAsync(SmsPart part, IUpdateModel updater, UpdatePartEditorContext context)
        {
            var viewModel = new SmsPartViewModel();

            await updater.TryUpdateModelAsync(viewModel, Prefix);

            part.Created_at = viewModel.Created_at;
            part.Sender = viewModel.Sender;
            part.Text = viewModel.Text;
            part.Code = viewModel.Code;
            part.Email = viewModel.Email;
            part.UserId = viewModel.UserId;
            part.UserName = viewModel.UserName;
            part.Date = viewModel.Date;
            part.OrderId = viewModel.OrderId;

            return await EditAsync(part, context);
        }


        private static void PopulateViewModel(SmsPart part, SmsPartViewModel viewModel)
        {
            viewModel.Created_at = part.Created_at;
            viewModel.Sender = part.Sender;
            viewModel.Text = part.Text;
            viewModel.Code = part.Code;
            viewModel.Email = part.Email;
            viewModel.UserId = part.UserId;
            viewModel.UserName = part.UserName;
            viewModel.Date = part.Date;
            viewModel.OrderId = part.OrderId;
        }
    }
}
