// CoreKit.IAM | CoreKit.IAM/Configuration/UserRoleConfiguration.cs
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using CoreKit.IAM.Entities;

namespace CoreKit.IAM.Configuration;

public class UserRoleConfiguration : IEntityTypeConfiguration<UserRole>
{
    // CoreKit.IAM/Configuration/UserRoleConfiguration.cs
    public void Configure(EntityTypeBuilder<UserRole> builder)
    {
        builder.ToTable("IAM_UserRoles");

        // Surrogate PK — avoids the nullable-in-PK EF Core crash entirely.
        // The meaningful uniqueness constraint is the index below.
        builder.HasKey(x => new { x.UserId, x.RoleId });

        // A user+role combination is unique per tenant scope.
        // PostgreSQL treats two NULLs as distinct in unique indexes by default,
        // so (userId, roleId, NULL) and (userId, roleId, NULL) would both be
        // allowed without the filtered index below.
        // Use a COALESCE trick or two partial indexes to enforce uniqueness:
        builder.HasIndex(x => new { x.UserId, x.RoleId, x.TenantId })
            .IsUnique()
            .HasFilter("\"TenantId\" IS NOT NULL")
            .HasDatabaseName("IX_IAM_UserRoles_UserId_RoleId_TenantId_NotNull");

        // Separate index for the global (null) case:
        builder.HasIndex(x => new { x.UserId, x.RoleId })
            .IsUnique()
            .HasFilter("\"TenantId\" IS NULL")
            .HasDatabaseName("IX_IAM_UserRoles_UserId_RoleId_Global");

        builder.HasOne(ur => ur.User)
            .WithMany(u => u.Roles)
            .HasForeignKey(ur => ur.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(ur => ur.Role)
            .WithMany(r => r.UserRoles)
            .HasForeignKey(ur => ur.RoleId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}