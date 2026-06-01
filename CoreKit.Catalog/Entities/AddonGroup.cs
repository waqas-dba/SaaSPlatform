using CoreKit.SharedKernel.Common;

namespace CoreKit.Catalog.Entities;

public class AddonGroup : AuditableEntity, ITenantScoped
{
    public string Name { get; set; } = default!;
    public string? StoreTypeCode { get; set; }
    public Guid TenantId { get; set; }

    public ICollection<Addon> Addons { get; set; } = new List<Addon>();
    public ICollection<Product> Products { get; set; } = new List<Product>();
}