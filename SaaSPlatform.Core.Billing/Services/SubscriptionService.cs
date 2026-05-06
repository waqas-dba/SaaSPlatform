using SaaSPlatform.Core.Billing.Entities;
using SaaSPlatform.Core.Billing.Enums;

namespace SaaSPlatform.Core.Billing.Services;

public class SubscriptionService
{
    public void HandlePaymentFailed(Subscription subscription)
    {
        subscription.PaymentFailedAt = DateTime.UtcNow;
        subscription.Status = SubscriptionStatus.GracePeriod;
        subscription.GraceDaysUsed = 0;
    }

    public void EvaluateSubscription(Subscription subscription)
    {
        if (subscription.Status != SubscriptionStatus.GracePeriod)
            return;

        if (!subscription.PaymentFailedAt.HasValue)
            return;

        var daysPassed = (DateTime.UtcNow - subscription.PaymentFailedAt.Value).TotalDays;

        if (daysPassed <= subscription.MaxGraceDays)
        {
            subscription.GraceDaysUsed = (int)daysPassed;
            subscription.Status = SubscriptionStatus.GracePeriod;
        }
        else
        {
            subscription.Status = SubscriptionStatus.Suspended;
        }
    }

    public bool IsTenantActive(Subscription subscription)
    {
        return subscription.Status is SubscriptionStatus.Active
            or SubscriptionStatus.GracePeriod
            or SubscriptionStatus.Trialing;
    }
}