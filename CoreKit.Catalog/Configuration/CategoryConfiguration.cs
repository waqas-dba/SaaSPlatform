// CoreKit.Catalog/Configuration/CategoryConfiguration.cs

using CoreKit.Catalog.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CoreKit.Catalog.Configuration;

public class CategoryConfiguration : IEntityTypeConfiguration<Category>
{
    public void Configure(EntityTypeBuilder<Category> builder)
    {
        builder.ToTable("Catalog_Categories");
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Name).IsRequired().HasMaxLength(200);
        builder.Property(x => x.Slug).IsRequired().HasMaxLength(200);

        builder.HasIndex(x => new { x.TenantId, x.StoreId, x.Slug })
               .IsUnique()
               .HasFilter("\"StoreId\" IS NOT NULL");

        builder.HasIndex(x => new { x.TenantId, x.Slug })
               .IsUnique()
               .HasFilter("\"StoreId\" IS NULL");

        builder.Property(x => x.IconUrl).HasMaxLength(2000);

        builder.HasOne(x => x.ParentCategory)
               .WithMany(x => x.SubCategories)
               .HasForeignKey(x => x.ParentCategoryId)
               .OnDelete(DeleteBehavior.Restrict);

        builder.Property(x => x.Level).HasDefaultValue(1);
        builder.Property(x => x.StoreTypeCode).HasMaxLength(50);

        builder.HasMany(x => x.Products)
               .WithOne(x => x.Category)
               .HasForeignKey(x => x.CategoryId)
               .OnDelete(DeleteBehavior.Cascade);

        // Shadow FK on ProductAttributeTemplate — keeps the template entity
        // free of a back-navigation to Category while still enforcing the
        // cascade. No back-nav on ProductAttributeTemplate is needed because
        // template resolution always goes through the service layer.
        builder.HasMany(x => x.AttributeTemplates)
               .WithOne()
               .HasForeignKey("CategoryId")
               .OnDelete(DeleteBehavior.Cascade)
               .IsRequired(false);
    }
}