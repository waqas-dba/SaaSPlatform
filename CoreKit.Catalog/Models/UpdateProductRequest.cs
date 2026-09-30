// CoreKit.Catalog/Models/UpdateProductRequest.cs
namespace CoreKit.Catalog.Models;

public class UpdateProductRequest
{
    public string? Name { get; set; }
    public string? Description { get; set; }
    public decimal? BasePrice { get; set; }
    public Guid? CategoryId { get; set; }
    public bool? IsActive { get; set; }
    public bool? IsVegetarian { get; set; }
    public int? PreparationTimeMinutes { get; set; }
    public int? SortOrder { get; set; }

    public bool? AvailableForDineIn { get; set; }
    public bool? AvailableForCollection { get; set; }
    public bool? AvailableForDelivery { get; set; }

    /// <summary>When not null, replaces the product's modifier groups (in this order).</summary>
    public List<Guid>? AddonGroupIds { get; set; }

    /// <summary>
    /// When not null, replaces the product's variants, matched by name:
    /// existing names are updated, new names are added, missing names are removed.
    /// </summary>
    public List<CreateVariantRequest>? Variants { get; set; }
}