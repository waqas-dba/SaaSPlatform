using SaaSPlatform.Core.Billing.Entities;
using SaaSPlatform.Core.Catalog.Entities;
using SaaSPlatform.Core.IAM.Entities;
using SaaSPlatform.Core.Tenant.Entities;

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

    // ===== NEW: CUISINES =====
    public static readonly List<Cuisine> Cuisines = new()
{
    new() { Id = Guid.Parse("60000000-0000-0000-0000-000000000001"), Name = "Fast Food" },
    new() { Id = Guid.Parse("60000000-0000-0000-0000-000000000002"), Name = "Dessert" },
    new() { Id = Guid.Parse("60000000-0000-0000-0000-000000000003"), Name = "BBQ" },
    new() { Id = Guid.Parse("60000000-0000-0000-0000-000000000004"), Name = "Sea Food" },
    new() { Id = Guid.Parse("60000000-0000-0000-0000-000000000005"), Name = "Hot Beverages" },
    new() { Id = Guid.Parse("60000000-0000-0000-0000-000000000006"), Name = "Continental" },
    new() { Id = Guid.Parse("60000000-0000-0000-0000-000000000007"), Name = "Drinks & Beverages" },
    new() { Id = Guid.Parse("60000000-0000-0000-0000-000000000008"), Name = "Broast" },
    new() { Id = Guid.Parse("60000000-0000-0000-0000-000000000009"), Name = "Chinese" },
    new() { Id = Guid.Parse("60000000-0000-0000-0000-00000000000A"), Name = "Karahi and Handi" },
    new() { Id = Guid.Parse("60000000-0000-0000-0000-00000000000B"), Name = "Pizza" },
    new() { Id = Guid.Parse("60000000-0000-0000-0000-00000000000C"), Name = "Steak" },
    new() { Id = Guid.Parse("60000000-0000-0000-0000-00000000000D"), Name = "Vegetarian" },
    new() { Id = Guid.Parse("60000000-0000-0000-0000-00000000000E"), Name = "Middle Eastern" },
    new() { Id = Guid.Parse("60000000-0000-0000-0000-00000000000F"), Name = "Ice Cream" },
    new() { Id = Guid.Parse("60000000-0000-0000-0000-000000000010"), Name = "Savouries" },
    new() { Id = Guid.Parse("60000000-0000-0000-0000-000000000011"), Name = "Biryani" },
    new() { Id = Guid.Parse("60000000-0000-0000-0000-000000000012"), Name = "Pulao" },
    new() { Id = Guid.Parse("60000000-0000-0000-0000-000000000013"), Name = "Rice" },
    new() { Id = Guid.Parse("60000000-0000-0000-0000-000000000014"), Name = "Sandwiches" },
    new() { Id = Guid.Parse("60000000-0000-0000-0000-000000000015"), Name = "Street Food" },
    new() { Id = Guid.Parse("60000000-0000-0000-0000-000000000016"), Name = "Desi" },
    new() { Id = Guid.Parse("60000000-0000-0000-0000-000000000017"), Name = "Breakfast" }
};

    // ===== NEW: ZONES (sample for Islamabad) =====
    public static readonly List<Zone> Zones = new()
{
    new() { Id = Guid.Parse("70000000-0000-0000-0000-000000000001"), Name = "H-Sectors", City = "Islamabad" },
    new() { Id = Guid.Parse("70000000-0000-0000-0000-000000000002"), Name = "F-Sectors", City = "Islamabad" },
    new() { Id = Guid.Parse("70000000-0000-0000-0000-000000000003"), Name = "G-Sectors", City = "Islamabad" },
    new() { Id = Guid.Parse("70000000-0000-0000-0000-000000000004"), Name = "I-Sectors", City = "Islamabad" },
    new() { Id = Guid.Parse("70000000-0000-0000-0000-000000000005"), Name = "Blue Area", City = "Islamabad" },
    new() { Id = Guid.Parse("70000000-0000-0000-0000-000000000006"), Name = "Bahria Town", City = "Islamabad" },
    new() { Id = Guid.Parse("70000000-0000-0000-0000-000000000007"), Name = "DHA", City = "Islamabad" },
    new() { Id = Guid.Parse("70000000-0000-0000-0000-000000000008"), Name = "Saddar", City = "Rawalpindi" },
    new() { Id = Guid.Parse("70000000-0000-0000-0000-000000000009"), Name = "Chaklala", City = "Rawalpindi" }
};

    public static readonly List<Addon> Addons = new()
    {
        new() { Id = Guid.Parse("80000000-0000-0000-0000-000000000001"), Name = "Extra Cheese", Price = 1.50m },
        new() { Id = Guid.Parse("80000000-0000-0000-0000-000000000002"), Name = "Pepperoni",      Price = 2.00m },
        new() { Id = Guid.Parse("80000000-0000-0000-0000-000000000003"), Name = "Mushrooms",      Price = 1.80m },
        new() { Id = Guid.Parse("80000000-0000-0000-0000-000000000004"), Name = "Olives",         Price = 1.20m },
        new() { Id = Guid.Parse("80000000-0000-0000-0000-000000000005"), Name = "Ketchup",        Price = 0.00m },
        new() { Id = Guid.Parse("80000000-0000-0000-0000-000000000006"), Name = "Mayo",           Price = 0.00m },
        new() { Id = Guid.Parse("80000000-0000-0000-0000-000000000007"), Name = "Hot Sauce",      Price = 0.50m }
    };

    // ===== NEW: ADDON GROUPS =====
    public static readonly Guid ToppingsGroupId = Guid.Parse("90000000-0000-0000-0000-000000000001");
    public static readonly Guid SaucesGroupId = Guid.Parse("90000000-0000-0000-0000-000000000002");

    public static readonly List<AddonGroup> AddonGroups = new()
    {
        new AddonGroup
        {
            Id = ToppingsGroupId,
            Name = "Choose your toppings",
            MaxSelect = 3,
            IsRequired = false,
            DisplayOrder = 1
        },
        new AddonGroup
        {
            Id = SaucesGroupId,
            Name = "Select sauces",
            MaxSelect = 2,
            IsRequired = false,
            DisplayOrder = 2
        }
    };

    // ===== NEW: ADDON GROUP ITEMS =====
    public static readonly List<AddonGroupItem> AddonGroupItems = new()
    {
        // Toppings
        new() { Id = Guid.Parse("A0000000-0000-0000-0000-000000000001"), AddonGroupId = ToppingsGroupId, AddonId = Guid.Parse("80000000-0000-0000-0000-000000000001"), PriceAdjustment = 0, DisplayOrder = 1 },
        new() { Id = Guid.Parse("A0000000-0000-0000-0000-000000000002"), AddonGroupId = ToppingsGroupId, AddonId = Guid.Parse("80000000-0000-0000-0000-000000000002"), PriceAdjustment = 0, DisplayOrder = 2 },
        new() { Id = Guid.Parse("A0000000-0000-0000-0000-000000000003"), AddonGroupId = ToppingsGroupId, AddonId = Guid.Parse("80000000-0000-0000-0000-000000000003"), PriceAdjustment = 0, DisplayOrder = 3 },
        new() { Id = Guid.Parse("A0000000-0000-0000-0000-000000000004"), AddonGroupId = ToppingsGroupId, AddonId = Guid.Parse("80000000-0000-0000-0000-000000000004"), PriceAdjustment = 0, DisplayOrder = 4 },
        // Sauces
        new() { Id = Guid.Parse("A0000000-0000-0000-0000-000000000005"), AddonGroupId = SaucesGroupId,   AddonId = Guid.Parse("80000000-0000-0000-0000-000000000005"), PriceAdjustment = 0, DisplayOrder = 1 },
        new() { Id = Guid.Parse("A0000000-0000-0000-0000-000000000006"), AddonGroupId = SaucesGroupId,   AddonId = Guid.Parse("80000000-0000-0000-0000-000000000006"), PriceAdjustment = 0, DisplayOrder = 2 },
        new() { Id = Guid.Parse("A0000000-0000-0000-0000-000000000007"), AddonGroupId = SaucesGroupId,   AddonId = Guid.Parse("80000000-0000-0000-0000-000000000007"), PriceAdjustment = 0, DisplayOrder = 3 }
    };
}