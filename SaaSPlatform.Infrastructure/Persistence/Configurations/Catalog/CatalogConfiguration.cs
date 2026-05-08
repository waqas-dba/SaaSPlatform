using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SaaSPlatform.Core.Catalog.Entities;

namespace SaaSPlatform.Infrastructure.Persistence.Configurations.Catalog;


public class CategoryConfiguration : IEntityTypeConfiguration<Category>
{
    public void Configure(EntityTypeBuilder<Category> builder)
    {
        // Composite index for tenant/store/parent
        builder.HasIndex(x => new { x.TenantId, x.StoreId, x.ParentCategoryId });
        // Fast lookup per store + name
        builder.HasIndex(x => new { x.StoreId, x.Name });

        builder.Property(x => x.PhotoUrl).HasMaxLength(500);
        builder.Property(x => x.Icon).HasMaxLength(200);

        // FK to Store
        builder.HasOne<SaaSPlatform.Core.Tenant.Entities.Store>()
               .WithMany()
               .HasForeignKey(c => c.StoreId)
               .OnDelete(DeleteBehavior.Cascade);
    }
}


