using SaaSPlatform.Core.Catalog.Entities;
using SaaSPlatform.SharedKernel.Common;

namespace SaaSPlatform.Core.Tenant.Entities;

public class StoreCuisine : BaseEntity
{
    public Guid StoreId { get; set; }
    public Store Store { get; set; } = default!;
    public Guid CuisineId { get; set; }
    public Cuisine Cuisine { get; set; } = default!;
}