using SaaSPlatform.SharedKernel.Common;

namespace SaaSPlatform.Core.Billing.Entities;

public class Plan : AuditableEntity
{
    public string Name { get; set; } = default!;

    // ===== PRICING =====
    public decimal PriceMonthly { get; set; }
    public decimal PriceYearly { get; set; }

    // ===== LIMITS =====
    public int MaxStores { get; set; }
    public int MaxUsers { get; set; }
    public int MaxCategories { get; set; }
    public int MaxProducts { get; set; }
    public int MaxOrdersPerMonth { get; set; }

    // ===== BILLING RULES =====
    public int GraceDays { get; set; } = 7;
    public int TrialDays { get; set; } = 0;

    // ===== FEATURES =====
    public bool AllowOrders { get; set; } = true;
    public bool AllowProductCreation { get; set; } = true;
    public bool AllowAdminAccess { get; set; } = true;

    public bool IsActive { get; set; } = true;
}