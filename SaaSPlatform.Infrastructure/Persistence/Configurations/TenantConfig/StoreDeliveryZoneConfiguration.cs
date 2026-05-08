// SaaSPlatform.Infrastructure/Persistence/Configurations/TenantConfig/StoreDeliveryZoneConfiguration.cs
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SaaSPlatform.Core.Tenant.Entities;

namespace SaaSPlatform.Infrastructure.Persistence.Configurations.TenantConfig;

public class StoreDeliveryZoneConfiguration : IEntityTypeConfiguration<StoreDeliveryZone>
{
    public void Configure(EntityTypeBuilder<StoreDeliveryZone> builder)
    {
        builder.HasKey(x => new { x.StoreId, x.ZoneId });
        builder.HasOne(x => x.Store).WithMany(s => s.DeliveryZones).HasForeignKey(x => x.StoreId);
        builder.HasOne(x => x.Zone).WithMany().HasForeignKey(x => x.ZoneId);
    }
}