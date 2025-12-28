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
            .Include(u => u.Mfa)
            .FirstOrDefaultAsync(u => u.Email == email);
    }

    public async Task<User?> GetByIdAsync(Guid id)
    {
        return await _context
            .Users.Include(u => u.Roles)
            .ThenInclude(ur => ur.Role)
            .Include(u => u.Mfa)
            .FirstOrDefaultAsync(u => u.Id == id);
    }

    public async Task AddAsync(User user)
    {
        // Agregar usuario
        await _context.Users.AddAsync(user);

        // Asegurar que cada rol asignado esté trackeado
        foreach (var userRole in user.Roles)
        {
            _context.Entry(userRole).State = EntityState.Added;
        }

        await _context.SaveChangesAsync(); // 🔥 Persistir todo
    }

    public async Task<bool> UserHasPermissionAsync(Guid userId, string permission)
    {
        return await _context
            .UserRoles.Where(ur => ur.UserId == userId)
            .SelectMany(ur => ur.Role.Permissions)
            .AnyAsync(rp => rp.Permission.Code == permission);
    }

    public async Task UpdateAsync(User user)
    {
        var trackedUser = await _context
            .Users.Include(u => u.Mfa)
            .Include(u => u.Roles)
            .FirstAsync(u => u.Id == user.Id);

        // Actualizar propiedades simples
        _context.Entry(trackedUser).CurrentValues.SetValues(user);

        // MFA
        if (user.Mfa != null)
        {
            if (trackedUser.Mfa == null)
            {
                trackedUser.SetupMfa(user.Mfa.Secret);
            }
            else
            {
                _context.Entry(trackedUser.Mfa).CurrentValues.SetValues(user.Mfa);
            }
        }

        // Roles
        foreach (var role in user.Roles)
        {
            if (!trackedUser.Roles.Any(r => r.RoleId == role.RoleId))
            {
                trackedUser.Roles.Add(
                    new UserRole { RoleId = role.RoleId, UserId = trackedUser.Id }
                );
            }
        }

        await _context.SaveChangesAsync();
    }
}
