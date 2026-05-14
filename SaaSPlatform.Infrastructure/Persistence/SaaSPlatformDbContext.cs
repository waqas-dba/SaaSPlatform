using AuthCoreKit.IAM.Interfaces;
using Microsoft.EntityFrameworkCore;
using SaaSPlatform.Core.Billing.Entities;
using SaaSPlatform.Core.Catalog.Entities;
using SaaSPlatform.Core.Tenant.Entities;
using SaaSPlatform.SharedKernel.Common;
using TenantKit.Abstractions;
using TenantKit.Entities;
using TenantKit.Interfaces;

namespace SaaSPlatform.Infrastructure.Persistence;

public class SaaSPlatformDbContext : AuditableDbContext, IIamDbContext, ITenantKitDbContext
{
    private readonly ITenantContext _tenantContext;

    public SaaSPlatformDbContext(DbContextOptions<SaaSPlatformDbContext> options, ITenantContext tenantContext, ICurrentUserService? currentUser = null)
        : base(options, currentUser) => _tenantContext = tenantContext;

    public DbSet<TenantKit.Entities.Tenant> Tenants => Set<TenantKit.Entities.Tenant>();
    public DbSet<TenantKit.Entities.Store> Stores => Set<TenantKit.Entities.Store>();
    public DbSet<TenantKit.Entities.TenantLegalInfo>? TenantLegalInfos => Set<TenantKit.Entities.TenantLegalInfo>();

    public DbSet<AuthCoreKit.IAM.Entities.User> Users => Set<AuthCoreKit.IAM.Entities.User>();
    public DbSet<AuthCoreKit.IAM.Entities.Role> Roles => Set<AuthCoreKit.IAM.Entities.Role>();
    public DbSet<AuthCoreKit.IAM.Entities.UserRole> UserRoles => Set<AuthCoreKit.IAM.Entities.UserRole>();
    public DbSet<AuthCoreKit.IAM.Entities.Permission> Permissions => Set<AuthCoreKit.IAM.Entities.Permission>();
    public DbSet<AuthCoreKit.IAM.Entities.PermissionModule> PermissionModules => Set<AuthCoreKit.IAM.Entities.PermissionModule>();
    public DbSet<AuthCoreKit.IAM.Entities.RolePermission> RolePermissions => Set<AuthCoreKit.IAM.Entities.RolePermission>();
    public DbSet<AuthCoreKit.IAM.Entities.RefreshToken> RefreshTokens => Set<AuthCoreKit.IAM.Entities.RefreshToken>();
    public DbSet<AuthCoreKit.IAM.Entities.UserDocument> UserDocuments => Set<AuthCoreKit.IAM.Entities.UserDocument>();
    public DbSet<AuthCoreKit.IAM.Entities.UserIdentity> UserIdentities => Set<AuthCoreKit.IAM.Entities.UserIdentity>();
    public DbSet<AuthCoreKit.IAM.Entities.RoleDocumentRequirement> RoleDocumentRequirements => Set<AuthCoreKit.IAM.Entities.RoleDocumentRequirement>();

    public DbSet<Product> Products => Set<Product>();
    public DbSet<Addon> Addons => Set<Addon>();
    public DbSet<AddonGroup> AddonGroups => Set<AddonGroup>();
    public DbSet<AddonGroupItem> AddonGroupItems => Set<AddonGroupItem>();
    public DbSet<Plan> Plans => Set<Plan>();
    public DbSet<Subscription> Subscriptions => Set<Subscription>();
    public DbSet<Payment> Payments => Set<Payment>();
    public DbSet<Cuisine> Cuisines => Set<Cuisine>();
    public DbSet<Zone> Zones => Set<Zone>();
    public DbSet<StoreCuisine> StoreCuisines => Set<StoreCuisine>();
    public DbSet<StoreDeliveryZone> StoreDeliveryZones => Set<StoreDeliveryZone>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AuthCoreKit.IAM.Configuration.UserConfiguration).Assembly);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(TenantKit.Configuration.TenantConfiguration).Assembly);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(SaaSPlatformDbContext).Assembly);

        ApplyTenantFilters(modelBuilder);
    }

    private void ApplyTenantFilters(ModelBuilder modelBuilder)
    {
        foreach (var entityType in modelBuilder.Model.GetEntityTypes())
        {
            if (typeof(ITenantScoped).IsAssignableFrom(entityType.ClrType))
            {
                var method = typeof(SaaSPlatformDbContext)
                    .GetMethod(nameof(SetTenantFilter),
                        System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)!
                    .MakeGenericMethod(entityType.ClrType);
                method.Invoke(this, new object[] { modelBuilder });
            }
        }
    }

    private void SetTenantFilter<TEntity>(ModelBuilder builder) where TEntity : class, ITenantScoped
    {
        builder.Entity<TEntity>().HasQueryFilter(e =>
            !_tenantContext.TenantId.HasValue || e.TenantId == _tenantContext.TenantId.Value);
    }
}