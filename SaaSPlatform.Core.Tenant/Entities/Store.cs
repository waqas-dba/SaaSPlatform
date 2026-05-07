using SaaSPlatform.Core.Orders.Entities;
using SaaSPlatform.Core.SharedKernel.Common;

namespace SaaSPlatform.Core.Tenant.Entities;

public class Store : AuditableEntity, ITenantScoped
{
    public Guid TenantId { get; set; }

    public string Name { get; set; } = default!;

    public string? Phone { get; set; }

    public string? Address { get; set; }

    // GEO LOCATION
    public decimal? Latitude { get; set; }

    public decimal? Longitude { get; set; }

    public bool IsActive { get; set; } = true;

    public string? Slug { get; set; }

    public ICollection<RestaurantTable>? Tables { get; set; }
}