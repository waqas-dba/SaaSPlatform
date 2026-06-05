// CoreKit.Catalog/Entities/Addon.cs
namespace CoreKit.Catalog.Entities;

public class Addon
{
    public Guid Id { get; set; }
    public Guid? AddonGroupId { get; set; }
    public AddonGroup? AddonGroup { get; set; }
    public string Name { get; set; } = default!;
    public decimal AdditionalPrice { get; set; }
    public bool IsActive { get; set; } = true;

    // REMOVED: ProductId / Product navigation — was dead code with no EF config
    // Products link to AddonGroups, not individual Addons
}