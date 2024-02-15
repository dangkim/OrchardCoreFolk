using GraphQL.Types;
using OrchardCore.SongServices.ContentParts;

namespace OrchardCore.SongServices.GraphQL.Services.Dealer;

// We add a model for the content part to the GraphQL schema. Content Types are added by Orchard Core automatically.
public class DealerPartObjectGraphType : ObjectGraphType<DealerPart>
{
    // These fields have counterparts in the index so we should include the same text in the
    // DealerPartWhereInputObjectGraphType without duplication.
    internal const string NameDescription = "The Dealer's name.";
    internal const string TableDescription = "The Dealer's table.";
    internal const string ValueDescription = "The Dealer's value.";
    internal const string DateTimeDescription = "The Dealer's date";

    public DealerPartObjectGraphType()
    {
        Field(part => part.Name, nullable: true).Description(NameDescription);
        Field(part => part.Table, nullable: true).Description(TableDescription);
        Field(part => part.Value, nullable: true).Description(ValueDescription);
        Field(part => part.DateTime, nullable: true).Description(DateTimeDescription);
    }
}
