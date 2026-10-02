using CoreKit.Catalog.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CoreKit.Catalog.Configuration;

public class ProductConfiguration : IEntityTypeConfiguration<Product>
{
    public void Configure(EntityTypeBuilder<Product> builder)
    {
        builder.ToTable("Catalog_Products");
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Name).IsRequired().HasMaxLength(200);
        builder.Property(x => x.Slug).IsRequired().HasMaxLength(200);
        builder.Property(x => x.Description).HasMaxLength(2000);
        builder.Property(x => x.BasePrice).HasColumnType("decimal(18,2)").IsRequired();

        builder.HasIndex(x => new { x.TenantId, x.Slug })
            .IsUnique()
            .HasFilter("\"IsDeleted\" = false");

        builder.HasIndex(x => new { x.TenantId, x.CategoryId, x.SortOrder });
        builder.HasIndex(x => new { x.TenantId, x.IsActive });

        builder.HasOne(x => x.Category)
            .WithMany(x => x.Products)
            .HasForeignKey(x => x.CategoryId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(x => x.Variants)
            .WithOne(x => x.Product)
            .HasForeignKey(x => x.ProductId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(x => x.Images)
            .WithOne(x => x.Product)
            .HasForeignKey(x => x.ProductId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Property(x => x.SearchVector)
            .HasColumnType("tsvector")
            .IsRequired()
            .HasComputedColumnSql(
                "to_tsvector('english', coalesce(\"Name\", '') || ' ' || coalesce(\"Description\", ''))",
                stored: true);

        builder.HasIndex(x => x.SearchVector).HasMethod("GIN");
    }
}