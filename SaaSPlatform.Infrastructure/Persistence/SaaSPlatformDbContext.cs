using AuthCoreKit.IAM.Entities;
using AuthCoreKit.IAM.Interfaces;
using Microsoft.EntityFrameworkCore;
using SaaSPlatform.Core.Billing.Entities;
using SaaSPlatform.Core.Catalog.Entities;
using SaaSPlatform.Core.Tenant.Entities;
using SaaSPlatform.Core.Tenant.Interfaces;
using SaaSPlatform.Infrastructure.Persistence.Extensions;
using SaaSPlatform.Infrastructure.Services;
using SaaSPlatform.Infrastructure.Services.TenantServices;

namespace SaaSPlatform.Infrastructure.Persistence;

public class SaaSPlatformDbContext : AuditableDbContext, IIamDbContext
{
    private readonly ITenantContext _tenantContext;

    public SaaSPlatformDbContext(
        DbContextOptions<SaaSPlatformDbContext> options,
        ITenantContext tenantContext,
        ICurrentUserService? currentUser = null)
        : base(options, currentUser)
    {
        _tenantContext = tenantContext ?? new NullTenantContext();
    }

    // Domain DbSets (Tenant, Catalog, Billing, Support)
    public DbSet<TenantAccount> TenantAccounts => Set<TenantAccount>();
    public DbSet<TenantLegalInfo> TenantLegalInfos => Set<TenantLegalInfo>();
    public DbSet<Store> Stores => Set<Store>();
    public DbSet<Product> Products => Set<Product>();
    public DbSet<Addon> Addons => Set<Addon>();
    public DbSet<AddonGroup> AddonGroups => Set<AddonGroup>();
    public DbSet<AddonGroupItem> AddonGroupItems => Set<AddonGroupItem>();
    public DbSet<Plan> Plans => Set<Plan>();
    public DbSet<Subscription> Subscriptions => Set<Subscription>();
    public DbSet<Payment> Payments => Set<Payment>();
    public DbSet<Zone> Zones => Set<Zone>();
    public DbSet<Cuisine> Cuisines => Set<Cuisine>();
    public DbSet<StoreCuisine> StoreCuisines => Set<StoreCuisine>();
    public DbSet<StoreDeliveryZone> StoreDeliveryZones => Set<StoreDeliveryZone>();

    // IAM DbSets
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
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Apply IAM configurations (now includes relationships)
        modelBuilder.ApplyConfigurationsFromAssembly(
            typeof(AuthCoreKit.IAM.Configuration.UserConfiguration).Assembly);

        // Apply your local configurations
        modelBuilder.ApplyConfigurationsFromAssembly(
            typeof(SaaSPlatformDbContext).Assembly);

        // Tenant global query filter
        modelBuilder.ApplyTenantFilters(_tenantContext);
    }
}