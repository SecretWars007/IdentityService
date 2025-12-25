using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace IdentityService.Application.Interfaces
{
    public interface IConfirmMfaUseCase
    {
        Task ExecuteAsync(Domain.Entities.User user, string code, string ip);
    }
}
