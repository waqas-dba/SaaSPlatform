using CoreKit.SharedKernel.Common;

namespace CoreKit.Catalog.Entities;

/// <summary>Links a product to one of its modifier groups.</summary>
public class ProductAddonGroup : BaseEntity
{
    public Guid ProductId { get; set; }
    public Product Product { get; set; } = default!;

    public Guid AddonGroupId { get; set; }
    public AddonGroup AddonGroup { get; set; } = default!;

    public int SortOrder { get; set; }
}