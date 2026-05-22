using CoreKit.Subscription.Entities;

namespace CoreKit.Subscription.Interfaces;

public interface ITenantSubscriptionRepository
{
    Task<TenantSubscription?> GetActiveSubscriptionAsync(Guid tenantId, CancellationToken ct = default);
    void Add(TenantSubscription subscription);
    void Update(TenantSubscription subscription);
}