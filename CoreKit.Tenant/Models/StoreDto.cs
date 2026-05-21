// ============================================================
// FILE: CoreKit.Tenant/Models/StoreDto.cs
// ============================================================

namespace CoreKit.Tenant.Models;

public class StoreDto
{
    public Guid Id { get; set; }

    public Guid TenantId { get; set; }

    public string Name { get; set; } = default!;

    public string Slug { get; set; } = default!;

    public Guid StoreTypeId { get; set; }

    public string StoreTypeName { get; set; } = default!;

    public string StoreCategory { get; set; } = default!;

    public bool IsActive { get; set; }

    public bool IsListedOnMarketplace { get; set; }
}