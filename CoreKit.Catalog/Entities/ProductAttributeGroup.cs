// CoreKit.Catalog/Entities/ProductAttributeGroup.cs
namespace CoreKit.Catalog.Entities;

/// <summary>
/// A named section that groups related attributes together on a product form.
/// e.g. "Nutritional Info", "Sizing", "Technical Specs".
/// Groups are owned by a template and ordered by SortOrder.
/// When StoreTypeCode is set and TenantId is null  → platform-level group.
/// When TenantId is set                            → tenant-scoped group.
/// </summary>
public class ProductAttributeGroup
{
    public Guid Id { get; set; }

    /// <summary>Display label shown on the product form.</summary>
    public string Name { get; set; } = default!;

    public int SortOrder { get; set; }

    // Scope — mirrors the same pattern used on ProductAttributeTemplate.
    public string StoreTypeCode { get; set; } = default!;
    public Guid? TenantId { get; set; }

    public ICollection<ProductAttributeTemplate> Attributes { get; set; }
        = new List<ProductAttributeTemplate>();
}