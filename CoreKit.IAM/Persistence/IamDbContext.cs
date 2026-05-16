using CoreKit.IAM.Entities;
using CoreKit.IAM.Interfaces;
using CoreKit.SharedKernel.Common;
using CoreKit.SharedKernel.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace CoreKit.IAM.Persistence;

public class IamDbContext : AuditableDbContext
{
    private readonly ICurrentUserService? _currentUserService;

    public IamDbContext(
        DbContextOptions<IamDbContext> options,
        ICurrentUser? currentUser = null,
        ICurrentUserService? currentUserService = null)
        : base(options, currentUser)
    {
        _currentUserService = currentUserService;
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
    public DbSet<RoleDocumentRequirement> RoleDocumentRequirements => Set<RoleDocumentRequirement>();
    public DbSet<UserStoreAssignment> UserStoreAssignments => Set<UserStoreAssignment>();

    private bool IsGlobalScope
    {
        get
        {
            try { return _currentUserService?.GetTenantScope().IsGlobal ?? true; }
            catch { return true; }
        }
    }

    private Guid? CurrentTenantId
    {
        get
        {
            try
            {
                var scope = _currentUserService?.GetTenantScope();
                return scope is { IsGlobal: false } ? scope.TenantId : null;
            }
            catch { return null; }
        }
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(IamDbContext).Assembly);

        // Tenant-scoped entities (automatic global/tenant filter)
        modelBuilder.Entity<User>().HasQueryFilter(u =>
            IsGlobalScope || u.TenantId == CurrentTenantId);
        modelBuilder.Entity<Role>().HasQueryFilter(r =>
            IsGlobalScope || r.TenantId == CurrentTenantId);
        // Add other tenant‑scoped entities here as needed
    }
}