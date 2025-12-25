using IdentityService.Api.Security;
using Microsoft.AspNetCore.Authorization;

namespace IdentityService.Api.Extensions
{
    public static class AuthorizationExtensions
    {
        public static IServiceCollection AddDynamicPolicies(this IServiceCollection services)
        {
            services.AddSingleton<IAuthorizationPolicyProvider, DynamicPolicyProvider>();
            services.AddSingleton<IAuthorizationHandler, PermissionHandler>();
            return services;
        }
    }
}
