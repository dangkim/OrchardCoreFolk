using GraphQL.Types;
using OrchardCore.SongServices.Indexes;
using OrchardCore.SongServices.ContentParts;
using OrchardCore.Apis.GraphQL.Queries;
using static OrchardCore.SongServices.GraphQL.Services.RoomBetInfo.RoomBetInfoPartObjectGraphType;

namespace OrchardCore.SongServices.GraphQL.Services.RoomBetInfo;

// This class adds a content part-specific section in the "where" filter. It relies on the content part index to
// automatically generate YesSql logic for database-side filtering.
public class RoomBetInfoPartWhereInputObjectGraphType : WhereInputObjectGraphType<RoomBetInfoPart>
{
    public RoomBetInfoPartWhereInputObjectGraphType()
    {        
        // Since filters depend on the index, we use their "nameof" as reference.
        AddScalarFilterFields<StringGraphType>(nameof(RoomBetInfoPartIndex.Amounts), AmountsDescription);
        AddScalarFilterFields<StringGraphType>(nameof(RoomBetInfoPartIndex.Avatars), AvatarsDescription);
        AddScalarFilterFields<StringGraphType>(nameof(RoomBetInfoPartIndex.Cats), CatsDescription);
        AddScalarFilterFields<StringGraphType>(nameof(RoomBetInfoPartIndex.Ids), IdsDescription);
        AddScalarFilterFields<StringGraphType>(nameof(RoomBetInfoPartIndex.Nicknames), NicknamesDescription);
        AddScalarFilterFields<StringGraphType>(nameof(RoomBetInfoPartIndex.Room), RoomDescription);
        AddScalarFilterFields<StringGraphType>(nameof(RoomBetInfoPartIndex.Seats), SeatsDescription);
        AddScalarFilterFields<StringGraphType>(nameof(RoomBetInfoPartIndex.Table), TableDescription);
        AddScalarFilterFields<StringGraphType>(nameof(RoomBetInfoPartIndex.DateTime), DateTimeDescription);
    }
}

// NEXT STATION: Services/RoomBetInfoPartIndexAliasProvider.cs
