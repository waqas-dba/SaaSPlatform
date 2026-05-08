using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SaaSPlatform.Core.Catalog.Entities;

namespace SaaSPlatform.Infrastructure.Persistence.Configurations.Catalog;

public class ProductVariantConfiguration : IEntityTypeConfiguration<ProductVariant>
{
    public void Configure(EntityTypeBuilder<ProductVariant> builder)
    {
        builder.HasIndex(x => x.ProductId);
        builder.HasOne<Product>()
               .WithMany()
               .HasForeignKey(pv => pv.ProductId)
               .OnDelete(DeleteBehavior.Cascade);
    }
}