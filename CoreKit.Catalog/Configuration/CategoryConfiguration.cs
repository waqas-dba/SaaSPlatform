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
        builder.Property(x => x.IconUrl).HasMaxLength(2000);

        // Slug is unique per tenant among live rows, so a soft-deleted
        // category does not block re-creating the same name.
        builder.HasIndex(x => new { x.TenantId, x.Slug })
            .IsUnique()
            .HasFilter("\"IsDeleted\" = false");

        builder.HasIndex(x => new { x.TenantId, x.ParentCategoryId, x.SortOrder });

        builder.HasOne(x => x.ParentCategory)
            .WithMany(x => x.SubCategories)
            .HasForeignKey(x => x.ParentCategoryId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(x => x.Products)
            .WithOne(x => x.Category)
            .HasForeignKey(x => x.CategoryId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}