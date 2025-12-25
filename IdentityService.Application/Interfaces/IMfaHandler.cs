using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using IdentityService.Application.DTOs;
using IdentityService.Domain.Entities;

namespace IdentityService.Application.Interfaces
{
    public interface IMfaHandler
    {
        Task<EnableMfaResponse> EnableMfa(User user, string ipAddress);
        Task ConfirmMfa(User user, string code, string ipAddress);
        Task DisableMfa(User user, string ipAddress);
    }
}
