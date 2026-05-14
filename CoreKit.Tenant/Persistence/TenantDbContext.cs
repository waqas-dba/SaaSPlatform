using Microsoft.EntityFrameworkCore;
using CoreKit.Tenant.Entities;
using CoreKit.SharedKernel.Common;

namespace CoreKit.Tenant.Persistence;

public class TenantDbContext : DbContext
{
    public TenantDbContext(DbContextOptions<TenantDbContext> options) : base(options) { }

    public DbSet<TenantEntity> Tenants => Set<TenantEntity>();
    public DbSet<Store> Stores => Set<Store>();
    public DbSet<TenantLegalInfo>? TenantLegalInfos => Set<TenantLegalInfo>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(TenantDbContext).Assembly);
        // Apply global tenant filter – simplified version (see note below)
    }
}