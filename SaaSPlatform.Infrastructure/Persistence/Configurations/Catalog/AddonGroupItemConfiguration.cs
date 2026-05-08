// SaaSPlatform.Infrastructure/Persistence/Configurations/Catalog/AddonGroupItemConfiguration.cs
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SaaSPlatform.Core.Catalog.Entities;

namespace SaaSPlatform.Infrastructure.Persistence.Configurations.Catalog;

public class AddonGroupItemConfiguration : IEntityTypeConfiguration<AddonGroupItem>
{
    public void Configure(EntityTypeBuilder<AddonGroupItem> builder)
    {
        builder.HasIndex(x => new { x.AddonGroupId, x.AddonId }).IsUnique();
        builder.Property(x => x.PriceAdjustment).HasPrecision(18, 2);

        builder.HasOne(i => i.AddonGroup)
               .WithMany(g => g.Items)
               .HasForeignKey(i => i.AddonGroupId)
               .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(i => i.Addon)
               .WithMany()
               .HasForeignKey(i => i.AddonId)
               .OnDelete(DeleteBehavior.Restrict);  // don't delete addon if group item exists
    }
}