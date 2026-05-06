using SaaSPlatform.Core.SharedKernel.Common;
using SaaSPlatform.Core.Billing.Enums;

namespace SaaSPlatform.Core.Billing.Entities;

public class Subscription : BaseEntity, ITenantScoped
{
    public Guid TenantId { get; set; }
    public Guid PlanId { get; set; }

    public SubscriptionStatus Status { get; set; } = SubscriptionStatus.Trialing;

    public DateTime StartDate { get; set; }
    public DateTime? EndDate { get; set; }

    public DateTime? TrialEndsAt { get; set; }

    // 🔴 Payment failure tracking
    public DateTime? PaymentFailedAt { get; set; }

    // 🟡 GRAY DAYS CONFIG
    public int MaxGraceDays { get; set; } = 7;
    public int GraceDaysUsed { get; set; } = 0;

    public bool IsAutoRenew { get; set; } = true;

    public DateTime NextBillingDate { get; set; }

    public decimal PriceSnapshot { get; set; }
}