// SaaSPlatform.Core/Tenant/Models/UpdateStoreRequest.cs
namespace SaaSPlatform.Core.Tenant.Models;

public class UpdateStoreRequest
{
    public string? Name { get; set; }
    public List<Guid>? CuisineIds { get; set; }
    public List<Guid>? ZoneIds { get; set; }
    public string? Address { get; set; }
    public int? MinPreparingTime { get; set; }
    public int? MaxPreparingTime { get; set; }
    public bool? IsOnline { get; set; }
    public bool? SupportsDelivery { get; set; }
    public bool? SupportsPickup { get; set; }
    public bool? SupportsDineIn { get; set; }
    public string? CoverPhotoUrl { get; set; }
    public string? LogoUrl { get; set; }
}