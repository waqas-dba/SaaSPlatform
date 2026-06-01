// CoreKit.Catalog/Entities/Addon.cs
namespace CoreKit.Catalog.Entities;

public class Addon
{
    public Guid Id { get; set; }

    // FIX: Made nullable. Ad-hoc addons (attached directly to a product)
    // do not belong to any AddonGroup. Previously Guid.Empty was stored as
    // a sentinel value which either violated a FK constraint or silently
    // created orphaned records depending on whether the FK was enforced.
    public Guid? AddonGroupId { get; set; }
    public AddonGroup? AddonGroup { get; set; }

    public string Name { get; set; } = default!;
    public decimal AdditionalPrice { get; set; }
    public bool IsActive { get; set; } = true;

    public Guid? ProductId { get; set; }
    public Product? Product { get; set; }
}