using CoreKit.Tenant.Abstractions;
using CoreKit.Subscription.Entities;
using CoreKit.Subscription.Persistence;
using Microsoft.EntityFrameworkCore;

namespace CoreKit.Subscription.Services;

public class PlanLimitProvider : IPlanLimitProvider
{
    private readonly SubscriptionDbContext _db;

    public PlanLimitProvider(SubscriptionDbContext db) => _db = db;

    public async Task<int?> GetMaxStoresAsync(Guid tenantId, CancellationToken ct = default)
    {
        var plan = await GetActivePlanAsync(tenantId, ct);
        return plan?.MaxStores;
    }

    public async Task<int?> GetMaxProductsAsync(Guid tenantId, CancellationToken ct = default)
    {
        var plan = await GetActivePlanAsync(tenantId, ct);
        return plan?.MaxProducts;
    }

    public async Task<int?> GetMaxCategoriesAsync(Guid tenantId, CancellationToken ct = default)
    {
        var plan = await GetActivePlanAsync(tenantId, ct);
        return plan?.MaxCategories;
    }

    public async Task<bool> IsFeatureEnabledAsync(Guid tenantId, string featureCode, CancellationToken ct = default)
    {
        var plan = await GetActivePlanAsync(tenantId, ct);
        if (plan == null) return false;
        return featureCode switch
        {
            "custom_domain" => plan.CustomDomainEnabled,
            "theme_customization" => plan.ThemeCustomizationEnabled,
            _ => false
        };
    }

    private async Task<Plan?> GetActivePlanAsync(Guid tenantId, CancellationToken ct)
    {
        return await _db.TenantSubscriptions
            .Where(ts => ts.TenantId == tenantId
                         && ts.Status == SubscriptionStatus.Active
                         && ts.StartDate <= DateTime.UtcNow
                         && (ts.EndDate == null || ts.EndDate >= DateTime.UtcNow))
            .Select(ts => ts.Plan)
            .FirstOrDefaultAsync(ct);
    }

    public async Task<int> GetMaxCategoryLevelAsync(Guid tenantId, CancellationToken ct)
    {
        var plan = await GetActivePlanAsync(tenantId, ct);
        return plan?.MaxCategoryLevel ?? 1;
    }

    public async Task<bool> IsVariantsEnabledAsync(Guid tenantId, CancellationToken ct)
    {
        var plan = await GetActivePlanAsync(tenantId, ct);
        return plan?.EnableVariants ?? false;
    }

    public async Task<bool> IsAddonsEnabledAsync(Guid tenantId, CancellationToken ct)
    {
        var plan = await GetActivePlanAsync(tenantId, ct);
        return plan?.EnableAddons ?? false;
    }

    public async Task<int?> GetMaxVariantsPerProductAsync(Guid tenantId, CancellationToken ct)
    {
        var plan = await GetActivePlanAsync(tenantId, ct);
        return plan?.MaxVariantsPerProduct;
    }

    public async Task<int?> GetMaxAddonsPerProductAsync(Guid tenantId, CancellationToken ct)
    {
        var plan = await GetActivePlanAsync(tenantId, ct);
        return plan?.MaxAddonsPerProduct;
    }
}