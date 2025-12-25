using IdentityService.Application.Interfaces;
using IdentityService.Domain.Entities;
using IdentityService.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace IdentityService.Infrastructure.Repositories;

public sealed class UserRepository : IUserRepository
{
    private readonly IdentityDbContext _context;

    public UserRepository(IdentityDbContext context)
    {
        _context = context;
    }

    public async Task<User?> GetByEmailAsync(string email)
    {
        return await _context
            .Users.Include(u => u.Roles)
            .ThenInclude(ur => ur.Role)
            .Include(ux => ux.Mfa) // 🔥 CRÍTICO
            .AsNoTracking()
            .FirstOrDefaultAsync(u => u.Email == email);
    }

    public async Task<User?> GetByIdAsync(Guid id)
    {
        return await _context
            .Users.Include(u => u.Roles)
            .ThenInclude(ur => ur.Role)
            .Include(ux => ux.Mfa) // 🔥 CRÍTICO
            .AsNoTracking()
            .FirstOrDefaultAsync(u => u.Id == id);
    }

    public async Task AddAsync(User user)
    {
        await _context.Users.AddAsync(user);
        await _context.SaveChangesAsync();
    }

    public async Task<bool> UserHasPermissionAsync(Guid userId, string permission)
    {
        return await _context
            .UserRoles.Where(ur => ur.UserId == userId)
            .SelectMany(ur => ur.Role.Permissions)
            .AnyAsync(rp => rp.Permission.Code == permission);
    }

    // ✅ Implementación de UpdateAsync
    public async Task UpdateAsync(User user)
    {
        var trackedUser = await _context.Users.Include(u => u.Mfa).FirstAsync(u => u.Id == user.Id);

        // Actualizar propiedades simples del User
        _context.Entry(trackedUser).CurrentValues.SetValues(user);

        if (user.Mfa != null)
        {
            if (trackedUser.Mfa == null)
            {
                // 🆕 INSERT MFA
                trackedUser.SetupMfa(user.Mfa.Secret);
            }
            else
            {
                // 🔁 UPDATE MFA (ESTE ERA EL PROBLEMA)
                _context.Entry(trackedUser.Mfa).CurrentValues.SetValues(user.Mfa);
            }
        }

        await _context.SaveChangesAsync();
    }
}
