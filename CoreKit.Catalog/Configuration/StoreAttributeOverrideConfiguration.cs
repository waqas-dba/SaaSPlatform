// CoreKit.Catalog/Configuration/StoreAttributeOverrideConfiguration.cs
using CoreKit.Catalog.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CoreKit.Catalog.Configuration;

public class StoreAttributeOverrideConfiguration
    : IEntityTypeConfiguration<StoreAttributeOverride>
{
    public void Configure(EntityTypeBuilder<StoreAttributeOverride> builder)
    {
        builder.ToTable("Catalog_StoreAttributeOverrides");
        builder.HasKey(x => x.Id);

        // One override record per store per template.
        builder.HasIndex(x => new { x.StoreId, x.TemplateId })
            .IsUnique();

        builder.HasOne(x => x.Template)
            .WithMany(t => t.StoreOverrides)
            .HasForeignKey(x => x.TemplateId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}