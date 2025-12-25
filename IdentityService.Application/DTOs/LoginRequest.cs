using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace IdentityService.Application.DTOs
{
    public record LoginRequest(string Email, string Password, string? MfaCode);
}
