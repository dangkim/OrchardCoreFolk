using GraphQL.Types;
using OrchardCore.SongServices.Indexes;
using OrchardCore.SongServices.ContentParts;
using OrchardCore.Apis.GraphQL.Queries;
using static OrchardCore.SongServices.GraphQL.Services.ShowLobby.ShowLobbyPartObjectGraphType;

namespace OrchardCore.SongServices.GraphQL.Services.ShowLobby;

// This class adds a content part-specific section in the "where" filter. It relies on the content part index to
// automatically generate YesSql logic for database-side filtering.
public class ShowLobbyPartWhereInputObjectGraphType : WhereInputObjectGraphType<ShowLobbyPart>
{
    public ShowLobbyPartWhereInputObjectGraphType()
    {        
        // Since filters depend on the index, we use their "nameof" as reference.
        AddScalarFilterFields<StringGraphType>(nameof(ShowLobbyPartIndex.DealerGameName), DealerGameNameDescription);
        AddScalarFilterFields<StringGraphType>(nameof(ShowLobbyPartIndex.Kind), KindDescription);
        AddScalarFilterFields<StringGraphType>(nameof(ShowLobbyPartIndex.DealerName), DealerNameDescription);
        AddScalarFilterFields<StringGraphType>(nameof(ShowLobbyPartIndex.DealerTableId), DealerTableIdDescription);
        AddScalarFilterFields<StringGraphType>(nameof(ShowLobbyPartIndex.GameName), GameNameDescription);
        AddScalarFilterFields<StringGraphType>(nameof(ShowLobbyPartIndex.Id), IdDescription);
        AddScalarFilterFields<StringGraphType>(nameof(ShowLobbyPartIndex.Status), StatusDescription);
        AddScalarFilterFields<StringGraphType>(nameof(ShowLobbyPartIndex.DisplayStatus), DisplayStatusDescription);
        AddScalarFilterFields<StringGraphType>(nameof(ShowLobbyPartIndex.Ticks), TicksDescription);
        AddScalarFilterFields<StringGraphType>(nameof(ShowLobbyPartIndex.BetAmount), BetAmountDescription);
        AddScalarFilterFields<StringGraphType>(nameof(ShowLobbyPartIndex.TotalPlayers), TotalPlayersDescription);
        AddScalarFilterFields<StringGraphType>(nameof(ShowLobbyPartIndex.AttendedPlayers), AttendedPlayersDescription);
        AddScalarFilterFields<StringGraphType>(nameof(ShowLobbyPartIndex.TableId), TableIdDescription);
        AddScalarFilterFields<StringGraphType>(nameof(ShowLobbyPartIndex.TableNo), TableNoDescription);
        AddScalarFilterFields<StringGraphType>(nameof(ShowLobbyPartIndex.DateTime), DateTimeDescription);
    }
}
