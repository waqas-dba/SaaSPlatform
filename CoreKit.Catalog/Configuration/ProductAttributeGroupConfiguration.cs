// CoreKit.Catalog/Configuration/ProductAttributeGroupConfiguration.cs
using CoreKit.Catalog.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CoreKit.Catalog.Configuration;

public class ProductAttributeGroupConfiguration
    : IEntityTypeConfiguration<ProductAttributeGroup>
{
    public void Configure(EntityTypeBuilder<ProductAttributeGroup> builder)
    {
        builder.ToTable("Catalog_AttributeGroups");
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Name)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(x => x.StoreTypeCode)
            .IsRequired()
            .HasMaxLength(50);

        // Unique group name per store type per tenant (null = platform).
        builder.HasIndex(x => new { x.StoreTypeCode, x.TenantId, x.Name })
            .IsUnique();
    }
}