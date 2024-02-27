using OrchardCore.SongServices.Indexes;
using OrchardCore.SongServices.ContentParts;
using OrchardCore.ContentFields.Fields;
using OrchardCore.ContentFields.Settings;
using OrchardCore.ContentManagement.Metadata;
using OrchardCore.ContentManagement.Metadata.Settings;
using OrchardCore.Data.Migration;
using System;
using YesSql.Sql;
using OrchardCore.SongServices.Indexing;
using System.Threading.Tasks;

namespace OrchardCore.SongServices.Migrations;

// Here's another migrations file but specifically for Person-related operations, including how to define content types
// and configure content parts. Don't forget to register this class with the service provider (see Startup.cs). You can
// also generate such migration steps with the Code Generation feature of our Helpful Extensions module, check it out
// here: https://github.com/Lombiq/Helpful-Extensions
public class PersonMigrations : DataMigration
{
    private readonly IContentDefinitionManager _contentDefinitionManager;

    public PersonMigrations(IContentDefinitionManager contentDefinitionManager) => _contentDefinitionManager = contentDefinitionManager;

    public async Task<int> CreateAsync()
    {
        // Now you can configure PersonPart. For example you can add content fields (as mentioned earlier) here.
        await _contentDefinitionManager.AlterPartDefinitionAsync(nameof(PersonPart), part => part
            // Each field has its own configuration. Here you will give a display name for it and add some additional
            // settings like a hint to be displayed in the editor.
            .WithField(nameof(PersonPart.Biography), field => field
                .OfType(nameof(TextField))
                .WithDisplayName("Biography")
                .WithEditor("TextArea")
                .WithSettings(new TextFieldSettings
                {
                    Hint = "Person's biography",
                }))
        );

        // This one will create an index table for the PersonPartIndex as explained in the BookMigrations file.
        await SchemaBuilder.CreateMapIndexTableAsync<PersonPartIndex>(table => table
            .Column<DateTime>(nameof(PersonPartIndex.BirthDateUtc))
            .Column<string>(nameof(PersonPartIndex.Name))
            .Column<Handedness>(nameof(PersonPartIndex.Handedness))
            // The content item ID is always 26 characters.
            .Column<string>(nameof(PersonPartIndex.ContentItemId), column => column.WithLength(26))
        );

        await SchemaBuilder.AlterTableAsync(nameof(PersonPartIndex), table => table
                .CreateIndex(
                    $"IDX_{nameof(PersonPartIndex)}_{nameof(PersonPartIndex.BirthDateUtc)}",
                    nameof(PersonPartIndex.BirthDateUtc))
            );

        await SchemaBuilder.AlterTableAsync(nameof(PersonPartIndex), table => table
                        .CreateIndex(
                            $"IDX_{nameof(PersonPartIndex)}_{nameof(PersonPartIndex.Name)}",
                            nameof(PersonPartIndex.Name))
                    );

        // We create a new content type. Note that there's only an alter method: this will create the type if it doesn't
        // exist or modify it if it does. Make sure you understand what content types are:
        // https://docs.orchardcore.net/en/latest/docs/glossary/#content-type. The content type's name is arbitrary but
        // choose a meaningful one. Notice how we use a class with constants to store the type name so we prevent risky
        // copy-pasting.
        await _contentDefinitionManager.AlterTypeDefinitionAsync(ContentTypes.PersonPage, type => type
            .Creatable()
            .Listable()
            // We attach parts by specifying their name. For our own parts we use nameof(): This is not mandatory but
            // serves great if we change the part's name during development.
            .WithPart(nameof(PersonPart))
        );

        // We can even create a widget with the same content part. Widgets are just content types as usual but with the
        // Stereotype set as "Widget". You'll notice that our site's configuration includes three zones on the frontend
        // that you can add widgets to, as well as two layers. Check them out on the admin!
        await _contentDefinitionManager.AlterTypeDefinitionAsync("PersonWidget", type => type
            .Stereotype("Widget")
            .WithPart(nameof(PersonPart))
        );

        await SchemaBuilder.CreateMapIndexTableAsync<OfferFilteringPartIndex>(table => table
            .Column<decimal>(nameof(OfferFilteringPartIndex.MinAmount))
            .Column<decimal>(nameof(OfferFilteringPartIndex.MaxAmount))
            .Column<string>(nameof(OfferFilteringPartIndex.Status))
            .Column<string>(nameof(OfferFilteringPartIndex.Wallet))
            .Column<string>(nameof(OfferFilteringPartIndex.PaymentMethod))
            .Column<string>(nameof(OfferFilteringPartIndex.PreferredCurrency))
            .Column<decimal>(nameof(OfferFilteringPartIndex.Percentage))
            .Column<string>(nameof(OfferFilteringPartIndex.OfferType))
            .Column<DateTime>(nameof(OfferFilteringPartIndex.DateTime))
            // The content item ID is always 26 characters.
            .Column<string>(nameof(OfferFilteringPartIndex.ContentItemId), column => column.WithLength(26))
        );

        await SchemaBuilder.AlterTableAsync(nameof(OfferFilteringPartIndex), table => table
                .CreateIndex(
                    $"IDX_{nameof(OfferFilteringPartIndex)}_{nameof(OfferFilteringPartIndex.MinAmount)}",
                    nameof(OfferFilteringPartIndex.MinAmount))
            );

        await SchemaBuilder.AlterTableAsync(nameof(OfferFilteringPartIndex), table => table
                        .CreateIndex(
                            $"IDX_{nameof(OfferFilteringPartIndex)}_{nameof(OfferFilteringPartIndex.MaxAmount)}",
                            nameof(OfferFilteringPartIndex.MaxAmount))
                    );

        await SchemaBuilder.AlterTableAsync(nameof(OfferFilteringPartIndex), table => table
                        .CreateIndex(
                            $"IDX_{nameof(OfferFilteringPartIndex)}_{nameof(OfferFilteringPartIndex.Status)}",
                            nameof(OfferFilteringPartIndex.Status))
                    );

        await SchemaBuilder.AlterTableAsync(nameof(OfferFilteringPartIndex), table => table
                       .CreateIndex(
                           $"IDX_{nameof(OfferFilteringPartIndex)}_{nameof(OfferFilteringPartIndex.Wallet)}",
                           nameof(OfferFilteringPartIndex.Wallet))
                   );

        await SchemaBuilder.AlterTableAsync(nameof(OfferFilteringPartIndex), table => table
                       .CreateIndex(
                           $"IDX_{nameof(OfferFilteringPartIndex)}_{nameof(OfferFilteringPartIndex.PaymentMethod)}",
                           nameof(OfferFilteringPartIndex.PaymentMethod))
                   );

        await SchemaBuilder.AlterTableAsync(nameof(OfferFilteringPartIndex), table => table
                       .CreateIndex(
                           $"IDX_{nameof(OfferFilteringPartIndex)}_{nameof(OfferFilteringPartIndex.PreferredCurrency)}",
                           nameof(OfferFilteringPartIndex.PreferredCurrency))
                   );

        await SchemaBuilder.AlterTableAsync(nameof(OfferFilteringPartIndex), table => table
                       .CreateIndex(
                           $"IDX_{nameof(OfferFilteringPartIndex)}_{nameof(OfferFilteringPartIndex.Percentage)}",
                           nameof(OfferFilteringPartIndex.Percentage))
                   );

        await SchemaBuilder.AlterTableAsync(nameof(OfferFilteringPartIndex), table => table
                       .CreateIndex(
                           $"IDX_{nameof(OfferFilteringPartIndex)}_{nameof(OfferFilteringPartIndex.OfferType)}",
                           nameof(OfferFilteringPartIndex.OfferType))
                   );

        await SchemaBuilder.AlterTableAsync(nameof(OfferFilteringPartIndex), table => table
                       .CreateIndex(
                           $"IDX_{nameof(OfferFilteringPartIndex)}_{nameof(OfferFilteringPartIndex.DateTime)}",
                           nameof(OfferFilteringPartIndex.DateTime))
                   );

        // We create a new content type. Note that there's only an alter method: this will create the type if it doesn't
        // exist or modify it if it does. Make sure you understand what content types are:
        // https://docs.orchardcore.net/en/latest/docs/glossary/#content-type. The content type's name is arbitrary but
        // choose a meaningful one. Notice how we use a class with constants to store the type name so we prevent risky
        // copy-pasting.
        await _contentDefinitionManager.AlterTypeDefinitionAsync(ContentTypes.OfferPage, type => type
            .Creatable()
            .Listable()
            // We attach parts by specifying their name. For our own parts we use nameof(): This is not mandatory but
            // serves great if we change the part's name during development.
            .WithPart(nameof(OfferFilteringPart))
        );

        /*****************************************Trade Part*************************************************/
        await SchemaBuilder.CreateMapIndexTableAsync<TradeFilteringPartIndex>(table => table
            .Column<string>(nameof(TradeFilteringPartIndex.TradeType))
            .Column<string>(nameof(TradeFilteringPartIndex.PaymentMethod))
            .Column<int>(nameof(TradeFilteringPartIndex.FeeType))
            .Column<string>(nameof(TradeFilteringPartIndex.OfferType))
            .Column<string>(nameof(TradeFilteringPartIndex.OfferWallet))
            .Column<int>(nameof(TradeFilteringPartIndex.Duration))
            .Column<string>(nameof(TradeFilteringPartIndex.SellerContentId))
            .Column<string>(nameof(TradeFilteringPartIndex.BuyerContentId))
            .Column<string>(nameof(TradeFilteringPartIndex.CurrencyOfTrade))
            .Column<decimal>(nameof(TradeFilteringPartIndex.FeeVNDAmount))
            .Column<decimal>(nameof(TradeFilteringPartIndex.FeeBTCAmount))
            .Column<decimal>(nameof(TradeFilteringPartIndex.FeeETHAmount))
            .Column<decimal>(nameof(TradeFilteringPartIndex.FeeUSDT20Amount))
            .Column<decimal>(nameof(TradeFilteringPartIndex.TradeVNDAmount))
            .Column<decimal>(nameof(TradeFilteringPartIndex.TradeBTCAmount))
            .Column<decimal>(nameof(TradeFilteringPartIndex.TradeUSDT20Amount))
            .Column<decimal>(nameof(TradeFilteringPartIndex.TradeETHAmount))
            .Column<string>(nameof(TradeFilteringPartIndex.Seller))
            .Column<string>(nameof(TradeFilteringPartIndex.Buyer))
            .Column<string>(nameof(TradeFilteringPartIndex.Status))
            .Column<string>(nameof(TradeFilteringPartIndex.OfferId))
            .Column<DateTime>(nameof(TradeFilteringPartIndex.DateTime))
            // The content item ID is always 26 characters.
            .Column<string>(nameof(TradeFilteringPartIndex.ContentItemId), column => column.WithLength(26))
        );

        await SchemaBuilder.AlterTableAsync(nameof(TradeFilteringPartIndex), table => table
                .CreateIndex(
                    $"IDX_{nameof(TradeFilteringPartIndex)}_{nameof(TradeFilteringPartIndex.PaymentMethod)}",
                    nameof(TradeFilteringPartIndex.PaymentMethod))
            );

        await SchemaBuilder.AlterTableAsync(nameof(TradeFilteringPartIndex), table => table
                        .CreateIndex(
                            $"IDX_{nameof(TradeFilteringPartIndex)}_{nameof(TradeFilteringPartIndex.FeeType)}",
                            nameof(TradeFilteringPartIndex.FeeType))
                    );

        await SchemaBuilder.AlterTableAsync(nameof(TradeFilteringPartIndex), table => table
                        .CreateIndex(
                            $"IDX_{nameof(TradeFilteringPartIndex)}_{nameof(TradeFilteringPartIndex.OfferType)}",
                            nameof(TradeFilteringPartIndex.OfferType))
                    );

        await SchemaBuilder.AlterTableAsync(nameof(TradeFilteringPartIndex), table => table
                       .CreateIndex(
                           $"IDX_{nameof(TradeFilteringPartIndex)}_{nameof(TradeFilteringPartIndex.OfferWallet)}",
                           nameof(TradeFilteringPartIndex.OfferWallet))
                   );

        await SchemaBuilder.AlterTableAsync(nameof(TradeFilteringPartIndex), table => table
                       .CreateIndex(
                           $"IDX_{nameof(TradeFilteringPartIndex)}_{nameof(TradeFilteringPartIndex.Duration)}",
                           nameof(TradeFilteringPartIndex.Duration))
                   );

        await SchemaBuilder.AlterTableAsync(nameof(TradeFilteringPartIndex), table => table
                       .CreateIndex(
                           $"IDX_{nameof(TradeFilteringPartIndex)}_{nameof(TradeFilteringPartIndex.SellerContentId)}",
                           nameof(TradeFilteringPartIndex.SellerContentId))
                   );

        await SchemaBuilder.AlterTableAsync(nameof(TradeFilteringPartIndex), table => table
                       .CreateIndex(
                           $"IDX_{nameof(TradeFilteringPartIndex)}_{nameof(TradeFilteringPartIndex.BuyerContentId)}",
                           nameof(TradeFilteringPartIndex.BuyerContentId))
                   );

        await SchemaBuilder.AlterTableAsync(nameof(TradeFilteringPartIndex), table => table
                       .CreateIndex(
                           $"IDX_{nameof(TradeFilteringPartIndex)}_{nameof(TradeFilteringPartIndex.CurrencyOfTrade)}",
                           nameof(TradeFilteringPartIndex.CurrencyOfTrade))
                   );

        await SchemaBuilder.AlterTableAsync(nameof(TradeFilteringPartIndex), table => table
                       .CreateIndex(
                           $"IDX_{nameof(TradeFilteringPartIndex)}_{nameof(TradeFilteringPartIndex.FeeVNDAmount)}",
                           nameof(TradeFilteringPartIndex.FeeVNDAmount))
                   );

        await SchemaBuilder.AlterTableAsync(nameof(TradeFilteringPartIndex), table => table
                       .CreateIndex(
                           $"IDX_{nameof(TradeFilteringPartIndex)}_{nameof(TradeFilteringPartIndex.FeeBTCAmount)}",
                           nameof(TradeFilteringPartIndex.FeeBTCAmount))
                   );

        await SchemaBuilder.AlterTableAsync(nameof(TradeFilteringPartIndex), table => table
                       .CreateIndex(
                           $"IDX_{nameof(TradeFilteringPartIndex)}_{nameof(TradeFilteringPartIndex.FeeETHAmount)}",
                           nameof(TradeFilteringPartIndex.FeeETHAmount))
                   );

        await SchemaBuilder.AlterTableAsync(nameof(TradeFilteringPartIndex), table => table
                       .CreateIndex(
                           $"IDX_{nameof(TradeFilteringPartIndex)}_{nameof(TradeFilteringPartIndex.FeeUSDT20Amount)}",
                           nameof(TradeFilteringPartIndex.FeeUSDT20Amount))
                   );

        await SchemaBuilder.AlterTableAsync(nameof(TradeFilteringPartIndex), table => table
                       .CreateIndex(
                           $"IDX_{nameof(TradeFilteringPartIndex)}_{nameof(TradeFilteringPartIndex.TradeVNDAmount)}",
                           nameof(TradeFilteringPartIndex.TradeVNDAmount))
                   );

        await SchemaBuilder.AlterTableAsync(nameof(TradeFilteringPartIndex), table => table
                       .CreateIndex(
                           $"IDX_{nameof(TradeFilteringPartIndex)}_{nameof(TradeFilteringPartIndex.TradeBTCAmount)}",
                           nameof(TradeFilteringPartIndex.TradeBTCAmount))
                   );

        await SchemaBuilder.AlterTableAsync(nameof(TradeFilteringPartIndex), table => table
                       .CreateIndex(
                           $"IDX_{nameof(TradeFilteringPartIndex)}_{nameof(TradeFilteringPartIndex.TradeUSDT20Amount)}",
                           nameof(TradeFilteringPartIndex.TradeUSDT20Amount))
                   );

        await SchemaBuilder.AlterTableAsync(nameof(TradeFilteringPartIndex), table => table
                       .CreateIndex(
                           $"IDX_{nameof(TradeFilteringPartIndex)}_{nameof(TradeFilteringPartIndex.TradeETHAmount)}",
                           nameof(TradeFilteringPartIndex.TradeETHAmount))
                   );

        await SchemaBuilder.AlterTableAsync(nameof(TradeFilteringPartIndex), table => table
                       .CreateIndex(
                           $"IDX_{nameof(TradeFilteringPartIndex)}_{nameof(TradeFilteringPartIndex.Seller)}",
                           nameof(TradeFilteringPartIndex.Seller))
                   );

        await SchemaBuilder.AlterTableAsync(nameof(TradeFilteringPartIndex), table => table
                       .CreateIndex(
                           $"IDX_{nameof(TradeFilteringPartIndex)}_{nameof(TradeFilteringPartIndex.Buyer)}",
                           nameof(TradeFilteringPartIndex.Buyer))
                   );

        await SchemaBuilder.AlterTableAsync(nameof(TradeFilteringPartIndex), table => table
                       .CreateIndex(
                           $"IDX_{nameof(TradeFilteringPartIndex)}_{nameof(TradeFilteringPartIndex.Status)}",
                           nameof(TradeFilteringPartIndex.Status))
                   );

        await SchemaBuilder.AlterTableAsync(nameof(TradeFilteringPartIndex), table => table
                       .CreateIndex(
                           $"IDX_{nameof(TradeFilteringPartIndex)}_{nameof(TradeFilteringPartIndex.OfferId)}",
                           nameof(TradeFilteringPartIndex.OfferId))
                   );

        await SchemaBuilder.AlterTableAsync(nameof(TradeFilteringPartIndex), table => table
                       .CreateIndex(
                           $"IDX_{nameof(TradeFilteringPartIndex)}_{nameof(TradeFilteringPartIndex.DateTime)}",
                           nameof(TradeFilteringPartIndex.DateTime))
                   );

        // We create a new content type. Note that there's only an alter method: this will create the type if it doesn't
        // exist or modify it if it does. Make sure you understand what content types are:
        // https://docs.orchardcore.net/en/latest/docs/glossary/#content-type. The content type's name is arbitrary but
        // choose a meaningful one. Notice how we use a class with constants to store the type name so we prevent risky
        // copy-pasting.
        await _contentDefinitionManager.AlterTypeDefinitionAsync(ContentTypes.TradePage, type => type
            .Creatable()
            .Listable()
            // We attach parts by specifying their name. For our own parts we use nameof(): This is not mandatory but
            // serves great if we change the part's name during development.
            .WithPart(nameof(TradeFilteringPart))
        );

        /*****************************************Traderrrrr Part*************************************************/
        await SchemaBuilder.CreateMapIndexTableAsync<TraderForFilteringPartIndex>(table => table
            .Column<string>(nameof(TraderForFilteringPartIndex.Name))
            .Column<bool>(nameof(TraderForFilteringPartIndex.IsActivatedTele))
            .Column<decimal>(nameof(TraderForFilteringPartIndex.BondVndBalance))
            .Column<decimal>(nameof(TraderForFilteringPartIndex.VndBalance))
            .Column<decimal>(nameof(TraderForFilteringPartIndex.BTCBalance))
            .Column<decimal>(nameof(TraderForFilteringPartIndex.ETHBalance))
            .Column<decimal>(nameof(TraderForFilteringPartIndex.USDT20Balance))
            .Column<string>(nameof(TraderForFilteringPartIndex.WithdrawVNDStatus))
            .Column<int>(nameof(TraderForFilteringPartIndex.ReferenceCode))
            .Column<string>(nameof(TraderForFilteringPartIndex.DateSend))
            .Column<int>(nameof(TraderForFilteringPartIndex.UserId))
            .Column<string>(nameof(TraderForFilteringPartIndex.Email))
            .Column<string>(nameof(TraderForFilteringPartIndex.PhoneNumber))
            .Column<string>(nameof(TraderForFilteringPartIndex.BankAccounts))
            .Column<string>(nameof(TraderForFilteringPartIndex.ChatIdTele))
            .Column<string>(nameof(TraderForFilteringPartIndex.DeviceId))
            .Column<DateTime>(nameof(TraderForFilteringPartIndex.DateTime))
            // The content item ID is always 26 characters.
            .Column<string>(nameof(TraderForFilteringPartIndex.ContentItemId), column => column.WithLength(26))
        );

        await SchemaBuilder.AlterTableAsync(nameof(TraderForFilteringPartIndex), table => table
                .CreateIndex(
                    $"IDX_{nameof(TraderForFilteringPartIndex)}_{nameof(TraderForFilteringPartIndex.Name)}",
                    nameof(TraderForFilteringPartIndex.Name))
            );

        await SchemaBuilder.AlterTableAsync(nameof(TraderForFilteringPartIndex), table => table
                        .CreateIndex(
                            $"IDX_{nameof(TraderForFilteringPartIndex)}_{nameof(TraderForFilteringPartIndex.IsActivatedTele)}",
                            nameof(TraderForFilteringPartIndex.IsActivatedTele))
                    );

        await SchemaBuilder.AlterTableAsync(nameof(TraderForFilteringPartIndex), table => table
                        .CreateIndex(
                            $"IDX_{nameof(TraderForFilteringPartIndex)}_{nameof(TraderForFilteringPartIndex.BondVndBalance)}",
                            nameof(TraderForFilteringPartIndex.BondVndBalance))
                    );

        await SchemaBuilder.AlterTableAsync(nameof(TraderForFilteringPartIndex), table => table
                       .CreateIndex(
                           $"IDX_{nameof(TraderForFilteringPartIndex)}_{nameof(TraderForFilteringPartIndex.VndBalance)}",
                           nameof(TraderForFilteringPartIndex.VndBalance))
                   );

        await SchemaBuilder.AlterTableAsync(nameof(TraderForFilteringPartIndex), table => table
                       .CreateIndex(
                           $"IDX_{nameof(TraderForFilteringPartIndex)}_{nameof(TraderForFilteringPartIndex.BTCBalance)}",
                           nameof(TraderForFilteringPartIndex.BTCBalance))
                   );

        await SchemaBuilder.AlterTableAsync(nameof(TraderForFilteringPartIndex), table => table
                       .CreateIndex(
                           $"IDX_{nameof(TraderForFilteringPartIndex)}_{nameof(TraderForFilteringPartIndex.ETHBalance)}",
                           nameof(TraderForFilteringPartIndex.ETHBalance))
                   );

        await SchemaBuilder.AlterTableAsync(nameof(TraderForFilteringPartIndex), table => table
                       .CreateIndex(
                           $"IDX_{nameof(TraderForFilteringPartIndex)}_{nameof(TraderForFilteringPartIndex.USDT20Balance)}",
                           nameof(TraderForFilteringPartIndex.USDT20Balance))
                   );

        await SchemaBuilder.AlterTableAsync(nameof(TraderForFilteringPartIndex), table => table
                       .CreateIndex(
                           $"IDX_{nameof(TraderForFilteringPartIndex)}_{nameof(TraderForFilteringPartIndex.WithdrawVNDStatus)}",
                           nameof(TraderForFilteringPartIndex.WithdrawVNDStatus))
                   );

        await SchemaBuilder.AlterTableAsync(nameof(TraderForFilteringPartIndex), table => table
                       .CreateIndex(
                           $"IDX_{nameof(TraderForFilteringPartIndex)}_{nameof(TraderForFilteringPartIndex.ReferenceCode)}",
                           nameof(TraderForFilteringPartIndex.ReferenceCode))
                   );

        await SchemaBuilder.AlterTableAsync(nameof(TraderForFilteringPartIndex), table => table
                       .CreateIndex(
                           $"IDX_{nameof(TraderForFilteringPartIndex)}_{nameof(TraderForFilteringPartIndex.DateSend)}",
                           nameof(TraderForFilteringPartIndex.DateSend))
                   );

        await SchemaBuilder.AlterTableAsync(nameof(TraderForFilteringPartIndex), table => table
                       .CreateIndex(
                           $"IDX_{nameof(TraderForFilteringPartIndex)}_{nameof(TraderForFilteringPartIndex.UserId)}",
                           nameof(TraderForFilteringPartIndex.UserId))
                   );

        await SchemaBuilder.AlterTableAsync(nameof(TraderForFilteringPartIndex), table => table
                       .CreateIndex(
                           $"IDX_{nameof(TraderForFilteringPartIndex)}_{nameof(TraderForFilteringPartIndex.Email)}",
                           nameof(TraderForFilteringPartIndex.Email))
                   );

        await SchemaBuilder.AlterTableAsync(nameof(TraderForFilteringPartIndex), table => table
                       .CreateIndex(
                           $"IDX_{nameof(TraderForFilteringPartIndex)}_{nameof(TraderForFilteringPartIndex.PhoneNumber)}",
                           nameof(TraderForFilteringPartIndex.PhoneNumber))
                   );

        await SchemaBuilder.AlterTableAsync(nameof(TraderForFilteringPartIndex), table => table
                       .CreateIndex(
                           $"IDX_{nameof(TraderForFilteringPartIndex)}_{nameof(TraderForFilteringPartIndex.BankAccounts)}",
                           nameof(TraderForFilteringPartIndex.BankAccounts))
                   );

        await SchemaBuilder.AlterTableAsync(nameof(TraderForFilteringPartIndex), table => table
                       .CreateIndex(
                           $"IDX_{nameof(TraderForFilteringPartIndex)}_{nameof(TraderForFilteringPartIndex.ChatIdTele)}",
                           nameof(TraderForFilteringPartIndex.ChatIdTele))
                   );

        await SchemaBuilder.AlterTableAsync(nameof(TraderForFilteringPartIndex), table => table
                       .CreateIndex(
                           $"IDX_{nameof(TraderForFilteringPartIndex)}_{nameof(TraderForFilteringPartIndex.DeviceId)}",
                           nameof(TraderForFilteringPartIndex.DeviceId))
                   );

        await SchemaBuilder.AlterTableAsync(nameof(TraderForFilteringPartIndex), table => table
                       .CreateIndex(
                           $"IDX_{nameof(TraderForFilteringPartIndex)}_{nameof(TraderForFilteringPartIndex.DateTime)}",
                           nameof(TraderForFilteringPartIndex.DateTime))
                   );

        // We create a new content type. Note that there's only an alter method: this will create the type if it doesn't
        // exist or modify it if it does. Make sure you understand what content types are:
        // https://docs.orchardcore.net/en/latest/docs/glossary/#content-type. The content type's name is arbitrary but
        // choose a meaningful one. Notice how we use a class with constants to store the type name so we prevent risky
        // copy-pasting.
        await _contentDefinitionManager.AlterTypeDefinitionAsync(ContentTypes.TraderPage, type => type
            .Creatable()
            .Listable()
            // We attach parts by specifying their name. For our own parts we use nameof(): This is not mandatory but
            // serves great if we change the part's name during development.
            .WithPart(nameof(TraderForFilteringPart))
        );
        return 1;
    }

    public async Task<int> UpdateFrom1Async()
    {
        await _contentDefinitionManager.AlterPartDefinitionAsync(nameof(TraderForFilteringPart), part => part
            .WithField(nameof(TraderForFilteringPart.Name), field => field
                .OfType(nameof(TextField))
                .WithDisplayName("Name")
                .WithEditor("Text")
                .WithSettings(new TextFieldSettings
                {
                    Hint = "Trader's name",
                }))
        );
        return 2;
    }

    public async Task<int> UpdateFrom2Async()
    {
        await _contentDefinitionManager.AlterTypeDefinitionAsync(ContentTypes.TraderPage, type => type
            .Creatable()
            .Listable()
            // We attach parts by specifying their name. For our own parts we use nameof(): This is not mandatory but
            // serves great if we change the part's name during development.
            .WithPart(nameof(TraderForFilteringPart))
        );

        return 3;
    }

    public async Task<int> UpdateFrom3Async()
    {
        // remove the AccreditationsPart part cleanly
        await _contentDefinitionManager.DeletePartDefinitionAsync(typeof(TraderForFilteringPart).Name);
        // re-add the AccreditationsPart again
        await _contentDefinitionManager.AlterPartDefinitionAsync(typeof(TraderForFilteringPart).Name,
            cfg => cfg
            .Attachable());
        return 4;
    }

    public async Task<int> UpdateFrom4Async()
    {
        await _contentDefinitionManager.DeleteTypeDefinitionAsync(nameof(TraderForFilteringPartIndex));

        await _contentDefinitionManager.AlterTypeDefinitionAsync(ContentTypes.TraderPage, type => type
            .Creatable()
            .Listable()
            // We attach parts by specifying their name. For our own parts we use nameof(): This is not mandatory but
            // serves great if we change the part's name during development.
            .WithPart(nameof(TraderForFilteringPart))
        );
        return 5;
    }

    public async Task<int> UpdateFrom5Async()
    {
        await _contentDefinitionManager.AlterTypeDefinitionAsync(ContentTypes.TraderPage, type => type
        .RemovePart(nameof(TraderForFilteringPartIndex)));
        return 6;
    }

    public async Task<int> UpdateFrom6Async()
    {
        await _contentDefinitionManager.AlterTypeDefinitionAsync(ContentTypes.TradePage, type => type
        .RemovePart(nameof(TradeFilteringPartIndex)));

        await _contentDefinitionManager.AlterTypeDefinitionAsync(ContentTypes.TradePage, type => type
            .Creatable()
            .Listable()
            .WithPart(nameof(TradeFilteringPart))
        );
        return 7;
    }

    public async Task<int> UpdateFrom7Async()
    {
        await _contentDefinitionManager.AlterPartDefinitionAsync(typeof(TradeFilteringPart).Name,
            cfg => cfg
            .Attachable());

        return 8;
    }

    public async Task<int> UpdateFrom8Async()
    {
        await _contentDefinitionManager.AlterPartDefinitionAsync(typeof(OfferFilteringPart).Name,
            cfg => cfg
            .Attachable());
        /*****************************************Traderrrrr Part*************************************************/
        await _contentDefinitionManager.AlterPartDefinitionAsync(typeof(LocationPart).Name,
           cfg => cfg
           .Attachable());

        await SchemaBuilder.CreateMapIndexTableAsync<LocationPartIndex>(table => table
            .Column<string>(nameof(LocationPartIndex.Country))
            .Column<string>(nameof(LocationPartIndex.City))
            .Column<string>(nameof(LocationPartIndex.Street))
            .Column<string>(nameof(LocationPartIndex.Site))
            .Column<string>(nameof(LocationPartIndex.Building))
            .Column<string>(nameof(LocationPartIndex.Floor))
            .Column<string>(nameof(LocationPartIndex.Zone))
            .Column<string>(nameof(LocationPartIndex.Room))
            .Column<DateTime>(nameof(LocationPartIndex.DateTime))
            // The content item ID is always 26 characters.
            .Column<string>(nameof(LocationPartIndex.ContentItemId), column => column.WithLength(26))
        );

        await SchemaBuilder.AlterTableAsync(nameof(LocationPartIndex), table => table
                .CreateIndex(
                    $"IDX_{nameof(LocationPartIndex)}_{nameof(LocationPartIndex.Country)}",
                    nameof(LocationPartIndex.Country))
            );

        await SchemaBuilder.AlterTableAsync(nameof(LocationPartIndex), table => table
                        .CreateIndex(
                            $"IDX_{nameof(LocationPartIndex)}_{nameof(LocationPartIndex.City)}",
                            nameof(LocationPartIndex.City))
                    );

        await SchemaBuilder.AlterTableAsync(nameof(LocationPartIndex), table => table
                        .CreateIndex(
                            $"IDX_{nameof(LocationPartIndex)}_{nameof(LocationPartIndex.Street)}",
                            nameof(LocationPartIndex.Street))
                    );

        await SchemaBuilder.AlterTableAsync(nameof(LocationPartIndex), table => table
                       .CreateIndex(
                           $"IDX_{nameof(LocationPartIndex)}_{nameof(LocationPartIndex.Site)}",
                           nameof(LocationPartIndex.Site))
                   );

        await SchemaBuilder.AlterTableAsync(nameof(LocationPartIndex), table => table
                       .CreateIndex(
                           $"IDX_{nameof(LocationPartIndex)}_{nameof(LocationPartIndex.Building)}",
                           nameof(LocationPartIndex.Building))
                   );

        await SchemaBuilder.AlterTableAsync(nameof(LocationPartIndex), table => table
                       .CreateIndex(
                           $"IDX_{nameof(LocationPartIndex)}_{nameof(LocationPartIndex.Floor)}",
                           nameof(LocationPartIndex.Floor))
                   );

        await SchemaBuilder.AlterTableAsync(nameof(LocationPartIndex), table => table
                       .CreateIndex(
                           $"IDX_{nameof(LocationPartIndex)}_{nameof(LocationPartIndex.Zone)}",
                           nameof(LocationPartIndex.Zone))
                   );

        await SchemaBuilder.AlterTableAsync(nameof(LocationPartIndex), table => table
                       .CreateIndex(
                           $"IDX_{nameof(LocationPartIndex)}_{nameof(LocationPartIndex.Room)}",
                           nameof(LocationPartIndex.Room))
                   );

        await SchemaBuilder.AlterTableAsync(nameof(LocationPartIndex), table => table
                       .CreateIndex(
                           $"IDX_{nameof(LocationPartIndex)}_{nameof(LocationPartIndex.DateTime)}",
                           nameof(LocationPartIndex.DateTime))
                   );

        // We create a new content type. Note that there's only an alter method: this will create the type if it doesn't
        // exist or modify it if it does. Make sure you understand what content types are:
        // https://docs.orchardcore.net/en/latest/docs/glossary/#content-type. The content type's name is arbitrary but
        // choose a meaningful one. Notice how we use a class with constants to store the type name so we prevent risky
        // copy-pasting.
        await _contentDefinitionManager.AlterTypeDefinitionAsync(ContentTypes.LocationPage, type => type
            .Creatable()
            .Listable()
            // We attach parts by specifying their name. For our own parts we use nameof(): This is not mandatory but
            // serves great if we change the part's name during development.
            .WithPart(nameof(LocationPart))
        );

        return 9;
    }

    public async Task<int> UpdateFrom9Async()
    {
        /*****************************************Specification Part*************************************************/
        await SchemaBuilder.CreateMapIndexTableAsync<SpecificationPartIndex>(table => table
            .Column<string>(nameof(SpecificationPartIndex.Description))
            .Column<string>(nameof(SpecificationPartIndex.AssignerContentItemId))
            .Column<int>(nameof(SpecificationPartIndex.AssigneeContentItemId))
            .Column<string>(nameof(SpecificationPartIndex.Supplement))
            .Column<string>(nameof(SpecificationPartIndex.RootCause))
            .Column<bool>(nameof(SpecificationPartIndex.IsPlanned))
            .Column<bool>(nameof(SpecificationPartIndex.IsIncident))
            .Column<bool>(nameof(SpecificationPartIndex.IsInHouse))
            .Column<bool>(nameof(SpecificationPartIndex.IsOutsourced))
            .Column<string>(nameof(SpecificationPartIndex.Status))
            .Column<string>(nameof(SpecificationPartIndex.OfferContentItemId))
            .Column<string>(nameof(SpecificationPartIndex.Behavior))
            .Column<string>(nameof(SpecificationPartIndex.Asset))
            .Column<string>(nameof(SpecificationPartIndex.Event))
            .Column<string>(nameof(SpecificationPartIndex.Others))
            .Column<string>(nameof(SpecificationPartIndex.Sender))
            .Column<string>(nameof(SpecificationPartIndex.Writer))
            .Column<string>(nameof(SpecificationPartIndex.Photos))
            .Column<string>(nameof(SpecificationPartIndex.Clips))
            .Column<string>(nameof(SpecificationPartIndex.Audio))
            .Column<string>(nameof(SpecificationPartIndex.Files))
            .Column<string>(nameof(SpecificationPartIndex.LocationContentItemId))
            .Column<DateTime>(nameof(SpecificationPartIndex.DateTime))
            // The content item ID is always 26 characters.
            .Column<string>(nameof(SpecificationPartIndex.ContentItemId), column => column.WithLength(26))
        );
        await SchemaBuilder.AlterTableAsync(nameof(SpecificationPartIndex), table => table
                .CreateIndex(
                    $"IDX_{nameof(SpecificationPartIndex)}_{nameof(SpecificationPartIndex.Description)}",
                    nameof(SpecificationPartIndex.Description))
            );

        await SchemaBuilder.AlterTableAsync(nameof(SpecificationPartIndex), table => table
                .CreateIndex(
                    $"IDX_{nameof(SpecificationPartIndex)}_{nameof(SpecificationPartIndex.AssignerContentItemId)}",
                    nameof(SpecificationPartIndex.AssignerContentItemId))
            );

        await SchemaBuilder.AlterTableAsync(nameof(SpecificationPartIndex), table => table
                .CreateIndex(
                    $"IDX_{nameof(SpecificationPartIndex)}_{nameof(SpecificationPartIndex.AssigneeContentItemId)}",
                    nameof(SpecificationPartIndex.AssigneeContentItemId))
            );

        await SchemaBuilder.AlterTableAsync(nameof(SpecificationPartIndex), table => table
                        .CreateIndex(
                            $"IDX_{nameof(SpecificationPartIndex)}_{nameof(SpecificationPartIndex.Supplement)}",
                            nameof(SpecificationPartIndex.Supplement))
                    );

        await SchemaBuilder.AlterTableAsync(nameof(SpecificationPartIndex), table => table
                        .CreateIndex(
                            $"IDX_{nameof(SpecificationPartIndex)}_{nameof(SpecificationPartIndex.RootCause)}",
                            nameof(SpecificationPartIndex.RootCause))
                    );

        await SchemaBuilder.AlterTableAsync(nameof(SpecificationPartIndex), table => table
                       .CreateIndex(
                           $"IDX_{nameof(SpecificationPartIndex)}_{nameof(SpecificationPartIndex.IsPlanned)}",
                           nameof(SpecificationPartIndex.IsPlanned))
                   );

        await SchemaBuilder.AlterTableAsync(nameof(SpecificationPartIndex), table => table
                       .CreateIndex(
                           $"IDX_{nameof(SpecificationPartIndex)}_{nameof(SpecificationPartIndex.IsIncident)}",
                           nameof(SpecificationPartIndex.IsIncident))
                   );

        await SchemaBuilder.AlterTableAsync(nameof(SpecificationPartIndex), table => table
                       .CreateIndex(
                           $"IDX_{nameof(SpecificationPartIndex)}_{nameof(SpecificationPartIndex.IsInHouse)}",
                           nameof(SpecificationPartIndex.IsInHouse))
                   );

        await SchemaBuilder.AlterTableAsync(nameof(SpecificationPartIndex), table => table
                       .CreateIndex(
                           $"IDX_{nameof(SpecificationPartIndex)}_{nameof(SpecificationPartIndex.IsOutsourced)}",
                           nameof(SpecificationPartIndex.IsOutsourced))
                   );

        await SchemaBuilder.AlterTableAsync(nameof(SpecificationPartIndex), table => table
                       .CreateIndex(
                           $"IDX_{nameof(SpecificationPartIndex)}_{nameof(SpecificationPartIndex.Status)}",
                           nameof(SpecificationPartIndex.Status))
                   );

        await SchemaBuilder.AlterTableAsync(nameof(SpecificationPartIndex), table => table
                       .CreateIndex(
                           $"IDX_{nameof(SpecificationPartIndex)}_{nameof(SpecificationPartIndex.OfferContentItemId)}",
                           nameof(SpecificationPartIndex.OfferContentItemId))
                   );

        await SchemaBuilder.AlterTableAsync(nameof(SpecificationPartIndex), table => table
                       .CreateIndex(
                           $"IDX_{nameof(SpecificationPartIndex)}_{nameof(SpecificationPartIndex.Behavior)}",
                           nameof(SpecificationPartIndex.Behavior))
                   );

        await SchemaBuilder.AlterTableAsync(nameof(SpecificationPartIndex), table => table
                       .CreateIndex(
                           $"IDX_{nameof(SpecificationPartIndex)}_{nameof(SpecificationPartIndex.Asset)}",
                           nameof(SpecificationPartIndex.Asset))
                   );

        await SchemaBuilder.AlterTableAsync(nameof(SpecificationPartIndex), table => table
                       .CreateIndex(
                           $"IDX_{nameof(SpecificationPartIndex)}_{nameof(SpecificationPartIndex.Event)}",
                           nameof(SpecificationPartIndex.Event))
                   );

        await SchemaBuilder.AlterTableAsync(nameof(SpecificationPartIndex), table => table
                       .CreateIndex(
                           $"IDX_{nameof(SpecificationPartIndex)}_{nameof(SpecificationPartIndex.Others)}",
                           nameof(SpecificationPartIndex.Others))
                   );

        await SchemaBuilder.AlterTableAsync(nameof(SpecificationPartIndex), table => table
                       .CreateIndex(
                           $"IDX_{nameof(SpecificationPartIndex)}_{nameof(SpecificationPartIndex.Sender)}",
                           nameof(SpecificationPartIndex.Sender))
                   );

        await SchemaBuilder.AlterTableAsync(nameof(SpecificationPartIndex), table => table
                       .CreateIndex(
                           $"IDX_{nameof(SpecificationPartIndex)}_{nameof(SpecificationPartIndex.Writer)}",
                           nameof(SpecificationPartIndex.Writer))
                   );

        await SchemaBuilder.AlterTableAsync(nameof(SpecificationPartIndex), table => table
                       .CreateIndex(
                           $"IDX_{nameof(SpecificationPartIndex)}_{nameof(SpecificationPartIndex.Photos)}",
                           nameof(SpecificationPartIndex.Photos))
                   );

        await SchemaBuilder.AlterTableAsync(nameof(SpecificationPartIndex), table => table
                       .CreateIndex(
                           $"IDX_{nameof(SpecificationPartIndex)}_{nameof(SpecificationPartIndex.Clips)}",
                           nameof(SpecificationPartIndex.Clips))
                   );

        await SchemaBuilder.AlterTableAsync(nameof(SpecificationPartIndex), table => table
                       .CreateIndex(
                           $"IDX_{nameof(SpecificationPartIndex)}_{nameof(SpecificationPartIndex.Audio)}",
                           nameof(SpecificationPartIndex.Audio))
                   );

        await SchemaBuilder.AlterTableAsync(nameof(SpecificationPartIndex), table => table
                       .CreateIndex(
                           $"IDX_{nameof(SpecificationPartIndex)}_{nameof(SpecificationPartIndex.Files)}",
                           nameof(SpecificationPartIndex.Files))
                   );

        await SchemaBuilder.AlterTableAsync(nameof(SpecificationPartIndex), table => table
                       .CreateIndex(
                           $"IDX_{nameof(SpecificationPartIndex)}_{nameof(SpecificationPartIndex.LocationContentItemId)}",
                           nameof(SpecificationPartIndex.LocationContentItemId))
                   );

        await SchemaBuilder.AlterTableAsync(nameof(SpecificationPartIndex), table => table
                       .CreateIndex(
                           $"IDX_{nameof(SpecificationPartIndex)}_{nameof(SpecificationPartIndex.DateTime)}",
                           nameof(SpecificationPartIndex.DateTime))
                   );

        // We create a new content type. Note that there's only an alter method: this will create the type if it doesn't
        // exist or modify it if it does. Make sure you understand what content types are:
        // https://docs.orchardcore.net/en/latest/docs/glossary/#content-type. The content type's name is arbitrary but
        // choose a meaningful one. Notice how we use a class with constants to store the type name so we prevent risky
        // copy-pasting.
        await _contentDefinitionManager.AlterTypeDefinitionAsync(ContentTypes.SpecificationPage, type => type
            .Creatable()
            .Listable()
            // We attach parts by specifying their name. For our own parts we use nameof(): This is not mandatory but
            // serves great if we change the part's name during development.
            .WithPart(nameof(SpecificationPart))
        );

        await _contentDefinitionManager.AlterPartDefinitionAsync(typeof(SpecificationPart).Name,
            cfg => cfg
            .Attachable());

        return 10;
    }

    public async Task<int> UpdateFrom10Async()
    {
        await SchemaBuilder.AlterTableAsync(nameof(SpecificationPartIndex), table => table
                        .DropIndex(
                            $"IDX_{nameof(SpecificationPartIndex)}_{nameof(SpecificationPartIndex.AssigneeContentItemId)}"));

        await SchemaBuilder.AlterTableAsync(nameof(SpecificationPartIndex), table => table
                        .DropColumn(nameof(SpecificationPartIndex.AssigneeContentItemId)));

        await SchemaBuilder.AlterTableAsync(nameof(SpecificationPartIndex), table => table
                        .AddColumn<string>(nameof(SpecificationPartIndex.AssigneeContentItemId)));

        await SchemaBuilder.AlterTableAsync(nameof(SpecificationPartIndex), table => table
                .CreateIndex(
                    $"IDX_{nameof(SpecificationPartIndex)}_{nameof(SpecificationPartIndex.AssigneeContentItemId)}",
                    nameof(SpecificationPartIndex.AssigneeContentItemId))
            );

        return 11;
    }

    public async Task<int> UpdateFrom11Async()
    {
        /*****************************************Staff Part*************************************************/
        await SchemaBuilder.CreateMapIndexTableAsync<StaffPartIndex>(table => table
            .Column<string>(nameof(StaffPartIndex.Nickname))
            .Column<string>(nameof(StaffPartIndex.AvatarId))
            .Column<string>(nameof(StaffPartIndex.Operator))
            .Column<string>(nameof(StaffPartIndex.Team))
            .Column<string>(nameof(StaffPartIndex.FullName))
            .Column<string>(nameof(StaffPartIndex.UserName))
            .Column<string>(nameof(StaffPartIndex.BookmarkedReportContentItemIds))
            .Column<decimal>(nameof(StaffPartIndex.Balance))
            .Column<string>(nameof(StaffPartIndex.Currency))
            .Column<string>(nameof(StaffPartIndex.CustomNickname))
            .Column<string>(nameof(StaffPartIndex.StaffId))
            .Column<DateTime>(nameof(StaffPartIndex.Birthday))
            .Column<DateTime>(nameof(StaffPartIndex.DateTime))
            // The content item ID is always 26 characters.
            .Column<string>(nameof(StaffPartIndex.ContentItemId), column => column.WithLength(26))
        );

        await SchemaBuilder.AlterTableAsync(nameof(StaffPartIndex), table => table
               .CreateIndex(
                   $"IDX_{nameof(StaffPartIndex)}_{nameof(StaffPartIndex.Nickname)}",
                   nameof(StaffPartIndex.Nickname))
           );

        await SchemaBuilder.AlterTableAsync(nameof(StaffPartIndex), table => table
                       .CreateIndex(
                           $"IDX_{nameof(StaffPartIndex)}_{nameof(StaffPartIndex.AvatarId)}",
                           nameof(StaffPartIndex.AvatarId))
                   );

        await SchemaBuilder.AlterTableAsync(nameof(StaffPartIndex), table => table
                       .CreateIndex(
                           $"IDX_{nameof(StaffPartIndex)}_{nameof(StaffPartIndex.Operator)}",
                           nameof(StaffPartIndex.Operator))
                   );

        await SchemaBuilder.AlterTableAsync(nameof(StaffPartIndex), table => table
                       .CreateIndex(
                           $"IDX_{nameof(StaffPartIndex)}_{nameof(StaffPartIndex.Team)}",
                           nameof(StaffPartIndex.Team))
                   );

        await SchemaBuilder.AlterTableAsync(nameof(StaffPartIndex), table => table
                       .CreateIndex(
                           $"IDX_{nameof(StaffPartIndex)}_{nameof(StaffPartIndex.FullName)}",
                           nameof(StaffPartIndex.FullName))
                   );

        await SchemaBuilder.AlterTableAsync(nameof(StaffPartIndex), table => table
                       .CreateIndex(
                           $"IDX_{nameof(StaffPartIndex)}_{nameof(StaffPartIndex.UserName)}",
                           nameof(StaffPartIndex.UserName))
                   );

        await SchemaBuilder.AlterTableAsync(nameof(StaffPartIndex), table => table
                       .CreateIndex(
                           $"IDX_{nameof(StaffPartIndex)}_{nameof(StaffPartIndex.BookmarkedReportContentItemIds)}",
                           nameof(StaffPartIndex.BookmarkedReportContentItemIds))
                   );

        await SchemaBuilder.AlterTableAsync(nameof(StaffPartIndex), table => table
                       .CreateIndex(
                           $"IDX_{nameof(StaffPartIndex)}_{nameof(StaffPartIndex.Balance)}",
                           nameof(StaffPartIndex.Balance))
                   );

        await SchemaBuilder.AlterTableAsync(nameof(StaffPartIndex), table => table
                       .CreateIndex(
                           $"IDX_{nameof(StaffPartIndex)}_{nameof(StaffPartIndex.Currency)}",
                           nameof(StaffPartIndex.Currency))
                   );

        await SchemaBuilder.AlterTableAsync(nameof(StaffPartIndex), table => table
                       .CreateIndex(
                           $"IDX_{nameof(StaffPartIndex)}_{nameof(StaffPartIndex.CustomNickname)}",
                           nameof(StaffPartIndex.CustomNickname))
                   );

        await SchemaBuilder.AlterTableAsync(nameof(StaffPartIndex), table => table
                       .CreateIndex(
                           $"IDX_{nameof(StaffPartIndex)}_{nameof(StaffPartIndex.StaffId)}",
                           nameof(StaffPartIndex.StaffId))
                   );

        await SchemaBuilder.AlterTableAsync(nameof(StaffPartIndex), table => table
                       .CreateIndex(
                           $"IDX_{nameof(StaffPartIndex)}_{nameof(StaffPartIndex.Birthday)}",
                           nameof(StaffPartIndex.Birthday))
                   );

        await SchemaBuilder.AlterTableAsync(nameof(StaffPartIndex), table => table
                       .CreateIndex(
                           $"IDX_{nameof(StaffPartIndex)}_{nameof(StaffPartIndex.DateTime)}",
                           nameof(StaffPartIndex.DateTime))
                   );

        // We create a new content type. Note that there's only an alter method: this will create the type if it doesn't
        // exist or modify it if it does. Make sure you understand what content types are:
        // https://docs.orchardcore.net/en/latest/docs/glossary/#content-type. The content type's name is arbitrary but
        // choose a meaningful one. Notice how we use a class with constants to store the type name so we prevent risky
        // copy-pasting.
        await _contentDefinitionManager.AlterTypeDefinitionAsync(ContentTypes.StaffPage, type => type
            .Creatable()
            .Listable()
            // We attach parts by specifying their name. For our own parts we use nameof(): This is not mandatory but
            // serves great if we change the part's name during development.
            .WithPart(nameof(StaffPart))
        );

        await _contentDefinitionManager.AlterPartDefinitionAsync(typeof(StaffPart).Name,
            cfg => cfg
            .Attachable());

        return 12;
    }
}

// NEXT STATION: Indexes/PersonPartIndex
