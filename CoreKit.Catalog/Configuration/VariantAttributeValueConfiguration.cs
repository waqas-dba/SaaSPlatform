// CoreKit.Catalog | Configuration/VariantAttributeValueConfiguration.cs
using CoreKit.Catalog.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
namespace CoreKit.Catalog.Configuration;

public class VariantAttributeValueConfiguration
    : IEntityTypeConfiguration<VariantAttributeValue>
{
    public void Configure(EntityTypeBuilder<VariantAttributeValue> builder)
    {
        builder.ToTable("Catalog_VariantAttributeValues");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Value)
            .IsRequired()
            .HasMaxLength(500);

        // FIX: Unique constraint to prevent duplicate variant attribute entries
        builder.HasIndex(x => new { x.VariantId, x.TemplateId }).IsUnique();

        builder.HasOne(x => x.Variant)
            .WithMany(x => x.AttributeValues)
            .HasForeignKey(x => x.VariantId)
            .OnDelete(DeleteBehavior.Cascade);
        builder.HasOne(x => x.Template)
            .WithMany()
            .HasForeignKey(x => x.TemplateId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}