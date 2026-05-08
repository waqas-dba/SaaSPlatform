// SaaSPlatform.Infrastructure/Persistence/Configurations/Catalog/FeaturedProductConfiguration.cs
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SaaSPlatform.Core.Catalog.Entities;

namespace SaaSPlatform.Infrastructure.Persistence.Configurations.Catalog;

public class FeaturedProductConfiguration : IEntityTypeConfiguration<FeaturedProduct>
{
    public void Configure(EntityTypeBuilder<FeaturedProduct> builder)
    {
        builder.HasIndex(fp => new { fp.TenantId, fp.Section, fp.ProductId }).IsUnique();
        builder.HasIndex(fp => new { fp.TenantId, fp.Section, fp.CategoryId });

        builder.HasOne(fp => fp.Product)
               .WithMany()
               .HasForeignKey(fp => fp.ProductId)
               .OnDelete(DeleteBehavior.Restrict);
    }
}