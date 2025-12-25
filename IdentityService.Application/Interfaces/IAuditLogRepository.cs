using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using IdentityService.Domain.Entities;

namespace IdentityService.Application.Interfaces
{
    public interface IAuditLogRepository
    {
        Task LogAsync(AuditLog log);
    }
}
