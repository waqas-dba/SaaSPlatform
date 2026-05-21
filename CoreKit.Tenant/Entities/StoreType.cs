using CoreKit.SharedKernel.Common;

namespace CoreKit.Tenant.Entities;

public class StoreType : AuditableEntity
{
    public string Name { get; set; } = default!;

    public string Code { get; set; } = default!;

    public string Category { get; set; } = default!;

    public string? Description { get; set; }

    public string? Icon { get; set; }

    public string? Color { get; set; }

    public bool IsSystem { get; set; }

    public bool IsActive { get; set; } = true;

    public int SortOrder { get; set; }

    public ICollection<Store>? Stores { get; set; }
}