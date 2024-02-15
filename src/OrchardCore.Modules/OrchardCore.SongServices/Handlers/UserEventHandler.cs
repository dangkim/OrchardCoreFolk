using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using OrchardCore.Users;

namespace OrchardCore.SongServices.Handlers
{
    public class UserEventHandler
    {
        private readonly IHttpContextAccessor _hca;        

        public UserEventHandler(
            IHttpContextAccessor hca)
        {
            _hca = hca;
        }

        // Need to resolve the UserManager from the HttpContext to prevent circular dependency. 
        public UserManager<IUser> GetUserManagerFromHttpContext() =>
            _hca.HttpContext.RequestServices.GetRequiredService<UserManager<IUser>>();
    }
}
