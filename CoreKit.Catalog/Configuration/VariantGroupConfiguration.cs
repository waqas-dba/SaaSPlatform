// CoreKit.Catalog/Configuration/VariantGroupConfiguration.cs
using CoreKit.Catalog.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CoreKit.Catalog.Configuration;

public class VariantGroupConfiguration
    : IEntityTypeConfiguration<VariantGroup>
{
    public void Configure(EntityTypeBuilder<VariantGroup> builder)
    {
        builder.ToTable("Catalog_VariantGroups");
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Name)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(x => x.StoreTypeCode)
            .IsRequired()
            .HasMaxLength(50);

        builder.HasIndex(x => new { x.TenantId, x.Name })
            .IsUnique();

        builder.HasMany(x => x.Options)
            .WithOne(x => x.VariantGroup)
            .HasForeignKey(x => x.VariantGroupId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(x => x.Products)
            .WithOne(x => x.VariantGroup)
            .HasForeignKey(x => x.VariantGroupId)
            .OnDelete(DeleteBehavior.SetNull);
    }
}