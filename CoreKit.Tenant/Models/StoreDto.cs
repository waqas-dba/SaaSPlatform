using CoreKit.Tenant.Enums;

namespace CoreKit.Tenant.Models;

public class StoreDto
{
    public StoreDto() { }

    public Guid Id { get; set; }

    public Guid TenantId { get; set; }

    public string Name { get; set; } = default!;

    public string Slug { get; set; } = default!;

    // FIXED
    public StoreType Type { get; set; }

    public bool IsActive { get; set; }

    public bool IsListedOnMarketplace { get; set; }
}