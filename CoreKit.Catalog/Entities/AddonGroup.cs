using CoreKit.SharedKernel.Common;

namespace CoreKit.Catalog.Entities;

/// <summary>
/// A modifier group such as "Choose your sauce" (Min 1, Max 1) or
/// "Extra toppings" (Min 0, Max 3). Reusable across products.
/// </summary>
public class AddonGroup : AuditableEntity, ITenantScoped
{
    public Guid TenantId { get; set; }

    public string Name { get; set; } = default!;

    /// <summary>Minimum options the customer must pick. 0 means optional.</summary>
    public int MinSelect { get; set; }

    /// <summary>Maximum options the customer may pick. At least 1.</summary>
    public int MaxSelect { get; set; } = 1;

    public int SortOrder { get; set; }

    public ICollection<Addon> Addons { get; set; } = new List<Addon>();
    public ICollection<ProductAddonGroup> ProductLinks { get; set; } = new List<ProductAddonGroup>();
}