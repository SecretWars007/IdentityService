using IdentityService.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

public class SystemSettingsConfig : IEntityTypeConfiguration<SystemSettings>
{
    public void Configure(EntityTypeBuilder<SystemSettings> builder)
    {
        builder.HasKey(x => x.Id);

        builder.HasData(
            new SystemSettings
            {
                Id = Guid.NewGuid(),
                MaxFailedLoginAttempts = 3,
                LockoutMinutes = 15,
                MaxLockouts = 3,
            }
        );
    }
}
