using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using IdentityService.Domain.Entities;

namespace IdentityService.Application.Interfaces
{
    public interface IAuthorizationService
    {
        void EnsureOwner(Guid resourceOwnerId, Guid userId);
        void EnsurePermission(User user, string permission);
    }
}
