using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using IdentityService.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;

namespace IdentityService.Application.Authorization
{
    public class PermissionAuthorizationHandler : AuthorizationHandler<PermissionRequirement>
    {
        private readonly IUserRepository _userRepository;

        public PermissionAuthorizationHandler(IUserRepository userRepository)
        {
            _userRepository = userRepository;
        }

        protected override async Task HandleRequirementAsync(
            AuthorizationHandlerContext context,
            PermissionRequirement requirement
        )
        {
            var userIdClaim = context.User.FindFirst(ClaimTypes.NameIdentifier);

            if (userIdClaim == null)
                return;

            if (!Guid.TryParse(userIdClaim.Value, out var userId))
                return;

            var hasPermission = await _userRepository.UserHasPermissionAsync(
                userId,
                requirement.Permission
            );

            if (hasPermission)
                context.Succeed(requirement);
        }
    }
}
