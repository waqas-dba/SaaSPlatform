using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

using SaaSPlatform.Core.Tenant.Entities;

namespace SaaSPlatform.Infrastructure.Persistence.Configurations.TenantConfig;

public class TenantConfiguration
    : IEntityTypeConfiguration<Tenant>
{
    public void Configure(
        EntityTypeBuilder<Tenant> builder)
    {
        builder.Property(x => x.Latitude)
            .HasPrecision(9, 6);

        builder.Property(x => x.Longitude)
            .HasPrecision(9, 6);

        builder.HasIndex(x => x.Slug)
            .IsUnique();
    }
}

public class StoreConfiguration
    : IEntityTypeConfiguration<Store>
{
    public void Configure(
        EntityTypeBuilder<Store> builder)
    {
        builder.Property(x => x.Latitude)
            .HasPrecision(9, 6);

        builder.Property(x => x.Longitude)
            .HasPrecision(9, 6);

        builder.HasIndex(x => new
        {
            x.TenantId,
            x.Slug
        });
    }
}