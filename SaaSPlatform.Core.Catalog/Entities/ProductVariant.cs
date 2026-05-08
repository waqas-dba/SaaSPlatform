using SaaSPlatform.SharedKernel.Common;

namespace SaaSPlatform.Core.Catalog.Entities;

public class ProductVariant : BaseEntity
{
    public Guid ProductId { get; set; }

    public string Name { get; set; } = default!; // Small, Medium, Large
    public decimal Price { get; set; }
}