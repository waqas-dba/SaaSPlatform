using SaaSPlatform.Core.SharedKernel.Common;

namespace SaaSPlatform.Core.Catalog.Entities;

public class Category : AuditableEntity, ITenantScoped
{
    public Guid TenantId { get; set; }

    public string Name { get; set; } = default!;

    public Guid? ParentCategoryId { get; set; }

    public bool IsActive { get; set; } = true;
}