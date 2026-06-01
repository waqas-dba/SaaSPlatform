// CoreKit.Catalog/Entities/StoreAttributeOverride.cs
namespace CoreKit.Catalog.Entities;

/// <summary>
/// Lets a store manager toggle IsRequired / IsVisible for a specific
/// ProductAttributeTemplate on their store without touching the template itself.
///
/// Precedence (highest → lowest):
///   1. StoreAttributeOverride  (store-level toggle)
///   2. Tenant-scoped template  (TenantId set, OverridesTemplateId set)
///   3. Platform base template  (TenantId null)
/// </summary>
public class StoreAttributeOverride
{
    public Guid Id { get; set; }

    public Guid StoreId { get; set; }

    public Guid TemplateId { get; set; }
    public ProductAttributeTemplate Template { get; set; } = default!;

    /// <summary>
    /// Null = inherit from template.
    /// Set  = store-level override.
    /// </summary>
    public bool? IsRequired { get; set; }
    public bool? IsVisible { get; set; }
}