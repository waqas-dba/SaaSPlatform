using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SaaSPlatform.Core.Catalog.Entities;

namespace SaaSPlatform.Infrastructure.Persistence.Configurations.Catalog;

public class FeaturedProductConfiguration : IEntityTypeConfiguration<FeaturedProduct>
{
    public void Configure(EntityTypeBuilder<FeaturedProduct> builder)
    {
        // Uniqueness per store + section + product
        builder.HasIndex(fp => new { fp.TenantId, fp.StoreId, fp.Section, fp.ProductId })
               .IsUnique();

        // Fast lookup by store and section
        builder.HasIndex(fp => new { fp.StoreId, fp.Section, fp.CategoryId });

        builder.HasOne(fp => fp.Product)
               .WithMany()
               .HasForeignKey(fp => fp.ProductId)
               .OnDelete(DeleteBehavior.Restrict);

        // FK to Store (optional but clarifies relationship)
        builder.HasOne<SaaSPlatform.Core.Tenant.Entities.Store>()
               .WithMany()
               .HasForeignKey(fp => fp.StoreId)
               .OnDelete(DeleteBehavior.Cascade);
    }
}