using IdentityService.Domain.Entities;

namespace IdentityService.Application.Interfaces
{
    public interface IRoleRepository
    {
        Task<Role?> GetByIdAsync(Guid id);
        Task<Role?> GetByNameAsync(string name);
        Task AddAsync(Role role);
    }
}
