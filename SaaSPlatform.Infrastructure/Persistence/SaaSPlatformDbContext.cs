using Microsoft.EntityFrameworkCore;
using SaaSPlatform.Core.IAM.Entities;
using System.Data;
using System.Reflection;
using System.Reflection.Emit;
using System.Security;

namespace SaaSPlatform.Infrastructure.Persistence;

public partial class SaaSPlatformDbContext : DbContext
{
    public DbSet<User> Users => Set<User>();
    public DbSet<TenantUser> TenantUsers => Set<TenantUser>();
    public DbSet<Role> Roles => Set<Role>();
    public DbSet<UserRole> UserRoles => Set<UserRole>();
    public DbSet<Permission> Permissions => Set<Permission>();
    public DbSet<RolePermission> RolePermissions => Set<RolePermission>();
    public DbSet<Module> Modules => Set<Module>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        ConfigureIAM(modelBuilder);
    }

    private void ConfigureIAM(ModelBuilder modelBuilder)
    {
        // USER
        modelBuilder.Entity<User>()
            .HasIndex(x => x.Email)
            .IsUnique();

        // USER ROLE (composite key)
        modelBuilder.Entity<UserRole>()
            .HasKey(x => new { x.UserId, x.RoleId, x.TenantId });

        modelBuilder.Entity<UserRole>()
            .HasIndex(x => new { x.UserId, x.TenantId });

        // ROLE
        modelBuilder.Entity<Role>()
            .HasIndex(x => new { x.TenantId, x.Name });

        // ROLE PERMISSION
        modelBuilder.Entity<RolePermission>()
            .HasKey(x => new { x.RoleId, x.PermissionId });

        // MODULE
        modelBuilder.Entity<PermissionModule>()
            .HasIndex(x => x.Code)
            .IsUnique();

        // PERMISSION
        modelBuilder.Entity<Permission>()
            .HasIndex(x => x.Name)
            .IsUnique();

        modelBuilder.Entity<Permission>()
            .HasIndex(x => x.ModuleId);
    }
}