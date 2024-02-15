using GraphQL.Types;
using OrchardCore.SongServices.ContentParts;

namespace OrchardCore.SongServices.GraphQL.Services.Ticks;

// We add a model for the content part to the GraphQL schema. Content Types are added by Orchard Core automatically.
public class TicksPartObjectGraphType : ObjectGraphType<TicksPart>
{
    // These fields have counterparts in the index so we should include the same text in the
    // TicksPartWhereInputObjectGraphType without duplication.
    internal const string TableDescription = "The Ticks's table.";
    internal const string ValueDescription = "The Ticks's value.";
    internal const string DateTimeDescription = "The Ticks's date";

    public TicksPartObjectGraphType()
    {
        Field(part => part.Table, nullable: true).Description(TableDescription);
        Field(part => part.Value, nullable: true).Description(ValueDescription);
        Field(part => part.DateTime, nullable: true).Description(DateTimeDescription);
    }
}
