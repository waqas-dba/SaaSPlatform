using SaaSPlatform.Core.Catalog.Entities;   // only needed if Cuisine referenced elsewhere
using SaaSPlatform.Core.SharedKernel.Common;

namespace SaaSPlatform.Core.Tenant.Entities;

public class Store : AuditableEntity, ITenantScoped
{
    public Guid TenantId { get; set; }
    public string Name { get; set; } = default!;
    public string? Slug { get; set; }

    // Many-to-many to Cuisine
    public ICollection<StoreCuisine>? StoreCuisines { get; set; }

    public string? Address { get; set; }
    public string? City { get; set; }
    public string? ZoneName { get; set; }

    public double Latitude { get; set; }
    public double Longitude { get; set; }

    public int MinPreparingTime { get; set; }
    public int MaxPreparingTime { get; set; }

    public string? CoverPhotoUrl { get; set; }
    public string? LogoUrl { get; set; }

    public bool IsOnline { get; set; } = true;

    public bool SupportsDelivery { get; set; } = true;
    public bool SupportsPickup { get; set; } = true;
    public bool SupportsDineIn { get; set; } = false;

    public ICollection<StoreDeliveryZone>? DeliveryZones { get; set; }
}