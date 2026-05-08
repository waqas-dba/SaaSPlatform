// SaaSPlatform.Infrastructure/Persistence/Configurations/Catalog/AddonConfiguration.cs
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SaaSPlatform.Core.Catalog.Entities;

namespace SaaSPlatform.Infrastructure.Persistence.Configurations.Catalog;

public class AddonConfiguration : IEntityTypeConfiguration<Addon>
{
    public void Configure(EntityTypeBuilder<Addon> builder)
    {
        builder.HasIndex(x => new { x.TenantId, x.StoreId, x.Name });
        builder.Property(x => x.Price).HasPrecision(18, 2);

        // FK to Store (optional but clean)
        builder.HasOne<SaaSPlatform.Core.Tenant.Entities.Store>()
               .WithMany()
               .HasForeignKey(a => a.StoreId)
               .OnDelete(DeleteBehavior.Cascade);
    }
}