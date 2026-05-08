// SaaSPlatform.Infrastructure/Persistence/Configurations/TenantConfig/ZoneConfiguration.cs
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SaaSPlatform.Core.Tenant.Entities;

namespace SaaSPlatform.Infrastructure.Persistence.Configurations.TenantConfig;

public class ZoneConfiguration : IEntityTypeConfiguration<Zone>
{
    public void Configure(EntityTypeBuilder<Zone> builder)
    {
        builder.HasIndex(z => new { z.City, z.Name }).IsUnique();
    }
}