// CoreKit.Catalog/Models/UpdateVariantRequest.cs
namespace CoreKit.Catalog.Models;

public class UpdateVariantRequest
{
    public string? Sku { get; set; }
    public decimal? Price { get; set; }
    public List<VariantAttributeItem>? Attributes { get; set; }
}