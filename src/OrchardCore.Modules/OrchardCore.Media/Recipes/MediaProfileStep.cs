using System.Text.Json.Nodes;
using OrchardCore.Media.Models;
using OrchardCore.Media.Services;
using OrchardCore.Recipes.Models;
using OrchardCore.Recipes.Services;

namespace OrchardCore.Media.Recipes;

/// <summary>
/// This recipe step creates or updates a media profile.
/// </summary>
public sealed class MediaProfileStep : IRecipeStepHandler
{
    private readonly MediaProfilesManager _mediaProfilesManager;

    public MediaProfileStep(MediaProfilesManager mediaProfilesManager)
    {
        _mediaProfilesManager = mediaProfilesManager;
    }

    public async Task ExecuteAsync(RecipeExecutionContext context)
    {
        if (!string.Equals(context.Name, "MediaProfiles", StringComparison.OrdinalIgnoreCase))
        {
            return;
        }

        var model = context.Step.ToObject<MediaProfileStepModel>();

        foreach (var mediaProfile in model.MediaProfiles)
        {
            await _mediaProfilesManager.UpdateMediaProfileAsync(mediaProfile.Key, mediaProfile.Value);
        }
    }
}

public sealed class MediaProfileStepModel
{
    public Dictionary<string, MediaProfile> MediaProfiles { get; set; }
}
