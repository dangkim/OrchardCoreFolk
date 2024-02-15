using GraphQL.Types;
using OrchardCore.SongServices.ContentParts;

namespace OrchardCore.SongServices.GraphQL.Services.RoomBetInfo;

// We add a model for the content part to the GraphQL schema. Content Types are added by Orchard Core automatically.
public class RoomBetInfoPartObjectGraphType : ObjectGraphType<RoomBetInfoPart>
{
    // These fields have counterparts in the index so we should include the same text in the
    // RoomBetInfoPartWhereInputObjectGraphType without duplication.
    internal const string AmountsDescription = "The RoomBetInfo's amounts.";
    internal const string AvatarsDescription = "The RoomBetInfo's avatars.";
    internal const string CatsDescription = "The RoomBetInfo's cats.";
    internal const string IdsDescription = "The RoomBetInfo's ids.";
    internal const string NicknamesDescription = "The RoomBetInfo's nicknames.";
    internal const string RoomDescription = "The RoomBetInfo's room.";
    internal const string SeatsDescription = "The RoomBetInfo's seats.";
    internal const string TableDescription = "The RoomBetInfo's table.";
    internal const string DateTimeDescription = "The RoomBetInfo's date";

    public RoomBetInfoPartObjectGraphType()
    {
        Field(part => part.Amounts, nullable: true).Description(AmountsDescription);
        Field(part => part.Avatars, nullable: true).Description(AvatarsDescription);
        Field(part => part.Cats, nullable: true).Description(CatsDescription);
        Field(part => part.Ids, nullable: true).Description(IdsDescription);
        Field(part => part.Nicknames, nullable: true).Description(NicknamesDescription);
        Field(part => part.Room, nullable: true).Description(RoomDescription);
        Field(part => part.Seats, nullable: true).Description(SeatsDescription);
        Field(part => part.Table, nullable: true).Description(TableDescription);
        Field(part => part.DateTime, nullable: true).Description(DateTimeDescription);
    }
}
