using OrchardCore.Modules.Manifest;

[assembly: Module(
    Name = "Virtual Sim Services",
    Author = "Kevin",
    Website = ManifestConstants.OrchardCoreWebsite,
    Version = "0.0.1",
    Description = "Virtual Sim services",
    Category = "Virtual Sim",
    Dependencies = new[]
    {
        "OrchardCore.Content",
        "OrchardCore.ContentTypes",
        "OrchardCore.ContentFields",
    }
)]
