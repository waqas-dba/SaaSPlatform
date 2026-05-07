using SaaSPlatform.Core.Billing.Entities;
using SaaSPlatform.Core.Billing.Enums;

namespace SaaSPlatform.Core.Billing.Services;

public class SubscriptionService
{
    public void HandlePaymentFailed(
        Subscription subscription)
    {
        subscription.PaymentFailedAt = DateTime.UtcNow;

        subscription.Status =
            SubscriptionStatus.GracePeriod;
    }

    public void EvaluateSubscription(
        Subscription subscription,
        Plan plan)
    {
        if (subscription.Status != SubscriptionStatus.GracePeriod)
            return;

        if (!subscription.PaymentFailedAt.HasValue)
            return;

        var graceEndDate =
            subscription.NextBillingDate
                .AddDays(plan.GraceDays);

        if (DateTime.UtcNow > graceEndDate)
        {
            subscription.Status =
                SubscriptionStatus.Suspended;
        }
    }

    public bool IsTenantActive(
        Subscription subscription)
    {
        return subscription.Status is
            SubscriptionStatus.Active
            or SubscriptionStatus.Trialing
            or SubscriptionStatus.GracePeriod;
    }
}