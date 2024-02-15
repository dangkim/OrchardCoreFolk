using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using OrchardCore.ContentManagement.Handlers;
using OrchardCore.SimService.ApiModels;
using OrchardCore.SimService.Models;

namespace OrchardCore.SimService.Handlers
{
    public class UserProfilePartHandler : ContentPartHandler<UserProfilePart>
    {
        public override Task UpdatedAsync(UpdateContentContext context, UserProfilePart instance)
        {
            context.ContentItem.DisplayText = instance.ProfileId
                                                + ";" + instance.UserId
                                                + ";" + instance.UserName
                                                + ";" + instance.Vendor
                                                + ";" + instance.Balance.ToString();
            return Task.CompletedTask;
        }

        public override Task PublishedAsync(PublishContentContext context, UserProfilePart instance)
        {
            context.ContentItem.DisplayText = instance.ProfileId
                                                + ";" + instance.UserId
                                                + ";" + instance.UserName
                                                + ";" + instance.Vendor
                                                + ";" + instance.Balance.ToString();
            return Task.CompletedTask;
        }
    }
}
