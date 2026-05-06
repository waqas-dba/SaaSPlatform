using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SaaSPlatform.Core.Catalog.Entities;

namespace SaaSPlatform.Infrastructure.Persistence.Configurations.Catalog;

public class CategoryConfiguration : IEntityTypeConfiguration<Category>
{
    public void Configure(EntityTypeBuilder<Category> builder)
    {
        builder.HasIndex(x => new { x.TenantId, x.ParentCategoryId });
    }
}

public class ProductConfiguration : IEntityTypeConfiguration<Product>
{
    public void Configure(EntityTypeBuilder<Product> builder)
    {
        builder.HasIndex(x => new { x.TenantId, x.CategoryId });
    }
}

public class AddonConfiguration : IEntityTypeConfiguration<Addon>
{
    public void Configure(EntityTypeBuilder<Addon> builder)
    {
        builder.HasIndex(x => new { x.TenantId, x.Name });
    }
}

public class ProductAddonConfiguration : IEntityTypeConfiguration<ProductAddon>
{
    public void Configure(EntityTypeBuilder<ProductAddon> builder)
    {
        builder.HasIndex(x => new { x.ProductId, x.AddonId });
    }
}

public class ProductVariantConfiguration : IEntityTypeConfiguration<ProductVariant>
{
    public void Configure(EntityTypeBuilder<ProductVariant> builder)
    {
        builder.HasIndex(x => x.ProductId);
    }
}