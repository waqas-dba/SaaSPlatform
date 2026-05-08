using Microsoft.Extensions.Caching.Memory;
using SaaSPlatform.Core.Billing.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;

namespace SaaSPlatform.Infrastructure.Services.Subscriptions;

public class SubscriptionAccessService : ISubscriptionAccessService
{
    private readonly ISubscriptionRepository _subRepo;
    private readonly ISubscriptionRuleEngine _ruleEngine;
    private readonly IMemoryCache _cache;

    public SubscriptionAccessService(
        ISubscriptionRepository subRepo,
        ISubscriptionRuleEngine ruleEngine,
        IMemoryCache cache)
    {
        _subRepo = subRepo;
        _ruleEngine = ruleEngine;
        _cache = cache;
    }

    public async Task<bool> IsTenantAllowedAsync(Guid tenantId)
    {
        var cacheKey = $"tenant_subscription_{tenantId}";
        if (_cache.TryGetValue(cacheKey, out bool cachedValue)) return cachedValue;

        var subscription = await _subRepo.GetActiveByTenantIdAsync(tenantId);
        bool allowed = subscription?.Plan != null
                       && await _ruleEngine.CanAccessAsync(subscription, subscription.Plan);

        _cache.Set(cacheKey, allowed, TimeSpan.FromSeconds(60));
        return allowed;
    }
}
