using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using OrchardCore.ContentManagement.Handlers;
using OrchardCore.SimService.ApiModels;
using OrchardCore.SimService.Models;

namespace OrchardCore.SimService.Handlers
{
    public class SmsPartHandler : ContentPartHandler<SmsPart>
    {
        //public override Task UpdatedAsync(UpdateContentContext context, SmsPart instance)
        //{
            //context.ContentItem.DisplayText = instance.Sender
            //                                    + ";" + instance.Text
            //                                    + ";" + instance.Code
            //                                    + ";" + instance.Email
            //                                    + ";" + instance.UserId
            //                                    + ";" + instance.UserName;
            //return Task.CompletedTask;
        //}

        public override Task PublishedAsync(PublishContentContext context, SmsPart instance)
        {
            context.ContentItem.DisplayText = instance.Sender
                                                + ";" + instance.Text
                                                + ";" + instance.Code
                                                + ";" + instance.Email
                                                + ";" + instance.UserId
                                                + ";" + instance.UserName;
            return Task.CompletedTask;
        }
    }
}
