using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using CoreKit.IAM.Entities;

namespace CoreKit.IAM.Configuration;

/// <summary>
/// EF Core configuration for the UserRole join entity.
/// </summary>
public class UserRoleConfiguration : IEntityTypeConfiguration<UserRole>
{
    public void Configure(EntityTypeBuilder<UserRole> builder)
    {
        builder.ToTable("IAM_UserRoles");

        // Composite primary key
        builder.HasKey(x => new { x.UserId, x.RoleId, x.TenantId });

        // Relationship: UserRole -> User (User.Roles, not User.UserRoles)
        builder.HasOne(ur => ur.User)
            .WithMany(u => u.Roles)                 // Correct navigation property
            .HasForeignKey(ur => ur.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        // Relationship: UserRole -> Role (Role.UserRoles is correct)
        builder.HasOne(ur => ur.Role)
            .WithMany(r => r.UserRoles)
            .HasForeignKey(ur => ur.RoleId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}