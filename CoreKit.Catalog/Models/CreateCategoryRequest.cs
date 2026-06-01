// CoreKit.Catalog/Models/CreateCategoryRequest.cs
namespace CoreKit.Catalog.Models;

public class CreateCategoryRequest
{
    public string Name { get; set; } = default!;
    public string? IconUrl { get; set; }
    public Guid? ParentCategoryId { get; set; }
    public string? StoreTypeCode { get; set; }
    public Guid? StoreId { get; set; }
    public Guid TenantId { get; set; }
}