// SaaSPlatform.Infrastructure/Services/SubscriptionAccessService.cs
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using SaaSPlatform.Core.Billing.Enums;
using SaaSPlatform.Core.Billing.Interfaces;
using SaaSPlatform.Infrastructure.Persistence;

namespace SaaSPlatform.Infrastructure.Services;

public class SubscriptionAccessService : ISubscriptionAccessService
{
    private readonly SaaSPlatformDbContext _db;
    private readonly IMemoryCache _cache;

    public SubscriptionAccessService(SaaSPlatformDbContext db, IMemoryCache cache)
    {
        _db = db;
        _cache = cache;
    }

    public async Task<bool> IsTenantAllowedAsync(Guid tenantId)
    {
        var cacheKey = $"tenant_subscription_{tenantId}";

        if (_cache.TryGetValue(cacheKey, out bool cachedValue))
            return cachedValue;

        var subscription = await _db.Subscriptions
            .AsNoTracking()
            .Include(s => s.Plan)
            .FirstOrDefaultAsync(s => s.TenantId == tenantId);

        bool allowed = false;
        if (subscription?.Plan != null)
        {
            allowed = subscription.Status == SubscriptionStatus.Active
                      || subscription.Status == SubscriptionStatus.Trialing
                      || (subscription.Status == SubscriptionStatus.GracePeriod
                          && DateTime.UtcNow < subscription.NextBillingDate.AddDays(subscription.Plan.GraceDays));
        }

        _cache.Set(cacheKey, allowed, TimeSpan.FromSeconds(60));
        return allowed;
    }
}