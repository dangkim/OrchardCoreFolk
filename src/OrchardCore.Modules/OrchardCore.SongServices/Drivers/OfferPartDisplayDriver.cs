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
public class OfferPartDisplayDriver : ContentPartDisplayDriver<OfferFilteringPart>
{
    public override IDisplayResult Display(OfferFilteringPart part, BuildPartDisplayContext context) =>
        Initialize<OfferFilteringPartViewModel>(GetDisplayShapeType(context), viewModel => PopulateViewModel(part, viewModel))
            .Location("Detail", "Content:1")
            .Location("Summary", "Content:1");

    public override IDisplayResult Edit(OfferFilteringPart part, BuildPartEditorContext context) =>
        Initialize<OfferFilteringPartViewModel>(GetEditorShapeType(context), viewModel => PopulateViewModel(part, viewModel));

    public override async Task<IDisplayResult> UpdateAsync(OfferFilteringPart part, IUpdateModel updater, UpdatePartEditorContext context)
    {
        var viewModel = new OfferFilteringPartViewModel();

        await updater.TryUpdateModelAsync(viewModel, Prefix);

        part.MinAmount = viewModel.MinAmount;
        part.MaxAmount = viewModel.MaxAmount;
        part.OfferStatus = viewModel.OfferStatus;
        part.Wallet = viewModel.Wallet;
        part.PaymentMethod = viewModel.PaymentMethod;
        part.PreferredCurrency = viewModel.PreferredCurrency;
        part.Percentage = viewModel.Percentage;
        part.OfferType = viewModel.OfferType;
        part.OfferLabel = viewModel.OfferType;
        part.OfferTerms = viewModel.OfferTerms;
        part.TradeInstructions = viewModel.TradeInstructions;
        part.OfferPrice = viewModel.OfferPrice;
        part.OfferGet = viewModel.OfferGet;
        part.CurrentRate = viewModel.CurrentRate;
        part.EscrowFee = viewModel.EscrowFee;
        part.DateTime = DateTime.UtcNow;

        return await EditAsync(part, context);
    }

    private static void PopulateViewModel(OfferFilteringPart part, OfferFilteringPartViewModel viewModel)
    {
        viewModel.OfferFilteringPart = part;

        viewModel.MinAmount = part.MinAmount;
        viewModel.MaxAmount = part.MaxAmount;
        viewModel.OfferStatus = part.OfferStatus;
        viewModel.Wallet = part.Wallet;
        viewModel.PaymentMethod = part.PaymentMethod;
        viewModel.PreferredCurrency = part.PreferredCurrency;
        viewModel.Percentage = part.Percentage;
        viewModel.OfferType = part.OfferType;
        viewModel.OfferLabel = part.OfferType;
        viewModel.OfferTerms = part.OfferTerms;
        viewModel.TradeInstructions = part.TradeInstructions;
        viewModel.OfferPrice = part.OfferPrice;
        viewModel.OfferGet = part.OfferGet;
        viewModel.CurrentRate = part.CurrentRate;
        viewModel.EscrowFee = part.EscrowFee;
        viewModel.DateTime = part.DateTime;
    }
}
