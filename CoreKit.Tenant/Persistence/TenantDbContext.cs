using CoreKit.IAM.Interfaces;
using CoreKit.SharedKernel.Common;
using CoreKit.SharedKernel.Interfaces;
using CoreKit.Tenant.Entities;
using Microsoft.EntityFrameworkCore;

namespace CoreKit.Tenant.Persistence;

public class TenantDbContext : AuditableDbContext
{
    public TenantDbContext(
        DbContextOptions<TenantDbContext> options,
        ICurrentUser? currentUser = null)
        : base(options, currentUser)
    {
    }


    public DbSet<TenantEntity> Tenants => Set<TenantEntity>();
    public DbSet<Store> Stores => Set<Store>();
    public DbSet<TenantLegalInfo> TenantLegalInfos => Set<TenantLegalInfo>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(TenantDbContext).Assembly);
    }
}