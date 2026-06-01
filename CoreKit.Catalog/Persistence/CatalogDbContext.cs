// CoreKit.Catalog/Persistence/CatalogDbContext.cs
using CoreKit.Catalog.Configuration;
using CoreKit.Catalog.Entities;
using CoreKit.SharedKernel.Common;
using CoreKit.SharedKernel.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace CoreKit.Catalog.Persistence;

public class CatalogDbContext : AuditableDbContext
{
    public CatalogDbContext(
        DbContextOptions<CatalogDbContext> options,
        ICurrentUser? currentUser = null)
        : base(options, currentUser) { }

    public DbSet<Category> Categories => Set<Category>();
    public DbSet<Product> Products => Set<Product>();
    public DbSet<ProductImage> ProductImages => Set<ProductImage>();
    public DbSet<ProductVariant> ProductVariants => Set<ProductVariant>();
    public DbSet<VariantAttributeTemplate> VariantAttributeTemplates
        => Set<VariantAttributeTemplate>();
    public DbSet<VariantAttributeValue> VariantAttributeValues
        => Set<VariantAttributeValue>();
    public DbSet<AddonGroup> AddonGroups => Set<AddonGroup>();
    public DbSet<Addon> Addons => Set<Addon>();

    // ── New attribute template sets ───────────────────────────────────────
    public DbSet<ProductAttributeGroup> AttributeGroups
        => Set<ProductAttributeGroup>();
    public DbSet<ProductAttributeTemplate> AttributeTemplates
        => Set<ProductAttributeTemplate>();
    public DbSet<ProductAttributeValue> ProductAttributeValues
        => Set<ProductAttributeValue>();
    public DbSet<StoreAttributeOverride> StoreAttributeOverrides
        => Set<StoreAttributeOverride>();
    public DbSet<TenantTemplateAssignment> TenantTemplateAssignments
        => Set<TenantTemplateAssignment>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.ApplyConfigurationsFromAssembly(
            typeof(CatalogDbContext).Assembly);
    }
}