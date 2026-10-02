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

        builder.Property(x => x.Name).IsRequired().HasMaxLength(100);
        builder.Property(x => x.Price).HasColumnType("decimal(18,2)").IsRequired();

        builder.HasIndex(x => new { x.ProductId, x.Name }).IsUnique();
        builder.HasIndex(x => new { x.ProductId, x.SortOrder });
    }
}