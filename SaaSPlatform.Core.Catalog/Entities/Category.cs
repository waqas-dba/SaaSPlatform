// SaaSPlatform.Core/Catalog/Entities/Category.cs
using SaaSPlatform.SharedKernel.Common;

namespace SaaSPlatform.Core.Catalog.Entities;

public class Category : AuditableEntity, ITenantScoped
{
    public Guid TenantId { get; set; }
    public Guid StoreId { get; set; }
    public string Name { get; set; } = default!;
    public Guid? ParentCategoryId { get; set; }

    // New fields
    public string? PhotoUrl { get; set; }
    public string? Icon { get; set; }
    public int DisplayOrder { get; set; }
    public bool IsActive { get; set; } = true;
}