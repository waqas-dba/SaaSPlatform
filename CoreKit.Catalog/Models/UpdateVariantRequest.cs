// CoreKit.Catalog/Models/UpdateVariantRequest.cs
namespace CoreKit.Catalog.Models;

public class UpdateVariantRequest
{
    public string? Name { get; set; }
    public decimal? Price { get; set; }
    public int? SortOrder { get; set; }
    public bool? IsActive { get; set; }
}