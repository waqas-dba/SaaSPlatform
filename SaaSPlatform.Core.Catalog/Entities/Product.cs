// SaaSPlatform.Core/Catalog/Entities/Product.cs
using SaaSPlatform.SharedKernel.Common;

namespace SaaSPlatform.Core.Catalog.Entities;

public class Product : AuditableEntity, ITenantScoped
{
    public Guid TenantId { get; set; }
    public Guid CategoryId { get; set; }

    public Guid StoreId { get; set; }

    public string Name { get; set; } = default!;
    public string? Description { get; set; }
    public decimal Price { get; set; }

    public bool IsActive { get; set; } = true;

    // New
    public int DisplayOrder { get; set; }
    public string? MainImageUrl { get; set; }

    public ICollection<ProductImage>? Images { get; set; }
}