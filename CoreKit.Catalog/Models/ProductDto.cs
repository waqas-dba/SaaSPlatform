// CoreKit.Catalog/Models/ProductDto.cs
namespace CoreKit.Catalog.Models;

public class ProductDto
{
    public Guid Id { get; set; }
    public Guid TenantId { get; set; }
    public Guid CategoryId { get; set; }
    public string CategoryName { get; set; } = string.Empty;

    public string Name { get; set; } = default!;
    public string Slug { get; set; } = default!;
    public string? Description { get; set; }
    public decimal BasePrice { get; set; }

    public bool IsActive { get; set; }
    public bool IsVegetarian { get; set; }
    public int? PreparationTimeMinutes { get; set; }
    public int SortOrder { get; set; }

    public bool AvailableForDineIn { get; set; }
    public bool AvailableForCollection { get; set; }
    public bool AvailableForDelivery { get; set; }

    public List<ProductVariantDto> Variants { get; set; } = new();
    public List<ProductImageDto> Images { get; set; } = new();
    public List<AddonGroupDto> AddonGroups { get; set; } = new();

    /// <summary>Stores currently selling this product.</summary>
    public List<Guid> StoreIds { get; set; } = new();
}