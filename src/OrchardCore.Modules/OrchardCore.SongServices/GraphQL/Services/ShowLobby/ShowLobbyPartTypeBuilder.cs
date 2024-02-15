using GraphQL.Types;
using OrchardCore.ContentManagement.GraphQL.Queries.Types;
using OrchardCore.ContentManagement.Metadata.Models;
using System.Linq;

namespace OrchardCore.SongServices.GraphQL.Services.ShowLobby;

// Services that implement IContentTypeBuilder extend the features of existing ContentItem type fields, including the
// top level fields automatically created by Orchard Core for every content type. You can use this to add new sub-fields
// or filter attributes to existing ContentItem type fields.
public class ShowLobbyPartTypeBuilder : IContentTypeBuilder
{
    // It's a good practice to make the argument name a const because you will reuse it in the IGraphQLFilter.
    public const string DealerGameNameFilter = "dealerGameName";
    public const string KindFilter = "kind";
    public const string DealerNameFilter = "dealerName";
    public const string DealerTableIdFilter = "dealerTableId";
    public const string GameNameFilter = "gameName";
    public const string IdFilter = "id";
    public const string StatusFilter = "lobbystatus";
    public const string DisplayStatusFilter = "displayStatus";
    public const string TicksFilter = "ticks";
    public const string BetAmountFilter = "betAmount";
    public const string TotalPlayersFilter = "totalPlayers";
    public const string AttendedPlayersFilter = "attendedPlayers";
    public const string TableIdFilter = "tableId";
    public const string TableNoFilter = "tableNo";
    public const string DateFilter = "date";

    // Here you can add arguments to every Content Type (top level) field.
    public void Build(
        FieldType contentQuery,
        ContentTypeDefinition contentTypeDefinition,
        ContentItemType contentItemType)
    {
        if (contentItemType.Fields.All(field => field.Name != "ShowLobby")) return;

        contentQuery.Arguments.Add(new QueryArgument<StringGraphType>
        {
            Name = DealerGameNameFilter,
            ResolvedType = new StringGraphType(),
        });

        contentQuery.Arguments.Add(new QueryArgument<StringGraphType>
        {
            Name = KindFilter,
            ResolvedType = new StringGraphType(),
        });
        
        contentQuery.Arguments.Add(new QueryArgument<StringGraphType>
        {
            Name = DealerNameFilter,
            ResolvedType = new StringGraphType(),
        });
        
        contentQuery.Arguments.Add(new QueryArgument<StringGraphType>
        {
            Name = DealerTableIdFilter,
            ResolvedType = new StringGraphType(),
        });
        
        contentQuery.Arguments.Add(new QueryArgument<StringGraphType>
        {
            Name = GameNameFilter,
            ResolvedType = new StringGraphType(),
        });
        
        contentQuery.Arguments.Add(new QueryArgument<StringGraphType>
        {
            Name = IdFilter,
            ResolvedType = new StringGraphType(),
        });
        
        contentQuery.Arguments.Add(new QueryArgument<StringGraphType>
        {
            Name = StatusFilter,
            ResolvedType = new StringGraphType(),
        });
        
        contentQuery.Arguments.Add(new QueryArgument<StringGraphType>
        {
            Name = DisplayStatusFilter,
            ResolvedType = new StringGraphType(),
        });
        
        contentQuery.Arguments.Add(new QueryArgument<StringGraphType>
        {
            Name = TicksFilter,
            ResolvedType = new StringGraphType(),
        });
        
        contentQuery.Arguments.Add(new QueryArgument<StringGraphType>
        {
            Name = DateFilter,
            ResolvedType = new StringGraphType(),
        });
    }
}

