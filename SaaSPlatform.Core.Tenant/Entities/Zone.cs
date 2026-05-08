// SaaSPlatform.Core/Tenant/Entities/Zone.cs
using SaaSPlatform.Core.SharedKernel.Common;

namespace SaaSPlatform.Core.Tenant.Entities;

public class Zone : BaseEntity
{
    public string Name { get; set; } = default!;
    public string City { get; set; } = default!;
    // Future: polygon field for geofencing
}