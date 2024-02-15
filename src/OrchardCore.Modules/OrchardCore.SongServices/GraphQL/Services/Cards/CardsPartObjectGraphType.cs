using GraphQL.Types;
using OrchardCore.SongServices.ContentParts;

namespace OrchardCore.SongServices.GraphQL.Services.Cards;

// We add a model for the content part to the GraphQL schema. Content Types are added by Orchard Core automatically.
public class CardsPartObjectGraphType : ObjectGraphType<CardsPart>
{
    // These fields have counterparts in the index so we should include the same text in the
    // CardsPartWhereInputObjectGraphType without duplication.
    internal const string KindDescription = "The Cards's kind.";
    internal const string CardDescription = "The Cards's card.";
    internal const string PosDescription = "The Cards's pos.";
    internal const string TableDescription = "The Cards's table.";
    internal const string DateTimeDescription = "The Cards's date";

    public CardsPartObjectGraphType()
    {
        Field(part => part.Kind, nullable: true).Description(KindDescription);
        Field(part => part.Card, nullable: true).Description(CardDescription);
        Field(part => part.Pos, nullable: true).Description(PosDescription);
        Field(part => part.Table, nullable: true).Description(TableDescription);
        Field(part => part.DateTime, nullable: true).Description(DateTimeDescription);
    }
}
