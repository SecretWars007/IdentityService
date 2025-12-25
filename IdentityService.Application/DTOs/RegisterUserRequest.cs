using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace IdentityService.Application.DTOs
{
    public class RegisterUserRequest
    {
        public string FullName { get; set; } = null!;
        public string Email { get; set; } = null!;
        public string Password { get; set; } = null!;

        // Datos personales
        public string DocumentNumber { get; set; } = null!;
        public string Address { get; set; } = null!;
        public string Phone { get; set; } = null!;
        public DateTime BirthDate { get; set; }

        // Seguridad / roles
        public Guid RoleId { get; set; }

        // Foto (base64 o url)
        public string? PhotoUrl { get; set; }
    }
}
