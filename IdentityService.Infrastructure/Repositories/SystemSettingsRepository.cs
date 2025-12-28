using IdentityService.Application.Interfaces;
using IdentityService.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace IdentityService.Infrastructure.Repositories
{
    public class SystemSettingsRepository : ISystemSettingsRepository
    {
        private readonly IdentityDbContext _context;

        public SystemSettingsRepository(IdentityDbContext context)
        {
            _context = context;
        }

        public async Task<SystemSettings> GetAsync()
        {
            return await _context.SystemSettings.AsNoTracking().FirstAsync();
        }
    }
}
