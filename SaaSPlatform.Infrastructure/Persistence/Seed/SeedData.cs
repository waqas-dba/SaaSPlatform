using SaaSPlatform.Core.Billing.Entities;
using SaaSPlatform.Core.IAM.Entities;

namespace SaaSPlatform.Infrastructure.Persistence.Seed;

public static class SeedData
{
    // SYSTEM TENANT
    public static readonly Guid SystemTenantId =
        Guid.Parse("11111111-1111-1111-1111-111111111111");

    // SUPER ADMIN ROLE
    public static readonly Guid SuperAdminRoleId =
        Guid.Parse("22222222-2222-2222-2222-222222222222");

    // MODULES
    public static readonly List<PermissionModule> Modules =
    [
        new()
        {
            Id = Guid.Parse("30000000-0000-0000-0000-000000000001"),
            Name = "Catalog",
            Code = "catalog"
        },

        new()
        {
            Id = Guid.Parse("30000000-0000-0000-0000-000000000002"),
            Name = "Orders",
            Code = "orders"
        },

        new()
        {
            Id = Guid.Parse("30000000-0000-0000-0000-000000000003"),
            Name = "Billing",
            Code = "billing"
        },

        new()
        {
            Id = Guid.Parse("30000000-0000-0000-0000-000000000004"),
            Name = "IAM",
            Code = "iam"
        }
    ];

    // PERMISSIONS
    public static readonly List<Permission> Permissions =
    [
        // CATALOG
        new()
        {
            Id = Guid.Parse("40000000-0000-0000-0000-000000000001"),
            Name = "catalog.view",
            PermissionModuleId = Guid.Parse("30000000-0000-0000-0000-000000000001")
        },

        new()
        {
            Id = Guid.Parse("40000000-0000-0000-0000-000000000002"),
            Name = "catalog.create",
            PermissionModuleId = Guid.Parse("30000000-0000-0000-0000-000000000001")
        },

        new()
        {
            Id = Guid.Parse("40000000-0000-0000-0000-000000000003"),
            Name = "catalog.update",
            PermissionModuleId = Guid.Parse("30000000-0000-0000-0000-000000000001")
        },

        // ORDERS
        new()
        {
            Id = Guid.Parse("40000000-0000-0000-0000-000000000004"),
            Name = "orders.view",
            PermissionModuleId = Guid.Parse("30000000-0000-0000-0000-000000000002")
        },

        new()
        {
            Id = Guid.Parse("40000000-0000-0000-0000-000000000005"),
            Name = "orders.manage",
            PermissionModuleId = Guid.Parse("30000000-0000-0000-0000-000000000002")
        },

        // BILLING
        new()
        {
            Id = Guid.Parse("40000000-0000-0000-0000-000000000006"),
            Name = "billing.view",
            PermissionModuleId = Guid.Parse("30000000-0000-0000-0000-000000000003")
        },

        new()
        {
            Id = Guid.Parse("40000000-0000-0000-0000-000000000007"),
            Name = "billing.manage",
            PermissionModuleId = Guid.Parse("30000000-0000-0000-0000-000000000003")
        },

        // IAM
        new()
        {
            Id = Guid.Parse("40000000-0000-0000-0000-000000000008"),
            Name = "users.manage",
            PermissionModuleId = Guid.Parse("30000000-0000-0000-0000-000000000004")
        }
    ];

    // PLANS
    public static readonly List<Plan> Plans =
    [
        new()
        {
            Id = Guid.Parse("50000000-0000-0000-0000-000000000001"),
            Name = "Starter",
            PriceMonthly = 0,
            PriceYearly = 0,
            MaxStores = 1,
            MaxUsers = 2,
            MaxProducts = 50,
            MaxCategories = 10,
            MaxOrdersPerMonth = 1000,
            IsActive = true
        },

        new()
        {
            Id = Guid.Parse("50000000-0000-0000-0000-000000000002"),
            Name = "Pro",
            PriceMonthly = 29,
            PriceYearly = 290,
            MaxStores = 5,
            MaxUsers = 20,
            MaxProducts = 1000,
            MaxCategories = 100,
            MaxOrdersPerMonth = 50000,
            IsActive = true
        },

        new()
        {
            Id = Guid.Parse("50000000-0000-0000-0000-000000000003"),
            Name = "Enterprise",
            PriceMonthly = 199,
            PriceYearly = 1990,
            MaxStores = 999,
            MaxUsers = 999,
            MaxProducts = 999999,
            MaxCategories = 999999,
            MaxOrdersPerMonth = 999999,
            IsActive = true
        }
    ];

    // SUPER ADMIN ROLE
    public static readonly Role SuperAdminRole = new()
    {
        Id = SuperAdminRoleId,
        TenantId = SystemTenantId,
        Name = "SuperAdmin",
        Description = "System Super Administrator",
        IsSystem = true
    };
}