using SaaSPlatform.Core.Billing.Entities;
using SaaSPlatform.Core.Billing.Enums;
using SaaSPlatform.Core.Billing.Interfaces;

namespace SaaSPlatform.Core.Billing.Services;

public class SubscriptionRuleEngine : ISubscriptionRuleEngine
{
    public Task<bool> CanAccessAsync(
        Subscription subscription,
        Plan plan)
    {
        var now = DateTime.UtcNow;

        // ===== ACTIVE =====
        if (subscription.Status == SubscriptionStatus.Active)
        {
            return Task.FromResult(true);
        }

        // ===== TRIAL =====
        if (subscription.Status == SubscriptionStatus.Trialing)
        {
            if (subscription.TrialEndsAt.HasValue &&
                subscription.TrialEndsAt.Value > now)
            {
                return Task.FromResult(true);
            }

            return Task.FromResult(false);
        }

        // ===== GRACE PERIOD =====
        if (subscription.Status == SubscriptionStatus.GracePeriod)
        {
            var graceEndDate =
                subscription.NextBillingDate
                    .AddDays(plan.GraceDays);

            if (graceEndDate > now)
            {
                return Task.FromResult(true);
            }

            return Task.FromResult(false);
        }

        // ===== DEFAULT BLOCK =====
        return Task.FromResult(false);
    }
}