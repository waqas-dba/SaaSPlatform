// CoreKit.IAM | CoreKit.IAM/Configuration/UserRoleConfiguration.cs
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using CoreKit.IAM.Entities;

namespace CoreKit.IAM.Configuration;

public class UserRoleConfiguration : IEntityTypeConfiguration<UserRole>
{
    public void Configure(EntityTypeBuilder<UserRole> builder)
    {
        builder.ToTable("IAM_UserRoles");
        builder.HasKey(x => new { x.UserId, x.RoleId });

        builder.HasIndex(x => new { x.UserId, x.RoleId, x.TenantId })
            .IsUnique()
            .HasFilter("\"TenantId\" IS NOT NULL")
            .HasDatabaseName("IX_IAM_UserRoles_UserId_RoleId_TenantId_NotNull");

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

        // Match the soft-delete filters on both User and Role
        builder.HasQueryFilter(ur => !ur.User.IsDeleted && !ur.Role.IsDeleted);
    }
}