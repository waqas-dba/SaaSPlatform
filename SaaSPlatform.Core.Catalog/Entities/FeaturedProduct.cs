// SaaSPlatform.Core/Catalog/Entities/FeaturedProduct.cs
using SaaSPlatform.Core.Catalog.Enums;
using SaaSPlatform.Core.SharedKernel.Common;

namespace SaaSPlatform.Core.Catalog.Entities;

public class FeaturedProduct : BaseEntity, ITenantScoped
{
    public Guid TenantId { get; set; }
    public Guid ProductId { get; set; }
    public Product Product { get; set; } = default!;

    public ProductFeatureSection Section { get; set; }

    public Guid? CategoryId { get; set; }

    public int DisplayOrder { get; set; }

    public DateTime FeaturedFrom { get; set; }
    public DateTime? FeaturedUntil { get; set; }
}