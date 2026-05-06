using SaaSPlatform.Core.SharedKernel.Common;

namespace SaaSPlatform.Core.Billing.Entities;

public class TenantUsage : BaseEntity
{
    public Guid TenantId { get; set; }

    public int CategoryCount { get; set; }
    public int ProductCount { get; set; }
    public int OrderCountThisMonth { get; set; }

    public DateTime LastUpdated { get; set; } = DateTime.UtcNow;
}