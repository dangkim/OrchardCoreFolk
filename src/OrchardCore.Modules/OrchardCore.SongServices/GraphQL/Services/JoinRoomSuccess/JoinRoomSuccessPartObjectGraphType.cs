using GraphQL.Types;
using OrchardCore.SongServices.ContentParts;

namespace OrchardCore.SongServices.GraphQL.Services.JoinRoomSuccess;

// We add a model for the content part to the GraphQL schema. Content Types are added by Orchard Core automatically.
public class JoinRoomSuccessPartObjectGraphType : ObjectGraphType<JoinRoomSuccessPart>
{
    // These fields have counterparts in the index so we should include the same text in the
    // JoinRoomSuccessPartWhereInputObjectGraphType without duplication.
    internal const string RoomDescription = "The JoinRoomSuccess's room.";
    internal const string SeatDescription = "The JoinRoomSuccess's seat.";
    internal const string TableDescription = "The JoinRoomSuccess's table.";
    internal const string DateTimeDescription = "The JoinRoomSuccess's date";

    public JoinRoomSuccessPartObjectGraphType()
    {
        Field(part => part.Room, nullable: true).Description(RoomDescription);
        Field(part => part.Seat, nullable: true).Description(SeatDescription);
        Field(part => part.Table, nullable: true).Description(TableDescription);
        Field(part => part.DateTime, nullable: true).Description(DateTimeDescription);
    }
}
