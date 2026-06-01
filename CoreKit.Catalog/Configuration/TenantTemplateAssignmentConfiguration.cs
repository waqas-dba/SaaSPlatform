// CoreKit.Catalog/Configuration/TenantTemplateAssignmentConfiguration.cs
using CoreKit.Catalog.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CoreKit.Catalog.Configuration;

public class TenantTemplateAssignmentConfiguration
    : IEntityTypeConfiguration<TenantTemplateAssignment>
{
    public void Configure(EntityTypeBuilder<TenantTemplateAssignment> builder)
    {
        builder.ToTable("Catalog_TenantTemplateAssignments");
        builder.HasKey(x => x.Id);

        builder.Property(x => x.StoreTypeCode)
            .IsRequired()
            .HasMaxLength(50);

        // One assignment per tenant+store+template combination.
        builder.HasIndex(x => new { x.TenantId, x.StoreId, x.TemplateId })
            .IsUnique()
            .HasFilter("\"StoreId\" IS NOT NULL")
            .HasDatabaseName("IX_TTA_Tenant_Store_Template");

        builder.HasIndex(x => new { x.TenantId, x.TemplateId })
            .IsUnique()
            .HasFilter("\"StoreId\" IS NULL")
            .HasDatabaseName("IX_TTA_Tenant_Template_Global");

        builder.HasOne(x => x.Template)
            .WithMany()
            .HasForeignKey(x => x.TemplateId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}