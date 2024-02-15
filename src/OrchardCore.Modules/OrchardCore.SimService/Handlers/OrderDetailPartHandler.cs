using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using OrchardCore.ContentManagement.Handlers;
using OrchardCore.SimService.ApiModels;
using OrchardCore.SimService.Models;

namespace OrchardCore.SimService.Handlers
{
    public class OrderDetailPartHandler : ContentPartHandler<OrderDetailPart>
    {
        public override Task UpdatedAsync(UpdateContentContext context, OrderDetailPart instance)
        {
            var enumDisplayStatus = (InventoryEnum)instance.InventoryId;
            var inventoryValue = enumDisplayStatus.ToString();

            context.ContentItem.DisplayText = instance.UserId + ";"
                + inventoryValue + ";"
                + instance.Phone + ";"
                + instance.Operator + ";"
                + instance.Product + ";"
                + instance.Status + ";"
                + instance.Country + ";"
                + instance.Email + ";"
                + instance.UserName + ";";

            return Task.CompletedTask;
        }

        public override Task PublishedAsync(PublishContentContext context, OrderDetailPart instance)
        {
            var enumDisplayStatus = (InventoryEnum)instance.InventoryId;
            var inventoryValue = enumDisplayStatus.ToString();

            context.ContentItem.DisplayText = instance.UserId + ";"
                + inventoryValue + ";"
                + instance.Phone + ";"
                + instance.Operator + ";"
                + instance.Product + ";"
                + instance.Status + ";"
                + instance.Country + ";"
                + instance.Email + ";"
                + instance.UserName + ";";

            return Task.CompletedTask;
        }
    }
}
