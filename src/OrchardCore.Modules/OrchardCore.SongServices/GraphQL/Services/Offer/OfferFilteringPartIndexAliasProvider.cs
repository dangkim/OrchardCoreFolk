using Lombiq.HelpfulLibraries.OrchardCore.GraphQL;
using OrchardCore.SongServices.Indexes;
using OrchardCore.SongServices.Indexing;

namespace OrchardCore.SongServices.GraphQL.Services.Offer;

// If your content part's index ends with PartIndex (as it should) then you can use this base class from our Helpful
// Libraries project to eliminate boilerplate.
public class OfferFilteringPartIndexAliasProvider : PartIndexAliasProvider<OfferFilteringPartIndex> { }

// NEXT STATION: Services/ContentItemTypeBuilder.cs
