// CoreKit.Catalog/Configuration/ProductAttributeTemplateConfiguration.cs
using CoreKit.Catalog.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CoreKit.Catalog.Configuration;

public class ProductAttributeTemplateConfiguration
    : IEntityTypeConfiguration<ProductAttributeTemplate>
{
    public void Configure(EntityTypeBuilder<ProductAttributeTemplate> builder)
    {
        builder.ToTable("Catalog_AttributeTemplates");
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Name)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(x => x.StoreTypeCode)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(x => x.OptionsJson)
            .HasMaxLength(2000);

        builder.Property(x => x.IsVisible)
            .HasDefaultValue(true);

        // Unique attribute name per store type per tenant scope.
        // Platform templates:  TenantId IS NULL
        // Tenant templates:    TenantId IS NOT NULL
        builder.HasIndex(x => new { x.StoreTypeCode, x.TenantId, x.Name })
            .IsUnique()
            .HasFilter("\"TenantId\" IS NOT NULL")
            .HasDatabaseName("IX_AttrTemplate_StoreType_Tenant_Name");

        builder.HasIndex(x => new { x.StoreTypeCode, x.Name })
            .IsUnique()
            .HasFilter("\"TenantId\" IS NULL")
            .HasDatabaseName("IX_AttrTemplate_StoreType_Name_Global");

        // Self-referencing override link.
        builder.HasOne(x => x.OverridesTemplate)
            .WithMany()
            .HasForeignKey(x => x.OverridesTemplateId)
            .OnDelete(DeleteBehavior.Restrict)
            .IsRequired(false);

        // Group membership.
        builder.HasOne(x => x.Group)
            .WithMany(g => g.Attributes)
            .HasForeignKey(x => x.GroupId)
            .OnDelete(DeleteBehavior.SetNull)
            .IsRequired(false);
    }
}