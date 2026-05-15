using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using CoreKit.Tenant.Entities;
using CoreKit.Tenant.Enums;

namespace CoreKit.Tenant.Configuration;

/// <summary>
/// EF Core configuration for TenantEntity.
/// </summary>
public class TenantConfiguration : IEntityTypeConfiguration<TenantEntity>
{
    public void Configure(EntityTypeBuilder<TenantEntity> builder)
    {
        builder.ToTable("TNT_Tenants");
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Name).IsRequired().HasMaxLength(200);
        builder.HasIndex(x => x.Slug).IsUnique();
        builder.Property(x => x.Status).HasConversion<string>().HasMaxLength(30);

        // Unique name across all tenants (consider soft‑delete filter)
        builder.HasIndex(x => x.Name).IsUnique();

        // OwnerUserId unique but allows nulls (multiple nulls = allowed in PostgreSQL)
        builder.HasIndex(x => x.OwnerUserId).IsUnique();
    }
}