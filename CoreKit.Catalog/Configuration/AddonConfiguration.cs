using CoreKit.Catalog.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CoreKit.Catalog.Configuration;

public class AddonConfiguration : IEntityTypeConfiguration<Addon>
{
    public void Configure(EntityTypeBuilder<Addon> builder)
    {
        builder.ToTable("Catalog_Addons");
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Name).IsRequired().HasMaxLength(200);
        builder.Property(x => x.AdditionalPrice).HasColumnType("decimal(18,2)").IsRequired();

        builder.HasIndex(x => new { x.AddonGroupId, x.Name }).IsUnique();
        builder.HasIndex(x => new { x.AddonGroupId, x.IsActive, x.SortOrder });
    }
}