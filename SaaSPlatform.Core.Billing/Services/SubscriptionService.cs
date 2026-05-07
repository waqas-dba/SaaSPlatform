using SaaSPlatform.Core.Billing.Entities;
using SaaSPlatform.Core.Billing.Enums;

namespace SaaSPlatform.Core.Billing.Services;

public class SubscriptionService
{
    public void HandlePaymentFailed(Subscription subscription)
    {
        subscription.PaymentFailedAt = DateTime.UtcNow;
        subscription.Status = SubscriptionStatus.PastDue;
    }

    public void Activate(Subscription subscription)
    {
        subscription.Status = SubscriptionStatus.Active;
        subscription.PaymentFailedAt = null;
        subscription.CancelledAt = null;
    }

    public void SuspendIfExpired(Subscription subscription, Plan plan)
    {
        if (!subscription.PaymentFailedAt.HasValue)
            return;

        var graceEnd = subscription.PaymentFailedAt
            .Value.AddDays(plan.GraceDays);

        if (DateTime.UtcNow > graceEnd)
        {
            subscription.Status = SubscriptionStatus.Suspended;
        }
    }
}