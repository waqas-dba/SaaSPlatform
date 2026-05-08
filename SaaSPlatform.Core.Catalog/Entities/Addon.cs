// SaaSPlatform.Core/Catalog/Entities/Addon.cs
using SaaSPlatform.Core.SharedKernel.Common;

namespace SaaSPlatform.Core.Catalog.Entities;

public class Addon : AuditableEntity, ITenantScoped
{
    public Guid TenantId { get; set; }
    public Guid StoreId { get; set; }

    public string Name { get; set; } = default!;
    public decimal Price { get; set; }

    public bool IsActive { get; set; } = true;
}