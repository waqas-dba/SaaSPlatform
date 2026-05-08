using Microsoft.EntityFrameworkCore;
using SaaSPlatform.Core.Billing.Entities;
using SaaSPlatform.Core.Catalog.Entities;
using SaaSPlatform.Core.IAM.Entities;

using SaaSPlatform.Core.Tenant.Entities;
using SaaSPlatform.Core.Tenant.Interfaces;
using SaaSPlatform.Infrastructure.Persistence.Extensions;
using SaaSPlatform.Infrastructure.Services;

namespace SaaSPlatform.Infrastructure.Persistence;

/// <summary>
/// Main database context.
/// </summary>
public class SaaSPlatformDbContext : AuditableDbContext
{
    private readonly ITenantContext? _tenantContext;

    public SaaSPlatformDbContext(
        DbContextOptions<SaaSPlatformDbContext> options,
        ITenantContext? tenantContext = null,
        CurrentUserService? currentUser = null)
        : base(options, currentUser)
    {
        _tenantContext = tenantContext;
    }

    // =========================================================
    // TENANT
    // =========================================================

    public DbSet<Tenant> Tenants => Set<Tenant>();

    public DbSet<TenantUser> TenantUsers => Set<TenantUser>();

    public DbSet<TenantLegalInfo> TenantLegalInfos =>
        Set<TenantLegalInfo>();

    // =========================================================
    // STORE
    // =========================================================

    public DbSet<Store> Stores => Set<Store>();

    public DbSet<StoreCuisine> StoreCuisines =>
        Set<StoreCuisine>();

    public DbSet<StoreDeliveryZone> StoreDeliveryZones =>
        Set<StoreDeliveryZone>();

    // =========================================================
    // IAM
    // =========================================================

    public DbSet<User> Users => Set<User>();

    public DbSet<Role> Roles => Set<Role>();

    public DbSet<UserRole> UserRoles => Set<UserRole>();

    public DbSet<Permission> Permissions =>
        Set<Permission>();

    public DbSet<RolePermission> RolePermissions =>
        Set<RolePermission>();

    public DbSet<PermissionModule> PermissionModules =>
        Set<PermissionModule>();

    public DbSet<RefreshToken> RefreshTokens =>
        Set<RefreshToken>();

    // =========================================================
    // BILLING
    // =========================================================

    public DbSet<Plan> Plans => Set<Plan>();

    public DbSet<Subscription> Subscriptions =>
        Set<Subscription>();

    public DbSet<Payment> Payments => Set<Payment>();

    // =========================================================
    // LOCATION
    // =========================================================

    public DbSet<Zone> Zones => Set<Zone>();

    public DbSet<Cuisine> Cuisines => Set<Cuisine>();

    // =========================================================
    // MENU
    // =========================================================

    public DbSet<Product> Products => Set<Product>();

    public DbSet<Addon> Addons => Set<Addon>();

    public DbSet<AddonGroup> AddonGroups =>
        Set<AddonGroup>();

    public DbSet<AddonGroupItem> AddonGroupItems =>
        Set<AddonGroupItem>();

    protected override void OnModelCreating(
        ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.ApplyConfigurationsFromAssembly(
            typeof(SaaSPlatformDbContext).Assembly);

        // =====================================================
        // Apply tenant filters ONLY when available
        // =====================================================

        if (_tenantContext is not null)
        {
            modelBuilder.ApplyTenantFilters(
                _tenantContext);
        }
    }
}