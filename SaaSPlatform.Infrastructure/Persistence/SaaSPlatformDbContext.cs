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
    public SaaSPlatformDbContext(
        DbContextOptions<SaaSPlatformDbContext> options)
        : base(options)
    {
    }

    // TENANT
    public DbSet<Tenant> Tenants => Set<Tenant>();
    public DbSet<TenantDomain> TenantDomains => Set<TenantDomain>();
    public DbSet<Store> Stores => Set<Store>();

    // IAM
    public DbSet<User> Users => Set<User>();
    public DbSet<Role> Roles => Set<Role>();
    public DbSet<UserRole> UserRoles => Set<UserRole>();
    public DbSet<Permission> Permissions => Set<Permission>();
    public DbSet<PermissionModule> PermissionModules => Set<PermissionModule>();
    public DbSet<RolePermission> RolePermissions => Set<RolePermission>();
    public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();

    // CATALOG
    public DbSet<Category> Categories => Set<Category>();
    public DbSet<Product> Products => Set<Product>();
    public DbSet<ProductVariant> ProductVariants => Set<ProductVariant>();
    public DbSet<Addon> Addons => Set<Addon>();
    public DbSet<ProductAddon> ProductAddons => Set<ProductAddon>();

    // ORDERS
    public DbSet<Order> Orders => Set<Order>();
    public DbSet<OrderItem> OrderItems => Set<OrderItem>();
    public DbSet<OrderItemAddon> OrderItemAddons => Set<OrderItemAddon>();
    public DbSet<RestaurantTable> RestaurantTables => Set<RestaurantTable>();

    // BILLING
    public DbSet<Subscription> Subscriptions => Set<Subscription>();
    public DbSet<Plan> Plans => Set<Plan>();
    public DbSet<Payment> Payments => Set<Payment>();
    public DbSet<TenantUsageLedger> TenantUsageLedgers => Set<TenantUsageLedger>();

    // AGENTS
    public DbSet<Agent> Agents => Set<Agent>();
    public DbSet<AgentCommission> AgentCommissions => Set<AgentCommission>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.ApplyConfigurationsFromAssembly(
            typeof(RolePermission).Assembly);

        modelBuilder.ApplyConfigurationsFromAssembly(
            typeof(SaaSPlatformDbContext).Assembly);

        ApplySoftDeleteFilter(modelBuilder); // 👈 ADD THIS

    }


    private static void ApplySoftDeleteFilter(ModelBuilder modelBuilder)
    {
        foreach (var entityType in modelBuilder.Model.GetEntityTypes())
        {
            // Only apply to entities that have IsDeleted property
            if (entityType.ClrType.GetProperty("IsDeleted") == null)
                continue;

            var parameter = Expression.Parameter(entityType.ClrType, "x");

            var property = Expression.Property(parameter, "IsDeleted");

            var filter = Expression.Lambda(
                Expression.Equal(property, Expression.Constant(false)),
                parameter
            );

            modelBuilder.Entity(entityType.ClrType)
                .HasQueryFilter(filter);
        }
    }
}