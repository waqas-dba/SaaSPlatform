using SaaSPlatform.Core.Billing.Entities;

namespace SaaSPlatform.Core.Billing.Interfaces;

public interface ISubscriptionRuleEngine
{
    Task<bool> CanAccessAsync(
        Subscription subscription,
        Plan plan);
}