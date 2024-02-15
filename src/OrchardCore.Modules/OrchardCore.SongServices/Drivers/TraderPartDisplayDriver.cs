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
public class TraderPartDisplayDriver : ContentPartDisplayDriver<TraderForFilteringPart>
{
    public override IDisplayResult Display(TraderForFilteringPart part, BuildPartDisplayContext context) =>
        Initialize<TraderForFilteringPartViewModel>(GetDisplayShapeType(context), viewModel => PopulateViewModel(part, viewModel))
            .Location("Detail", "Content:1")
            .Location("Summary", "Content:1");

    // This is something that wasn't implemented in the BookDisplayDriver (but could've been). It will generate the
    // editor shape for the PersonPart.
    public override IDisplayResult Edit(TraderForFilteringPart part, BuildPartEditorContext context) =>
        Initialize<TraderForFilteringPartViewModel>(GetEditorShapeType(context), viewModel => PopulateViewModel(part, viewModel));

    // NEXT STATION: Startup.cs and find the static constructor.

    // So we had an Edit (or EditAsync) method that generates the editor shape. Now it's time to do the content
    // part-specific model binding and validation.
    public override async Task<IDisplayResult> UpdateAsync(TraderForFilteringPart part, IUpdateModel updater, UpdatePartEditorContext context)
    {
        var viewModel = new TraderForFilteringPartViewModel();

        await updater.TryUpdateModelAsync(viewModel, Prefix);

        part.Name = viewModel.Name;
        part.IsActivatedTele = viewModel.IsActivatedTele;
        part.BondVndBalance = viewModel.BondVndBalance;
        part.VndBalance = viewModel.VndBalance;
        part.BTCBalance = viewModel.BTCBalance;
        part.ETHBalance = viewModel.ETHBalance;
        part.USDT20Balance = viewModel.USDT20Balance;
        part.WithdrawVNDStatus = viewModel.WithdrawVNDStatus;
        part.ReferenceCode = viewModel.ReferenceCode;
        part.DateSend = viewModel.DateSend;
        part.UserId = viewModel.UserId;
        part.Email = viewModel.Email;
        part.PhoneNumber = viewModel.PhoneNumber;
        part.BankAccounts = viewModel.BankAccounts;
        part.ChatIdTele = viewModel.ChatIdTele;
        part.DeviceId = viewModel.DeviceId;
        part.MoneyStatus = viewModel.MoneyStatus;
        part.Amount = viewModel.Amount;
        part.TotalFeeBTC = viewModel.TotalFeeBTC;
        part.TotalFeeETH = viewModel.TotalFeeETH;
        part.TotalFeeUSDT = viewModel.TotalFeeUSDT;
        part.TotalFeeVND = viewModel.TotalFeeVND;
        part.BookmarkOffers = viewModel.BookmarkOffers;
        part.DateTime = DateTime.UtcNow;

        return await EditAsync(part, context);
    }

    private static void PopulateViewModel(TraderForFilteringPart part, TraderForFilteringPartViewModel viewModel)
    {
        viewModel.TraderForFilteringPart = part;

        viewModel.Name = part.Name;
        viewModel.IsActivatedTele = part.IsActivatedTele;
        viewModel.BondVndBalance = part.BondVndBalance;
        viewModel.VndBalance = part.VndBalance;
        viewModel.BTCBalance = part.BTCBalance;
        viewModel.ETHBalance = part.ETHBalance;
        viewModel.USDT20Balance = part.USDT20Balance;
        viewModel.WithdrawVNDStatus = part.WithdrawVNDStatus;
        viewModel.ReferenceCode = part.ReferenceCode;
        viewModel.DateSend = part.DateSend;
        viewModel.UserId = part.UserId;
        viewModel.Email = part.Email;
        viewModel.PhoneNumber = part.PhoneNumber;
        viewModel.BankAccounts = part.BankAccounts;
        viewModel.ChatIdTele = part.ChatIdTele;
        viewModel.DeviceId = part.DeviceId;
        viewModel.MoneyStatus = part.MoneyStatus;
        viewModel.Amount = part.Amount;
        viewModel.TotalFeeBTC = part.TotalFeeBTC;
        viewModel.TotalFeeETH = part.TotalFeeETH;
        viewModel.TotalFeeUSDT = part.TotalFeeUSDT;
        viewModel.TotalFeeVND = part.TotalFeeVND;
        viewModel.BookmarkOffers = part.BookmarkOffers;
        viewModel.DateTime = part.DateTime;
    }
}

// NEXT STATION: Controllers/PersonListController and go back to the OlderThan30 method where we left.
