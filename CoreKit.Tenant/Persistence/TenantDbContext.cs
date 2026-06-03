using CoreKit.SharedKernel.Common;
using CoreKit.SharedKernel.Interfaces;
using CoreKit.SharedKernel.Tenancy;
using CoreKit.Tenant.Entities;
using Microsoft.EntityFrameworkCore;

namespace CoreKit.Tenant.Persistence;

public class TenantDbContext : AuditableDbContext
{
    private readonly ITenantContext _tenantContext;

    public TenantDbContext(
        DbContextOptions<TenantDbContext> options,
        ITenantContext tenantContext,
        ICurrentUser? currentUser = null)
        : base(options, currentUser)
    {
        _tenantContext = tenantContext;
    }

    public DbSet<TenantEntity> Tenants => Set<TenantEntity>();

    // Fully qualified to avoid ambiguity with CoreKit.IAM.Constants.Permissions.Store
    public DbSet<CoreKit.Tenant.Entities.Store> Stores
        => Set < CoreKit.Tenant.Entities.Store > ();

    // Fully qualified to avoid ambiguity with CoreKit.Tenant.Enums.StoreType
    public DbSet<CoreKit.Tenant.Entities.StoreType> StoreTypes
        => Set < CoreKit.Tenant.Entities.StoreType > ();

    public DbSet<TenantLegalInfo> TenantLegalInfos => Set<TenantLegalInfo>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(TenantDbContext).Assembly);

        modelBuilder.Entity < CoreKit.Tenant.Entities.Store > ().HasQueryFilter(s =>
            _tenantContext.TenantId == null || s.TenantId == _tenantContext.TenantId);

        modelBuilder.Entity<TenantLegalInfo>().HasQueryFilter(t =>
            _tenantContext.TenantId == null || t.TenantId == _tenantContext.TenantId);
    }
}