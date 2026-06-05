// CoreKit.Catalog | Configuration/ProductVariantConfiguration.cs
using CoreKit.Catalog.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
namespace CoreKit.Catalog.Configuration;

public class ProductVariantConfiguration : IEntityTypeConfiguration<ProductVariant>
{
    public void Configure(EntityTypeBuilder<ProductVariant> builder)
    {
        builder.ToTable("Catalog_ProductVariants");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Sku).IsRequired().HasMaxLength(100);

        // FIX: Ensure SKU is unique within a product (no duplicate SKUs for the same product)
        builder.HasIndex(x => new { x.ProductId, x.Sku }).IsUnique();

        builder.Property(x => x.Price).HasColumnType("decimal(18,2)").IsRequired();
        builder.Property(x => x.IsActive).HasDefaultValue(true);

        builder.HasOne(x => x.Product)
            .WithMany(p => p.Variants)
            .HasForeignKey(x => x.ProductId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(x => x.AttributeValues)
            .WithOne(av => av.Variant)
            .HasForeignKey(av => av.VariantId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}