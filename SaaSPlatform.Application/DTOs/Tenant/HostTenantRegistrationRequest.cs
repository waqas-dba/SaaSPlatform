namespace SaaSPlatform.Application.Models;

public class HostTenantRegistrationRequest
{
    public string RestaurantName { get; set; } = default!;
    public List<Guid> CuisineIds { get; set; } = new();
    public List<Guid> ZoneIds { get; set; } = new();
    public string? Address { get; set; }
    public int MinPreparingTime { get; set; }
    public int MaxPreparingTime { get; set; }
    public string FirstName { get; set; } = default!;
    public string LastName { get; set; } = default!;
    public string Phone { get; set; } = default!;
    public string Email { get; set; } = default!;
    public string Password { get; set; } = default!;
    public string CnicNumber { get; set; } = default!;
    public string? NtnNumber { get; set; }
    public bool HasFoodLicense { get; set; }
    public string? CnicFrontImageUrl { get; set; }
    public string? CnicBackImageUrl { get; set; }
    public string? MetadataJson { get; set; }
}