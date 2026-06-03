// CoreKit.Catalog/Configuration/VariantGroupOptionConfiguration.cs
using CoreKit.Catalog.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CoreKit.Catalog.Configuration;

public class VariantGroupOptionConfiguration
    : IEntityTypeConfiguration<VariantGroupOption>
{
    public void Configure(EntityTypeBuilder<VariantGroupOption> builder)
    {
        builder.ToTable("Catalog_VariantGroupOptions");
        builder.HasKey(x => x.Id);

        builder.Property(x => x.AllowedValuesJson)
            .HasMaxLength(2000);

        // One template can appear only once per group
        builder.HasIndex(x => new { x.VariantGroupId, x.TemplateId })
            .IsUnique();

        builder.HasOne(x => x.Template)
            .WithMany()
            .HasForeignKey(x => x.TemplateId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}