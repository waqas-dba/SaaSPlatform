// ProductVariantDto.cs
namespace CoreKit.Catalog.Models;

public class ProductVariantDto
{
    public Guid Id { get; set; }
    public string Sku { get; set; } = default!;
    public decimal Price { get; set; }
    public bool IsActive { get; set; }
    public List<VariantAttributeValueDto> Attributes { get; set; } = new();
}