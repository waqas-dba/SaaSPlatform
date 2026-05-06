using Microsoft.EntityFrameworkCore;
using SaaSPlatform.Core.Agents.Entities;
using SaaSPlatform.Core.Billing.Entities;
using SaaSPlatform.Core.Catalog.Entities;
using SaaSPlatform.Core.IAM.Entities;
using SaaSPlatform.Core.Orders.Entities;
using SaaSPlatform.Core.Tenant.Entities;

public class SaaSPlatformDbContext : DbContext
{
    public SaaSPlatformDbContext(DbContextOptions<SaaSPlatformDbContext> options) : base(options) { }

    public DbSet<Tenant> Tenants { get; set; }
    public DbSet<TenantDomain> TenantDomains { get; set; }
    public DbSet<User> Users { get; set; }
    public DbSet<Role> Roles { get; set; }
    public DbSet<UserRole> UserRoles { get; set; }
    public DbSet<Permission> Permissions { get; set; }
    public DbSet<PermissionModule> PermissionModules { get; set; }
    public DbSet<RolePermission> RolePermissions { get; set; }
    public DbSet<Category> Categories { get; set; }
    public DbSet<Product> Products { get; set; }
    public DbSet<ProductVariant> ProductVariants { get; set; }
    public DbSet<Addon> Addons { get; set; }
    public DbSet<ProductAddon> ProductAddons { get; set; }
    public DbSet<Order> Orders { get; set; }
    public DbSet<OrderItem> OrderItems { get; set; }
    public DbSet<OrderItemAddon> OrderItemAddons { get; set; }
    public DbSet<Subscription> Subscriptions { get; set; }
    public DbSet<Plan> Plans { get; set; }
    public DbSet<TenantUsage> TenantUsages { get; set; }
    public DbSet<Agent> Agents { get; set; }
    public DbSet<AgentCommission> AgentCommissions { get; set; }
}