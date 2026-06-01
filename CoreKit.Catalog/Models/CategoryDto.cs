// CoreKit.Catalog/Models/CategoryDto.cs
namespace CoreKit.Catalog.Models;

public class CategoryDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = default!;
    public string Slug { get; set; } = default!;
    public string? IconUrl { get; set; }
    public Guid? ParentCategoryId { get; set; }
    public int Level { get; set; }
    public string? StoreTypeCode { get; set; }
    public Guid? StoreId { get; set; }
    public Guid TenantId { get; set; }
    public List<CategoryDto> SubCategories { get; set; } = new();
}