using CoreKit.IAM.Configuration;
using CoreKit.IAM.Entities;
using CoreKit.SharedKernel.Common;
using CoreKit.SharedKernel.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace CoreKit.IAM.Persistence;

public class IamDbContext : AuditableDbContext
{
    public IamDbContext(
        DbContextOptions<IamDbContext> options,
        ICurrentUser? currentUser = null)
        : base(options, currentUser)
    {
    }

    public DbSet<User> Users => Set<User>();

    public DbSet<Role> Roles => Set<Role>();

    public DbSet<UserRole> UserRoles => Set<UserRole>();

    public DbSet<Permission> Permissions => Set<Permission>();

    public DbSet<PermissionModule> PermissionModules => Set<PermissionModule>();

    public DbSet<RolePermission> RolePermissions => Set<RolePermission>();

    public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();

    public DbSet<UserDocument> UserDocuments => Set<UserDocument>();

    public DbSet<UserIdentity> UserIdentities => Set<UserIdentity>();

    // In IamDbContext.cs
    public DbSet<AuditLog> AuditLogs => Set<AuditLog>();

    public DbSet<RoleDocumentRequirement> RoleDocumentRequirements
        => Set<RoleDocumentRequirement>();

    public DbSet<UserStoreAssignment> UserStoreAssignments
        => Set<UserStoreAssignment>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.ApplyConfiguration(new AuditLogConfiguration());

        modelBuilder.ApplyConfigurationsFromAssembly(
            typeof(IamDbContext).Assembly);
    }
}