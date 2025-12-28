using IdentityService.Domain.Entities;

namespace IdentityService.Application.Interfaces
{
    public interface ISystemSettingsRepository
    {
        Task<SystemSettings> GetAsync();
    }
}
