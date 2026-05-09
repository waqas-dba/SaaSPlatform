using Microsoft.EntityFrameworkCore;
using SaaSPlatform.Core.Billing.Entities;
using SaaSPlatform.Core.Catalog.Entities;
using SaaSPlatform.Core.IAM.Entities;
using SaaSPlatform.Core.Tenant.Entities;
using SaaSPlatform.Core.Tenant.Interfaces;
using SaaSPlatform.Infrastructure.Persistence.Extensions;
using SaaSPlatform.Infrastructure.Services;

namespace SaaSPlatform.Infrastructure.Persistence;

public class SaaSPlatformDbContext : AuditableDbContext
{
    private readonly ITenantContext _tenantContext;

    public SaaSPlatformDbContext(
        DbContextOptions<SaaSPlatformDbContext> options,
        ITenantContext tenantContext,
        CurrentUserService? currentUser = null)
        : base(options, currentUser)
    {
        _tenantContext = tenantContext
            ?? throw new ArgumentNullException(nameof(tenantContext));
    }

    // =========================
    // TENANT MODULE
    // =========================
    public DbSet<TenantAccount> TenantAccounts => Set<TenantAccount>();
    public DbSet<TenantUser> TenantUsers => Set<TenantUser>();
    public DbSet<TenantLegalInfo> TenantLegalInfos => Set<TenantLegalInfo>();

    // =========================
    // IAM MODULE
    // =========================
    public DbSet<User> Users => Set<User>();
    public DbSet<Role> Roles => Set<Role>();
    public DbSet<UserRole> UserRoles => Set<UserRole>();
    public DbSet<Permission> Permissions => Set<Permission>();
    public DbSet<RolePermission> RolePermissions => Set<RolePermission>();
    public DbSet<PermissionModule> PermissionModules => Set<PermissionModule>();
    public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();

    // =========================
    // CATALOG MODULE
    // =========================
    public DbSet<Store> Stores => Set<Store>();
    public DbSet<Product> Products => Set<Product>();
    public DbSet<Addon> Addons => Set<Addon>();
    public DbSet<AddonGroup> AddonGroups => Set<AddonGroup>();
    public DbSet<AddonGroupItem> AddonGroupItems => Set<AddonGroupItem>();

    // =========================
    // BILLING MODULE
    // =========================
    public DbSet<Plan> Plans => Set<Plan>();
    public DbSet<Subscription> Subscriptions => Set<Subscription>();
    public DbSet<Payment> Payments => Set<Payment>();

    // =========================
    // SUPPORT MODULE
    // =========================
    public DbSet<Zone> Zones => Set<Zone>();
    public DbSet<Cuisine> Cuisines => Set<Cuisine>();
    public DbSet<StoreCuisine> StoreCuisines => Set<StoreCuisine>();
    public DbSet<StoreDeliveryZone> StoreDeliveryZones => Set<StoreDeliveryZone>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Apply all configurations
        modelBuilder.ApplyConfigurationsFromAssembly(
            typeof(SaaSPlatformDbContext).Assembly);

        // 🚨 ALWAYS APPLY TENANT FILTER (NO NULL ALLOWED)
        modelBuilder.ApplyTenantFilters(_tenantContext);
    }
}