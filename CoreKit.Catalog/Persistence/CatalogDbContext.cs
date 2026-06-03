using CoreKit.Catalog.Configuration;
using CoreKit.Catalog.Entities;
using CoreKit.SharedKernel.Common;
using CoreKit.SharedKernel.Interfaces;
using CoreKit.SharedKernel.Tenancy;
using Microsoft.EntityFrameworkCore;

namespace CoreKit.Catalog.Persistence;

public class CatalogDbContext : AuditableDbContext
{
    private readonly ITenantContext _tenantContext;

    public CatalogDbContext(
        DbContextOptions<CatalogDbContext> options,
        ITenantContext tenantContext,
        ICurrentUser? currentUser = null)
        : base(options, currentUser)
    {
        _tenantContext = tenantContext;
    }

    public DbSet<Category> Categories => Set < Category > ();
    public DbSet<Product> Products => Set<Product>();
    public DbSet<ProductImage> ProductImages => Set<ProductImage>();
    public DbSet<ProductVariant> ProductVariants => Set<ProductVariant>();
    public DbSet<VariantAttributeTemplate> VariantAttributeTemplates => Set < VariantAttributeTemplate > ();
    public DbSet<VariantAttributeValue> VariantAttributeValues => Set < VariantAttributeValue > ();
    public DbSet<VariantGroup> VariantGroups => Set < VariantGroup > ();
    public DbSet<VariantGroupOption> VariantGroupOptions => Set < VariantGroupOption > ();
    public DbSet<AddonGroup> AddonGroups => Set < AddonGroup > ();
    public DbSet<Addon> Addons => Set < Addon > ();
    public DbSet<ProductAttributeGroup> AttributeGroups => Set<ProductAttributeGroup>();
    public DbSet<ProductAttributeTemplate> AttributeTemplates => Set<ProductAttributeTemplate>();
    public DbSet<ProductAttributeValue> ProductAttributeValues => Set<ProductAttributeValue>();
    public DbSet<StoreAttributeOverride> StoreAttributeOverrides => Set < StoreAttributeOverride > ();
    public DbSet<TenantTemplateAssignment> TenantTemplateAssignments => Set<TenantTemplateAssignment>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(CatalogDbContext).Assembly);

        modelBuilder.Entity<Product>().HasQueryFilter(p =>
            _tenantContext.TenantId == null || p.TenantId == _tenantContext.TenantId);

        modelBuilder.Entity < Category > ().HasQueryFilter(c =>
            _tenantContext.TenantId == null || c.TenantId == _tenantContext.TenantId);

        modelBuilder.Entity < AddonGroup > ().HasQueryFilter(a =>
            _tenantContext.TenantId == null || a.TenantId == _tenantContext.TenantId);

        modelBuilder.Entity < VariantGroup > ().HasQueryFilter(v =>
            _tenantContext.TenantId == null || v.TenantId == _tenantContext.TenantId);

        modelBuilder.Entity<ProductAttributeTemplate>().HasQueryFilter(t =>
            t.TenantId == null || t.TenantId == _tenantContext.TenantId);

        modelBuilder.Entity<ProductAttributeGroup>().HasQueryFilter(g =>
            g.TenantId == null || g.TenantId == _tenantContext.TenantId);
    }
}