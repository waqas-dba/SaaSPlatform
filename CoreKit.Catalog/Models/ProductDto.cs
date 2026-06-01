// ProductDto.cs
namespace CoreKit.Catalog.Models;

public class ProductDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = default!;
    public string Slug { get; set; } = default!;
    public string? Description { get; set; }
    public decimal BasePrice { get; set; }
    public Guid CategoryId { get; set; }
    public string CategoryName { get; set; } = default!;
    public Guid StoreId { get; set; }
    public Guid TenantId { get; set; }
    public bool TrackInventory { get; set; }
    public bool IsActive { get; set; }
    public Guid? AddonGroupId { get; set; }
    public List<ProductImageDto> Images { get; set; } = new();
    public List<ProductAttributeValueDto> AttributeValues { get; set; } = new();
    public List<ProductVariantDto> Variants { get; set; } = new();
    public List<AddonDto> Addons { get; set; } = new();
    public AddonGroupDto? AddonGroup { get; set; }
}