using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using IdentityService.Domain.Entities;

namespace IdentityService.Application.Interfaces
{
    public interface IMfaService
    {
        (string secret, string qrBase64) GenerateMfa(User user);
        bool ValidateMfa(string secret, string code);
    }
}
