using CoreKit.Catalog.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CoreKit.Catalog.Configuration;

public class VariantAttributeTemplateConfiguration
    : IEntityTypeConfiguration<VariantAttributeTemplate>
{
    public void Configure(EntityTypeBuilder<VariantAttributeTemplate> builder)
    {
        builder.ToTable("Catalog_VariantAttributeTemplates");
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Name)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(x => x.StoreTypeCode)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(x => x.OptionsJson)
            .HasMaxLength(2000);

        // Global platform templates (TenantId IS NULL)
        builder.HasIndex(x => new { x.StoreTypeCode, x.Name })
            .IsUnique()
            .HasFilter("\"TenantId\" IS NULL")
            .HasDatabaseName("IX_VariantAttrTemplate_StoreType_Name_Global");

        // Tenant-scoped templates (TenantId IS NOT NULL)
        builder.HasIndex(x => new { x.StoreTypeCode, x.TenantId, x.Name })
            .IsUnique()
            .HasFilter("\"TenantId\" IS NOT NULL")
            .HasDatabaseName("IX_VariantAttrTemplate_StoreType_Tenant_Name");
    }
}