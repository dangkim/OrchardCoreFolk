using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using OrchardCore.ContentManagement.Handlers;
using OrchardCore.SimService.ApiModels;
using OrchardCore.SimService.Models;

namespace OrchardCore.SimService.Handlers
{
    public class PaymentDetailPartHandler : ContentPartHandler<PaymentDetailPart>
    {
        public override Task UpdatedAsync(UpdateContentContext context, PaymentDetailPart instance)
        {
            context.ContentItem.DisplayText = instance.PaymentId
                                                + ";" + instance.OrderId
                                                + ";" + instance.TypeName
                                                + ";" + instance.ProviderName
                                                + ";" + instance.Amount.ToString()
                                                + ";" + instance.Balance.ToString()
                                                + ";" + instance.Email
                                                + ";" + instance.UserId
                                                + ";" + instance.UserName;
            return Task.CompletedTask;
        }

        public override Task PublishedAsync(PublishContentContext context, PaymentDetailPart instance)
        {
            context.ContentItem.DisplayText = instance.PaymentId
                                                + ";" + instance.OrderId
                                                + ";" + instance.TypeName
                                                + ";" + instance.ProviderName
                                                + ";" + instance.Amount.ToString()
                                                + ";" + instance.Balance.ToString()
                                                + ";" + instance.Email
                                                + ";" + instance.UserId
                                                + ";" + instance.UserName;
            return Task.CompletedTask;
        }
    }
}
