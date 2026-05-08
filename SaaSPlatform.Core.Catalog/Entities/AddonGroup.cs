using SaaSPlatform.Core.SharedKernel.Common;

namespace SaaSPlatform.Core.Catalog.Entities;

public class AddonGroup : AuditableEntity, ITenantScoped
{
    public Guid TenantId { get; set; }
    public Guid StoreId { get; set; }
    public string Name { get; set; } = default!;          // e.g. "Choose your sauce"
    public string? Description { get; set; }
    public bool IsRequired { get; set; } = false;         // customer must pick one?
    public int MinSelect { get; set; } = 0;               // for multi-select groups
    public int MaxSelect { get; set; } = 1;               // 1 = single choice, >1 = multiple
    public bool IsActive { get; set; } = true;
    public int DisplayOrder { get; set; }

    public ICollection<AddonGroupItem> Items { get; set; } = new List<AddonGroupItem>();
}