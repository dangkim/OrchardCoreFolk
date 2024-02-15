using OrchardCore.SongServices.ContentParts;
using OrchardCore.SongServices.ViewModels;
using OrchardCore.ContentManagement.Display.ContentDisplay;
using OrchardCore.ContentManagement.Display.Models;
using OrchardCore.DisplayManagement.ModelBinding;
using OrchardCore.DisplayManagement.Views;
using System.Threading.Tasks;
using AngleSharp.Dom.Events;
using Irony.Parsing.Construction;
using OrchardCore.ContentManagement;
using OrchardCore.Modules;
using OrchardCore.SongServices.ApiModels;
using System.Runtime.CompilerServices;
using System;
using Telegram.Bot.Types;

namespace OrchardCore.SongServices.Drivers;

public class SpecificationPartDisplayDriver : ContentPartDisplayDriver<SpecificationPart>
{
    public override IDisplayResult Display(SpecificationPart part, BuildPartDisplayContext context) =>
        Initialize<SpecificationPartViewModel>(GetDisplayShapeType(context), viewModel => PopulateViewModel(part, viewModel))
            .Location("Detail", "Content:1")
            .Location("Summary", "Content:1");

    public override IDisplayResult Edit(SpecificationPart part, BuildPartEditorContext context) =>
        Initialize<SpecificationPartViewModel>(GetEditorShapeType(context), viewModel => PopulateViewModel(part, viewModel));

    public override async Task<IDisplayResult> UpdateAsync(SpecificationPart part, IUpdateModel updater, UpdatePartEditorContext context)
    {
        var viewModel = new SpecificationPartViewModel();

        await updater.TryUpdateModelAsync(viewModel, Prefix);

        part.Description = viewModel.Description;
        part.AssignerContentItemId = viewModel.AssignerContentItemId;
        part.AssigneeContentItemId = viewModel.AssigneeContentItemId;
        part.Supplement = viewModel.Supplement;
        part.RootCause = viewModel.RootCause;
        part.IsPlanned = viewModel.IsPlanned;
        part.IsIncident = viewModel.IsIncident;
        part.IsInHouse = viewModel.IsInHouse;
        part.IsOutsourced = viewModel.IsOutsourced;
        part.ReportStatus = viewModel.ReportStatus;
        part.OfferContentItemId = viewModel.OfferContentItemId;
        part.Behavior = viewModel.Behavior;
        part.Asset = viewModel.Asset;
        part.Event = viewModel.Event;
        part.Others = viewModel.Others;
        part.Sender = viewModel.Sender;
        part.Writer = viewModel.Writer;
        part.Photos = viewModel.Photos;
        part.Clips = viewModel.Clips;
        part.Audio = viewModel.Audio;
        part.Files = viewModel.Files;
        part.LocationContentItemId = viewModel.LocationContentItemId;

        part.DateTime = DateTime.UtcNow;

        return await EditAsync(part, context);
    }

    private static void PopulateViewModel(SpecificationPart part, SpecificationPartViewModel viewModel)
    {
        viewModel.SpecificationPart = part;

        viewModel.Description = part.Description;
        viewModel.AssignerContentItemId = part.AssignerContentItemId;
        viewModel.AssigneeContentItemId = part.AssigneeContentItemId;
        viewModel.Supplement = part.Supplement;
        viewModel.RootCause = part.RootCause;
        viewModel.IsPlanned = part.IsPlanned;
        viewModel.IsIncident = part.IsIncident;
        viewModel.IsInHouse = part.IsInHouse;
        viewModel.IsOutsourced = part.IsOutsourced;
        viewModel.ReportStatus = part.ReportStatus;
        viewModel.OfferContentItemId = part.OfferContentItemId;
        viewModel.Behavior = part.Behavior;
        viewModel.Asset = part.Asset;
        viewModel.Event = part.Event;
        viewModel.Others = part.Others;
        viewModel.Sender = part.Sender;
        viewModel.Writer = part.Writer;
        viewModel.Photos = part.Photos;
        viewModel.Clips = part.Clips;
        viewModel.Audio = part.Audio;
        viewModel.Files = part.Files;
        viewModel.LocationContentItemId = part.LocationContentItemId;

        viewModel.DateTime = part.DateTime;
    }
}
