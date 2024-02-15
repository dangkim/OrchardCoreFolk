using GraphQL.Types;
using OrchardCore.SongServices.ContentParts;

namespace OrchardCore.SongServices.GraphQL.Services.StartStopBetSucceed;

// We add a model for the content part to the GraphQL schema. Content Types are added by Orchard Core automatically.
public class StartStopBetSucceedPartObjectGraphType : ObjectGraphType<StartStopBetSucceedPart>
{
    // These fields have counterparts in the index so we should include the same text in the
    // StartStopBetSucceedPartWhereInputObjectGraphType without duplication.
    internal const string PowerupDescription = "The StartStopBetSucceed's powerup.";
    internal const string KindDescription = "The StartStopBetSucceed's kind.";
    internal const string ContentDescription = "The StartStopBetSucceed's content.";
    internal const string RoundDescription = "The StartStopBetSucceed's round.";
    internal const string RoundIdDescription = "The StartStopBetSucceed's roundId.";
    internal const string ShoeDescription = "The StartStopBetSucceed's shoe.";
    internal const string StatusDescription = "The StartStopBetSucceed's status.";
    internal const string TableDescription = "The StartStopBetSucceed's table.";
    internal const string DateTimeDescription = "The StartStopBetSucceed's date";

    public StartStopBetSucceedPartObjectGraphType()
    {
        Field(part => part.Powerup, nullable: true).Description(PowerupDescription);
        Field(part => part.Kind, nullable: true).Description(KindDescription);
        Field(part => part.Content, nullable: true).Description(ContentDescription);
        Field(part => part.Round, nullable: true).Description(RoundDescription);
        Field(part => part.RoundId, nullable: true).Description(RoundIdDescription);
        Field(part => part.Shoe, nullable: true).Description(ShoeDescription);
        Field(part => part.Table, nullable: true).Description(TableDescription);
        Field(part => part.Status, nullable: true).Description(StatusDescription);
        Field(part => part.DateTime, nullable: true).Description(DateTimeDescription);
    }
}
