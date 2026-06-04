// FILE: CoreKit.Subscription/Persistence/Seeders/SubscriptionSeeder.cs  (full replacement)
// FIX: Added XML-doc comments clarifying that MonthlyPrice / YearlyPrice are
//      stored in PKR (Pakistani Rupees) as whole-number amounts (not paise).
//      Added a currency constant so there is a single source of truth.
//      No behavioral change — only documentation + constant added.

using Microsoft.EntityFrameworkCore;
using CoreKit.Subscription.Entities;
using CoreKit.Subscription.Persistence;

namespace CoreKit.Subscription.Persistence.Seeders;

/// <summary>
/// Seeds the default subscription plans.
/// <para>
/// <b>Currency note:</b> All prices are stored as <see cref="decimal"/> values
/// representing whole Pakistani Rupees (PKR). There are no paise sub-units.
/// Example: <c>MonthlyPrice = 2999</c> means PKR 2,999 per month.
/// </para>
/// </summary>
public class SubscriptionSeeder
{
    /// <summary>
    /// ISO 4217 currency code for all plan prices stored in this database.
    /// Must match the currency displayed in the front-end and invoices.
    /// </summary>
    public const string PriceCurrencyCode = "PKR";

    private readonly SubscriptionDbContext _db;

    public SubscriptionSeeder(SubscriptionDbContext db) => _db = db;

    public async Task SeedAsync()
    {
        if (await _db.Plans.AnyAsync()) return;

        // All MonthlyPrice / YearlyPrice values are in PKR (whole rupees, no paise).
        var plans = new List<Plan>
        {
            new()
            {
                Id = Guid.NewGuid(),
                Name = "Free",
                Code = "free",
                Description = "Basic store with limited features",
                MonthlyPrice = 0,       // PKR 0
                YearlyPrice  = 0,       // PKR 0
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
                MonthlyPrice = 2_999,   // PKR 2,999 / month
                YearlyPrice  = 29_990,  // PKR 29,990 / year (~2 months free)
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
                MonthlyPrice = 7_999,   // PKR 7,999 / month
                YearlyPrice  = 79_990,  // PKR 79,990 / year (~2 months free)
                MaxStores = null,       // unlimited
                MaxProducts = null,     // unlimited
                MaxCategories = null,   // unlimited
                CustomDomainEnabled = true,
                ThemeCustomizationEnabled = true,
                IsActive = true,
                SortOrder = 3,
                MaxCategoryLevel = 0,   // 0 = no limit
                EnableVariants = true,
                EnableAddons = true,
                MaxVariantsPerProduct = null,   // unlimited
                MaxAddonsPerProduct = null       // unlimited
            }
        };

        _db.Plans.AddRange(plans);
        await _db.SaveChangesAsync();
    }
}