using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using IdentityService.Application.Interfaces;
using IdentityService.Domain.Entities;

namespace IdentityService.Infrastructure.Persistence
{
    public class AuditLogRepository : IAuditLogRepository
    {
        private readonly IdentityDbContext _dbContext;

        public AuditLogRepository(IdentityDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task LogAsync(AuditLog log)
        {
            _dbContext.AuditLogs.Add(log);
            await _dbContext.SaveChangesAsync();
        }
    }
}
