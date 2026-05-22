using CoreKit.SharedKernel.Common;

namespace CoreKit.Subscription.Entities;

public class Plan : AuditableEntity
{
    public string Name { get; set; } = default!;
    public string Code { get; set; } = default!;
    public string? Description { get; set; }
    public decimal MonthlyPrice { get; set; }
    public decimal YearlyPrice { get; set; }
    public int? MaxStores { get; set; }
    public int? MaxProducts { get; set; }
    public int? MaxCategories { get; set; }
    public bool CustomDomainEnabled { get; set; }
    public bool ThemeCustomizationEnabled { get; set; }
    public bool IsActive { get; set; } = true;
    public int SortOrder { get; set; }
    public ICollection<TenantSubscription> Subscriptions { get; set; } = new List<TenantSubscription>();
}