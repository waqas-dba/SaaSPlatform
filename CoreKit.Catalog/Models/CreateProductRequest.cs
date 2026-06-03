// CoreKit.Catalog/Models/CreateProductRequest.cs
namespace CoreKit.Catalog.Models;

public class CreateProductRequest
{
    public string Name { get; set; } = default!;
    public string? Description { get; set; }
    public decimal BasePrice { get; set; }
    public Guid CategoryId { get; set; }
    public Guid StoreId { get; set; }
    public bool TrackInventory { get; set; } = false;

    /// <summary>Pre-configured addon group to attach.</summary>
    public Guid? AddonGroupId { get; set; }

    /// <summary>
    /// Pre-configured variant group to attach.
    /// When set, Variants must conform to the group's option templates.
    /// </summary>
    public Guid? VariantGroupId { get; set; }

    /// <summary>
    /// Actual variant SKUs with attribute values.
    /// Each attribute name/templateId must match an option
    /// in the attached VariantGroup.
    /// </summary>
    public List<CreateVariantItem> Variants { get; set; } = new();

    public List<AttributeValueItem> Attributes { get; set; } = new();
    public List<CreateImageItem> Images { get; set; } = new();
}