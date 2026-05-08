// SaaSPlatform.Core/Tenant/Entities/StoreDeliveryZone.cs
using SaaSPlatform.SharedKernel.Common;

namespace SaaSPlatform.Core.Tenant.Entities;

public class StoreDeliveryZone : BaseEntity
{
    public Guid StoreId { get; set; }
    public Store Store { get; set; } = default!;

    public Guid ZoneId { get; set; }
    public Zone Zone { get; set; } = default!;
}