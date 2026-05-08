using SaaSPlatform.SharedKernel.Common;

namespace SaaSPlatform.Core.Catalog.Entities;

public class AddonGroupItem : BaseEntity
{
    public Guid AddonGroupId { get; set; }
    public AddonGroup AddonGroup { get; set; } = default!;

    public Guid AddonId { get; set; }
    public Addon Addon { get; set; } = default!;

    public decimal PriceAdjustment { get; set; } = 0;   // override price in this group (default = addon's price)
    public bool IsDefault { get; set; } = false;         // pre-selected in UI
    public int DisplayOrder { get; set; }
}