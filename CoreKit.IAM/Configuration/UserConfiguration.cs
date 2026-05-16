// CoreKit.IAM | CoreKit.IAM/Configuration/UserConfiguration.cs
// FIX 5: Prevent duplicate global users (TenantId IS NULL) using partial
// unique indexes. Standard EF unique indexes allow multiple NULL rows in
// PostgreSQL < 15. A partial index scoped to IS NULL rows enforces
// uniqueness only for the global (null-tenant) partition.
using CoreKit.IAM.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CoreKit.IAM.Persistence.Configurations;

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

        // Tenant-scoped uniqueness: works correctly when TenantId is NOT NULL
        // because two NULLs are never considered equal in standard SQL.
        builder.HasIndex(x => new { x.Phone, x.TenantId })
            .IsUnique()
            .HasFilter("\"TenantId\" IS NOT NULL")
            .HasDatabaseName("IX_IAM_Users_Phone_TenantId_NotNull");

        builder.HasIndex(x => new { x.Email, x.TenantId })
            .IsUnique()
            .HasFilter("\"TenantId\" IS NOT NULL AND \"Email\" IS NOT NULL")
            .HasDatabaseName("IX_IAM_Users_Email_TenantId_NotNull");

        // FIX 5: Partial indexes for the global (null-tenant) partition.
        // These enforce uniqueness for platform-level accounts where TenantId
        // IS NULL — something the composite indexes above cannot do.
        builder.HasIndex(x => x.Phone)
            .IsUnique()
            .HasFilter("\"TenantId\" IS NULL")
            .HasDatabaseName("IX_IAM_Users_Phone_Global");

        builder.HasIndex(x => x.Email)
            .IsUnique()
            .HasFilter("\"TenantId\" IS NULL AND \"Email\" IS NOT NULL")
            .HasDatabaseName("IX_IAM_Users_Email_Global");
    }
}