using System;
using OrchardCore.ContentFields.Fields;
using OrchardCore.ContentFields.Settings;
using OrchardCore.ContentManagement.Metadata;
using OrchardCore.ContentManagement.Metadata.Settings;
using OrchardCore.Data.Migration;
using YesSql.Sql;
using OrchardCore.SimService.Indexes;
using OrchardCore.SimService.Models;
using System.Threading.Tasks;

namespace OrchardCore.SimService.Migrations;
public class SimMigration : DataMigration
{
    private readonly IContentDefinitionManager _contentDefinitionManager;

    public SimMigration(IContentDefinitionManager contentDefinitionManager) => _contentDefinitionManager = contentDefinitionManager;

    public async Task<int> CreateAsync()
    {
        await _contentDefinitionManager.AlterPartDefinitionAsync(nameof(OrderDetailPart), part => part
                .Attachable()
            );

        await _contentDefinitionManager.AlterPartDefinitionAsync(nameof(SmsPart), part => part
            .Attachable()
        );

        await _contentDefinitionManager.AlterTypeDefinitionAsync("Orders", type => type
            .Creatable()
            .Listable()
            .WithPart(nameof(OrderDetailPart))
            .WithPart(nameof(SmsPart))
        );

        await _contentDefinitionManager.AlterPartDefinitionAsync(nameof(PaymentDetailPart), part => part
            .Attachable()
        );

        await _contentDefinitionManager.AlterTypeDefinitionAsync("Payments", type => type
            .Creatable()
            .Listable()
            .WithPart(nameof(PaymentDetailPart))
        );

        await _contentDefinitionManager.AlterPartDefinitionAsync(nameof(UserProfilePart), part => part
            .Attachable()
        );

        await _contentDefinitionManager.AlterTypeDefinitionAsync("UserProfile", type => type
            .Creatable()
            .Listable()
            .WithPart(nameof(UserProfilePart))

        );

        await SchemaBuilder.CreateMapIndexTableAsync<OrderDetailPartIndex>(table => table
            .Column<string>(nameof(OrderDetailPartIndex.ContentItemId), column => column.WithLength(26))
            .Column<long>(nameof(OrderDetailPartIndex.OrderId))
            .Column<string>(nameof(OrderDetailPartIndex.Phone), column => column.WithLength(20))
            .Column<string>(nameof(OrderDetailPartIndex.Operator), column => column.WithLength(50))
            .Column<string>(nameof(OrderDetailPartIndex.Product), column => column.WithLength(200))
            .Column<decimal>(nameof(OrderDetailPartIndex.Price))
            .Column<string>(nameof(OrderDetailPartIndex.Status), column => column.WithLength(18))
            .Column<DateTime>(nameof(OrderDetailPartIndex.Expires))
            .Column<DateTime>(nameof(OrderDetailPartIndex.Created_at))
            .Column<string>(nameof(OrderDetailPartIndex.Country), column => column.WithLength(50))
            .Column<string>(nameof(OrderDetailPartIndex.Email))
            .Column<int>(nameof(OrderDetailPartIndex.UserId))
            .Column<string>(nameof(OrderDetailPartIndex.UserName))
            .Column<string>(nameof(OrderDetailPartIndex.Category))
        );

        await SchemaBuilder.AlterTableAsync(nameof(OrderDetailPartIndex), table => table
            .CreateIndex(
                $"IDX_{nameof(OrderDetailPartIndex)}_{nameof(OrderDetailPartIndex.OrderId)}",
                nameof(OrderDetailPartIndex.OrderId))
        );

        await SchemaBuilder.AlterTableAsync(nameof(OrderDetailPartIndex), table => table
            .CreateIndex(
                $"IDX_{nameof(OrderDetailPartIndex)}_{nameof(OrderDetailPartIndex.UserId)}",
                nameof(OrderDetailPartIndex.UserId))
        );

        await SchemaBuilder.AlterTableAsync(nameof(OrderDetailPartIndex), table => table
            .CreateIndex(
                $"IDX_{nameof(OrderDetailPartIndex)}_{nameof(OrderDetailPartIndex.UserName)}",
                nameof(OrderDetailPartIndex.UserName))
        );

        await SchemaBuilder.AlterTableAsync(nameof(OrderDetailPartIndex), table => table
            .CreateIndex(
                $"IDX_{nameof(OrderDetailPartIndex)}_{nameof(OrderDetailPartIndex.Email)}",
                nameof(OrderDetailPartIndex.Email))
        );

        await SchemaBuilder.AlterTableAsync(nameof(OrderDetailPartIndex), table => table
                .CreateIndex(
                    $"IDX_{nameof(OrderDetailPartIndex)}_{nameof(OrderDetailPartIndex.Category)}",
                    nameof(OrderDetailPartIndex.Category))
        );

        ////////////////////// UserProfile////////////////////////
        await SchemaBuilder.CreateMapIndexTableAsync<UserProfilePartIndex>(table => table
            .Column<string>(nameof(UserProfilePartIndex.ContentItemId), column => column.WithLength(26))
            .Column<int>(nameof(UserProfilePartIndex.ProfileId))
            .Column<string>(nameof(UserProfilePartIndex.Vendor), column => column.WithLength(26))
            .Column<string>(nameof(UserProfilePartIndex.DefaultForwardingNumber), column => column.WithLength(26))
            .Column<decimal>(nameof(UserProfilePartIndex.Balance))
            .Column<int>(nameof(UserProfilePartIndex.Rating))
            .Column<string>(nameof(UserProfilePartIndex.DefaultCoutryName), column => column.WithLength(26))
            .Column<string>(nameof(UserProfilePartIndex.DefaultIso), column => column.WithLength(26))
            .Column<string>(nameof(UserProfilePartIndex.DefaultPrefix), column => column.WithLength(26))
            .Column<string>(nameof(UserProfilePartIndex.DefaultOperatorName), column => column.WithLength(26))
            .Column<decimal>(nameof(UserProfilePartIndex.FrozenBalance))
            .Column<string>(nameof(UserProfilePartIndex.Email))
            .Column<int>(nameof(UserProfilePartIndex.UserId))
            .Column<string>(nameof(UserProfilePartIndex.UserName))
            .Column<decimal>(nameof(UserProfilePartIndex.OriginalAmount))
            .Column<decimal>(nameof(UserProfilePartIndex.Amount))
            .Column<decimal>(nameof(UserProfilePartIndex.RateInUsd))
            .Column<string>(nameof(UserProfilePartIndex.GmailMsgId), column => column.WithLength(20))
            .Column<string>(nameof(UserProfilePartIndex.TokenApi))
        );

        await SchemaBuilder.AlterTableAsync(nameof(UserProfilePartIndex), table => table
            .CreateIndex(
                $"IDX_{nameof(UserProfilePartIndex)}_{nameof(UserProfilePartIndex.ProfileId)}",
                nameof(UserProfilePartIndex.ProfileId))
        );

        await SchemaBuilder.AlterTableAsync(nameof(UserProfilePartIndex), table => table
            .CreateIndex(
                $"IDX_{nameof(UserProfilePartIndex)}_{nameof(UserProfilePartIndex.UserId)}",
                nameof(UserProfilePartIndex.UserId))
        );

        await SchemaBuilder.AlterTableAsync(nameof(UserProfilePartIndex), table => table
            .CreateIndex(
                $"IDX_{nameof(UserProfilePartIndex)}_{nameof(UserProfilePartIndex.UserName)}",
                nameof(UserProfilePartIndex.UserName))
        );

        await SchemaBuilder.AlterTableAsync(nameof(UserProfilePartIndex), table => table
            .CreateIndex(
                $"IDX_{nameof(UserProfilePartIndex)}_{nameof(UserProfilePartIndex.Email)}",
                nameof(UserProfilePartIndex.Email))
        );

        await SchemaBuilder.AlterTableAsync(nameof(UserProfilePartIndex), table => table
                .CreateIndex(
                    $"IDX_{nameof(UserProfilePartIndex)}_{nameof(UserProfilePartIndex.TokenApi)}",
                    nameof(UserProfilePartIndex.TokenApi))
        );

        ////////////////////////////Payments////////////////////////
        await SchemaBuilder.CreateMapIndexTableAsync<PaymentDetailPartIndex>(table => table
            .Column<string>(nameof(PaymentDetailPartIndex.ContentItemId), column => column.WithLength(26))
            .Column<int>(nameof(PaymentDetailPartIndex.PaymentId))
            .Column<string>(nameof(PaymentDetailPartIndex.TypeName), column => column.WithLength(26))
            .Column<string>(nameof(PaymentDetailPartIndex.ProviderName), column => column.WithLength(26))
            .Column<decimal>(nameof(PaymentDetailPartIndex.Amount), column => column.WithLength(26))
            .Column<decimal>(nameof(PaymentDetailPartIndex.Balance))
            .Column<DateTime>(nameof(PaymentDetailPartIndex.CreatedAt))
            .Column<string>(nameof(PaymentDetailPartIndex.Email))
            .Column<int>(nameof(PaymentDetailPartIndex.UserId))
            .Column<string>(nameof(PaymentDetailPartIndex.UserName))
            .Column<long>(nameof(PaymentDetailPartIndex.OrderId))
        );

        await SchemaBuilder.AlterTableAsync(nameof(PaymentDetailPartIndex), table => table
            .CreateIndex(
                $"IDX_{nameof(PaymentDetailPartIndex)}_{nameof(PaymentDetailPartIndex.PaymentId)}",
                nameof(PaymentDetailPartIndex.PaymentId))
        );

        await SchemaBuilder.AlterTableAsync(nameof(PaymentDetailPartIndex), table => table
            .CreateIndex(
                $"IDX_{nameof(PaymentDetailPartIndex)}_{nameof(PaymentDetailPartIndex.UserId)}",
                nameof(PaymentDetailPartIndex.UserId))
        );

        await SchemaBuilder.AlterTableAsync(nameof(PaymentDetailPartIndex), table => table
            .CreateIndex(
                $"IDX_{nameof(PaymentDetailPartIndex)}_{nameof(PaymentDetailPartIndex.UserName)}",
                nameof(PaymentDetailPartIndex.UserName))
        );

        await SchemaBuilder.AlterTableAsync(nameof(PaymentDetailPartIndex), table => table
            .CreateIndex(
                $"IDX_{nameof(PaymentDetailPartIndex)}_{nameof(PaymentDetailPartIndex.Email)}",
                nameof(PaymentDetailPartIndex.Email))
        );

        await SchemaBuilder.AlterTableAsync(nameof(PaymentDetailPartIndex), table => table
                .CreateIndex(
                    $"IDX_{nameof(PaymentDetailPartIndex)}_{nameof(PaymentDetailPartIndex.OrderId)}",
                    nameof(PaymentDetailPartIndex.OrderId))
        );

        //////////////////////////////SMS part///////////////////////////////
        await SchemaBuilder.CreateMapIndexTableAsync<SmsPartIndex>(table => table
            .Column<string>(nameof(SmsPartIndex.ContentItemId), column => column.WithLength(26))
            .Column<string>(nameof(SmsPartIndex.Sender), column => column.WithLength(26))
            .Column<string>(nameof(SmsPartIndex.Text))
            .Column<string>(nameof(SmsPartIndex.Code))
            .Column<DateTime>(nameof(SmsPartIndex.Created_at))
            .Column<string>(nameof(SmsPartIndex.Email))
            .Column<int>(nameof(SmsPartIndex.UserId))
            .Column<string>(nameof(SmsPartIndex.UserName))
            .Column<long>(nameof(SmsPartIndex.OrderId))
            .Column<DateTime>(nameof(SmsPartIndex.Date))
        );

        await SchemaBuilder.AlterTableAsync(nameof(SmsPartIndex), table => table
            .CreateIndex(
                $"IDX_{nameof(SmsPartIndex)}_{nameof(SmsPartIndex.UserId)}",
                nameof(SmsPartIndex.UserId))
        );

        await SchemaBuilder.AlterTableAsync(nameof(SmsPartIndex), table => table
            .CreateIndex(
                $"IDX_{nameof(SmsPartIndex)}_{nameof(SmsPartIndex.UserName)}",
                nameof(SmsPartIndex.UserName))
        );

        await SchemaBuilder.AlterTableAsync(nameof(SmsPartIndex), table => table
            .CreateIndex(
                $"IDX_{nameof(SmsPartIndex)}_{nameof(SmsPartIndex.Email)}",
                nameof(SmsPartIndex.Email))
        );

        await SchemaBuilder.AlterTableAsync(nameof(SmsPartIndex), table => table
                .CreateIndex(
                    $"IDX_{nameof(SmsPartIndex)}_{nameof(SmsPartIndex.OrderId)}",
                    nameof(SmsPartIndex.OrderId))
            );

        return 1;
    }
}
