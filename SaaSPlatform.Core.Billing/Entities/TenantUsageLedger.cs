using SaaSPlatform.SharedKernel.Common;

namespace SaaSPlatform.Core.Billing.Entities;

public class TenantUsageLedger : BaseEntity, ITenantScoped
{
    public Guid TenantId { get; set; }

    // Example: 202605
    public int UsageMonthYear { get; set; }

    public int ProductCount { get; set; }

    public int CategoryCount { get; set; }

    public int OrderCount { get; set; }

    public int StoreCount { get; set; }

    public int UserCount { get; set; }
}