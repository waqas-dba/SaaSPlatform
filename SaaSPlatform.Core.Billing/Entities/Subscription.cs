using SaaSPlatform.Core.SharedKernel.Common;

namespace SaaSPlatform.Core.Billing.Entities;

public class Subscription : BaseEntity, ITenantScoped
{
    public Guid TenantId { get; set; }   // ✅ REQUIRED

    public Guid PlanId { get; set; }

    public DateTime StartDate { get; set; }
    public DateTime? EndDate { get; set; }

    public bool IsActive { get; set; } = true;

    public string Status { get; set; } = "Active";

    public bool IsYearly { get; set; }

    public DateTime NextBillingDate { get; set; }

    public decimal PriceSnapshot { get; set; }
}