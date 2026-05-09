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

        builder.Property(x => x.TenantId)
            .IsRequired();

        // Global unique phone – one phone = one account across all tenants
        builder.HasIndex(x => x.Phone)
            .IsUnique();

        // Global unique email – optional but recommended
        builder.HasIndex(x => x.Email)
            .IsUnique();

        builder.Property(x => x.FailedLoginAttempts)
            .HasDefaultValue(0);

        builder.Property(x => x.IsActive)
            .HasDefaultValue(true);

        builder.HasMany(x => x.Roles)
            .WithOne()
            .HasForeignKey(x => x.UserId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}