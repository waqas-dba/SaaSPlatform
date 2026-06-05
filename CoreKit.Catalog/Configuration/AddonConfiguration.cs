// CoreKit.Catalog/Configuration/AddonConfiguration.cs
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

        builder.Property(x => x.Name)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(x => x.AdditionalPrice)
            .HasColumnType("decimal(18,2)")
            .IsRequired();

        builder.HasOne(x => x.AddonGroup)
            .WithMany(x => x.Addons)
            .HasForeignKey(x => x.AddonGroupId)
            .OnDelete(DeleteBehavior.Cascade);

        // INDEX: frequently filtered by group + active status
        builder.HasIndex(x => new { x.AddonGroupId, x.IsActive });
    }
}