using SaaSPlatform.Core.Billing.Entities;
using SaaSPlatform.Core.Billing.Enums;

namespace SaaSPlatform.Core.Billing.Services;

public class SubscriptionRuleEngine
{
    public Task<bool> CanAccessAsync(Subscription subscription, Plan plan)
    {
        var now = DateTime.UtcNow;

        // TRIAL
        if (subscription.Status == SubscriptionStatus.Trialing)
        {
            return Task.FromResult(
                subscription.TrialEndsAt.HasValue &&
                subscription.TrialEndsAt > now);
        }

        // ACTIVE
        if (subscription.Status == SubscriptionStatus.Active)
            return Task.FromResult(true);

        // PAST DUE → GRACE HANDLING
        if (subscription.Status == SubscriptionStatus.PastDue)
        {
            if (!subscription.PaymentFailedAt.HasValue)
                return Task.FromResult(false);

            var graceEnd = subscription.PaymentFailedAt
                .Value.AddDays(plan.GraceDays);

            return Task.FromResult(graceEnd > now);
        }

        return Task.FromResult(false);
    }
}