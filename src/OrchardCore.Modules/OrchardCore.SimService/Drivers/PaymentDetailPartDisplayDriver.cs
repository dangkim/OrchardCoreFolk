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
    public class PaymentDetailPartDisplayDriver : ContentPartDisplayDriver<PaymentDetailPart>
    {
        public override IDisplayResult Display(PaymentDetailPart part, BuildPartDisplayContext context) =>
            Initialize<PaymentDetailPartViewModel>(
                GetDisplayShapeType(context),
                viewModel => PopulateViewModel(part, viewModel))
            .Location("Detail", "Content:5")
            .Location("Summary", "Content:5");

        public override IDisplayResult Edit(PaymentDetailPart part, BuildPartEditorContext context) =>
            Initialize<PaymentDetailPartViewModel>(
                GetEditorShapeType(context),
                viewModel => PopulateViewModel(part, viewModel))
            .Location("Content:5");

        public override async Task<IDisplayResult> UpdateAsync(PaymentDetailPart part, IUpdateModel updater, UpdatePartEditorContext context)
        {
            var viewModel = new PaymentDetailPartViewModel();

            await updater.TryUpdateModelAsync(viewModel, Prefix);

            part.PaymentId = viewModel.PaymentId;
            part.TypeName = viewModel.TypeName;
            part.ProviderName = viewModel.ProviderName;
            part.Amount = viewModel.Amount;
            part.Balance = viewModel.Balance;
            part.CreatedAt = viewModel.CreatedAt;
            part.UserId = viewModel.UserId;
            part.UserName = viewModel.UserName;
            part.Email = viewModel.Email;
            part.OrderId = viewModel.OrderId;

            return await EditAsync(part, context);
        }


        private static void PopulateViewModel(PaymentDetailPart part, PaymentDetailPartViewModel viewModel)
        {
            viewModel.PaymentId = part.PaymentId;
            viewModel.TypeName = part.TypeName;
            viewModel.ProviderName = part.ProviderName;
            viewModel.Amount = part.Amount;
            viewModel.Balance = part.Balance;
            viewModel.CreatedAt = part.CreatedAt;
            viewModel.UserId = part.UserId;
            viewModel.UserName = part.UserName;
            viewModel.Email = part.Email;
            viewModel.OrderId = part.OrderId;
        }
    }
}
