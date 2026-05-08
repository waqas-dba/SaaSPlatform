using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

using SaaSPlatform.Core.Tenant.Entities;

namespace SaaSPlatform.Infrastructure.Persistence.Configurations.TenantConfig;

public class TenantAccountConfiguration
    : IEntityTypeConfiguration<TenantAccount>
{
    public void Configure(
        EntityTypeBuilder<TenantAccount> builder)
    {
        builder.Property(x => x.Latitude)
            .HasPrecision(9, 6);

        builder.Property(x => x.Longitude)
            .HasPrecision(9, 6);

        builder.HasIndex(x => x.Slug)
            .IsUnique();
    }
}
