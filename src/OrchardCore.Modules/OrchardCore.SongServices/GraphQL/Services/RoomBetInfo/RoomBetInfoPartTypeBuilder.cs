using GraphQL.Types;
using OrchardCore.ContentManagement.GraphQL.Queries.Types;
using OrchardCore.ContentManagement.Metadata.Models;
using System.Linq;

namespace OrchardCore.SongServices.GraphQL.Services.RoomBetInfo;

// Services that implement IContentTypeBuilder extend the features of existing ContentItem type fields, including the
// top level fields automatically created by Orchard Core for every content type. You can use this to add new sub-fields
// or filter attributes to existing ContentItem type fields.
public class RoomBetInfoPartTypeBuilder : IContentTypeBuilder
{
    // It's a good practice to make the argument name a const because you will reuse it in the IGraphQLFilter.
    public const string AmountsFilter = "amounts";
    public const string AvatarsFilter = "avatars";
    public const string CatsFilter = "cats";
    public const string IdsFilter = "ids";
    public const string NicknamesFilter = "nicknames";
    public const string RoomFilter = "room";
    public const string SeatsFilter = "seats";
    public const string TableFilter = "table";
    public const string DateFilter = "date";

    // Here you can add arguments to every Content Type (top level) field.
    public void Build(
        ISchema schema,
        FieldType contentQuery,
        ContentTypeDefinition contentTypeDefinition,
        ContentItemType contentItemType)
    {
        // You can check to see if the field has any specific sub-field, if you want to rely on its features. For
        // example if you only want to apply to ContentItem fields that have the "person" sub-field (i.e. those that
        // have a PersonPart). This is useful if you want to expand your content part field in another module.
        if (contentItemType.Fields.All(field => field.Name != "RoomBetInfo")) return;

        contentQuery.Arguments.Add(new QueryArgument<StringGraphType>
        {
            Name = AmountsFilter,
            ResolvedType = new StringGraphType(),
        });

        contentQuery.Arguments.Add(new QueryArgument<StringGraphType>
        {
            Name = AvatarsFilter,
            ResolvedType = new StringGraphType(),
        });
        
        contentQuery.Arguments.Add(new QueryArgument<StringGraphType>
        {
            Name = CatsFilter,
            ResolvedType = new StringGraphType(),
        });
        
        contentQuery.Arguments.Add(new QueryArgument<StringGraphType>
        {
            Name = IdsFilter,
            ResolvedType = new StringGraphType(),
        });
        
        contentQuery.Arguments.Add(new QueryArgument<StringGraphType>
        {
            Name = NicknamesFilter,
            ResolvedType = new StringGraphType(),
        });
        
        contentQuery.Arguments.Add(new QueryArgument<StringGraphType>
        {
            Name = RoomFilter,
            ResolvedType = new StringGraphType(),
        });
        
        contentQuery.Arguments.Add(new QueryArgument<StringGraphType>
        {
            Name = SeatsFilter,
            ResolvedType = new StringGraphType(),
        });
        
        contentQuery.Arguments.Add(new QueryArgument<StringGraphType>
        {
            Name = TableFilter,
            ResolvedType = new StringGraphType(),
        });

        contentQuery.Arguments.Add(new QueryArgument<StringGraphType>
        {
            Name = DateFilter,
            ResolvedType = new StringGraphType(),
        });
    }
}

