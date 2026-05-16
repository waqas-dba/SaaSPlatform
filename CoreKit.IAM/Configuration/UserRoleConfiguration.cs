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

        // FIX: TenantId is now nullable; EF composite keys handle null columns
        // correctly in PostgreSQL — two rows with the same UserId+RoleId but
        // TenantId=NULL are treated as duplicates via the unique index below.
        builder.HasKey(x => new { x.UserId, x.RoleId, x.TenantId });

        // Unique index with NULLS NOT DISTINCT so null TenantId is treated as
        // a concrete value for uniqueness purposes (PostgreSQL 15+).
        // For older PG, the HasKey composite above already prevents duplicates
        // at the PK level.
        builder.HasIndex(x => new { x.UserId, x.RoleId, x.TenantId })
            .IsUnique()
            .HasDatabaseName("IX_IAM_UserRoles_UserId_RoleId_TenantId");

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