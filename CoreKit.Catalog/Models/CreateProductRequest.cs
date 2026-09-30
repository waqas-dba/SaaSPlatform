using System.Xml.Linq;
using static System.Net.Mime.MediaTypeNames;

namespace CoreKit.Catalog.Models;

public class CreateProductRequest
{
    public string Name { get; set; } = default!;
    public string? Description { get; set; }
    public decimal BasePrice { get; set; }
    public Guid CategoryId { get; set; }

    public bool IsVegetarian { get; set; }
    public int? PreparationTimeMinutes { get; set; }
    public int SortOrder { get; set; }

    public bool AvailableForDineIn { get; set; } = true;
    public bool AvailableForCollection { get; set; } = true;
    public bool AvailableForDelivery { get; set; } = true;

    public List<CreateVariantRequest> Variants { get; set; } = new();
    public List<Guid> AddonGroupIds { get; set; } = new();
    public List<CreateImageItem> Images { get; set; } = new();

    /// <summary>Stores that should sell this product immediately. Optional.</summary>
    public List<Guid> StoreIds { get; set; } = new();
}