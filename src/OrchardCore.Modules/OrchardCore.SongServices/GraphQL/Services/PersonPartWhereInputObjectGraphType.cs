using GraphQL.Types;
using OrchardCore.SongServices.Indexes;
using OrchardCore.SongServices.ContentParts;
using OrchardCore.Apis.GraphQL.Queries;
using static OrchardCore.SongServices.GraphQL.Services.PersonPartObjectGraphType;

namespace OrchardCore.SongServices.GraphQL.Services;

// This class adds a content part-specific section in the "where" filter. It relies on the content part index to
// automatically generate YesSql logic for database-side filtering.
public class PersonPartWhereInputObjectGraphType : WhereInputObjectGraphType<PersonPart>
{
    public PersonPartWhereInputObjectGraphType()
    {
        // Since filters depend on the index, we use their "nameof" as reference.
        AddScalarFilterFields<DateTimeGraphType>(nameof(PersonPartIndex.BirthDateUtc), BirthDateDescription);

        // Since filters depend on the index, we use their "nameof" as reference.
        AddScalarFilterFields<StringGraphType>(nameof(PersonPartIndex.Name), NameDescription);

        // Using the enumeration graph type turns the filter into a dropdown of the valid options in GraphiQL.
        AddScalarFilterFields<HandednessEnumerationGraphType>(nameof(PersonPartIndex.Handedness), HandednessDescription);
    }
}

// NEXT STATION: Services/PersonPartIndexAliasProvider.cs
