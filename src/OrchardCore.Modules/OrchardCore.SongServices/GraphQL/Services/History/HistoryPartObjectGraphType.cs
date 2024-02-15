using GraphQL.Types;
using OrchardCore.SongServices.ContentParts;

namespace OrchardCore.SongServices.GraphQL.Services.History;

// We add a model for the content part to the GraphQL schema. Content Types are added by Orchard Core automatically.
public class HistoryPartObjectGraphType : ObjectGraphType<HistoryPart>
{
    // These fields have counterparts in the index so we should include the same text in the
    // HistoryPartWhereInputObjectGraphType without duplication.
    internal const string CardsDescription = "The History's cards.";
    internal const string PowerupDescription = "The History's powerup.";
    internal const string StatusDescription = "The History's status.";
    internal const string KindDescription = "The History's kind.";
    internal const string ResultDescription = "The History's result.";
    internal const string RoundDescription = "The History's round.";
    internal const string RoundIdDescription = "The History's roundId.";
    internal const string ShoeDescription = "The History's shoe.";
    internal const string TableDescription = "The History's table.";
    internal const string DateTimeDescription = "The History's date";

    public HistoryPartObjectGraphType()
    {
        Field(part => part.Cards, nullable: true).Description(CardsDescription);
        Field(part => part.Powerup, nullable: true).Description(PowerupDescription);
        Field(part => part.Status, nullable: true).Description(StatusDescription);
        Field(part => part.Kind, nullable: true).Description(KindDescription);
        Field(part => part.Result, nullable: true).Description(ResultDescription);
        Field(part => part.Round, nullable: true).Description(RoundDescription);
        Field(part => part.RoundId, nullable: true).Description(RoundIdDescription);
        Field(part => part.Shoe, nullable: true).Description(ShoeDescription);
        Field(part => part.Table, nullable: true).Description(TableDescription);
        Field(part => part.DateTime, nullable: true).Description(DateTimeDescription);
    }
}
