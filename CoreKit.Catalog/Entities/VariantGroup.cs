// CoreKit.Catalog/Entities/VariantGroup.cs
using CoreKit.SharedKernel.Common;

namespace CoreKit.Catalog.Entities;

public class VariantGroup : AuditableEntity, ITenantScoped
{
    public string Name { get; set; } = default!;
    public Guid TenantId { get; set; }
    public string StoreTypeCode { get; set; } = default!;
    public ICollection<VariantGroupOption> Options { get; set; }
        = new List<VariantGroupOption>();
    public ICollection<Product> Products { get; set; }
        = new List<Product>();
}