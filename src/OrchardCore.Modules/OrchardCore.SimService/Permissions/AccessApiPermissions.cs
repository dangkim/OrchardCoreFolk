using OrchardCore.Security.Permissions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OrchardCore.SimService.Permissions
{
    public class SimApiPermissions : IPermissionProvider
    {       
        public static readonly Permission AccessContentApi = 
            new Permission(nameof(AccessContentApi), "Can access content api");

        public static readonly Permission ExecuteGraphQL =
            new Permission(nameof(ExecuteGraphQL), "Can execute GraphQL");


        public Task<IEnumerable<Permission>> GetPermissionsAsync() =>
            Task.FromResult(new[]
            {
                AccessContentApi,
                ExecuteGraphQL
            }
            .AsEnumerable());

        public IEnumerable<PermissionStereotype> GetDefaultStereotypes() =>
            new[]
            {
                new PermissionStereotype
                {
                    Name = "NormalUser",
                    Permissions = new[] { AccessContentApi, ExecuteGraphQL },
                },
            };
    }
}
