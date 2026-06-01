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

        // FIX: AddonGroupId is now nullable so ad-hoc addons (ProductId set,
        // AddonGroupId null) and group addons (AddonGroupId set, ProductId null)
        // are both valid without needing Guid.Empty as a sentinel.
        builder.Property(x => x.AddonGroupId)
            .IsRequired(false);

        builder.HasOne(x => x.AddonGroup)
            .WithMany(x => x.Addons)
            .HasForeignKey(x => x.AddonGroupId)
            .OnDelete(DeleteBehavior.Cascade)
            .IsRequired(false);

        builder.HasOne(x => x.Product)
            .WithMany(x => x.AdHocAddons)
            .HasForeignKey(x => x.ProductId)
            .OnDelete(DeleteBehavior.Cascade)
            .IsRequired(false);
    }
}