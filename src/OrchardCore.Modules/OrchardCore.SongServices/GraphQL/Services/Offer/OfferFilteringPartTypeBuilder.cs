using GraphQL.Types;
using OrchardCore.ContentManagement.GraphQL.Queries.Types;
using OrchardCore.ContentManagement.Metadata.Models;
using System.Linq;

namespace OrchardCore.SongServices.GraphQL.Services.Offer;
public class OfferFilteringPartTypeBuilder : IContentTypeBuilder
{
    public const string MinAmountFilter = "minAmountOffer";
    public const string MaxAmountFilter = "maxAmountOffer";
    public const string PaymentMethodFilter = "paymentMethodOffer";
    public const string WalletFilter = "walletOffer";
    public const string PreferredCurrencyFilter = "preferredCurrencyOffer";
    public const string OfferTypeFilter = "offerTypeOffer";
    public const string OfferStatusFilter = "offerStatusOffer";
    public const string PercentageFilter = "percentageOffer";

    public void Build(
        ISchema schema,
        FieldType contentQuery,
        ContentTypeDefinition contentTypeDefinition,
        ContentItemType contentItemType)
    {
        if (contentItemType.Name != ContentTypes.OfferPage) return;

        contentQuery.Arguments.Add(new QueryArgument<StringGraphType>
        {
            Name = OfferTypeFilter,
            ResolvedType = new StringGraphType(),
        });

        contentQuery.Arguments.Add(new QueryArgument<StringGraphType>
        {
            Name = OfferStatusFilter,
            ResolvedType = new StringGraphType(),
        });

        contentQuery.Arguments.Add(new QueryArgument<DecimalGraphType>
        {
            Name = MinAmountFilter,
            ResolvedType = new DecimalGraphType(),
        });

        contentQuery.Arguments.Add(new QueryArgument<DecimalGraphType>
        {
            Name = MaxAmountFilter,
            ResolvedType = new DecimalGraphType(),
        });

        contentQuery.Arguments.Add(new QueryArgument<StringGraphType>
        {
            Name = PaymentMethodFilter,
            ResolvedType = new StringGraphType(),
        });

        contentQuery.Arguments.Add(new QueryArgument<StringGraphType>
        {
            Name = WalletFilter,
            ResolvedType = new StringGraphType(),
        });

        contentQuery.Arguments.Add(new QueryArgument<StringGraphType>
        {
            Name = PreferredCurrencyFilter,
            ResolvedType = new StringGraphType(),
        });

        contentQuery.Arguments.Add(new QueryArgument<DecimalGraphType>
        {
            Name = PercentageFilter,
            ResolvedType = new DecimalGraphType(),
        });

        /*------------------------------------------------------------*/
        AddFilterOfferType(contentQuery, "_ne");
        AddFilterOfferStatus(contentQuery, "_ne");
        AddFilterPaymentMethod(contentQuery, "_ne");
        AddFilterWallet(contentQuery, "_ne");
        AddFilterPreferredCurrency(contentQuery, "_ne");

        /*------------------------------------------------------------*/
        AddFilterMinAmount(contentQuery, "_lt");
        AddFilterMinAmount(contentQuery, "_le");

        AddFilterMinAmount(contentQuery, "_ge");
        AddFilterMinAmount(contentQuery, "_gt");

        AddFilterMinAmount(contentQuery, "_ne");

        /*------------------------------------------------------------*/
        AddFilterMaxAmount(contentQuery, "_lt");
        AddFilterMaxAmount(contentQuery, "_le");

        AddFilterMaxAmount(contentQuery, "_ge");
        AddFilterMaxAmount(contentQuery, "_gt");

        AddFilterMaxAmount(contentQuery, "_ne");

        /*------------------------------------------------------------*/
        AddFilterPercentage(contentQuery, "_lt");
        AddFilterPercentage(contentQuery, "_le");

        AddFilterPercentage(contentQuery, "_ge");
        AddFilterPercentage(contentQuery, "_gt");

        AddFilterPercentage(contentQuery, "_ne");
    }

    private static void AddFilterOfferType(FieldType contentQuery, string suffix) =>
        contentQuery.Arguments.Add(new QueryArgument<StringGraphType>
        {
            Name = OfferTypeFilter + suffix,
            ResolvedType = new StringGraphType(),
        });

    private static void AddFilterOfferStatus(FieldType contentQuery, string suffix) =>
        contentQuery.Arguments.Add(new QueryArgument<StringGraphType>
        {
            Name = OfferStatusFilter + suffix,
            ResolvedType = new StringGraphType(),
        });

    private static void AddFilterMinAmount(FieldType contentQuery, string suffix) =>
        contentQuery.Arguments.Add(new QueryArgument<DecimalGraphType>
        {
            Name = MinAmountFilter + suffix,
            ResolvedType = new DecimalGraphType(),
        });

    private static void AddFilterMaxAmount(FieldType contentQuery, string suffix) =>
        contentQuery.Arguments.Add(new QueryArgument<DecimalGraphType>
        {
            Name = MaxAmountFilter + suffix,
            ResolvedType = new DecimalGraphType(),
        });

    private static void AddFilterPaymentMethod(FieldType contentQuery, string suffix) =>
        contentQuery.Arguments.Add(new QueryArgument<StringGraphType>
        {
            Name = PaymentMethodFilter + suffix,
            ResolvedType = new StringGraphType(),
        });

    private static void AddFilterWallet(FieldType contentQuery, string suffix) =>
        contentQuery.Arguments.Add(new QueryArgument<StringGraphType>
        {
            Name = WalletFilter + suffix,
            ResolvedType = new StringGraphType(),
        });

    private static void AddFilterPreferredCurrency(FieldType contentQuery, string suffix) =>
        contentQuery.Arguments.Add(new QueryArgument<StringGraphType>
        {
            Name = PreferredCurrencyFilter + suffix,
            ResolvedType = new StringGraphType(),
        });

    private static void AddFilterPercentage(FieldType contentQuery, string suffix) =>
        contentQuery.Arguments.Add(new QueryArgument<DecimalGraphType>
        {
            Name = PercentageFilter + suffix,
            ResolvedType = new DecimalGraphType(),
        });
}
