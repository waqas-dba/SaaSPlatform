// SaaSPlatform.Core/Catalog/Entities/ProductImage.cs
using SaaSPlatform.Core.SharedKernel.Common;

namespace SaaSPlatform.Core.Catalog.Entities;

public class ProductImage : BaseEntity
{
    public Guid ProductId { get; set; }
    public string ImageUrl { get; set; } = default!;
    public int DisplayOrder { get; set; }
    public bool IsPrimary { get; set; }
}