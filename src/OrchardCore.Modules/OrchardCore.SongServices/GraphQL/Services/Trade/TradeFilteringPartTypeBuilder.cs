using GraphQL.Types;
using OrchardCore.ContentManagement.GraphQL.Queries.Types;
using OrchardCore.ContentManagement.Metadata.Models;
using System.Linq;

namespace OrchardCore.SongServices.GraphQL.Services.Trade;
public class TradeFilteringPartTypeBuilder : IContentTypeBuilder
{
    public const string TradeTypeFilter = "tradeType";
    public const string PaymentMethodFilter = "paymentMethod";
    public const string FeeTypeFilter = "feeType";
    public const string OfferTypeFilter = "offerType";
    public const string OfferWalletFilter = "offerWallet";
    public const string DurationFilter = "duration";
    public const string SellerContentIdFilter = "sellerContentId";
    public const string BuyerContentIdFilter = "buyerContentId";
    public const string CurrencyOfTradeFilter = "currencyOfTrade";
    public const string FeeVNDAmountFilter = "feeVNDAmount";
    public const string FeeBTCAmountFilter = "feeBTCAmount";
    public const string FeeETHAmountFilter = "feeETHAmount";
    public const string FeeUSDT20AmountFilter = "feeUSDT20Amount";
    public const string TradeVNDAmountFilter = "tradeVNDAmount";
    public const string TradeBTCAmountFilter = "tradeBTCAmount";
    public const string TradeUSDT20AmountFilter = "tradeUSDT20Amount";
    public const string TradeETHAmountFilter = "tradeETHAmount";
    public const string SellerFilter = "seller";
    public const string BuyerFilter = "buyer";
    public const string TradeStatusFilter = "tradeStatus";
    public const string OfferIdFilter = "offerId";

    public void Build(
        FieldType contentQuery,
        ContentTypeDefinition contentTypeDefinition,
        ContentItemType contentItemType)
    {
        if (contentItemType.Name != ContentTypes.TradePage) return;

        contentQuery.Arguments.Add(new QueryArgument<StringGraphType>
        {
            Name = TradeTypeFilter,
            ResolvedType = new StringGraphType(),
        });

        contentQuery.Arguments.Add(new QueryArgument<StringGraphType>
        {
            Name = PaymentMethodFilter,
            ResolvedType = new StringGraphType(),
        });

        contentQuery.Arguments.Add(new QueryArgument<DecimalGraphType>
        {
            Name = FeeTypeFilter,
            ResolvedType = new DecimalGraphType(),
        });

        contentQuery.Arguments.Add(new QueryArgument<StringGraphType>
        {
            Name = OfferTypeFilter,
            ResolvedType = new StringGraphType(),
        });

        contentQuery.Arguments.Add(new QueryArgument<StringGraphType>
        {
            Name = OfferWalletFilter,
            ResolvedType = new StringGraphType(),
        });

        contentQuery.Arguments.Add(new QueryArgument<IntGraphType>
        {
            Name = DurationFilter,
            ResolvedType = new IntGraphType(),
        });

        contentQuery.Arguments.Add(new QueryArgument<StringGraphType>
        {
            Name = SellerContentIdFilter,
            ResolvedType = new StringGraphType(),
        });

        contentQuery.Arguments.Add(new QueryArgument<StringGraphType>
        {
            Name = BuyerContentIdFilter,
            ResolvedType = new StringGraphType(),
        });

        contentQuery.Arguments.Add(new QueryArgument<StringGraphType>
        {
            Name = CurrencyOfTradeFilter,
            ResolvedType = new StringGraphType(),
        });

        contentQuery.Arguments.Add(new QueryArgument<DecimalGraphType>
        {
            Name = FeeVNDAmountFilter,
            ResolvedType = new DecimalGraphType(),
        });

        contentQuery.Arguments.Add(new QueryArgument<DecimalGraphType>
        {
            Name = FeeBTCAmountFilter,
            ResolvedType = new DecimalGraphType(),
        });

        contentQuery.Arguments.Add(new QueryArgument<DecimalGraphType>
        {
            Name = FeeETHAmountFilter,
            ResolvedType = new DecimalGraphType(),
        });

        contentQuery.Arguments.Add(new QueryArgument<DecimalGraphType>
        {
            Name = FeeUSDT20AmountFilter,
            ResolvedType = new DecimalGraphType(),
        });

        contentQuery.Arguments.Add(new QueryArgument<DecimalGraphType>
        {
            Name = TradeVNDAmountFilter,
            ResolvedType = new DecimalGraphType(),
        });

        contentQuery.Arguments.Add(new QueryArgument<DecimalGraphType>
        {
            Name = TradeBTCAmountFilter,
            ResolvedType = new DecimalGraphType(),
        });

        contentQuery.Arguments.Add(new QueryArgument<DecimalGraphType>
        {
            Name = TradeUSDT20AmountFilter,
            ResolvedType = new DecimalGraphType(),
        });

        contentQuery.Arguments.Add(new QueryArgument<DecimalGraphType>
        {
            Name = TradeETHAmountFilter,
            ResolvedType = new DecimalGraphType(),
        });

        contentQuery.Arguments.Add(new QueryArgument<StringGraphType>
        {
            Name = SellerFilter,
            ResolvedType = new StringGraphType(),
        });

        contentQuery.Arguments.Add(new QueryArgument<StringGraphType>
        {
            Name = TradeStatusFilter,
            ResolvedType = new StringGraphType(),
        });

        contentQuery.Arguments.Add(new QueryArgument<StringGraphType>
        {
            Name = OfferIdFilter,
            ResolvedType = new StringGraphType(),
        });

        /*------------------------------------------------------------*/
        AddFilterTradeType(contentQuery, "_ne");
        AddFilterPaymentMethod(contentQuery, "_ne");
        AddFilterOfferType(contentQuery, "_ne");
        AddFilterOfferWallet(contentQuery, "_ne");
        AddFilterSellerContentId(contentQuery, "_ne");
        AddFilterBuyerContentId(contentQuery, "_ne");
        AddFilterCurrencyOfTrade(contentQuery, "_ne");
        AddFilterSeller(contentQuery, "_ne");
        AddFilterBuyer(contentQuery, "_ne");
        AddFilterTradeStatus(contentQuery, "_ne");
        AddFilterOfferId(contentQuery, "_ne");

        /*------------------------------------------------------------*/
        AddFilterDuration(contentQuery, "_lt");
        AddFilterDuration(contentQuery, "_le");
        AddFilterDuration(contentQuery, "_ge");
        AddFilterDuration(contentQuery, "_gt");
        AddFilterDuration(contentQuery, "_ne");

        /*------------------------------------------------------------*/
        AddFilterFeeBTCAmount(contentQuery, "_lt");
        AddFilterFeeBTCAmount(contentQuery, "_le");
        AddFilterFeeBTCAmount(contentQuery, "_ge");
        AddFilterFeeBTCAmount(contentQuery, "_gt");
        AddFilterFeeBTCAmount(contentQuery, "_ne");

        /*------------------------------------------------------------*/
        AddFilterFeeETHAmount(contentQuery, "_lt");
        AddFilterFeeETHAmount(contentQuery, "_le");
        AddFilterFeeETHAmount(contentQuery, "_ge");
        AddFilterFeeETHAmount(contentQuery, "_gt");
        AddFilterFeeETHAmount(contentQuery, "_ne");

        /*------------------------------------------------------------*/
        AddFilterFeeUSDT20Amount(contentQuery, "_lt");
        AddFilterFeeUSDT20Amount(contentQuery, "_le");
        AddFilterFeeUSDT20Amount(contentQuery, "_ge");
        AddFilterFeeUSDT20Amount(contentQuery, "_gt");
        AddFilterFeeUSDT20Amount(contentQuery, "_ne");

        /*------------------------------------------------------------*/
        AddFilterFeeVNDAmount(contentQuery, "_lt");
        AddFilterFeeVNDAmount(contentQuery, "_le");
        AddFilterFeeVNDAmount(contentQuery, "_ge");
        AddFilterFeeVNDAmount(contentQuery, "_gt");
        AddFilterFeeVNDAmount(contentQuery, "_ne");

        /*------------------------------------------------------------*/
        AddFilterTradeBTCAmount(contentQuery, "_lt");
        AddFilterTradeBTCAmount(contentQuery, "_le");
        AddFilterTradeBTCAmount(contentQuery, "_ge");
        AddFilterTradeBTCAmount(contentQuery, "_gt");
        AddFilterTradeBTCAmount(contentQuery, "_ne");

        /*------------------------------------------------------------*/
        AddFilterTradeUSDT20Amount(contentQuery, "_lt");
        AddFilterTradeUSDT20Amount(contentQuery, "_le");
        AddFilterTradeUSDT20Amount(contentQuery, "_ge");
        AddFilterTradeUSDT20Amount(contentQuery, "_gt");
        AddFilterTradeUSDT20Amount(contentQuery, "_ne");        

        /*------------------------------------------------------------*/
        AddFilterTradeETHAmount(contentQuery, "_lt");
        AddFilterTradeETHAmount(contentQuery, "_le");
        AddFilterTradeETHAmount(contentQuery, "_ge");
        AddFilterTradeETHAmount(contentQuery, "_gt");
        AddFilterTradeETHAmount(contentQuery, "_ne");

        /*------------------------------------------------------------*/
        AddFilterTradeVNDAmount(contentQuery, "_lt");
        AddFilterTradeVNDAmount(contentQuery, "_le");
        AddFilterTradeVNDAmount(contentQuery, "_ge");
        AddFilterTradeVNDAmount(contentQuery, "_gt");
        AddFilterTradeVNDAmount(contentQuery, "_ne");

        /*------------------------------------------------------------*/
        AddFilterFeeType(contentQuery, "_lt");
        AddFilterFeeType(contentQuery, "_le");
        AddFilterFeeType(contentQuery, "_ge");
        AddFilterFeeType(contentQuery, "_gt");
        AddFilterFeeType(contentQuery, "_ne");
    }

    private static void AddFilterTradeType(FieldType contentQuery, string suffix) =>
        contentQuery.Arguments.Add(new QueryArgument<StringGraphType>
        {
            Name = TradeTypeFilter + suffix,
            ResolvedType = new StringGraphType(),
        });

    private static void AddFilterPaymentMethod(FieldType contentQuery, string suffix) =>
        contentQuery.Arguments.Add(new QueryArgument<StringGraphType>
        {
            Name = PaymentMethodFilter + suffix,
            ResolvedType = new StringGraphType(),
        });

    private static void AddFilterFeeType(FieldType contentQuery, string suffix) =>
        contentQuery.Arguments.Add(new QueryArgument<IntGraphType>
        {
            Name = FeeTypeFilter + suffix,
            ResolvedType = new IntGraphType(),
        });

    private static void AddFilterOfferType(FieldType contentQuery, string suffix) =>
        contentQuery.Arguments.Add(new QueryArgument<DecimalGraphType>
        {
            Name = OfferTypeFilter + suffix,
            ResolvedType = new DecimalGraphType(),
        });

    private static void AddFilterOfferWallet(FieldType contentQuery, string suffix) =>
        contentQuery.Arguments.Add(new QueryArgument<StringGraphType>
        {
            Name = OfferWalletFilter + suffix,
            ResolvedType = new StringGraphType(),
        });

    private static void AddFilterDuration(FieldType contentQuery, string suffix) =>
        contentQuery.Arguments.Add(new QueryArgument<IntGraphType>
        {
            Name = DurationFilter + suffix,
            ResolvedType = new IntGraphType(),
        });

    private static void AddFilterSellerContentId(FieldType contentQuery, string suffix) =>
        contentQuery.Arguments.Add(new QueryArgument<StringGraphType>
        {
            Name = SellerContentIdFilter + suffix,
            ResolvedType = new StringGraphType(),
        });

    private static void AddFilterBuyerContentId(FieldType contentQuery, string suffix) =>
        contentQuery.Arguments.Add(new QueryArgument<StringGraphType>
        {
            Name = BuyerContentIdFilter + suffix,
            ResolvedType = new StringGraphType(),
        });

    private static void AddFilterCurrencyOfTrade(FieldType contentQuery, string suffix) =>
    contentQuery.Arguments.Add(new QueryArgument<DecimalGraphType>
    {
        Name = CurrencyOfTradeFilter + suffix,
        ResolvedType = new DecimalGraphType(),
    });

    private static void AddFilterFeeBTCAmount(FieldType contentQuery, string suffix) =>
        contentQuery.Arguments.Add(new QueryArgument<DecimalGraphType>
        {
            Name = FeeBTCAmountFilter + suffix,
            ResolvedType = new DecimalGraphType(),
        });

    private static void AddFilterFeeETHAmount(FieldType contentQuery, string suffix) =>
        contentQuery.Arguments.Add(new QueryArgument<DecimalGraphType>
        {
            Name = FeeETHAmountFilter + suffix,
            ResolvedType = new DecimalGraphType(),
        });

    private static void AddFilterFeeUSDT20Amount(FieldType contentQuery, string suffix) =>
        contentQuery.Arguments.Add(new QueryArgument<DecimalGraphType>
        {
            Name = FeeUSDT20AmountFilter + suffix,
            ResolvedType = new DecimalGraphType(),
        });

    private static void AddFilterFeeVNDAmount(FieldType contentQuery, string suffix) =>
        contentQuery.Arguments.Add(new QueryArgument<DecimalGraphType>
        {
            Name = FeeVNDAmountFilter + suffix,
            ResolvedType = new DecimalGraphType(),
        });

    private static void AddFilterTradeBTCAmount(FieldType contentQuery, string suffix) =>
        contentQuery.Arguments.Add(new QueryArgument<DecimalGraphType>
        {
            Name = TradeBTCAmountFilter + suffix,
            ResolvedType = new DecimalGraphType(),
        });

    private static void AddFilterTradeUSDT20Amount(FieldType contentQuery, string suffix) =>
        contentQuery.Arguments.Add(new QueryArgument<DecimalGraphType>
        {
            Name = TradeUSDT20AmountFilter + suffix,
            ResolvedType = new DecimalGraphType(),
        });

    private static void AddFilterTradeETHAmount(FieldType contentQuery, string suffix) =>
        contentQuery.Arguments.Add(new QueryArgument<DecimalGraphType>
        {
            Name = TradeETHAmountFilter + suffix,
            ResolvedType = new DecimalGraphType(),
        });

    private static void AddFilterTradeVNDAmount(FieldType contentQuery, string suffix) =>
    contentQuery.Arguments.Add(new QueryArgument<DecimalGraphType>
    {
        Name = TradeVNDAmountFilter + suffix,
        ResolvedType = new DecimalGraphType(),
    });

    private static void AddFilterSeller(FieldType contentQuery, string suffix) =>
        contentQuery.Arguments.Add(new QueryArgument<StringGraphType>
        {
            Name = SellerFilter + suffix,
            ResolvedType = new StringGraphType(),
        });

    private static void AddFilterBuyer(FieldType contentQuery, string suffix) =>
        contentQuery.Arguments.Add(new QueryArgument<StringGraphType>
        {
            Name = BuyerFilter + suffix,
            ResolvedType = new StringGraphType(),
        });

    private static void AddFilterTradeStatus(FieldType contentQuery, string suffix) =>
        contentQuery.Arguments.Add(new QueryArgument<StringGraphType>
        {
            Name = TradeStatusFilter + suffix,
            ResolvedType = new StringGraphType(),
        });

    private static void AddFilterOfferId(FieldType contentQuery, string suffix) =>
        contentQuery.Arguments.Add(new QueryArgument<StringGraphType>
        {
            Name = OfferIdFilter + suffix,
            ResolvedType = new StringGraphType(),
        });
}
