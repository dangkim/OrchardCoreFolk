using GraphQL.Types;
using OrchardCore.SongServices.ContentParts;

namespace OrchardCore.SongServices.GraphQL.Services.Trader;

// We add a model for the content part to the GraphQL schema. Content Types are added by Orchard Core automatically.
public class TraderForFilteringPartObjectGraphType : ObjectGraphType<TraderForFilteringPart>
{
    // These fields have counterparts in the index so we should include the same text in the
    // TraderForFilteringPartWhereInputObjectGraphType without duplication.
    internal const string UserIdDescription = "Trader's Id.";
    internal const string ChatIdTeleDescription = "Trader's TeleId.";
    internal const string NameDescription = "Trader's Name.";
    internal const string BondVndBalanceDescription = "Trader's BondVndBalance.";
    internal const string VndBalanceDescription = "Trader's VndBalance.";
    internal const string BTCBalanceDescription = "Trader's BTCBalance.";
    internal const string ETHBalanceDescription = "Trader's ETHBalance.";
    internal const string USDT20BalanceDescription = "Trader's USDT20Balance.";
    internal const string ReferenceCodeDescription = "Trader's ReferenceCode.";
    internal const string IsActivatedTeleDescription = "Trader's IsActivatedTele.";
    internal const string DeviceIdDescription = "Trader's DeviceId.";
    internal const string DateTimeDescription = "Trader's Date";

    public TraderForFilteringPartObjectGraphType()
    {
        Field(part => part.UserId, nullable: true).Description(UserIdDescription);
        Field(part => part.ChatIdTele, nullable: true).Description(ChatIdTeleDescription);
        Field(part => part.Name, nullable: true).Description(NameDescription);
        Field(part => part.BondVndBalance, nullable: true).Description(BondVndBalanceDescription);
        Field(part => part.VndBalance, nullable: true).Description(VndBalanceDescription);
        Field(part => part.BTCBalance, nullable: true).Description(BTCBalanceDescription);
        Field(part => part.ETHBalance, nullable: true).Description(ETHBalanceDescription);
        Field(part => part.USDT20Balance, nullable: true).Description(USDT20BalanceDescription);
        Field(part => part.ReferenceCode, nullable: true).Description(ReferenceCodeDescription);
        Field(part => part.IsActivatedTele, nullable: true).Description(IsActivatedTeleDescription);
        Field(part => part.DeviceId, nullable: true).Description(DeviceIdDescription);
        Field(part => part.DateTime, nullable: true).Description(DateTimeDescription);
    }
}
