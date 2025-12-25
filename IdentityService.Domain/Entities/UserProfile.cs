using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace IdentityService.Domain.Entities
{
    using System;

    namespace IdentityService.Domain.Entities
    {
        public class UserProfile
        {
            public Guid Id { get; set; } = Guid.NewGuid(); // 🔑 Clave primaria
            public Guid UserId { get; set; }
            public string? DocumentNumber { get; set; }
            public string? Address { get; set; }
            public string? Phone { get; set; }
            public string? PhotoUrl { get; set; }

            public User User { get; set; } = null!;

            public UserProfile() { }

            public UserProfile(
                Guid userId,
                string? documentNumber,
                string? address,
                string? phone,
                string? photoUrl
            )
            {
                Id = Guid.NewGuid();
                UserId = userId;
                DocumentNumber = documentNumber;
                Address = address;
                Phone = phone;
                PhotoUrl = photoUrl;
            }
        }
    }
}
