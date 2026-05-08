using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SaaSPlatform.Core.Catalog.Entities;

namespace SaaSPlatform.Infrastructure.Persistence.Configurations.Catalog;

public class ProductConfiguration : IEntityTypeConfiguration<Product>
{
    public void Configure(EntityTypeBuilder<Product> builder)
    {
        builder.HasIndex(x => new { x.TenantId, x.StoreId, x.CategoryId });
        builder.HasIndex(x => new { x.StoreId, x.Name });

        builder.Property(x => x.MainImageUrl).HasMaxLength(500);

        // FK to Store
        builder.HasOne<SaaSPlatform.Core.Tenant.Entities.Store>()
               .WithMany()
               .HasForeignKey(p => p.StoreId)
               .OnDelete(DeleteBehavior.Cascade);

        // Relationship to ProductImages
        builder.HasMany(p => p.Images)
               .WithOne()
               .HasForeignKey(i => i.ProductId)
               .OnDelete(DeleteBehavior.Cascade);
    }
}