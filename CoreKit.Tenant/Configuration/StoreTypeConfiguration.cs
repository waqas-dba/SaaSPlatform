// ============================================================
// FILE: CoreKit.Tenant/Persistence/Configurations/StoreTypeConfiguration.cs
// ============================================================

using CoreKit.Tenant.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CoreKit.Tenant.Persistence.Configurations;

public class StoreTypeConfiguration
    : IEntityTypeConfiguration<StoreType>
{
    public void Configure(EntityTypeBuilder<StoreType> builder)
    {
        builder.ToTable("StoreTypes");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Name)
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(x => x.Code)
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(x => x.Category)
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(x => x.Description)
            .HasMaxLength(1000);

        builder.Property(x => x.Icon)
            .HasMaxLength(200);

        builder.Property(x => x.Color)
            .HasMaxLength(20);

        builder.HasIndex(x => x.Code)
            .IsUnique();

        builder.HasMany(x => x.Stores)
            .WithOne(x => x.StoreType)
            .HasForeignKey(x => x.StoreTypeId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}