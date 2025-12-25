using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace IdentityService.Application.DTOs
{
    public record AuthResponse(string AccessToken, string RefreshToken);
}
