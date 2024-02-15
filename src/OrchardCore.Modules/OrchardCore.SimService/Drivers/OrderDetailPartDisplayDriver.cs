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
    public class OrderDetailPartDisplayDriver : ContentPartDisplayDriver<OrderDetailPart>
    {
        public override IDisplayResult Display(OrderDetailPart part, BuildPartDisplayContext context) =>
            Initialize<OrderDetailPartViewModel>(
                GetDisplayShapeType(context),
                viewModel => PopulateViewModel(part, viewModel))
            .Location("Detail", "Content:5")
            .Location("Summary", "Content:5");

        public override IDisplayResult Edit(OrderDetailPart part, BuildPartEditorContext context) =>
            Initialize<OrderDetailPartViewModel>(
                GetEditorShapeType(context),
                viewModel => PopulateViewModel(part, viewModel))
            .Location("Content:5");

        public override async Task<IDisplayResult> UpdateAsync(OrderDetailPart part, IUpdateModel updater, UpdatePartEditorContext context)
        {
            var viewModel = new OrderDetailPartViewModel();

            await updater.TryUpdateModelAsync(viewModel, Prefix);

            part.InventoryId = viewModel.InventoryId;
            part.OrderId = viewModel.Id;
            part.Phone = viewModel.Phone;
            part.Operator = viewModel.Operator;
            part.Product = viewModel.Product;
            part.Price = viewModel.Price;
            part.Status = viewModel.Status;
            part.Expires = viewModel.Expires;
            part.Created_at = viewModel.Created_at;
            part.Country = viewModel.Country;
            part.Email = viewModel.Email;
            part.UserId = viewModel.UserId;
            part.UserName = viewModel.UserName;
            part.Category = viewModel.Category;

            return await EditAsync(part, context);
        }


        private static void PopulateViewModel(OrderDetailPart part, OrderDetailPartViewModel viewModel)
        {
            viewModel.Id = part.OrderId;
            viewModel.InventoryId = part.InventoryId;
            viewModel.Phone = part.Phone;
            viewModel.Operator = part.Operator;
            viewModel.Product = part.Product;
            viewModel.Price = part.Price;
            viewModel.Status = part.Status;
            viewModel.Expires = part.Expires;
            viewModel.Created_at = part.Created_at;
            viewModel.Country = part.Country;
            viewModel.Email = part.Email;
            viewModel.UserId = part.UserId;
            viewModel.UserName = part.UserName;
            viewModel.Category = part.Category;
        }
    }
}
