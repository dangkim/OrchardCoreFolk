using OrchardCore.SongServices.ContentParts;
using OrchardCore.SongServices.ViewModels;
using OrchardCore.ContentManagement.Display.ContentDisplay;
using OrchardCore.ContentManagement.Display.Models;
using OrchardCore.DisplayManagement.ModelBinding;
using OrchardCore.DisplayManagement.Views;
using System.Threading.Tasks;
using System;

namespace OrchardCore.SongServices.Drivers;

// Drivers inherited from ContentPartDisplayDrivers have a functionality similar to the one described in
// BookDisplayDriver but these are for ContentParts. Don't forget to register this class with the service provider (see:
// Startup.cs).
public class TradePartDisplayDriver : ContentPartDisplayDriver<TradeFilteringPart>
{
    public override IDisplayResult Display(TradeFilteringPart part, BuildPartDisplayContext context) =>
        Initialize<TradeFilteringPartViewModel>(GetDisplayShapeType(context), viewModel => PopulateViewModel(part, viewModel))
            .Location("Detail", "Content:1")
            .Location("Summary", "Content:1");

    // This is something that wasn't implemented in the BookDisplayDriver (but could've been). It will generate the
    // editor shape for the PersonPart.
    public override IDisplayResult Edit(TradeFilteringPart part, BuildPartEditorContext context) =>
        Initialize<TradeFilteringPartViewModel>(GetEditorShapeType(context), viewModel => PopulateViewModel(part, viewModel));

    // NEXT STATION: Startup.cs and find the static constructor.

    // So we had an Edit (or EditAsync) method that generates the editor shape. Now it's time to do the content
    // part-specific model binding and validation.
    public override async Task<IDisplayResult> UpdateAsync(TradeFilteringPart part, IUpdateModel updater, UpdatePartEditorContext context)
    {
        var viewModel = new TradeFilteringPartViewModel();

        await updater.TryUpdateModelAsync(viewModel, Prefix);

        part.TradeType = viewModel.TradeType;
        part.PaymentMethod = viewModel.PaymentMethod;
        part.FeeType = viewModel.FeeType;
        part.OfferType = viewModel.OfferType;
        part.OfferWallet = viewModel.OfferWallet;
        part.Duration = viewModel.Duration;
        part.SellerContentId = viewModel.SellerContentId;
        part.BuyerContentId = viewModel.BuyerContentId;
        part.CurrencyOfTrade = viewModel.CurrencyOfTrade;
        part.FeeVNDAmount = viewModel.FeeVNDAmount;
        part.FeeBTCAmount = viewModel.FeeBTCAmount;
        part.FeeETHAmount = viewModel.FeeETHAmount;
        part.FeeUSDT20Amount = viewModel.FeeUSDT20Amount;
        part.TradeVNDAmount = viewModel.TradeVNDAmount;
        part.TradeBTCAmount = viewModel.TradeBTCAmount;
        part.TradeUSDT20Amount = viewModel.TradeUSDT20Amount;
        part.TradeETHAmount = viewModel.TradeETHAmount;
        part.Seller = viewModel.Seller;
        part.Buyer = viewModel.Buyer;
        part.TradeStatus = viewModel.Status;
        part.OfferId = viewModel.OfferId;
        part.DateTime = DateTime.UtcNow;

        return await EditAsync(part, context);
    }

    private static void PopulateViewModel(TradeFilteringPart part, TradeFilteringPartViewModel viewModel)
    {
        viewModel.TradeFilteringPart = part;

        viewModel.TradeType = part.TradeType;
        viewModel.PaymentMethod = part.PaymentMethod;
        viewModel.FeeType = part.FeeType;
        viewModel.OfferType = part.OfferType;
        viewModel.OfferWallet = part.OfferWallet;
        viewModel.Duration = part.Duration;
        viewModel.SellerContentId = part.SellerContentId;
        viewModel.BuyerContentId = part.BuyerContentId;
        viewModel.CurrencyOfTrade = part.CurrencyOfTrade;
        viewModel.FeeVNDAmount = part.FeeVNDAmount;
        viewModel.FeeBTCAmount = part.FeeBTCAmount;
        viewModel.FeeETHAmount = part.FeeETHAmount;
        viewModel.FeeUSDT20Amount = part.FeeUSDT20Amount;
        viewModel.TradeVNDAmount = part.TradeVNDAmount;
        viewModel.TradeBTCAmount = part.TradeBTCAmount;
        viewModel.TradeUSDT20Amount = part.TradeUSDT20Amount;
        viewModel.TradeETHAmount = part.TradeETHAmount;
        viewModel.Seller = part.Seller;
        viewModel.Buyer = part.Buyer;
        viewModel.Status = part.TradeStatus;
        viewModel.OfferId = part.OfferId;
        viewModel.DateTime = part.DateTime;
    }
}

// NEXT STATION: Controllers/PersonListController and go back to the OlderThan30 method where we left.
