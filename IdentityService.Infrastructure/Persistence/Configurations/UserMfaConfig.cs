using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using IdentityService.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace IdentityService.Infrastructure.Persistence.Configurations
{
    public class UserMfaConfig : IEntityTypeConfiguration<UserMfa>
    {
        public void Configure(EntityTypeBuilder<UserMfa> builder)
        {
            builder.ToTable("user_mfa");

            builder.HasKey(x => x.Id);

            builder.Property(x => x.Secret).IsRequired();

            builder.Property(x => x.Enabled).IsRequired();

            builder.HasIndex(x => x.UserId).IsUnique();
            builder.Property(m => m.Confirmed).IsRequired();
        }
    }
}
