namespace CoreKit.Catalog.Models;

public class ProductListDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = default!;
    public string Slug { get; set; } = default!;
    public decimal BasePrice { get; set; }
    public string CategoryName { get; set; } = default!;
    public string? PrimaryImageUrl { get; set; }
    public bool IsActive { get; set; }
}