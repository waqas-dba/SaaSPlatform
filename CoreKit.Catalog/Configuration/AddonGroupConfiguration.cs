using CoreKit.Catalog.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CoreKit.Catalog.Configuration;

public class AddonGroupConfiguration : IEntityTypeConfiguration<AddonGroup>
{
    public void Configure(EntityTypeBuilder<AddonGroup> builder)
    {
        builder.ToTable("Catalog_AddonGroups", t =>
            t.HasCheckConstraint(
                "CK_Catalog_AddonGroups_Selection",
                "\"MinSelect\" >= 0 AND \"MaxSelect\" >= 1 AND \"MinSelect\" <= \"MaxSelect\""));

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Name).IsRequired().HasMaxLength(200);

        builder.HasIndex(x => new { x.TenantId, x.Name })
            .IsUnique()
            .HasFilter("\"IsDeleted\" = false");

        builder.HasMany(x => x.Addons)
            .WithOne(x => x.AddonGroup)
            .HasForeignKey(x => x.AddonGroupId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}