using CoreKit.IAM.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CoreKit.IAM.Persistence.Configurations;

/// <summary>
/// EF Core configuration for the User entity.
/// </summary>
public class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.ToTable("IAM_Users");
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Name)
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(x => x.Phone)
            .HasMaxLength(20)
            .IsRequired();

        builder.Property(x => x.Email)
            .HasMaxLength(200);

        builder.Property(x => x.PasswordHash)
            .IsRequired();

        // Per‑tenant uniqueness (NULL TenantId allowed)
        builder.HasIndex(x => new { x.Phone, x.TenantId }).IsUnique();
        builder.HasIndex(x => new { x.Email, x.TenantId }).IsUnique();
    }
}