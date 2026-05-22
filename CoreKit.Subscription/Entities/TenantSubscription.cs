using CoreKit.SharedKernel.Common;

namespace CoreKit.Subscription.Entities;

public class TenantSubscription : AuditableEntity, ITenantScoped
{
    public Guid TenantId { get; set; }
    public Guid PlanId { get; set; }
    public Plan Plan { get; set; } = default!;
    public DateTime StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public SubscriptionStatus Status { get; set; } = SubscriptionStatus.Active;
    public string? PaymentReference { get; set; }
}

public enum SubscriptionStatus
{
    Active = 1,
    Expired = 2,
    Cancelled = 3,
    Trial = 4
}