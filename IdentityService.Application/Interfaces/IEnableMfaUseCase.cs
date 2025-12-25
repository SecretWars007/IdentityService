using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using IdentityService.Application.DTOs;
using IdentityService.Domain.Entities;

namespace IdentityService.Application.Interfaces
{
    public interface IEnableMfaUseCase
    {
        Task<EnableMfaResponse> ExecuteAsync(User user, string ip);
    }
}
