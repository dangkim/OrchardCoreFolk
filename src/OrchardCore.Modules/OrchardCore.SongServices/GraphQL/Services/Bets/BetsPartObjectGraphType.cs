using GraphQL.Types;
using OrchardCore.SongServices.ContentParts;

namespace OrchardCore.SongServices.GraphQL.Services.Bets;

// We add a model for the content part to the GraphQL schema. Content Types are added by Orchard Core automatically.
public class BetsPartObjectGraphType : ObjectGraphType<BetsPart>
{
    // These fields have counterparts in the index so we should include the same text in the
    // BetsPartWhereInputObjectGraphType without duplication.
    internal const string KindDescription = "The Bets's kind.";
    internal const string CodeDescription = "The Bets's code.";
    internal const string NameDescription = "The Bets's name.";
    internal const string ChipsDescription = "The Bets's chips.";
    internal const string IdDescription = "The Bets's id.";
    internal const string MaxDescription = "The Bets's max.";
    internal const string MinDescription = "The Bets's min.";
    internal const string AllowedDescription = "The Bets's allowed.";
    internal const string TableDescription = "The Bets's table.";
    internal const string DateTimeDescription = "The Bets's date";

    public BetsPartObjectGraphType()
    {
        Field(part => part.Kind, nullable: true).Description(KindDescription);
        Field(part => part.Code, nullable: true).Description(CodeDescription);
        Field(part => part.Name, nullable: true).Description(NameDescription);
        Field(part => part.Chips, nullable: true).Description(ChipsDescription);
        Field(part => part.Id, nullable: true).Description(IdDescription);
        Field(part => part.Max, nullable: true).Description(MaxDescription);
        Field(part => part.Min, nullable: true).Description(MinDescription);
        Field(part => part.Allowed, nullable: true).Description(AllowedDescription);
        Field(part => part.Table, nullable: true).Description(TableDescription);
        Field(part => part.DateTime, nullable: true).Description(DateTimeDescription);
    }
}
