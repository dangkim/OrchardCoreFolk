using OrchardCore.SongServices.ContentParts;
using OrchardCore.ContentManagement.Handlers;
using System.Threading.Tasks;
using System;

namespace OrchardCore.SongServices.Handlers;

// Handlers are basically event handlers for content parts and content items. When you ask something like "how can I run
// my code when my content part is published?" most possibly the answer will be to write a handler. This one here is a
// handler for a content part but you could similarly have a handler for whole content items by inheriting from
// ContentHandlerBase.
public class TraderPartHandler : ContentPartHandler<TraderForFilteringPart>
{
    // Did you notice that when you list Bets content items on the dashboard the title of the list item also says the
    // Bets's name? This is because what's displayed there is the content item's DisplayText (you can think of it as a
    // universal title) that we actually set here. Here we implement UpdatedAsync() which runs every time after a
    // content item is updated. Check out all the other events that you can use!
    public override Task UpdatedAsync(UpdateContentContext context, TraderForFilteringPart instance)
    {
        context.ContentItem.DisplayText = instance.Name + ";" + instance.UserId + ";" + instance.MoneyStatus + ";" + instance.Amount + ";" + instance.TotalFeeBTC + ";" + instance.TotalFeeETH + ";" + instance.TotalFeeUSDT + ";" + instance.TotalFeeVND + ";" + instance.VndBalance.ToString() + ";" + instance.BTCBalance.ToString() + ";" + instance.ETHBalance.ToString() + ";" + instance.USDT20Balance.ToString() + ";" + instance.BondVndBalance.ToString() + ";" + instance.DateTime.ToString();

        return Task.CompletedTask;
    }

    public override Task PublishedAsync(PublishContentContext context, TraderForFilteringPart instance)
    {
        context.ContentItem.DisplayText = instance.Name + ";" + instance.UserId + ";" + instance.MoneyStatus + ";" + instance.Amount + ";" + instance.TotalFeeBTC + ";" + instance.TotalFeeETH + ";" + instance.TotalFeeUSDT + ";" + instance.TotalFeeVND + ";" + instance.VndBalance.ToString() + ";" + instance.BTCBalance.ToString() + ";" + instance.ETHBalance.ToString() + ";" + instance.USDT20Balance.ToString() + ";" + instance.BondVndBalance.ToString() + ";" + instance.DateTime.ToString();

        return Task.CompletedTask;
    }
}
