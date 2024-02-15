using OrchardCore.Modules.Manifest;

[assembly: Module(
    Name = "SongServices",
    Author = "Kevin",
    Website = ManifestConstants.OrchardCoreWebsite,
    Description = "Provides Song's services",
    Version = "0.0.1",
    Category = "Song",
    Dependencies = new[]
    {
        "OrchardCore.Content",
        "OrchardCore.ContentTypes",
        "OrchardCore.ContentFields",
    }
)]

//[assembly: Feature(
//    Id = "OrchardCore.SongServices",
//    Name = "SongServices",
//    Description = "Provides Song's services",
//    Category = "Song",
//    Dependencies = new[]
//    {
//        "OrchardCore.Content",
//        "OrchardCore.ContentTypes",
//        "OrchardCore.ContentFields",
//    }
//)]
