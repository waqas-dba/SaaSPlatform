using CoreKit.Catalog.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CoreKit.Catalog.Configuration;

public class ProductImageConfiguration : IEntityTypeConfiguration<ProductImage>
{
    public void Configure(EntityTypeBuilder<ProductImage> builder)
    {
        builder.ToTable("Catalog_ProductImages");
        builder.HasKey(x => x.Id);

        builder.Property(x => x.ImageUrl).IsRequired().HasMaxLength(2000);

        builder.HasIndex(x => new { x.ProductId, x.SortOrder });
    }
}