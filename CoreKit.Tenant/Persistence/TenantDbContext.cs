using CoreKit.SharedKernel.Common;
using CoreKit.SharedKernel.Interfaces;
using CoreKit.Tenant.Entities;
using Microsoft.EntityFrameworkCore;

namespace CoreKit.Tenant.Persistence;

public class TenantDbContext : AuditableDbContext
{
    private Guid? _tenantId;
    private bool _isGlobal = true;

    public void SetTenantScope(Guid? tenantId, bool isGlobal)
    {
        _tenantId = tenantId;
        _isGlobal = isGlobal;
    }

    public TenantDbContext(
        DbContextOptions<TenantDbContext> options,
        ICurrentUser? currentUser = null)
        : base(options, currentUser)
    {
    }

    public DbSet<TenantEntity> Tenants => Set<TenantEntity>();
    public DbSet<Store> Stores => Set<Store>();
    public DbSet<TenantLegalInfo> TenantLegalInfos => Set<TenantLegalInfo>();

    public DbSet<StoreType> StoreTypes => Set<StoreType>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(TenantDbContext).Assembly);

        modelBuilder.Entity<Store>()
            .HasQueryFilter(x => _isGlobal || x.TenantId == _tenantId);

        modelBuilder.Entity<TenantLegalInfo>()
            .HasQueryFilter(x => _isGlobal || x.TenantId == _tenantId);
    }
}