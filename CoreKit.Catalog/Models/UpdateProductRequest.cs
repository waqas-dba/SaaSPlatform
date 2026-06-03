// CoreKit.Catalog/Models/UpdateProductRequest.cs
namespace CoreKit.Catalog.Models;

public class UpdateProductRequest
{
    public string? Name { get; set; }
    public string? Description { get; set; }
    public decimal? BasePrice { get; set; }
    public bool? IsActive { get; set; }

    /// <summary>Guid.Empty to detach.</summary>
    public Guid? AddonGroupId { get; set; }

    /// <summary>Guid.Empty to detach.</summary>
    public Guid? VariantGroupId { get; set; }

    /// <summary>
    /// Null = leave variants unchanged.
    /// Empty list = remove all variants.
    /// Populated = replace all variants.
    /// </summary>
    public List<CreateVariantItem>? Variants { get; set; }
}