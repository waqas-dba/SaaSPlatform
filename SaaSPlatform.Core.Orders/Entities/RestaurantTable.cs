using SaaSPlatform.Core.SharedKernel.Common;

namespace SaaSPlatform.Core.Orders.Entities;

public class RestaurantTable : AuditableEntity, ITenantScoped
{
    public Guid TenantId { get; set; }

    public Guid StoreId { get; set; }

    public string Name { get; set; } = default!;

    public int Capacity { get; set; }

    public bool IsActive { get; set; } = true;

    public string? QRCode { get; set; }
}