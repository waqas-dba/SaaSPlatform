using SaaSPlatform.Core.SharedKernel.Common;

namespace SaaSPlatform.Core.Billing.Entities;

public class Plan : AuditableEntity
{
    public string Name { get; set; } = default!;

    public decimal PriceMonthly { get; set; }
    public decimal PriceYearly { get; set; }

    public int MaxStores { get; set; }
    public int MaxUsers { get; set; }

    // ✅ NEW LIMITS
    public int MaxCategories { get; set; }
    public int MaxProducts { get; set; }

    public int MaxOrdersPerMonth { get; set; }

    public bool IsActive { get; set; } = true;
}