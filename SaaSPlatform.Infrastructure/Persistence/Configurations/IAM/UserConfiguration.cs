using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SaaSPlatform.Core.IAM.Entities;

namespace SaaSPlatform.Infrastructure.Persistence.Configurations.IAM;

public class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.ToTable("Users");

        builder.HasKey(x => x.Id);

        // =========================
        // REQUIRED FIELDS
        // =========================

        builder.Property(x => x.Name)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(x => x.Phone)
            .IsRequired()
            .HasMaxLength(20);

        builder.Property(x => x.Email)
            .HasMaxLength(200);

        builder.Property(x => x.PasswordHash)
            .IsRequired();

        // =========================
        // TENANT ISOLATION (CRITICAL)
        // =========================

        builder.Property(x => x.TenantId)
            .IsRequired();

        builder.HasIndex(x => new { x.TenantId, x.Phone })
            .IsUnique();

        builder.HasIndex(x => new { x.TenantId, x.Email });

        // =========================
        // SECURITY
        // =========================

        builder.Property(x => x.FailedLoginAttempts)
            .HasDefaultValue(0);

        builder.Property(x => x.IsActive)
            .HasDefaultValue(true);

        // =========================
        // RELATIONSHIPS
        // =========================

        builder.HasMany(x => x.Roles)
            .WithOne()
            .HasForeignKey(x => x.UserId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}