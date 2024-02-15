using GraphQL.Types;
using OrchardCore.SongServices.ContentParts;

namespace OrchardCore.SongServices.GraphQL.Services.Pool;

// We add a model for the content part to the GraphQL schema. Content Types are added by Orchard Core automatically.
public class PoolPartObjectGraphType : ObjectGraphType<PoolPart>
{
    // These fields have counterparts in the index so we should include the same text in the
    // PoolPartWhereInputObjectGraphType without duplication.
    internal const string AmountDescription = "The Pool's amount.";
    internal const string CatDescription = "The Pool's cat.";
    internal const string TableDescription = "The Pool's table.";
    internal const string DateTimeDescription = "The Pool's date";

    public PoolPartObjectGraphType()
    {
        Field(part => part.Amount, nullable: true).Description(AmountDescription);
        Field(part => part.Cat, nullable: true).Description(CatDescription);
        Field(part => part.Table, nullable: true).Description(TableDescription);
        Field(part => part.DateTime, nullable: true).Description(DateTimeDescription);
    }
}
