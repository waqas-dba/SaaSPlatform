using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using CoreKit.Subscription.Entities;

namespace CoreKit.Subscription.Configuration;

public class PlanConfiguration : IEntityTypeConfiguration<Plan>
{
    public void Configure(EntityTypeBuilder<Plan> builder)
    {
        builder.ToTable("SUB_Plans");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Name).IsRequired().HasMaxLength(200);
        builder.Property(x => x.Code).IsRequired().HasMaxLength(50);
        builder.HasIndex(x => x.Code).IsUnique();

        builder.Property(x => x.MaxCategoryLevel).HasDefaultValue(1);
        builder.Property(x => x.EnableVariants).HasDefaultValue(false);
        builder.Property(x => x.EnableAddons).HasDefaultValue(false);
        builder.Property(x => x.MaxVariantsPerProduct).IsRequired(false);
        builder.Property(x => x.MaxAddonsPerProduct).IsRequired(false);
    }
}