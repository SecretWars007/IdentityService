using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace IdentityService.Application.UseCases.Users
{
    public record RegisterUserCommand(
        string FullName,
        string Email,
        string Password,
        string DocumentNumber,
        string Address,
        string PhoneNumber,
        string? PhotoUrl,
        string Role
    );
}
