// SaaSPlatform.Infrastructure/Persistence/Configurations/Catalog/AddonGroupConfiguration.cs
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SaaSPlatform.Core.Catalog.Entities;

namespace SaaSPlatform.Infrastructure.Persistence.Configurations.Catalog;

public class AddonGroupConfiguration : IEntityTypeConfiguration<AddonGroup>
{
    public void Configure(EntityTypeBuilder<AddonGroup> builder)
    {
        builder.HasIndex(x => new { x.TenantId, x.StoreId, x.Name });
        builder.Property(x => x.Name).HasMaxLength(200);

        builder.HasMany(g => g.Items)
               .WithOne(i => i.AddonGroup)
               .HasForeignKey(i => i.AddonGroupId)
               .OnDelete(DeleteBehavior.Cascade);
    }
}