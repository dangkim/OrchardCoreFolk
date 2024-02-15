using GraphQL.Types;
using OrchardCore.SongServices.ContentParts;

namespace OrchardCore.SongServices.GraphQL.Services.ShowLobby;

// We add a model for the content part to the GraphQL schema. Content Types are added by Orchard Core automatically.
public class ShowLobbyPartObjectGraphType : ObjectGraphType<ShowLobbyPart>
{
    // These fields have counterparts in the index so we should include the same text in the
    // ShowLobbyPartWhereInputObjectGraphType without duplication.
    internal const string DealerGameNameDescription = "The ShowLobby's dealer game name.";
    internal const string KindDescription = "The ShowLobby's kind.";
    internal const string DealerNameDescription = "The ShowLobby's dealerName.";
    internal const string DealerTableIdDescription = "The ShowLobby's dealer table id.";
    internal const string GameNameDescription = "The ShowLobby's game name.";
    internal const string IdDescription = "The ShowLobby's id.";
    internal const string StatusDescription = "The ShowLobby's status.";
    internal const string DisplayStatusDescription = "The ShowLobby's display status.";
    internal const string TicksDescription = "The ShowLobby's ticks.";
    internal const string BetAmountDescription = "The ShowLobby's bet amount.";
    internal const string TotalPlayersDescription = "The ShowLobby's total players.";
    internal const string AttendedPlayersDescription = "The ShowLobby's attended players.";
    internal const string TableIdDescription = "The ShowLobby's tableId.";
    internal const string TableNoDescription = "The ShowLobby's tableNo.";
    internal const string DateTimeDescription = "The ShowLobby's date";

    public ShowLobbyPartObjectGraphType()
    {
        Field(part => part.DealerGameName, nullable: true).Description(DealerGameNameDescription);
        Field(part => part.Kind, nullable: true).Description(KindDescription);
        Field(part => part.DealerName, nullable: true).Description(DealerNameDescription);
        Field(part => part.DealerTableId, nullable: true).Description(DealerTableIdDescription);
        Field(part => part.GameName, nullable: true).Description(GameNameDescription);
        Field(part => part.Id, nullable: true).Description(IdDescription);
        Field(part => part.Status, nullable: true).Description(StatusDescription);
        Field(part => part.DisplayStatus, nullable: true).Description(DisplayStatusDescription);
        Field(part => part.Ticks, nullable: true).Description(TicksDescription);
        Field(part => part.BetAmount, nullable: true).Description(BetAmountDescription);
        Field(part => part.TotalPlayers, nullable: true).Description(TotalPlayersDescription);
        Field(part => part.AttendedPlayers, nullable: true).Description(AttendedPlayersDescription);
        Field(part => part.TableId, nullable: true).Description(TableIdDescription);
        Field(part => part.TableNo, nullable: true).Description(TableNoDescription);
        Field(part => part.DateTime, nullable: true).Description(DateTimeDescription);
    }
}
