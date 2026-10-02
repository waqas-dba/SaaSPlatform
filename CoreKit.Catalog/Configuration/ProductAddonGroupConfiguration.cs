using CoreKit.Catalog.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CoreKit.Catalog.Configuration;

public class ProductAddonGroupConfiguration : IEntityTypeConfiguration<ProductAddonGroup>
{
    public void Configure(EntityTypeBuilder<ProductAddonGroup> builder)
    {
        builder.ToTable("Catalog_ProductAddonGroups");
        builder.HasKey(x => x.Id);

        builder.HasIndex(x => new { x.ProductId, x.AddonGroupId }).IsUnique();
        builder.HasIndex(x => x.AddonGroupId);

        builder.HasOne(x => x.Product)
            .WithMany(x => x.AddonGroupLinks)
            .HasForeignKey(x => x.ProductId)
            .OnDelete(DeleteBehavior.Cascade);

        // A group that is still attached to a product cannot be deleted.
        builder.HasOne(x => x.AddonGroup)
            .WithMany(x => x.ProductLinks)
            .HasForeignKey(x => x.AddonGroupId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}