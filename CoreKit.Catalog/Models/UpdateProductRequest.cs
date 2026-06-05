// CoreKit.Catalog/Models/UpdateProductRequest.cs
namespace CoreKit.Catalog.Models;

public class UpdateProductRequest
{
    public string? Name { get; set; }
    public string? Description { get; set; }
    public decimal? BasePrice { get; set; }
    public bool? IsActive { get; set; }
    public Guid? AddonGroupId { get; set; }
    public Guid? VariantGroupId { get; set; }
    public List<CreateVariantRequest>? Variants { get; set; }

    // ── New hybrid availability flags ────────────────────────────────────
    public bool? AvailableForCollection { get; set; }
    public bool? AvailableForDelivery { get; set; }
}