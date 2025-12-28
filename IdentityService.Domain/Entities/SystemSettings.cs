using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace IdentityService.Domain.Entities
{
    public class SystemSettings
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public int MaxFailedLoginAttempts { get; set; }
        public int LockoutMinutes { get; set; }
        public int MaxLockouts { get; set; }

        public TimeSpan LockoutDuration => TimeSpan.FromMinutes(LockoutMinutes);

        // ==== CONSTRUCTORES ====
        public SystemSettings() { } // EF Core
    }
}
