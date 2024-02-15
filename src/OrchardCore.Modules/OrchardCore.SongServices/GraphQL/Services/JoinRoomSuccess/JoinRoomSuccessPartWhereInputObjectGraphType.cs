using GraphQL.Types;
using OrchardCore.SongServices.Indexes;
using OrchardCore.SongServices.ContentParts;
using OrchardCore.Apis.GraphQL.Queries;
using static OrchardCore.SongServices.GraphQL.Services.JoinRoomSuccess.JoinRoomSuccessPartObjectGraphType;

namespace OrchardCore.SongServices.GraphQL.Services.JoinRoomSuccess;

// This class adds a content part-specific section in the "where" filter. It relies on the content part index to
// automatically generate YesSql logic for database-side filtering.
public class JoinRoomSuccessPartWhereInputObjectGraphType : WhereInputObjectGraphType<JoinRoomSuccessPart>
{
    public JoinRoomSuccessPartWhereInputObjectGraphType()
    {        
        // Since filters depend on the index, we use their "nameof" as reference.
        AddScalarFilterFields<StringGraphType>(nameof(JoinRoomSuccessPartIndex.Room), RoomDescription);
        AddScalarFilterFields<StringGraphType>(nameof(JoinRoomSuccessPartIndex.Table), TableDescription);
        AddScalarFilterFields<StringGraphType>(nameof(JoinRoomSuccessPartIndex.Seat), SeatDescription);
        AddScalarFilterFields<StringGraphType>(nameof(JoinRoomSuccessPartIndex.DateTime), DateTimeDescription);
    }
}

// NEXT STATION: Services/JoinRoomSuccessPartIndexAliasProvider.cs
