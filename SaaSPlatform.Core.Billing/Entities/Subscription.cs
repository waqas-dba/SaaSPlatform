using SaaSPlatform.Core.Billing.Enums;
using SaaSPlatform.Core.SharedKernel.Common;

namespace SaaSPlatform.Core.Billing.Entities;

public class Subscription : AuditableEntity, ITenantScoped
{
    public Guid TenantId { get; set; }

    public Guid PlanId { get; set; }

    public SubscriptionStatus Status { get; set; } = SubscriptionStatus.Trialing;

    public DateTime StartDate { get; set; }

    public DateTime? EndDate { get; set; }

    public DateTime? TrialEndsAt { get; set; }

    public DateTime? CancelledAt { get; set; }

    public DateTime? PaymentFailedAt { get; set; }

    public int MaxGraceDays { get; set; } = 7;

    public int GraceDaysUsed { get; set; }

    public bool IsAutoRenew { get; set; } = true;

    public DateTime NextBillingDate { get; set; }

    public decimal PriceSnapshot { get; set; }
}