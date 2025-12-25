using IdentityService.Application.Exceptions;
using IdentityService.Application.Interfaces;
using IdentityService.Domain.Entities;

namespace IdentityService.Application.Services
{
    public class AuthorizationService : IAuthorizationService
    {
        public void EnsurePermission(User user, string permission)
        {
            var permissions = user
                .Roles.SelectMany(r => r.Role.Permissions)
                .Select(p => p.Permission.Code);

            if (!permissions.Contains(permission))
                throw new ForbiddenException("Permiso denegado");
        }

        public void EnsureOwner(Guid resourceOwnerId, Guid userId)
        {
            if (resourceOwnerId != userId)
                throw new ForbiddenException("IDOR detectado");
        }
    }
}
