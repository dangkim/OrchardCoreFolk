using OrchardCore.SongServices.ContentParts;
using OrchardCore.ContentManagement;
using System;
using YesSql.Indexes;

namespace OrchardCore.SongServices.Indexes;

public class ShowLobbyPartIndex : MapIndex
{
    public string ContentItemId { get; set; }
    public string DealerGameName { get; set; }
    public string Kind { get; set; }
    public string DealerName { get; set; }
    public string DealerTableId { get; set; }
    public string GameName { get; set; }
    public string Status { get; set; }
    public string DisplayStatus { get; set; }
    public string Ticks { get; set; }
    public decimal BetAmount { get; set; }
    public int TotalPlayers { get; set; }
    public int AttendedPlayers { get; set; }
    public int TableId { get; set; }
    public int TableNo { get; set; }
    public DateTime? DateTime { get; set; }
}

// Don't forget to register this class with the service provider (see: Startup.cs).
public class ShowLobbyPartIndexProvider : IndexProvider<ContentItem>
{
    public override void Describe(DescribeContext<ContentItem> context) =>
        context.For<ShowLobbyPartIndex>()
            .When(contentItem => contentItem.Has<ShowLobbyPart>())
            .Map(contentItem =>
            {
                var showLobbyPart = contentItem.As<ShowLobbyPart>();

                return new ShowLobbyPartIndex
                {
                    ContentItemId = contentItem.ContentItemId,
                    DealerGameName = showLobbyPart.DealerGameName,
                    Kind = showLobbyPart.Kind,
                    DealerName = showLobbyPart.DealerName,
                    DealerTableId = showLobbyPart.DealerTableId,
                    GameName = showLobbyPart.GameName,
                    Status = showLobbyPart.Status,
                    DisplayStatus = showLobbyPart.DisplayStatus,
                    Ticks = showLobbyPart.Ticks,
                    BetAmount = showLobbyPart.BetAmount,
                    TotalPlayers = showLobbyPart.TotalPlayers,
                    AttendedPlayers = showLobbyPart.AttendedPlayers,
                    TableId = showLobbyPart.TableId,
                    TableNo = showLobbyPart.TableNo,
                    DateTime = showLobbyPart.DateTime,
                };
            });
}
