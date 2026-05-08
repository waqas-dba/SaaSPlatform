// SaaSPlatform.Infrastructure/Persistence/Configurations/Catalog/ProductAddonGroupConfiguration.cs
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SaaSPlatform.Core.Catalog.Entities;

namespace SaaSPlatform.Infrastructure.Persistence.Configurations.Catalog;

public class ProductAddonGroupConfiguration : IEntityTypeConfiguration<ProductAddonGroup>
{
    public void Configure(EntityTypeBuilder<ProductAddonGroup> builder)
    {
        builder.HasIndex(x => new { x.ProductId, x.AddonGroupId }).IsUnique();

        builder.HasOne(x => x.Product)
               .WithMany()
               .HasForeignKey(x => x.ProductId)
               .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(x => x.AddonGroup)
               .WithMany()
               .HasForeignKey(x => x.AddonGroupId)
               .OnDelete(DeleteBehavior.Cascade);
    }
}