// CoreKit.Subscription/Persistence/Seeders/SubscriptionSeeder.cs
using Microsoft.EntityFrameworkCore;
using CoreKit.Subscription.Entities;
using CoreKit.Subscription.Persistence;

namespace CoreKit.Subscription.Persistence.Seeders;

public class SubscriptionSeeder
{
    private readonly SubscriptionDbContext _db;

    public SubscriptionSeeder(SubscriptionDbContext db) => _db = db;

    public async Task SeedAsync()
    {
        if (await _db.Plans.AnyAsync()) return;

        var plans = new List<Plan>
        {
            new()
            {
                Id = Guid.NewGuid(),
                Name = "Free",
                Code = "free",
                Description = "Basic store with limited features",
                MonthlyPrice = 0,
                YearlyPrice = 0,
                MaxStores = 1,
                MaxProducts = 50,
                MaxCategories = 10,
                CustomDomainEnabled = false,
                ThemeCustomizationEnabled = false,
                IsActive = true,
                SortOrder = 1,
                MaxCategoryLevel = 1,
                EnableVariants = false,
                EnableAddons = false,
                MaxVariantsPerProduct = 0,
                MaxAddonsPerProduct = 0
            },
            new()
            {
                Id = Guid.NewGuid(),
                Name = "Basic",
                Code = "basic",
                Description = "For growing businesses",
                MonthlyPrice = 2999,
                YearlyPrice = 29990,
                MaxStores = 3,
                MaxProducts = 500,
                MaxCategories = 50,
                CustomDomainEnabled = false,
                ThemeCustomizationEnabled = true,
                IsActive = true,
                SortOrder = 2,
                MaxCategoryLevel = 2,
                EnableVariants = true,
                EnableAddons = true,
                MaxVariantsPerProduct = 5,
                MaxAddonsPerProduct = 10
            },
            new()
            {
                Id = Guid.NewGuid(),
                Name = "Pro",
                Code = "pro",
                Description = "For established businesses",
                MonthlyPrice = 7999,
                YearlyPrice = 79990,
                MaxStores = null,       // unlimited
                MaxProducts = null,
                MaxCategories = null,
                CustomDomainEnabled = true,
                ThemeCustomizationEnabled = true,
                IsActive = true,
                SortOrder = 3,
                MaxCategoryLevel = 0,   // unlimited
                EnableVariants = true,
                EnableAddons = true,
                MaxVariantsPerProduct = null, // unlimited
                MaxAddonsPerProduct = null    // unlimited
            }
        };

        _db.Plans.AddRange(plans);
        await _db.SaveChangesAsync();
    }
}