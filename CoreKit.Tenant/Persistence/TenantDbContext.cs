using CoreKit.IAM.Interfaces;
using CoreKit.SharedKernel.Common;
using CoreKit.SharedKernel.Interfaces;
using CoreKit.Tenant.Entities;
using Microsoft.EntityFrameworkCore;

namespace CoreKit.Tenant.Persistence;

public class TenantDbContext : AuditableDbContext
{
    private readonly ICurrentUserService? _currentUserService;

    public TenantDbContext(
        DbContextOptions<TenantDbContext> options,
        ICurrentUser? currentUser = null,
        ICurrentUserService? currentUserService = null)
        : base(options, currentUser)
    {
        _currentUserService = currentUserService;
    }

    public DbSet<TenantEntity> Tenants => Set<TenantEntity>();
    public DbSet<Store> Stores => Set<Store>();
    public DbSet<TenantLegalInfo> TenantLegalInfos => Set<TenantLegalInfo>();

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
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(TenantDbContext).Assembly);

        // Tenant filter on Store (already on TenantEntity)
        modelBuilder.Entity<Store>().HasQueryFilter(s =>
            IsGlobalScope || s.TenantId == CurrentTenantId);
        // Add for TenantEntity if not already covered (it is not directly ITenantScoped, but it's the owner)
    }
}