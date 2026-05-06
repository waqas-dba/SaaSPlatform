namespace SaaSPlatform.Core.Billing.Enums;

public enum SubscriptionStatus
{
    Trialing,
    Active,
    PastDue,
    GracePeriod,
    Suspended,
    Canceled
}