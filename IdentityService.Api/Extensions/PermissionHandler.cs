using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;

namespace IdentityService.Api.Extensions
{
    public class PermissionHandler : AuthorizationHandler<PermissionRequirement>
    {
        protected override Task HandleRequirementAsync(
            AuthorizationHandlerContext context,
            PermissionRequirement requirement
        )
        {
            var permission = context.Resource as string;

            if (permission == null)
                return Task.CompletedTask;

            if (context.User.Claims.Any(c => c.Type == "permission" && c.Value == permission))
            {
                context.Succeed(requirement);
            }

            return Task.CompletedTask;
        }
    }
}
