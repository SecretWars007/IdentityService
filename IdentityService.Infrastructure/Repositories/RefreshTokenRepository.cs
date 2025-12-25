using IdentityService.Application.Interfaces;
using IdentityService.Domain.Entities;
using IdentityService.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace IdentityService.Infrastructure.Repositories;

public class RefreshTokenRepository : IRefreshTokenRepository
{
    private readonly IdentityDbContext _context;

    public RefreshTokenRepository(IdentityDbContext context)
    {
        _context = context;
    }

    public async Task AddAsync(RefreshToken token)
    {
        _context.RefreshTokens.Add(token);
        await _context.SaveChangesAsync();
    }

    public async Task<RefreshToken?> GetByHashAsync(string token)
    {
        return await _context.RefreshTokens.FirstOrDefaultAsync(x =>
            !(x.TokenHash != token || x.IsRevoked)
        );
    }

    public async Task RevokeAsync(RefreshToken token)
    {
        token.Revoke();
        await _context.SaveChangesAsync();
    }
}
