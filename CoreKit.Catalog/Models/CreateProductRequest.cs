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

    public Guid? AddonGroupId { get; set; }
    public List<CreateAddonItem> Addons { get; set; } = new();
    public List<CreateVariantItem> Variants { get; set; } = new();
    public List<AttributeValueItem> Attributes { get; set; } = new();
    public List<CreateImageItem> Images { get; set; } = new();
}
