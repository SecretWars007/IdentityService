using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace IdentityService.Domain.Entities
{
    public class Permission
    {
        public Guid Id { get; set; }
        public string Code { get; set; } = null!;
        public string Description { get; set; } = null!;

        public Permission() { }

        public Permission(string code, string description)
        {
            Id = Guid.NewGuid();
            Code = code;
            Description = description;
        }
    }
}
