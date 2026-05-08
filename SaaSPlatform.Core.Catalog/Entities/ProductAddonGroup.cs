using SaaSPlatform.Core.SharedKernel.Common;

namespace SaaSPlatform.Core.Catalog.Entities;

public class ProductAddonGroup : BaseEntity
{
    public Guid ProductId { get; set; }
    public Product Product { get; set; } = default!;

    public Guid AddonGroupId { get; set; }
    public AddonGroup AddonGroup { get; set; } = default!;

    public bool IsRequired { get; set; } = true;          // override group's IsRequired per product
    public int DisplayOrder { get; set; }
}