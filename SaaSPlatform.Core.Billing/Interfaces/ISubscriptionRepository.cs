using SaaSPlatform.Core.Billing.Entities;

namespace SaaSPlatform.Core.Billing.Interfaces;

public interface ISubscriptionRepository
{
    void Add(Subscription subscription);
    void Update(Subscription subscription);
    Task<Subscription?> GetActiveByTenantIdAsync(Guid tenantId, CancellationToken ct = default);
    Task<Plan?> GetPlanByNameAsync(string planName, CancellationToken ct = default);
}