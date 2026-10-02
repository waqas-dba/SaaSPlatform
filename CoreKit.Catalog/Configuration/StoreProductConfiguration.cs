using CoreKit.Catalog.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CoreKit.Catalog.Configuration;

public class StoreProductConfiguration : IEntityTypeConfiguration<StoreProduct>
{
    public void Configure(EntityTypeBuilder<StoreProduct> builder)
    {
        builder.ToTable("Catalog_StoreProducts");
        builder.HasKey(x => x.Id);

        builder.Property(x => x.PriceOverride).HasColumnType("decimal(18,2)");

        builder.HasIndex(x => new { x.StoreId, x.ProductId }).IsUnique();
        builder.HasIndex(x => new { x.StoreId, x.IsAvailable, x.SortOrder });
        builder.HasIndex(x => x.TenantId);

        builder.HasOne(x => x.Product)
            .WithMany(x => x.StoreProducts)
            .HasForeignKey(x => x.ProductId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}