// CoreKit.Catalog | Configuration/ProductAttributeValueConfiguration.cs
using CoreKit.Catalog.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
namespace CoreKit.Catalog.Configuration;

public class ProductAttributeValueConfiguration : IEntityTypeConfiguration<ProductAttributeValue>
{
    public void Configure(EntityTypeBuilder<ProductAttributeValue> builder)
    {
        builder.ToTable("Catalog_ProductAttributeValues");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Value).IsRequired().HasMaxLength(500);

        // FIX: Unique constraint to prevent duplicate attribute values per product
        builder.HasIndex(x => new { x.ProductId, x.TemplateId }).IsUnique();

        // FIX: Explicit Restrict on delete to avoid accidental cascade when template removed
        builder.HasOne(x => x.Template)
            .WithMany(t => t.ProductAttributeValues)
            .HasForeignKey(x => x.TemplateId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}