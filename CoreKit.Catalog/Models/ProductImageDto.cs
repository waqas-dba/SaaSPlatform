// ProductImageDto.cs
namespace CoreKit.Catalog.Models;

public class ProductImageDto
{
    public Guid Id { get; set; }
    public string ImageUrl { get; set; } = default!;
    public bool IsPrimary { get; set; }
}