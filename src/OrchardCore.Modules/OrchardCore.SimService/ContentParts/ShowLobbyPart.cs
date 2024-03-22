/*
 * Now let's see what practices Orchard Core provides when it stores data. Here you can see a content part. Each
 * content part can be attached to one or more content types. From the content type you can create content items (so
 * you kind of "instantiate" content types as content items). This is the most important part of the Orchard Core's
 * content management.
 *
 * Here is a PersonPart containing some properties of a person. We are planning to attach this content part to a Person
 * content type so when you create a Person content item it will have a PersonPart attached to it. You also need to
 * register this class with the service provider (see: Startup.cs).
 */

using OrchardCore.ContentFields.Fields;
using OrchardCore.ContentManagement;
using System;

namespace OrchardCore.SongServices.ContentParts;

public class ShowLobbyPart : ContentPart
{
    // A ContentPart is serialized as a JSON object so you need to keep this in mind when creating properties. For
    // further information check the Json.NET documentation: https://www.newtonsoft.com/json/help/html/Introduction.htm
    public string DealerGameName { get; set; }
    public string Kind { get; set; }
    public string DealerName { get; set; }
    public string DealerTableId { get; set; }
    public string GameName { get; set; }
    public string Id { get; set; }
    public string Status { get; set; }
    public string DisplayStatus { get; set; }
    public string Ticks { get; set; }

    /// <summary>
    /// ////////////////////////////////////////////
    ///  
    /// </summary>
    public decimal BetAmount { get; set; }
    public int TotalPlayers { get; set; }
    public int AttendedPlayers { get; set; }
    public int TableId { get; set; }
    public int TableNo { get; set; }

    public DateTime? DateTime { get; set; }
}

