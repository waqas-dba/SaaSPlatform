using SaaSPlatform.Core.SharedKernel.Common;

namespace SaaSPlatform.Core.Billing.Entities;

public class Subscription : BaseEntity
{
    public Guid PlanId { get; set; }

    public DateTime StartDate { get; set; }
    public DateTime? EndDate { get; set; }

    public bool IsActive { get; set; } = true;

    public string Status { get; set; } = "Active";
    // Active / Expired / Cancelled

    public bool IsYearly { get; set; }

    public DateTime NextBillingDate { get; set; }
    public decimal PriceSnapshot { get; set; }
}