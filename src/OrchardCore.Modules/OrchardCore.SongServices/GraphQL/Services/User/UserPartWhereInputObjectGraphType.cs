using GraphQL.Types;
using OrchardCore.SongServices.Indexes;
using OrchardCore.SongServices.ContentParts;
using OrchardCore.Apis.GraphQL.Queries;
using static OrchardCore.SongServices.GraphQL.Services.User.UserPartObjectGraphType;

namespace OrchardCore.SongServices.GraphQL.Services.User;

// This class adds a content part-specific section in the "where" filter. It relies on the content part index to
// automatically generate YesSql logic for database-side filtering.
public class UserPartWhereInputObjectGraphType : WhereInputObjectGraphType<UserPart>
{
    public UserPartWhereInputObjectGraphType()
    {        
        // Since filters depend on the index, we use their "nameof" as reference.
        AddScalarFilterFields<StringGraphType>(nameof(UserPartIndex.Name), NameDescription);
        AddScalarFilterFields<StringGraphType>(nameof(UserPartIndex.Currency), CurrencyDescription);
        AddScalarFilterFields<StringGraphType>(nameof(UserPartIndex.UserName), UserNameDescription);
        AddScalarFilterFields<StringGraphType>(nameof(UserPartIndex.DateTime), DateTimeDescription);
    }
}
