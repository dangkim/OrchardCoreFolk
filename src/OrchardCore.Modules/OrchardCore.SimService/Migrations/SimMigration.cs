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
using OrchardCore.SimService.ContentParts;

namespace OrchardCore.SimService.Migrations;
public class SimMigration : DataMigration
{
    private readonly IContentDefinitionManager _contentDefinitionManager;

    public SimMigration(IContentDefinitionManager contentDefinitionManager) => _contentDefinitionManager = contentDefinitionManager;

    public async Task<int> CreateAsync()
    {
                await _contentDefinitionManager.AlterPartDefinitionAsync(nameof(UserProfilePart), part => part
            .Attachable()
        );

        await _contentDefinitionManager.AlterTypeDefinitionAsync("UserProfile", type => type
            .Creatable()
            .Listable()
            .WithPart(nameof(UserProfilePart))

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

        return 1;
    }
}
