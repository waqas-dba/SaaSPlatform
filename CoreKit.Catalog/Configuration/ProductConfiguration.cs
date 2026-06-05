// CoreKit.Catalog/Configuration/ProductConfiguration.cs
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

        builder.Property(x => x.Name)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(x => x.Slug)
            .IsRequired()
            .HasMaxLength(200);

        builder.HasIndex(x => new { x.StoreId, x.Slug })
            .IsUnique();

        builder.Property(x => x.Description)
            .HasMaxLength(2000);

        builder.Property(x => x.BasePrice)
            .HasColumnType("decimal(18,2)")
            .IsRequired();

        builder.HasOne(x => x.Category)
            .WithMany(x => x.Products)
            .HasForeignKey(x => x.CategoryId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.AddonGroup)
            .WithMany(x => x.Products)
            .HasForeignKey(x => x.AddonGroupId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasOne(x => x.VariantGroup)
            .WithMany(x => x.Products)
            .HasForeignKey(x => x.VariantGroupId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasMany(x => x.Images)
            .WithOne(x => x.Product)
            .HasForeignKey(x => x.ProductId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(x => x.AttributeValues)
            .WithOne(x => x.Product)
            .HasForeignKey(x => x.ProductId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(x => x.Variants)
            .WithOne(x => x.Product)
            .HasForeignKey(x => x.ProductId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Property(x => x.AvailableForCollection).HasDefaultValue(true);
        builder.Property(x => x.AvailableForDelivery).HasDefaultValue(true);

        // CoreKit.Catalog | Configuration/ProductConfiguration.cs (add after HasMany Images)
        builder.Property(x => x.SearchVector)
            .HasColumnType("tsvector")
            .IsRequired()
            .HasComputedColumnSql("to_tsvector('english', coalesce(\"Name\", '') || ' ' || coalesce(\"Description\", ''))", stored: true);

        builder.HasIndex(x => x.SearchVector)
            .HasMethod("GIN");
    }
}