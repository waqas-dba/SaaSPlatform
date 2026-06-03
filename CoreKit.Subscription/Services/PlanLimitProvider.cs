using CoreKit.SharedKernel.Interfaces;
using CoreKit.Subscription.Entities;
using CoreKit.Subscription.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;

namespace CoreKit.Subscription.Services;

public class PlanLimitProvider : IPlanLimitProvider
{
    private readonly SubscriptionDbContext _db;
    private readonly IMemoryCache _cache;
    private static readonly TimeSpan CacheTtl = TimeSpan.FromMinutes(5);

    public PlanLimitProvider(SubscriptionDbContext db, IMemoryCache cache)
    {
        _db = db;
        _cache = cache;
    }

    private async Task<Plan?> GetActivePlanAsync(
        Guid tenantId, CancellationToken ct)
    {
        var key = $"plan:active:{tenantId}";

        if (_cache.TryGetValue(key, out Plan? cached))
            return cached;

        var plan = await _db.TenantSubscriptions
            .Where(ts =>
                ts.TenantId == tenantId &&
                ts.Status == SubscriptionStatus.Active &&
                ts.StartDate <= DateTime.UtcNow &&
                (ts.EndDate == null || ts.EndDate >= DateTime.UtcNow))
            .Select(ts => ts.Plan)
            .FirstOrDefaultAsync(ct);

        _cache.Set(key, plan, CacheTtl);
        return plan;
    }

    public async Task<int?> GetMaxStoresAsync(
        Guid tenantId, CancellationToken ct = default)
        => (await GetActivePlanAsync(tenantId, ct))?.MaxStores;

    public async Task<int?> GetMaxProductsAsync(
        Guid tenantId, CancellationToken ct = default)
        => (await GetActivePlanAsync(tenantId, ct))?.MaxProducts;

    public async Task<int?> GetMaxCategoriesAsync(
        Guid tenantId, CancellationToken ct = default)
        => (await GetActivePlanAsync(tenantId, ct))?.MaxCategories;

    public async Task<bool> IsFeatureEnabledAsync(
        Guid tenantId, string featureCode, CancellationToken ct = default)
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

    public async Task<int> GetMaxCategoryLevelAsync(
        Guid tenantId, CancellationToken ct = default)
        => (await GetActivePlanAsync(tenantId, ct))?.MaxCategoryLevel ?? 1;

    public async Task<bool> IsVariantsEnabledAsync(
        Guid tenantId, CancellationToken ct = default)
        => (await GetActivePlanAsync(tenantId, ct))?.EnableVariants ?? false;

    public async Task<bool> IsAddonsEnabledAsync(
        Guid tenantId, CancellationToken ct = default)
        => (await GetActivePlanAsync(tenantId, ct))?.EnableAddons ?? false;

    public async Task<int?> GetMaxVariantsPerProductAsync(
        Guid tenantId, CancellationToken ct = default)
        => (await GetActivePlanAsync(tenantId, ct))?.MaxVariantsPerProduct;

    public async Task<int?> GetMaxAddonsPerProductAsync(
        Guid tenantId, CancellationToken ct = default)
        => (await GetActivePlanAsync(tenantId, ct))?.MaxAddonsPerProduct;
}