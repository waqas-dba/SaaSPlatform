using SaaSPlatform.Core.Catalog.Enums;
using SaaSPlatform.SharedKernel.Common;

namespace SaaSPlatform.Core.Catalog.Entities;

public class FeaturedProduct : BaseEntity, ITenantScoped
{
    public Guid TenantId { get; set; }
    public Guid StoreId { get; set; }               // NEW
    public Guid ProductId { get; set; }
    public Product Product { get; set; } = default!;

    public ProductFeatureSection Section { get; set; }
    public Guid? CategoryId { get; set; }

    public int DisplayOrder { get; set; }
    public DateTime FeaturedFrom { get; set; }
    public DateTime? FeaturedUntil { get; set; }
}