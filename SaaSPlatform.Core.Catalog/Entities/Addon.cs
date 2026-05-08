// SaaSPlatform.Core/Catalog/Entities/Addon.cs
using SaaSPlatform.SharedKernel.Common;

namespace SaaSPlatform.Core.Catalog.Entities;

public class Addon : BaseTenantEntity
{
    public Guid StoreId { get; set; }

    public string Name { get; set; } = default!;
    public decimal Price { get; set; }

    public bool IsActive { get; set; } = true;
}