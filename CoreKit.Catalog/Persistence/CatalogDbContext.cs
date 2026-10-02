using CoreKit.Catalog.Entities;
using CoreKit.SharedKernel.Common;
using CoreKit.SharedKernel.Interfaces;
using CoreKit.SharedKernel.Tenancy;
using Microsoft.EntityFrameworkCore;

namespace CoreKit.Catalog.Persistence;

public class CatalogDbContext : AuditableDbContext
{
    private const string TenantFilterKey = "TenantFilter";

    private readonly ITenantContext? _tenantContext;

    public CatalogDbContext(
        DbContextOptions<CatalogDbContext> options,
        ITenantContext? tenantContext = null,
        ICurrentUser? currentUser = null)
        : base(options, currentUser)
    {
        _tenantContext = tenantContext;
    }

    public DbSet<Category> Categories => Set<Category>();
    public DbSet<Product> Products => Set<Product>();
    public DbSet<ProductImage> ProductImages => Set<ProductImage>();
    public DbSet<ProductVariant> ProductVariants => Set<ProductVariant>();
    public DbSet<AddonGroup> AddonGroups => Set<AddonGroup>();
    public DbSet<Addon> Addons => Set<Addon>();
    public DbSet<ProductAddonGroup> ProductAddonGroups => Set<ProductAddonGroup>();
    public DbSet<StoreProduct> StoreProducts => Set<StoreProduct>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(CatalogDbContext).Assembly);

        if (_tenantContext is null)
            return;

        // Named filters are added next to the soft-delete filter registered by
        // AuditableDbContext instead of replacing it.
        modelBuilder.Entity<Product>().HasQueryFilter(TenantFilterKey,
            e => _tenantContext!.TenantId == null || e.TenantId == _tenantContext.TenantId);

        modelBuilder.Entity<Category>().HasQueryFilter(TenantFilterKey,
            e => _tenantContext!.TenantId == null || e.TenantId == _tenantContext.TenantId);

        modelBuilder.Entity<AddonGroup>().HasQueryFilter(TenantFilterKey,
            e => _tenantContext!.TenantId == null || e.TenantId == _tenantContext.TenantId);

        modelBuilder.Entity<StoreProduct>().HasQueryFilter(TenantFilterKey,
            e => _tenantContext!.TenantId == null || e.TenantId == _tenantContext.TenantId);
    }
}