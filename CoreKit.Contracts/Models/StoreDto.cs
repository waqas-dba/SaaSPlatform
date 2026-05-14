namespace CoreKit.Contracts.Models;

public class StoreDto
{
    public Guid Id { get; set; }
    public Guid TenantId { get; set; }
    public string Name { get; set; } = default!;
    public string Slug { get; set; } = default!;
    public string? Type { get; set; }
    public bool IsActive { get; set; }
    public bool IsListedOnMarketplace { get; set; }
    // … other fields as needed
}