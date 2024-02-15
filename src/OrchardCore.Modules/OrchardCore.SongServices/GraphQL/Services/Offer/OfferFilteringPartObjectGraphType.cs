using GraphQL.Types;
using OrchardCore.SongServices.ContentParts;

namespace OrchardCore.SongServices.GraphQL.Services.Offer;

// We add a model for the content part to the GraphQL schema. Content Types are added by Orchard Core automatically.
public class OfferFilteringPartObjectGraphType : ObjectGraphType<OfferFilteringPart>
{
    // These fields have counterparts in the index so we should include the same text in the
    // OfferFilteringPartWhereInputObjectGraphType without duplication.
    internal const string MinAmountDescription = "Offer's MinAmount.";
    internal const string MaxAmountDescription = "Offer's MaxAmount.";
    internal const string OfferStatusDescription = "Offer's Status.";
    internal const string WalletDescription = "Offer's Wallet.";
    internal const string PaymentMethodDescription = "Offer's PaymentMethod.";
    internal const string PreferredCurrencyDescription = "Offer's PreferredCurrency.";
    internal const string PercentageDescription = "Offer's Percentage.";
    internal const string OfferTypeDescription = "Offer's OfferType.";
    internal const string OfferLabelDescription = "Offer's OfferLabel.";
    internal const string OfferTermsDescription = "Offer's OfferTerms.";
    internal const string TradeInstructionsDescription = "Offer's TradeInstructions.";
    internal const string EscrowFeeDescription = "Offer's EscrowFee.";
    internal const string CurrentRateDescription = "Offer's CurrentRate.";
    //internal const string DateTimeDescription = "Offer's Date";

    public OfferFilteringPartObjectGraphType()
    {
        Field(part => part.MinAmount, nullable: true).Description(MinAmountDescription);
        Field(part => part.MaxAmount, nullable: true).Description(MaxAmountDescription);
        Field(part => part.OfferStatus, nullable: true).Description(OfferStatusDescription);
        Field(part => part.Wallet, nullable: true).Description(WalletDescription);
        Field(part => part.PaymentMethod, nullable: true).Description(PaymentMethodDescription);
        Field(part => part.PreferredCurrency, nullable: true).Description(PreferredCurrencyDescription);
        Field(part => part.Percentage, nullable: true).Description(PercentageDescription);
        Field(part => part.OfferType, nullable: true).Description(OfferTypeDescription);
        Field(part => part.OfferTerms, nullable: true).Description(OfferTermsDescription);
        Field(part => part.TradeInstructions, nullable: true).Description(TradeInstructionsDescription);
        Field(part => part.OfferLabel, nullable: true).Description(OfferLabelDescription);
        Field(part => part.EscrowFee, nullable: true).Description(EscrowFeeDescription);
        Field(part => part.CurrentRate, nullable: true).Description(CurrentRateDescription);
        //Field(part => part.DateTime, nullable: true).Description(DateTimeDescription);
    }
}
