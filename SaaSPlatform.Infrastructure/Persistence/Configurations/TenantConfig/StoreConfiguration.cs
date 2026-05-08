using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SaaSPlatform.Core.Tenant.Entities;

namespace SaaSPlatform.Infrastructure.Persistence.Configurations.TenantConfig;

public class StoreConfiguration : IEntityTypeConfiguration<Store>
{
    public void Configure(EntityTypeBuilder<Store> builder)
    {
        builder.Property(x => x.Latitude).HasPrecision(9, 6);
        builder.Property(x => x.Longitude).HasPrecision(9, 6);

        builder.HasIndex(x => new { x.TenantId, x.Slug });
        builder.HasIndex(x => x.City);
        builder.HasIndex(x => x.ZoneName);
        builder.HasIndex(x => x.IsOnline);
        // Removed CuisineId index
    }
}