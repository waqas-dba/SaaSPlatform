using Microsoft.EntityFrameworkCore;
using SaaSPlatform.Core.Agents.Entities;
using SaaSPlatform.Core.Billing.Entities;
using SaaSPlatform.Core.Catalog.Entities;
using SaaSPlatform.Core.IAM.Entities;
using SaaSPlatform.Core.Orders.Entities;
using SaaSPlatform.Core.Tenant.Entities;
using System.Linq.Expressions;

namespace SaaSPlatform.Infrastructure.Persistence;

public class SaaSPlatformDbContext : DbContext
{
    public SaaSPlatformDbContext(DbContextOptions<SaaSPlatformDbContext> options) : base(options)
    {
    }

    // ========== TENANT ==========
    public DbSet<Tenant> Tenants => Set<Tenant>();
    public DbSet<TenantDomain> TenantDomains => Set<TenantDomain>();
    public DbSet<Store> Stores => Set<Store>();
    public DbSet<Zone> Zones => Set<Zone>();
    public DbSet<StoreDeliveryZone> StoreDeliveryZones => Set<StoreDeliveryZone>();
    public DbSet<StoreCuisine> StoreCuisines => Set<StoreCuisine>();
    public DbSet<TenantLegalInfo> TenantLegalInfos => Set<TenantLegalInfo>();

    // ========== IAM ==========
    public DbSet<User> Users => Set<User>();
    public DbSet<Role> Roles => Set<Role>();
    public DbSet<UserRole> UserRoles => Set<UserRole>();
    public DbSet<Permission> Permissions => Set<Permission>();
    public DbSet<PermissionModule> PermissionModules => Set<PermissionModule>();
    public DbSet<RolePermission> RolePermissions => Set<RolePermission>();
    public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();

    // ========== CATALOG ==========
    public DbSet<Cuisine> Cuisines => Set<Cuisine>();
    public DbSet<Category> Categories => Set<Category>();
    public DbSet<Product> Products => Set<Product>();
    public DbSet<ProductVariant> ProductVariants => Set<ProductVariant>();
    public DbSet<ProductImage> ProductImages => Set<ProductImage>();
    public DbSet<FeaturedProduct> FeaturedProducts => Set<FeaturedProduct>();

    // ========== ADDONS (Groups) ==========
    public DbSet<Addon> Addons => Set<Addon>();
    public DbSet<AddonGroup> AddonGroups => Set<AddonGroup>();
    public DbSet<AddonGroupItem> AddonGroupItems => Set<AddonGroupItem>();
    public DbSet<ProductAddonGroup> ProductAddonGroups => Set<ProductAddonGroup>();

    // ========== ORDERS ==========
    public DbSet<Order> Orders => Set<Order>();
    public DbSet<OrderItem> OrderItems => Set<OrderItem>();
    public DbSet<OrderItemAddon> OrderItemAddons => Set<OrderItemAddon>();
    public DbSet<RestaurantTable> RestaurantTables => Set<RestaurantTable>();

    // ========== BILLING ==========
    public DbSet<Subscription> Subscriptions => Set<Subscription>();
    public DbSet<Plan> Plans => Set<Plan>();
    public DbSet<Payment> Payments => Set<Payment>();
    public DbSet<TenantUsageLedger> TenantUsageLedgers => Set<TenantUsageLedger>();

    // ========== AGENTS ==========
    public DbSet<Agent> Agents => Set<Agent>();
    public DbSet<AgentCommission> AgentCommissions => Set<AgentCommission>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Auto-discover all configuration classes in the Infrastructure assembly
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(SaaSPlatformDbContext).Assembly);

        // Global soft-delete filter for all entities that implement AuditableEntity (IsDeleted)
        ApplySoftDeleteFilter(modelBuilder);
    }

    private static void ApplySoftDeleteFilter(ModelBuilder modelBuilder)
    {
        foreach (var entityType in modelBuilder.Model.GetEntityTypes())
        {
            // Only apply to entities that have an 'IsDeleted' property
            if (entityType.ClrType.GetProperty("IsDeleted") == null)
                continue;

            var parameter = Expression.Parameter(entityType.ClrType, "x");
            var property = Expression.Property(parameter, "IsDeleted");
            var filter = Expression.Lambda(
                Expression.Equal(property, Expression.Constant(false)),
                parameter);

            modelBuilder.Entity(entityType.ClrType).HasQueryFilter(filter);
        }
    }
}