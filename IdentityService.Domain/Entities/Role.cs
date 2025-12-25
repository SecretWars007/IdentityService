using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace IdentityService.Domain.Entities
{
    public class Role
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = null!;

        public ICollection<UserRole> Users { get; set; } = new List<UserRole>();
        public ICollection<RolePermission> Permissions { get; set; } = new List<RolePermission>();

        public Role() { }

        public Role(string name)
        {
            Id = Guid.NewGuid();
            Name = name.ToUpperInvariant();
        }
    }
}
