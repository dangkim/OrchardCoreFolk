using Lombiq.HelpfulLibraries.OrchardCore.GraphQL;
using OrchardCore.SongServices.Indexes;

namespace OrchardCore.SongServices.GraphQL.Services;

// If your content part's index ends with PartIndex (as it should) then you can use this base class from our Helpful
// Libraries project to eliminate boilerplate.
public class PoolPartIndexAliasProvider : PartIndexAliasProvider<PoolPartIndex> { }

// NEXT STATION: Services/ContentItemTypeBuilder.cs
