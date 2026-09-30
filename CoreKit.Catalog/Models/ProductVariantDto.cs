// ProductVariantDto.cs
namespace CoreKit.Catalog.Models;

public class ProductVariantDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = default!;
    public decimal Price { get; set; }
    public int SortOrder { get; set; }
    public bool IsActive { get; set; }
}