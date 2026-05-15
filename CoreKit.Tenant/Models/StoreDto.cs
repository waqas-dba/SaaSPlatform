namespace CoreKit.Tenant.Models;

public class StoreDto
{
    // Parameterless constructor for JSON deserialisation
    public StoreDto() { }

    // Your existing properties (unchanged)
    public Guid Id { get; set; }
    public Guid TenantId { get; set; }
    public string Name { get; set; } = default!;
    public string Slug { get; set; } = default!;
    public string? Type { get; set; }
    public bool IsActive { get; set; }
    public bool IsListedOnMarketplace { get; set; }
}